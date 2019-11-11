using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DAQ.Service;
using MaterialDesignThemes.Wpf;
using Stylet;
using StyletIoC;

namespace DAQ.Pages
{
    public class LaserViewModel:Screen,IMainTabViewModel
    {
        private LaserService _laser;
        public LaserViewModel([Inject]LaserService laser)
        {
            _laser = laser;
            Task.Run((() =>
            {
                laser?.CreateServer();
            }));
            _laser.LaserHandler += _laser_LaserHandler;
        }

        public BindableCollection<Laser> Lasers { get; } = new BindableCollection<Laser>();

        private void _laser_LaserHandler(object sender, Laser e)
        {
            if (Lasers.Count > 1000)
                Lasers.RemoveAt(0);
            Lasers.Add(e);
        }
        public int TabIndex { get; set; } = (int)Pages.TabIndex.LASER;
        public PackIconKind PackIcon { get; set; } = PackIconKind.FlashCircle;
        public string Header { get; set; } = "Laser";
        public bool Visible { get; set; } = true;
    }
}
