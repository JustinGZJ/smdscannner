using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.Service;
using MaterialDesignThemes.Wpf;
using StyletIoC;

namespace DAQ.Pages
{
    public class MaterialManagerViewModel
    {
        public MaterialManager Manager { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configure"></param>
        public MaterialManagerViewModel(IConfigureFile configure)
        {
            Manager = configure.GetValue<MaterialManager>(nameof(MaterialManager));
            if (Manager==null)
            {
                configure.SetValue<MaterialManager>(nameof(MaterialManager), new MaterialManager());
                Manager = configure.GetValue<MaterialManager>(nameof(MaterialManager));
            }
        }

    }
}
