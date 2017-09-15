using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Net;
using ZooKeeperNet;
using Org.Apache.Zookeeper.Data;

namespace MasterElectionDemo
{
    class MasterElectionClient
    {
        private ZooKeeper zk = null;
        private string clientNodePath;
        private bool isMaster = false;
        /// <summary>
        /// 是否为Master机
        /// </summary>
        public bool IsMaster
        {
            get
            {
                return isMaster;
            }
        }
        /// <summary>
        /// 初始化
        /// </summary>        
        public void Init()
        {
            zk = new ZooKeeper(MasterElectionHelper.ZKServer, TimeSpan.FromSeconds(MasterElectionHelper.ZKSessionTimeOut), new MasterElectionWatcher(this));
            ZooKeeper.WaitUntilConnected(zk);
            if (zk.Exists(MasterElectionHelper.ZKRootPath, false) == null)
            {
                zk.Create(MasterElectionHelper.ZKRootPath, string.Empty.GetBytes(), Ids.OPEN_ACL_UNSAFE, CreateMode.Persistent);
                zk.GetChildren(MasterElectionHelper.ZKRootPath, true);
            }

            string ip = MasterElectionHelper.GetHostIP().ToString();
            string nodePrefixName = string.Format("{0}_", ip.Replace(".", "-"));
            string nodePath = string.Format("{0}/{1}", MasterElectionHelper.ZKRootPath, nodePrefixName);
            if (string.IsNullOrWhiteSpace(this.clientNodePath) || zk.Exists(this.clientNodePath, false) == null)
            {
                this.clientNodePath = zk.Create(nodePath, string.Empty.GetBytes(), Ids.OPEN_ACL_UNSAFE, CreateMode.EphemeralSequential);
            }

            zk.GetChildren(MasterElectionHelper.ZKRootPath, true);
        }
        void ProcessWatchedEvent(WatchedEvent @event)
        {
            if (@event.Type != EventType.NodeChildrenChanged)
            {
                if (zk.Exists(MasterElectionHelper.ZKRootPath, false) != null)
                {
                    zk.GetChildren(MasterElectionHelper.ZKRootPath, true);
                }
                return;
            }

            IEnumerable<string> childrenList = zk.GetChildren(MasterElectionHelper.ZKRootPath, false);
            List<string> nameList = null;
            if (childrenList != null)
            {
                nameList = childrenList.ToList();
            }
            if (nameList == null || nameList.Count < 1)
            {
                zk.GetChildren(MasterElectionHelper.ZKRootPath, true);
                return;
            }
            if (nameList.Count == 1)
            {
                this.isMaster = true;
                zk.GetChildren(MasterElectionHelper.ZKRootPath, true);
                return;
            }

            List<string> tmpNameList = new List<string>();
            tmpNameList.AddRange(nameList);
            this.ShellSort(tmpNameList);

            string masterNodeName = tmpNameList[0];
            string path = string.Format("{0}/{1}", MasterElectionHelper.ZKRootPath, masterNodeName);
            if (path.Equals(this.clientNodePath))
            {
                this.isMaster = true;
            }

            zk.GetChildren(MasterElectionHelper.ZKRootPath, true);
        }

        void ShellSort(List<string> nameList)
        {
            int length = nameList.Count;

            int d = length / 2;
            string tmpNodeName;
            string[] tmpNodeNames;
            long tmpSessionId;
            long baseSessionId;
            int j;

            while (d > 0)
            {
                for (int i = d; i < length; i++)
                {
                    tmpNodeName = nameList[i];
                    tmpNodeNames = tmpNodeName.Split('_');
                    tmpSessionId = long.Parse(tmpNodeNames[1]);

                    j = i - d;
                    baseSessionId = long.Parse(nameList[j].Split('_')[1]);
                    while (j >= 0 && tmpSessionId < baseSessionId)
                    {
                        nameList[j + d] = nameList[j];
                        j = j - d;
                        if (j >= 0)
                        {
                            baseSessionId = long.Parse(nameList[j].Split('_')[1]);
                        }
                    }
                    nameList[j + d] = tmpNodeName;
                }
                d = d / 2;
            }
        }
        class MasterElectionWatcher : IWatcher
        {
            private MasterElectionClient _masterElectionClient = null;
            public MasterElectionWatcher(MasterElectionClient mec)
            {
                _masterElectionClient = mec;
            }
            public void Process(WatchedEvent @event)
            {
                _masterElectionClient.ProcessWatchedEvent(@event);
            }
        }
    }
}
