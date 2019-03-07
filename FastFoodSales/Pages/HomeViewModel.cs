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
    public class HomeViewModel : Conductor<MaterialViewModel>.Collection.AllActive,IMainTabViewModel
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
            get => settings.LotNo; set
            {
                settings.LotNo = value;
                settings.Save();
            }
        }

        public HomeViewModel([Inject("N1")] MaterialViewModel N1,
            [Inject("N2")] MaterialViewModel N2,
            [Inject("N3")] MaterialViewModel N3,
            [Inject("N4")] MaterialViewModel N4,
            [Inject("N5")] MaterialViewModel N5,
            [Inject("N6")] MaterialViewModel N6,
            [Inject("N7")] MaterialViewModel N7,
            [Inject("N8")] MaterialViewModel N8,
            [Inject("N9")] MaterialViewModel N9,
            [Inject("N10")] MaterialViewModel N10,
            [Inject("N11")] MaterialViewModel N11,
            [Inject("N12")] MaterialViewModel N12)
        {
            _items.Clear();
            _items.AddRange(new[] { N1, N2, N3, N4, N5, N6, N7, N8, N9, N10, N11, N12 });

            _radioCnt = _items.Count / _unit + (_items.Count % _unit > 0 ? 1 : 0);
            for (int i = 0; i < _radioCnt; i++)
            {
                Selector.Add(new OeeCollectionViewModel.Ts(){Selected = (i==0)});
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

        public int TabIndex { get; set; } =(int)Pages.TabIndex.SCANNER;
        public PackIconKind PackIcon { get; set; } = PackIconKind.ViewDashboard;
        public string Header { get; set; } = "Scanner";
    }
}
