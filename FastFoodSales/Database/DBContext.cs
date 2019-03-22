using System;
using System.Configuration;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace DAQ.Database
{
    public class OeedbContext : DbContext
    {
        public static Mutex DbMutex { get; } = new Mutex(false, "DataDb");
        public DbSet<StatusDto> Alarms { get; set; }
        public DbSet<StatusInfoDto> AlarmInfos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source = data.db;");
        //  optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings["Oeedb"].ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StatusDto>().HasIndex(x => x.Time);
            modelBuilder.Entity<ConfigDto>().HasKey(x => new {x.Section, x.Key});
            base.OnModelCreating(modelBuilder);
        }
    }


    public class StatusDto
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public int StatusInfoId { get; set; }
        public StatusInfoDto StatusInfo { get; set; }
        public  TimeSpan Span { get; set; }
    }

    public class ConfigDto
    {
        public string Section { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
    public class StatusInfoDto
    {
        public int Id { get; set; }
        public int StationId { get; set; }
        public int AlarmIndex { get; set; }
        public string AlarmContent { get; set; }
    }
}