using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using Stylet;
using StyletIoC;


namespace DAQ.Pages
{
    public class MsgViewModel : IHandle<MsgItem>, IMainTabViewModel
    {
        public MsgViewModel(IEventAggregator @event)
        {
            @event.Subscribe(this);
        }
        public BindableCollection<MsgItem> Items { get; set; } = new BindableCollection<MsgItem>()
        {
        };

        public void Handle(MsgItem message)
        {
            Items.Insert(0, message);
            if (Items.Count > 20)
            {
                Items.RemoveAt(Items.Count - 1);
            }
        }

        public PackIconKind PackIcon { get; set; } = PackIconKind.Message;
        public string Header { get; set; } = "Message";
        public int TabIndex { get; set; } = (int)Pages.TabIndex.MESSAGES;
    }

}

