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
    public class HomeViewModel : Conductor<MaterialViewModel>.Collection.AllActive
    {
        Properties.Settings settings = Properties.Settings.Default;
        public int SelectedIndex { get; set; }

        public string ShiftName
        {
            get => settings.ShiftName; set
            {
                settings.ShiftName = value;
                settings.Save();
            }
        }

        private int _unit = 3;
        private readonly int _radioCnt;
        private int _index;
        private readonly BindableCollection<MaterialViewModel> _items = new BindableCollection<MaterialViewModel>();

        public BindableCollection<OeeCollectionViewModel.Ts> Selector { get; set; } = new BindableCollection<OeeCollectionViewModel.Ts>();
        public int Index
        {
            get => _index;
            set
            {
                if (value >= _radioCnt || value < 0) return;
                _index = value;
                foreach (var v in Selector)
                {
                    v.Selected = false;
                }
                Selector[_index].Selected = true;
                var l = _items.Skip(Index * _unit).Take(_unit);
                Items.Clear();
                Items.AddRange(l);
            }
        }

        [Inject] public MaterialManager materialManager { get; set; }
        public string Shift
        {
            get => settings.Shift; set
            {
                settings.Shift = value;
                settings.Save();
            }
        }

        public string LineNo
        {
            get => settings.LineNo; set
            {
                settings.LineNo = value;
                settings.Save();
            }
        }

        public string LotNo
        {
            get => settings.Order; set
            {
                settings.Order = value;
                settings.Save();
            }
        }

        public HomeViewModel(IContainer container)
        {
            _items.Clear();
            for (int i = 0; i < 15; i++)
            {
                var unit = i + 3; 
                var m=container.Get<MaterialViewModel>();
                m.DisplayName = $"192.168.0.{unit}";
                m.IpUnit = unit;
                _items.Add(m);
            }
            _radioCnt = _items.Count / _unit + (_items.Count % _unit > 0 ? 1 : 0);
            for (int i = 0; i < _radioCnt; i++)
            {
                Selector.Add(new OeeCollectionViewModel.Ts() { Selected = (i == 0) });
            }
            var l = _items.Skip(Index * _unit).Take(_unit);
            Items.Clear();
            Items.AddRange(l);
        }

        [Inject]
        public ScannerService Scanner { get; set; }

        protected override void OnActivate()
        {
            base.OnActivate();
            var l = _items.Skip(Index * _unit).Take(_unit);
            Items.Clear();
            Items.AddRange(l);
        }

        public int TabIndex { get; set; } = (int)Pages.TabIndex.SCANNER;
        public PackIconKind PackIcon { get; set; } = PackIconKind.ViewDashboard;
        public string Header { get; set; } = "扫码";
        public bool Visible { get; set; } = true;

        //public async Task ShowSettingDialog()
        //{
        //    var vm = new MaterialManagerViewModel(materialManager);
        //    var dialog = new MaterialManagerView()
        //    {
        //        DataContext = vm
        //    };
        //    await DialogHost.Show(dialog, "root", ((sender, args) => { }),
        //        ((sender, args) => { vm.Manager.Save(); }));
        //}
    }
}
