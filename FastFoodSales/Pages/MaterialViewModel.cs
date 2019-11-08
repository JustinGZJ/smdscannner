using System;
using System.Collections.Generic;
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

    public class TLaser : ISource
    {
        public string Source { get; set; }
        public string BobbinCode { get; set; }
        public string CodeQuality { get; set; }
        public string ProductionOrder { get; set; }
        public string Shift { get; set; }
        public string LineNo { get; set; }
        public string MachineNo { get; set; }
        public string EmployeeNo { get; set; }
        public string BobbinLotNo { get; set; }
        public string FlyWireLotNo { get; set; }
        public string TubeLotNo { get; set; }
    }

    public class TScan : ISource
    {
        public string Source { get; set; }
        public string Bobbin { get; set; }
        public string Shift { get; set; }
        public string ShiftName { get; set; }
        public string SpindleNo { get; set; }
        public string WireLotNo { get; set; }
    }
    public class MaterialViewModel : Screen, IHandle<string>
    {
        MsgFileSaver<TScan> fileSaver = new MsgFileSaver<TScan>();
        public int SelectedMode { get; set; }

        public int Capcity { get; set; }
        public int IPUnit { get; set; }
        public OEEViewModel OEE { get; set; } = new OEEViewModel();
        public BindableCollection<TBarcode> Barcodes { get; set; } = new BindableCollection<TBarcode>()
        {
        };
        ScannerService service;
        IEventAggregator Events;
        [Inject]
        public Info Info { get; set; }
        public MaterialViewModel([Inject]IEventAggregator Events, [Inject]ScannerService service, int IPUnit, int Capcity = 12)
        {
            this.Events = Events;
            this.service = service;
            this.Capcity = Capcity;
            this.IPUnit = IPUnit;
            Events.Subscribe(this, IPUnit.ToString());
        }

        public int ItemsHeight => Capcity / 4 * 35 + 20;
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
            ScanRate = ((_cntBarcode - _cntErrorBarcode) * 1.0 / _cntBarcode).ToString("P").Replace(",","");
            Barcodes.Add(new TBarcode { Index = count + 1, Content = message });
            fileSaver.Process(new TScan()
            {
                Bobbin = message,
                Shift = Properties.Settings.Default.Shift,
                ShiftName = Properties.Settings.Default.ShiftName,
                Source = DisplayName,
                SpindleNo = count.ToString(),
                WireLotNo = Properties.Settings.Default.LotNo
            });

        }
    }
}
