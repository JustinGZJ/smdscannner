using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DAQ.Properties;
using DAQ.Service;
using MaterialDesignThemes.Wpf;
using Stylet;
using StyletIoC;
using Timer = System.Timers.Timer;
using LiveCharts.Wpf;
using LiveCharts;
using LiveCharts.Defaults;

namespace DAQ.Pages
{
    // public class LaserViewModel : Screen, IMainTabViewModel
    public class LaserViewModel : Screen, IMainTabViewModel
    {
        private LaserService _laser;
        public LaserViewModel([Inject] LaserService laser)
        {
            _laser = laser;
            Task.Run((() =>
            {
                laser?.CreateServer();
            }));
            _laser.LaserHandler += _laser_LaserHandler;
        }


        Dictionary<string, int> cnts = new Dictionary<string, int>();
        public BindableCollection<Laser> Lasers { get; } = new BindableCollection<Laser>() { };

        private void _laser_LaserHandler(object sender, Laser e)
        {
            if (Lasers.Count > 20)
                Lasers.RemoveAt(0);
            Lasers.Add(e);
   
        }
        public int TabIndex { get; set; } = (int)Pages.TabIndex.LASER;
        public PackIconKind PackIcon { get; set; } = PackIconKind.FlashCircle;
        public string Header { get; set; } = "镭射";
        public bool Visible { get; set; } = true;
        public int markingNo { get; set; } = Settings.Default.MarkingNo;


    }
}
