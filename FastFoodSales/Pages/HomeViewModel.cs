using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StyletIoC;
using Stylet;
using DAQ.Service;
using DAQ.Pages;
using MaterialDesignThemes.Wpf;

namespace DAQ
{
    public class HomeViewModel :IMainTabViewModel,IHandle<Scan>
    {
        private readonly IEventAggregator @event;

        public HomeViewModel([Inject] IEventAggregator @event)
        {
            this.@event = @event;
            @event.Subscribe(this);
        }

        [Inject]
        public ScannerService Scanner { get; set; }

        public int TabIndex { get; set; } = (int)Pages.TabIndex.SCANNER;
        public PackIconKind PackIcon { get; set; } = PackIconKind.ViewDashboard;
        public string Header { get; set; } = "扫码";
        public bool Visible { get; set; } = true;

        public BindableCollection<Scan> Items { get; } = new BindableCollection<Scan>();

        public void Handle(Scan message)
        {
            Items.Add(message);
            if (Items.Count > 20)
            {
                Items.RemoveAt(0);
            }

        }
    }
}
