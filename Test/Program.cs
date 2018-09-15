using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Test.Models.DataModels;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            using (demoContext dc = new demoContext())
            {
                SysPage sp = dc.SysPage.Single(d => d.Id == 1);
                sp.NValue = 1;

                var c = dc.Update(sp);

                c.Property("NValue").IsModified = false;
                c.Property("Id").IsModified = false;
                dc.SaveChanges();

                Console.WriteLine("我");
                Console.ReadKey();
            }
        }
       
    }
}
