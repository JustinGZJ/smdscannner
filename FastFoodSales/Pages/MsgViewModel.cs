using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using Serilog;
using Stylet;
using StyletIoC;


namespace DAQ.Pages
{
    public class MsgViewModel : IHandle<MsgItem>, IMainTabViewModel
    {
        ILogger log = new LoggerConfiguration()
.WriteTo.File("./log/log.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
.CreateLogger();
        public MsgViewModel(IEventAggregator @event)
        {
            @event.Subscribe(this);
        }
        public BindableCollection<MsgItem> Items { get; set; } = new BindableCollection<MsgItem>()
        {
        };

        public void Handle(MsgItem message)
        {
            try
            {
                if (Items.Count > 100)
                {
                    Items.RemoveAt(0);
                }
                Items.Add(message);
                log.Information(message.Value);

            }
            catch (Exception ex)
            {
                //    throw;
            }
        }

        public PackIconKind PackIcon { get; set; } = PackIconKind.Message;
        public string Header { get; set; } = "消息";
        public bool Visible { get; set; } = true;
        public int TabIndex { get; set; } = (int)Pages.TabIndex.MESSAGES;
    }

}

