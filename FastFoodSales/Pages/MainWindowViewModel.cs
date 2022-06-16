﻿using System;
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
        private readonly MaterialManagerViewModel materialManager;
        private readonly IConfigureFile configure;

        public LaserStaticViewModel LaserStatic { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mainTabs"></param>
        /// <param name="laserStatic"></param>
        /// <param name="MaterialManager"></param>
        /// <param name="configure"></param>
        public MainWindowViewModel([Inject] IEnumerable<IMainTabViewModel> mainTabs, [Inject] LaserStaticViewModel laserStatic,[Inject] MaterialManagerViewModel MaterialManager,[Inject] IConfigureFile configure)
        {
            Items.AddRange(mainTabs.Where(x => x.Visible == true).OrderBy(x => x.TabIndex));
            LaserStatic = laserStatic;
            materialManager = MaterialManager;
            this.configure = configure;
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


        public string SaveRootPath
        {
            get => settings.SaveRootPath;
            set
            {
                settings.SaveRootPath = value;
                settings.Save();
            }
        }

        public MaterialManagerViewModel MaterialManager => materialManager;

        //   public MaterialManagerViewModel MaterialManager =>new MaterialManagerViewModel();
        /// <summary>
        /// 异步加载
        /// </summary>
        /// <returns></returns>
        public async Task ShowSettingDialog()
        {

            var dialog = new MaterialManagerView()
            {
                DataContext = MaterialManager
            };
            await DialogHost.Show(dialog, "root", ((sender, args) => { }),
                ((sender, args) => {
                    configure.SetValue(nameof(MaterialManager), MaterialManager.Manager);
                }));
        }

        /// <summary>
        /// 异步加载页面
        /// </summary>
        /// <returns></returns>
        public async Task ShowUserSetting()
        {
            var dialog=new materialdialog();
            dialog.DataContext = this;
            await DialogHost.Show(dialog, closingEventHandler: (s, e) => configure.SetValue<MaterialManager>(nameof(MaterialManager),MaterialManager.Manager));
        }


        /// <summary>
        /// 
        /// </summary>
        protected override void OnInitialActivate()
        {
            ActiveMessages();
            base.OnInitialActivate();
        }
        /// <summary>
        /// 
        /// </summary>
        public void ShowSetting()
        {
          var item= Items.SingleOrDefault(x => x.TabIndex == (int) TabIndex.SETTING);
            ActivateItem(item);
        }
        /// <summary>
        /// 
        /// </summary>
        public void ActiveValues()
        {
            //  var item = Items.SingleOrDefault(x => x.TabIndex == (int)TabIndex.VALUES);
            CurrentPage = LaserStatic;
        }
        /// <summary>
        /// 
        /// </summary>
        public void ActiveMessages()
        {
            var item = Items.SingleOrDefault(x => x.TabIndex == (int)TabIndex.MESSAGES);
            CurrentPage = item;
        }
    }
}
