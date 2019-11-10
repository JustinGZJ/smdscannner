using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stylet;
using SimpleTCP;
using StyletIoC;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using DAQ.Service;

namespace DAQ.Pages
{

    public class TBarcode
    {
        static readonly string[] axisString = {
            "Fisrt Axis",
            "Second Axis",
            "Third Axis",
            "Fourth Axis",
            "Fifth Axis",
            "Sixth Axis",
            "Seventh Axis",
            "Eighth Axis",
            "Ninth Axis",
            "Tenth Axis",
            "Eleventh Axis",
            "Twelfth Axis",
            "Thirteenth Axis",
            "Fourteenth Axis",
            "Fifteenth Axis"};
        public int Index { get; set; }
        public string Axis
        {
            get
            {
                if (Index < 15)
                    return axisString[Index - 1];
                else
                    return (Index).ToString() + "th Axis ";
            }
        }
        public string Content { get; set; }
    }

    public class Info : PropertyChangedBase
    {
        public string Name { get; set; }
        public string Product { get; set; }
        public string Shift { get; set; }
    }

    public class Laser
    {
        [DisplayName("Bobbin Code")]
        public string BobbinCode { get; set; }
        [DisplayName("Code Quality")]
        public string CodeQuality { get; set; }
        public string Production { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;
        public string Order { get; set; }
        public string Station { get; set; } = "LaserCode";
        public string Shift { get; set; }
        public string ShiftName { get; set; }
        public string LineNo { get; set; }
        public string MachineNo { get; set; }
        public string EmployeeNo { get; set; }
        public string BobbinLotNo { get; set; }
        public string BobbinToolNo { get; set; }
        public string BobbinCavityNo { get; set; }
    }

    public class Scan
    {
        [DisplayName("Bobbin Code")]
        public string Bobbin { get; set; }
        [DisplayName("Production")]
        public string Production { get; set; }
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
        [DisplayName("Fly Wire lot No.")]
        public string FlyWireLotNo { get; set; }
        [DisplayName("Tube Lot No.")]
        public string TubeLotNo { get; set; }
    }
    public class MaterialViewModel : Screen, IHandle<string>
    {
        private MsgFileSaver<Scan> fileSaver;
        public int SelectedMode { get; set; }

        public int Capcity { get; set; }
        private int _ipunit;
        public int IpUnit
        {
            get => _ipunit;
            set
            {
                Events.Subscribe(this, value.ToString());
                _ipunit = value;
            }
        }
        public OEEViewModel OEE { get; set; } = new OEEViewModel();
        public BindableCollection<TBarcode> Barcodes { get; set; } = new BindableCollection<TBarcode>()
        {
        };
        ScannerService service;
        IEventAggregator Events;
        [Inject]
        public Info Info { get; set; }
        public MaterialViewModel([Inject]IEventAggregator events, [Inject] MsgFileSaver<Scan> fileSaver, [Inject]ScannerService service, int capcity = 12)
        {
            this.Events = events;
            this.service = service;
            this.Capcity = capcity;
            this.fileSaver = fileSaver;

        }
        public int ItemsHeight => Capcity / 4 * 40 + 20;
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
        }
        int _cntBarcode = 0;
        int _cntErrorBarcode = 0;

        public string ScanRate { get; set; } = "100.00%";
        public void Handle(string message)
        {
            var count = Barcodes.Count;
            if (count >= Capcity)
            {
                Barcodes.Clear();
                count = 0;
            }
            _cntBarcode++;
            if (message.ToUpper().Contains("ERROR"))
            {
                _cntErrorBarcode++;
            }
            ScanRate = ((_cntBarcode - _cntErrorBarcode) * 1.0 / _cntBarcode).ToString("P").Replace(",", "");
            Barcodes.Add(new TBarcode { Index = count + 1, Content = message });
            var settings = Properties.Settings.Default;
            fileSaver.Process(new Scan
            {
                Bobbin = message,
                Shift = settings.Shift,
                ShiftName = settings.ShiftName,
                Production = settings.Production,
                LineNo = settings.LineNo,
                MachineNo = settings.MachineNo,
                EmployeeNo = settings.EmployeeNo,
                FlyWireLotNo = settings.FlyWireLotNo,
                TubeLotNo = settings.TubeLotNo
            }

            );

        }
    }
}
