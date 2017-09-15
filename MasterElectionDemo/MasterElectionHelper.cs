using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;
using System.Net;

namespace MasterElectionDemo
{
    class MasterElectionHelper
    {
        internal static string ZKServer
        {
            get
            {
                return ConfigurationManager.AppSettings["ZKServer"];
            }
        }
        internal static double ZKSessionTimeOut
        {
            get
            {
                string zkSessionTimeOut = ConfigurationManager.AppSettings["ZKSessionTimeOut"];
                if (string.IsNullOrWhiteSpace(zkSessionTimeOut))
                {
                    return 3600;
                }
                return double.Parse(zkSessionTimeOut);
            }
        }
        internal static string ZKRootPath
        {
            get
            {
                string rootName = string.Format("{0}_{1}", AppID, ConfigurationManager.AppSettings["ZKMasterElectionRootNodeName"]);
                return string.Format(@"/MasterElection/{0}", rootName);
            }
        }
        internal static string AppID
        {
            get
            {
                return ConfigurationManager.AppSettings["AppID"];
            }
        }
        internal static IPAddress GetHostIP()
        {
            string name = Dns.GetHostName();
            IPHostEntry me = Dns.GetHostEntry(name);
            IPAddress[] ips = me.AddressList;
            foreach (IPAddress ip in ips)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    continue;
                }

                return ip;
            }
            return ips != null && ips.Length > 0 ? ips[0] : new IPAddress(0x0);
        }
    }
}
