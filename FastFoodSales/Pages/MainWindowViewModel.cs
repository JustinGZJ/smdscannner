using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stylet;
using StyletIoC;
using DAQ.Pages;
using DAQ.Properties;

namespace DAQ
{

    public class MainWindowViewModel : Conductor<IMainTabViewModel>.Collection.OneActive
    {
        int index = 0;

        public object CurrentPage { get; set; }
        public int Index
        {
            get { return index; }
            set
            {
                index = value;
                if (value >= 0)
                {
                    ActivateItem(Items[index]);
                }
            }
        }




        Settings settings = Properties.Settings.Default;
        public MainWindowViewModel([Inject]IEnumerable<IMainTabViewModel> mainTabs)
        {
            Items.AddRange(mainTabs.Where(x=>x.Visible==true).OrderBy(x=>x.TabIndex));
        }

        public string BobbinCavityNo
        {
            get=>settings.BobbinCavityNo;
            set
            {
                settings.BobbinCavityNo = value;
                settings.Save();
            }
        }
        public string BobbinToolNo
        {
            get => settings.BobbinToolNo;
            set
            {
                settings.BobbinToolNo = value;
                settings.Save();
            }
        }
        public string BobbinLotNo
        {
            get => settings.BobbinLotNo;
            set
            {
                settings.BobbinLotNo = value;
                settings.Save();
            }
        }
        public string EmployeeNo
        {
            get => settings.EmployeeNo;
            set
            {
                settings.EmployeeNo = value;
                settings.Save();
            }
        }

        public string Order
        {
            get => settings.Order;
            set
            {
                settings.Order = value;
                settings.Save();
            }
        }
        public string Production
        {
            get => settings.Production;
            set
            {
                settings.Production = value;
                settings.Save();
            }
        }
        public string ShiftName
        {
            get => settings.ShiftName;
            set
            {
                settings.ShiftName = value;
                settings.Save();
            }
        }

        public string LineNo
        {
            get => settings.LineNo;
            set
            {
                settings.LineNo = value;
                settings.Save();
            }
        }

        public string FlyWireLotNo
        {
            get => settings.FlyWireLotNo;
            set
            {
                settings.FlyWireLotNo = value;
                settings.Save();
            }
        }

        public string MachineNo
        {
            get => settings.MachineNo;
            set
            {
                settings.MachineNo = value;
                settings.Save();
            }
        }

        public string Shift
        {
            get => settings.Shift;
            set
            {
                settings.Shift = value;
                settings.Save();
            }
        }
        public string TubeLotNo
        {
            get => settings.TubeLotNo;
            set
            {
                settings.TubeLotNo = value;
                settings.Save();
            }
        }





        protected override void OnInitialActivate()
        {
            ActiveMessages();
            base.OnInitialActivate();
        }

        public void ShowSetting()
        {
          var item= Items.SingleOrDefault(x => x.TabIndex == (int) TabIndex.SETTING);
            ActivateItem(item);
        }

        public void ActiveValues()
        {
            var item = Items.SingleOrDefault(x => x.TabIndex == (int)TabIndex.VALUES);
            CurrentPage = item;
        }
        public void ActiveMessages()
        {
            var item = Items.SingleOrDefault(x => x.TabIndex == (int)TabIndex.MESSAGES);
            CurrentPage = item;
        }
    }
}
