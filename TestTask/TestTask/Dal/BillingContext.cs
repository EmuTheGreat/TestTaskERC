using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestTask.dal.Models;

namespace TestTask.dal
{
    /// <summary>
    /// ДБ контекст для расчета услуг
    /// </summary>
    public class BillingContext : DbContext
    {
        public DbSet<MonthlyReading> MonthlyReadings { get; set; }
        public DbSet<ResidentCount> ResidentCounts { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<ServiceTariff> Tariffs { get; set; }
        public DbSet<ServiceNorm> Norms { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options) =>
            options.UseSqlite("Data Source=billing.db");

        protected override void OnModelCreating(ModelBuilder model)
        {
            model.Entity<ServiceTariff>()
                 .HasIndex(t => new { t.Service, t.EffectiveFrom });
            model.Entity<ServiceNorm>()
                 .HasIndex(n => new { n.Service, n.EffectiveFrom });
        }
    }
}
