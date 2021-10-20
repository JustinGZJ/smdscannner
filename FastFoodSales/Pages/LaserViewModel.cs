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
using Timer=System.Timers.Timer;

namespace DAQ.Pages
{
    // public class LaserViewModel : Screen, IMainTabViewModel
    public class LaserViewModel : Screen
    {
        private LaserService _laser;
        private IIoService _ioService;
        public LaserViewModel([Inject]LaserService laser,[Inject]IIoService ioService)
        {
            _laser = laser;
            _ioService = ioService;
            Task.Run((() =>
            {
                laser?.CreateServer();
            }));
            _laser.LaserHandler += _laser_LaserHandler;
        }



        public BindableCollection<Laser> Lasers { get; } = new BindableCollection<Laser>(){};

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
        public async void GetMarkingNo()
        {
          markingNo = await Task.Run<int>(() =>_laser.GetMarkingNo());
          Settings.Default.MarkingNo = markingNo;
          Settings.Default.Save();
        }

        public async void DoSaveData()
        {
            _ioService.SetOutput(7, true);
            //   await Task.Run(() => { _laser.GetLaserData(); });
           // await Task.Delay(2000);
         //   _ioService.SetOutput(7, false);
        }
    }
}
