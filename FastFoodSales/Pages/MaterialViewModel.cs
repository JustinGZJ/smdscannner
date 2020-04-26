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

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SubFilePathAttribute : Attribute  //类名是特性的名称
    {
        public string Name { get; }

        public SubFilePathAttribute(string name) //name为定位参数
        {
            this.Name = name;
        }
    }
    [SubFilePath("N3")]
    public class Scan
    {
        [DisplayName("No.")]
        public int No { get; set; } = 1;
        [DisplayName("Bobbin Code")]
        public string Bobbin { get; set; }
        [DisplayName("ProductionOrder")]
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
        private readonly FileSaverFactory _factory;

        [Inject]
        public Info Info { get; set; }
        public MaterialViewModel([Inject]IEventAggregator events,
            [Inject] FileSaverFactory factory,
            [Inject]ScannerService service,
            [Inject] MaterialManager materialManager,
            int capcity = 12)
        {
            this.Events = events;
            _factory = factory;
            this.service = service;
            this.Capcity = capcity;
            _materialManager = materialManager;
        }
        public int ItemsHeight => Capcity / 4 * 40 + 20;
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
        }
        int _cntBarcode = 0;
        int _cntErrorBarcode = 0;
        private MaterialManager _materialManager;

        public string ScanRate { get; set; } = "100.00%";
        public void Handle(string message)
        {
            var splits=message.Split(',',':');
            if (splits.Length >= 2)
            {
                for (int i = 0; i < 2; i++)
                {
                    int mIndex = (IpUnit - 3) * 2 + i ;
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
                    Barcodes.Add(new TBarcode { Index = count + 1, Content = splits[i] });
                    var settings = Properties.Settings.Default;
                    var scan = new Scan
                    {
                        Bobbin = splits[i],
                        Shift = settings.Shift,
                        ShiftName = settings.ShiftName,
                        Production = settings.ProductionOrder,
                        LineNo = settings.LineNo,
                        MachineNo = settings.MachineNo,
                        EmployeeNo = settings.EmployeeNo,
                        FlyWireLotNo = this._materialManager.FlyWires[mIndex],
                        TubeLotNo = this._materialManager.Tubes[mIndex]
                    };
                    _factory.GetFileSaver<Scan>((mIndex+1).ToString()).Save(scan);
                    _factory.GetFileSaver<Scan>((mIndex+1).ToString(), @"D:\\SumidaFile\Monitor").Save(scan);
                }
            }
        }

        public void TestSave()
        {
            Handle("Helloworldfs");
        }
    }
}
