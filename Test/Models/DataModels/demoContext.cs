using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Test.Models.DataModels
{
    public partial class demoContext : DbContext
    {
        public demoContext()
        {
        }

        public demoContext(DbContextOptions<demoContext> options)
            : base(options)
        {
        }

        public virtual DbSet<SysPage> SysPage { get; set; }
        public virtual DbSet<SysUser> SysUser { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=demo;Integrated Security=false;uid=sa;pwd=manager1!");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entityTypes = Assembly.GetEntryAssembly().GetTypes()
                        .Where(type => typeof(IDbTable).IsAssignableFrom(type) && type != typeof(IDbTable));

            foreach (var type in entityTypes)
            {
                modelBuilder.Model.GetOrAddEntityType(type);
            }
        }
    }

    public interface IDbTable { }
}
