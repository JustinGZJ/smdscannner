using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.Service;
using MaterialDesignThemes.Wpf;
using Stylet;
using StyletIoC;

namespace DAQ.Pages
{

    public class OeeCollectionViewModel:IMainTabViewModel
    {
        public class Ts:PropertyChangedBase
        {
            public int Index { get; set; }
            public bool Selected { get; set; }
        }
        private IObservableCollection<OEEViewModel> _items = new BindableCollection<OEEViewModel>();

        public IObservableCollection<OEEViewModel> Items
        {
            get ;
            set;
        }=new BindableCollection<OEEViewModel>();
        

        public BindableCollection<Ts> Selector { get; set; } = new BindableCollection<Ts>();
        private DataStorage _data;
        public int _unit = 6;
        private int _radioCnt;
        private int _index;
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
               var l= _items.Skip(Index * _unit).Take(_unit);
               Items.Clear() ;
                Items.AddRange(l);
            }
        }

        public OeeCollectionViewModel([Inject] DataStorage storage)
        {
            _data = storage;

            SetupOees();
        }
        public void SetupOees()
        {
            for (int i = 0; i < 12; i++)
            {
                _items.Add(new OEEViewModel()
                {
                    DisplayName = $"N{i + 1}"
                }
                );
            }
            _radioCnt = _items.Count / _unit + (_items.Count % _unit > 0 ? 1 : 0);
            for (int i = 0; i < _radioCnt; i++)
            {
                Selector.Add(new Ts());
            }

            Index = 0;

            _data.Bind(X => X.DataValues, (S, E) =>
            {
                try
                {
                    for (int i = 0; i < 12; i++)
                    {
                        _items[i].Pass = (int)_data[$"DW_N{i + 1}_PASS"];
                        _items[i].Fail = (int)_data[$"DW_N{i + 1}_FAIL"];
                        _items[i].Run = (int)_data[$"DW_N{i + 1}_RUN"];
                        _items[i].Stop = (int)_data[$"DW_N{i + 1}_STOP"];
                    }
                }
                catch (Exception ex)
                {

                  //  throw;
                }
            });

        }


        public int TabIndex { get; set; } = (int)Pages.TabIndex.OEE;
        public PackIconKind PackIcon { get; set; } = PackIconKind.Home;
        public string Header { get; set; } = "OEE";
        public bool Visiable { get; set; } = false;
    }
}
