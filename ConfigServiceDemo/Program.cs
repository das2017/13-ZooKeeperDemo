using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ZooKeeperNet;

namespace ConfigServiceDemo
{
    class Program
    {
        static void Main(string[] args)
        {            
            ConfigServiceClient conf = new ConfigServiceClient("139.198.13.12:5011,139.198.13.12:5012,139.198.13.12:5013", TimeSpan.FromSeconds(3600));
            Console.WriteLine("已连接ZooKeeper。{0}", Environment.NewLine);

            try
            {
                conf.QueryPath = "/UserName";
                if (conf.ZK.Exists(conf.QueryPath, false) == null)
                {
                    conf.ConfigData = "Jack".GetBytes();
                    conf.ZK.Create(conf.QueryPath, conf.ConfigData, Ids.OPEN_ACL_UNSAFE, CreateMode.Persistent);
                }

                string configData = conf.ReadConfigData();
                Console.WriteLine("节点【{0}】目前的值为【{1}】。", conf.QueryPath, configData);
                Console.ReadLine();

                Random random = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                conf.ConfigData = string.Format("Mike_{0}", random.Next(100)).GetBytes();
                conf.ZK.SetData(conf.QueryPath, conf.ConfigData, -1);
                Console.WriteLine("节点【{0}】的值已被修改为【{1}】。", conf.QueryPath, Encoding.UTF8.GetString(conf.ConfigData));
                Console.ReadLine();

                if (conf.ZK.Exists(conf.QueryPath, false) != null)
                {
                    conf.ZK.Delete(conf.QueryPath, -1);
                    Console.WriteLine("已删除此【{0}】节点。{1}", conf.QueryPath, Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                if (conf.ZK == null)
                {
                    Console.WriteLine("已关闭ZooKeeper的连接。");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine("抛出异常：{0}【{1}】。", Environment.NewLine, ex.ToString());
            }
            finally
            {
                conf.Close();
                Console.WriteLine("已关闭ZooKeeper的连接。");
                Console.ReadLine();
            }
        }
    }
}
