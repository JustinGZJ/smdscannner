using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StyletIoC;
using DAQ.Service;
using Stylet;
using MaterialDesignThemes.Wpf;
namespace DAQ.Pages
{
    public class PLCViewModel : Screen, IMainTabViewModel
    {

        public object Dialog { get; set; }
        [Inject]
        public DataStorage Storage { get; set; }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
        }
        public async void AddStorageValue()
        {
            var dlg = new StorageValueDialog() { DataContext = new VAR() { Name = "", StartIndex = 0, Tag = 0, Type = TYPE.SHORT } };
            var result = await DialogHost.Show(dlg);

            if (result is bool r && r == true)
            {
                VAR v = dlg.DataContext as VAR;
                Storage.AddItem(v);
            }
        }
        public VAR SelectedItem { get; set; }
        public void RemoveStorageValue(VAR v)
        {
            Storage.RemoveItem(v);
        }

        public async void ModifyDataValue()
        {
            var dlg = new StorageValueDialog()
            {
                DataContext = new VAR()
                {
                    Name = SelectedItem.Name,
                    StartIndex = SelectedItem.StartIndex,
                    Tag = SelectedItem.Tag,
                    Type = SelectedItem.Type,
                    Value = SelectedItem.Value
                }
            };
            var result = await DialogHost.Show(dlg);
            lock (DataStorage.Locker)
            {
                if (result is bool r && r == true)
                {
                    VAR v = dlg.DataContext as VAR;
                    Storage.ModifyItem(SelectedItem, v);
                }
            }
        }
        public PLCViewModel()
        {

        }
        public int TabIndex { get; set; } = (int)Pages.TabIndex.VALUES;
        public PackIconKind PackIcon { get; set; } = PackIconKind.ViewSequential;
        public string Header { get; set; } = "Values";
        public bool Visiable { get; set; } = true;
    };
}


