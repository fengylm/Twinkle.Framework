using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"TID:{Thread.CurrentThread.GetHashCode()},main启动");
            Excute();
            Console.WriteLine($"TID:{Thread.CurrentThread.GetHashCode()},main结束");
            Console.ReadKey();
        }

        private static void   Excute()
        {
            Refresh("key");
             RefreshAsync("key");
            Console.WriteLine($"TID:{Thread.CurrentThread.GetHashCode()},啦啦");
        }

        public static void RefreshAsync(string key)
        {
            Task.Run(() =>
            {
                Console.WriteLine($"TID:{Thread.CurrentThread.GetHashCode()},异步写入缓存");
                Thread.Sleep(5000);
                Console.WriteLine($"TID:{Thread.CurrentThread.GetHashCode()},异步写入完成");
            }).GetAwaiter().GetResult();
        }

        public static void Refresh(string key)
        {
            Console.WriteLine($"TID:{Thread.CurrentThread.GetHashCode()},同步写入缓存");
            Thread.Sleep(5000);
            Console.WriteLine($"TID:{Thread.CurrentThread.GetHashCode()},同步写入完成");
        }
    }
}
