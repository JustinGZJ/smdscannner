using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StyletIoC;
using Stylet;
using DAQ.Service;
using DAQ.Pages;

namespace DAQ
{
    public class HomeViewModel : Conductor<MaterialViewModel>.Collection.AllActive
    {
        Properties.Settings settings = Properties.Settings.Default;

        [Inject]
        IEventAggregator EventAggregator { get; set; }
        public int SelectedIndex { get; set; }

        public string ShiftName
        {
            get => settings.ShiftName; set
            {
                settings.ShiftName = value;
                settings.Save();
            }
        }
        public string Shift
        {
            get => settings.Shift; set
            {
                settings.Shift = value;
                settings.Save();
            }
        }

        public string LineNo
        {
            get => settings.LineNo; set
            {
                settings.LineNo = value;
                settings.Save();

            }
        }

        public string LotNo
        {
            get => settings.LotNo; set
            {
                settings.LotNo = value;
                settings.Save();
            }
        }
        public HomeViewModel()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if (SelectedIndex < 11)
                    {
                        SelectedIndex++;
                    }
                    else
                    {
                        SelectedIndex = 0;
                    }
                    System.Threading.Thread.Sleep(20000);
                }
            });
        }

        protected override void OnActivate()
        {

            base.OnActivate();
        }
        protected override void OnInitialActivate()
        {

            base.OnInitialActivate();
        }



        [Inject("N1")]
        public MaterialViewModel N1 { get; set; }
        [Inject("N2")]
        public MaterialViewModel N2 { get; set; }
        [Inject("N3")]
        public MaterialViewModel N3 { get; set; }
        [Inject("N4")]
        public MaterialViewModel N4 { get; set; }
        [Inject("N5")]
        public MaterialViewModel N5 { get; set; }
        [Inject("N6")]
        public MaterialViewModel N6 { get; set; }
        [Inject("N7")]
        public MaterialViewModel N7 { get; set; }
        [Inject("N8")]
        public MaterialViewModel N8 { get; set; }
        [Inject("N9")]
        public MaterialViewModel N9 { get; set; }
        [Inject("N10")]
        public MaterialViewModel N10 { get; set; }
        [Inject("N11")]
        public MaterialViewModel N11 { get; set; }
        [Inject("N12")]
        public MaterialViewModel N12 { get; set; }

        [Inject]
        public ScannerService Scanner { get; set; }




    }
}
