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
        }
        public int TabIndex { get; set; } = (int)Pages.TabIndex.LASER;
        public PackIconKind PackIcon { get; set; } = PackIconKind.FlashCircle;
        public string Header { get; set; } = "Laser";
        public bool Visible { get; set; } = true;
    }
}
