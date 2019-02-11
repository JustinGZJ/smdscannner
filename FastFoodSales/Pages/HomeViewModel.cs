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
    public class HomeViewModel:Screen
    {
        [Inject]
        IEventAggregator EventAggregator { get; set; }
        [Inject]
        public PortAService PortAService{get;set;}
        [Inject]
        public PortBService PortBService { get; set; }
        public int SelectedIndex { get; set; }
        public HomeViewModel()
        {
            Task.Run(() =>
            {
                while(true)
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
