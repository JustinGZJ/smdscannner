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

        public MaterialManagerViewModel()
        {
            Manager = MaterialManager.Load();
        }

    }
}
