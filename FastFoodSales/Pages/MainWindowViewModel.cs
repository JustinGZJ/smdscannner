using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stylet;
using StyletIoC;
using DAQ.Pages;
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

        public MainWindowViewModel([Inject]IEnumerable<IMainTabViewModel> mainTabs)
        {
            Items.AddRange(mainTabs.Where(x=>x.Visiable==true).OrderBy(x=>x.TabIndex));
        }

        protected override void OnInitialActivate()
        {
            ActiveMessages();
            base.OnInitialActivate();
        }

        public void ShowSetting()
        {
            ActivateItem(Items[(int) TabIndex.SETTING]);
        }

        public void ActiveValues()
        {
            CurrentPage = Items[(int)TabIndex.VALUES];
        }
        public void ActiveMessages()
        {
            CurrentPage = Items[(int)TabIndex.MESSAGES];
        }
    }
}
