using System;
using System.ComponentModel;

namespace DAQ.Pages
{
    [SubFilePath("N5")]
    public class Scan
    {
        [DisplayName("No.")]
        public int No { get; set; } = 1;
        [DisplayName("ProductionOrder")]
        public string Production { get; set; }
        [DisplayName("Bobbin Code")]
        public string Bobbin { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;
        public string Station { get; set; } = "N3";
        public string Shift { get; set; }
        [DisplayName("Shift Name")]
        public string ShiftName { get; set; }
        [DisplayName("Line No.")]
        public string LineNo { get; set; }
        [DisplayName("Machine No.")]
        public string MachineNo { get; set; }
        [DisplayName("Employee No.")]
        public string EmployeeNo { get; set; }
        [DisplayName("Spindle No.")]
        public string SpindleNo { get; set; }

        [DisplayName("tube wire lot No.")]
        public string TubeLotNo { get; set; }

        [DisplayName("TIW wire lot No. ")]
        public string FlyWireLotNo { get; set; }
    }
}
