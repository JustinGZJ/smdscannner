using MaterialDesignThemes.Wpf;
using Stylet;
using StyletIoC;

namespace DAQ.Pages
{
    public class RunStatusContainerViewModel :Conductor<object>.StackNavigation, IMainTabViewModel,IHandle<StatusViewModel>,IHandle<string>
    {
       
        public int TabIndex { get; set; } = (int)Pages.TabIndex.RUNSTATUS;
        public PackIconKind PackIcon { get; set; } = PackIconKind.Home;
        public string Header { get; set; } = "Status";
        public bool Visible { get; set; } = true;
        private RunStatusCollectionViewModel _status;
        
        public RunStatusContainerViewModel([Inject] RunStatusCollectionViewModel status,[Inject]IEventAggregator @event)
        {
            _status = status;
            _status.Parent = this;
            ActivateItem(_status);
            @event.Subscribe(this,"STATUS");
        }

        public sealed override void ActivateItem(object item)
        {
            base.ActivateItem(item);
        }

        public void Handle(StatusViewModel message)
        {
            ActivateItem(new RuntimeHistoryViewModel(message.@event,message));
        }

        public void Handle(string message)
        {
            ActivateItem(_status);
        }
    }
}