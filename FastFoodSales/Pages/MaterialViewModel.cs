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
    public class RingBuffer : PropertyChangedBase
    {
        public string Name { get; set; }
        public BindableCollection<string> Bcs { get; set; }
        public int Index { get; set; }
        int _capcity;
        public RingBuffer(int Capcity)
        {
            _capcity = Capcity;
            Bcs = new BindableCollection<string>(new string[Capcity]);
        }
        public void Push(string data)
        {
            if (Index < _capcity)
            {
                Bcs[Index] = data;
            }
            else
            {
                Index = 0;
                Bcs[Index] = data;
            }
            Index++;
            NotifyOfPropertyChange("Bcs");
        }
    }

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

    public class TScan : ISource
    {
        public string Source { get ; set; }
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
        public BindableCollection<TBarcode> Barcodes { get; set; } = new BindableCollection<TBarcode>() {
            //new TBarcode() { Index = 1, Content = "11111111111" },
            //new TBarcode() { Index = 2, Content = "11111111111" },
            //new TBarcode() { Index = 3, Content = "44444444" },
            //new TBarcode() { Index = 4, Content = "11111121" } ,
            //new TBarcode() { Index = 5, Content = "11111111111" },
            //new TBarcode() { Index = 6, Content = "11111111111" },

            //new TBarcode() { Index = 7, Content = "11111121" } ,
            //new TBarcode() { Index = 8, Content = "11111111111" },
            //new TBarcode() { Index = 9, Content = "11111111111" }
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
            dataStore.Bind(x => x.DataValues, (s, e) =>
            {
                OEE.Pass = (int)dataStore[$"DW_N{IPUnit}_PASS"];
                OEE.Fail = (int)dataStore[$"DW_N{IPUnit}_FAIL"];
                OEE.Run = (int)dataStore[$"DW_N{IPUnit}_RUN"];
                OEE.Stop = (int)dataStore[$"DW_N{IPUnit}_STOP"];
            });
        }
        [Inject]
        public DataStore dataStore { get; set; }

        int cnt_barcode = 0;
        int cnt_error_barcode = 0;

        public string ScanRate { get; set; } = "0.00%";
        public void Handle(string message)
        {
            var count = Barcodes.Count;
            if (count >= Capcity)
            {
                Barcodes.Clear();
                count = 0;
            }
            cnt_barcode++;
            if (message.ToUpper().Contains("ERROR"))
            {
                cnt_error_barcode++;
            }
            ScanRate = ((cnt_barcode - cnt_error_barcode) * 1.0 / cnt_barcode).ToString("P");
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
