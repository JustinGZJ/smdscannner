using System;
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
       //     optionsBuilder.UseSqlite(@"Data Source = data.db;");
        }
    }


    public class StatusDto
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public int AlarmInfoId { get; set; }
        public StatusInfoDto StatusInfo { get; set; }
        public  TimeSpan Span { get; set; }
    }


    public class StatusInfoDto
    {
        public int Id { get; set; }
        public int StationId { get; set; }
        public int AlarmIndex { get; set; }
        public string AlarmContent { get; set; }
    }
}