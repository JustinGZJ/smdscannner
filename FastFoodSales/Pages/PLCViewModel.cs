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
    public class PLCViewModel : Screen
    {
        public BindableCollection<KV<bool>> Bits { get; set; } = new BindableCollection<KV<bool>>();
        public BindableCollection<KV<float>> Floats { get; set; } = new BindableCollection<KV<float>>();
        public BindableCollection<string> FloatsTags = new BindableCollection<string>();
        public object Dialog { get; set; }
        [Inject]
        public DataStore Store { get; set; }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
        }
        public async void AddStorageValue()
        {
            var dlg = new StorageValueDialog() { DataContext = new VAR() { Name = "", StartIndex = 0, tag = 0, Type = TYPE.SHORT } };
            var result = await DialogHost.Show(dlg);

            if (result is bool r && r == true)
            {
                VAR v = dlg.DataContext as VAR;
                Store.AddItem(v);
            }
        }
        public VAR SelectedItem { get; set; }
        public void RemoveStorageValue(VAR v)
        {
            Store.RemoveItem(v);
        }

        public async void ModifyDataValue()
        {
            var dlg = new StorageValueDialog() { DataContext = SelectedItem };
            var result = await DialogHost.Show(dlg);

            if (result is bool r && r == true)
            {
                VAR v = dlg.DataContext as VAR;
                //Store.AddItem(v);
                SelectedItem = v;
            }
        }
        public PLCViewModel()
        {

        }
    };
}
public class KV<T> : PropertyChangedBase
{
    public int Index { get; set; }
    public DateTime Time { get; set; }
    public string Key { get; set; }
    public T Value { get; set; }
}


