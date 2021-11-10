using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel;

namespace DAQ.Pages
{
    [SubFilePath("Laser")]
    public class Laser
    {
        [DisplayName("No.")] public int No { get; set; } = 1;
        [DisplayName("Bobbin Code")]
        public string BobbinCode { get; set; }
        [DisplayName("Production Order")]
        public string ProductionOrder { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;
        public string Station { get; set; } = Properties.Settings.Default.Station;
        public string Shift { get; set; }
        [DisplayName("Shift name")]
        public string ShiftName { get; set; }
        [DisplayName("Line No.")]
        public string LineNo { get; set; }
        [DisplayName("Machine No.")]
        public string MachineNo { get; set; }
        [DisplayName("Employee No.")]
        public string EmployeeNo { get; set; }
        [DisplayName("Bobbin Lot No.")]
        public string BobbinLotNo { get; set; }
        [DisplayName("Bobbin Part Name")]
        public string BobbinPartName { get; set; }
        [DisplayName("Bobbin tool number")]
        public string BobbinToolNo { get; set; }
        [DisplayName("Bobbin cavity number")]
        public string BobbinCavityNo { get; set; }
        [DisplayName("1st Barcode Quality")]
        public string CodeQuality { get; set; }
    }


    public class LaserPoco
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [DisplayName("No.")] public int No { get; set; } = 1;
        [DisplayName("Bobbin Code")]
        public string BobbinCode { get; set; }
        [DisplayName("Code Quality")]
        public string CodeQuality { get; set; }
        [DisplayName("ProductionOrder")]
        public string ProductionOrder { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;
        public string Station { get; set; } = Properties.Settings.Default.Station;
        public string Shift { get; set; }
        [DisplayName("Shift name")]
        public string ShiftName { get; set; }
        [DisplayName("Line No.")]
        public string LineNo { get; set; }
        [DisplayName("Machine No.")]
        public string MachineNo { get; set; }
        [DisplayName("Employee No.")]
        public string EmployeeNo { get; set; }
        [DisplayName("Bobbin Lot No.")]
        public string BobbinLotNo { get; set; }
        [DisplayName("Bobbin Part Name")]
        public string BobbinPartName { get; set; }
        [DisplayName("Bobbin tool number")]
        public string BobbinToolNo { get; set; }
        [DisplayName("Bobbin cavity number")]
        public string BobbinCavityNo { get; set; }
    }
}