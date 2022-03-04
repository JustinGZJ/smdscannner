using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stylet;
using StyletIoC;
using DAQ.Pages;
using DAQ.Properties;
using DAQ.Service;
using MaterialDesignThemes.Wpf;

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
            get => settings.BobbinCavityNo;
            set
            {
                settings.BobbinCavityNo = value;
                settings.Save();
            }
        }

        public string BobbinPartName
        {
            get=>settings.BobbinPartName;
            set
            {
                settings.BobbinPartName = value;
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
        public string ProductionOrder
        {
            get => settings.ProductionOrder;
            set
            {
                settings.ProductionOrder = value;
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

        public string Station
        {
            get => settings.Station;
            set
            {
                settings.Station = value;
                settings.Save();
            }
        }


        public string BobbinCavityNo1
        {
            get => settings.BobbinCavityNo1;
            set
            {
                settings.BobbinCavityNo1 = value;
                settings.Save();
            }
        }

        public string BobbinPartName1
        {
            get => settings.BobbinPartName1;
            set
            {
                settings.BobbinPartName1 = value;
                settings.Save();
            }
        }
        public string BobbinToolNo1
        {
            get => settings.BobbinToolNo1;
            set
            {
                settings.BobbinToolNo1 = value;
                settings.Save();
            }
        }
        public string BobbinLotNo1
        {
            get => settings.BobbinLotNo;
            set
            {
                settings.BobbinLotNo1 = value;
                settings.Save();
            }
        }
        public string EmployeeNo1
        {
            get => settings.EmployeeNo1;
            set
            {
                settings.EmployeeNo1 = value;
                settings.Save();
            }
        }
        public string ProductionOrder1
        {
            get => settings.ProductionOrder1;
            set
            {
                settings.ProductionOrder1 = value;
                settings.Save();
            }
        }
        public string ShiftName1
        {
            get => settings.ShiftName1;
            set
            {
                settings.ShiftName1 = value;
                settings.Save();
            }
        }

        public string LineNo1
        {
            get => settings.LineNo1;
            set
            {
                settings.LineNo1 = value;
                settings.Save();
            }
        }

        public string FlyWireLotNo1
        {
            get => settings.FlyWireLotNo1;
            set
            {
                settings.FlyWireLotNo1 = value;
                settings.Save();
            }
        }

        public string MachineNo1
        {
            get => settings.MachineNo1;
            set
            {
                settings.MachineNo1 = value;
                settings.Save();
            }
        }

        public string Shift1
        {
            get => settings.Shift;
            set
            {
                settings.Shift = value;
                settings.Save();
            }
        }
        public string TubeLotNo1
        {
            get => settings.TubeLotNo;
            set
            {
                settings.TubeLotNo = value;
                settings.Save();
            }
        }

        public string Station1
        {
            get => settings.Station;
            set
            {
                settings.Station = value;
                settings.Save();
            }
        }




        public string SaveRootPath1
        {
            get => settings.SaveRootPath1;
            set
            {
                settings.SaveRootPath1 = value;
                settings.Save();
            }
        }
        public MaterialManagerViewModel MaterialManager =>new MaterialManagerViewModel();
        public async Task ShowSettingDialog()
        {

            var dialog = new MaterialManagerView()
            {
                DataContext = MaterialManager
            };
            await DialogHost.Show(dialog, "root", ((sender, args) => { }),
                ((sender, args) => { MaterialManager.Manager.Save(); }));
        }

        public async Task ShowUserSetting()
        {
            var dialog=new materialdialog();
            dialog.DataContext = this;
            await DialogHost.Show(dialog,closingEventHandler:(s,e)=>MaterialManager.Manager.Save());
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
