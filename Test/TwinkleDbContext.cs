using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Test
{
    public class TwinkleDbContext: DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=demo;Integrated Security=false;uid=sa;pwd=manager1!");
        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    var entityTypes = Assembly.GetEntryAssembly().GetTypes()
        //                .Where(type => !String.IsNullOrEmpty(type.Namespace))
        //                .Where(type =>  type.GetTypeInfo().BaseType != null && type.GetTypeInfo().BaseType == typeof(EntityBase));

        //    foreach (var type in entityTypes)
        //    {
        //        modelBuilder.Model.GetOrAddEntityType(type);
        //    }
        //    base.OnModelCreating(modelBuilder);
        //}
    }
}
