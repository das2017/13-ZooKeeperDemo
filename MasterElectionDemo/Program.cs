using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MasterElectionDemo
{
    /// <summary>
    /// 这是匹配【Master选举】应用场景的Demo
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            MasterElectionClient mec = new MasterElectionClient();
            mec.Init();
            while (true)
            {
                if (mec.IsMaster)
                {
                    Console.WriteLine("执行Master相关逻辑。");
                }
                else
                {
                    Console.WriteLine("执行Slave相关逻辑。");
                }
                Thread.Sleep(1000);                
            }
        }
    }
}
