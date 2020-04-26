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
        [DisplayName("Code Quality")]
        public string CodeQuality { get; set; }
        [DisplayName("ProductionOrder")]
        public string ProductionOrder { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;
        public string Station { get; set; } = "LaserCode";
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