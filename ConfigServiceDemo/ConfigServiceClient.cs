using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ZooKeeperNet;
using Org.Apache.Zookeeper.Data;

namespace ConfigServiceDemo
{
    class ConfigServiceClient
    {
        public ZooKeeper ZK { get; set; }

        // 配置项
        private string _queryPath = "/Configuration";
        public string QueryPath
        {
            get
            {
                return _queryPath;
            }
            set
            {
                _queryPath = value;
            }
        }

        public Stat Stat { get; set; }

        // 配置数据
        private byte[] _configData = null;
        public byte[] ConfigData
        {
            get 
            { 
                return _configData; 
            }
            set
            {
                _configData = value;
            }
        }

        public ConfigServiceClient(string serviceAddress, TimeSpan timeout)
        {
            try
            {
                this.ZK = new ZooKeeper(serviceAddress, timeout, new ConfigServiceWatcher(this));
                ZooKeeper.WaitUntilConnected(this.ZK);
            }
            catch
            {
                this.Close();
            }
        }

        // 读取节点的配置数据
        public string ReadConfigData()
        {
            try
            {
                if (this.ZK == null)
                {
                    return string.Empty;
                }
                Stat stat = this.ZK.Exists(this._queryPath, true);
                if (stat == null)
                {
                    return string.Empty;
                }

                this._configData = this.ZK.GetData(this._queryPath, true, stat);
                this.Stat = stat;

                return Encoding.UTF8.GetString(this._configData);
            }
            catch
            {
            }

            return string.Empty;
        }

        // 关闭ZooKeeper连接
        // 释放资源
        public void Close()
        {
            try
            {
                if (this.ZK != null)
                {
                    this.ZK.Dispose();
                }
                this.ZK = null;
            }
            catch
            {
            }
        }

        void ProcessWatchedEvent(WatchedEvent @event)
        {
            try
            {
                if (@event.Type == EventType.NodeDataChanged)
                {
                    Console.WriteLine("{0}收到修改此节点【{1}】值的通知，其值已被改为【{2}】。", Environment.NewLine, this._queryPath, this.ReadConfigData());
                }
            }
            catch
            {
            }
        }
        class ConfigServiceWatcher : IWatcher
        {
            private ConfigServiceClient _cs = null;

            public ConfigServiceWatcher(ConfigServiceClient cs)
            {
                _cs = cs;
            }

            public void Process(WatchedEvent @event)
            {
                _cs.ProcessWatchedEvent(@event);
            }
        }
    }
}
