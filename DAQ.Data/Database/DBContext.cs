using System;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace DAQ.Data
{
    public class OeedbContext : DbContext
    {
        public static Mutex DbMutex { get; } = new Mutex(false, "DataDb");
        public DbSet<StatusDTO> Alarms { get; set; }
        public DbSet<StatusInfoDTO> AlarmInfos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Database=data.db");
        }
    }


    public class StatusDTO
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public int AlarmInfoId { get; set; }
        public StatusInfoDTO StatusInfo { get; set; }
        public  TimeSpan Span { get; set; }
    }


    public class StatusInfoDTO
    {
        public int Id { get; set; }
        public int StationId { get; set; }
        public int AlarmIndex { get; set; }
        public string AlarmContent { get; set; }
    }
}