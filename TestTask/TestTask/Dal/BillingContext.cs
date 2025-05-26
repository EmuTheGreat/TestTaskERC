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
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<MonthlyReading> MonthlyReadings { get; set; }
        public DbSet<ResidentCount> ResidentCounts { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<ServiceTariff> Tariffs { get; set; }
        public DbSet<ServiceNorm> Norms { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
            var dbPath = Path.Combine(projectRoot, "billing.db");
            Console.WriteLine($"Using database file: {dbPath}");
            options.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder model)
        {
            model.Entity<ResidentCount>()
                .HasOne<Tenant>()
                .WithMany(s => s.ResidentCounts)
                .HasForeignKey(r => r.TenantId);

            model.Entity<MonthlyReading>()
                .HasOne<Tenant>()
                .WithMany(s => s.MonthlyReadings)
                .HasForeignKey(r => r.TenantId);

            model.Entity<Bill>()
                .HasOne<Tenant>()
                .WithMany(s => s.Bills)
                .HasForeignKey(b => b.TenantId);

            model.Entity<ServiceTariff>()
                .HasIndex(t => new { t.Service, t.EffectiveFrom });
            model.Entity<ServiceNorm>()
                .HasIndex(n => new { n.Service, n.EffectiveFrom });
        }
    }
}
