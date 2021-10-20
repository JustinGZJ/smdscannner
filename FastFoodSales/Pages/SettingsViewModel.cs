using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using DAQ.Pages;
using Stylet;
using StyletIoC;
using DAQ.Service;
using MaterialDesignThemes.Wpf;

namespace DAQ
{
    public class SettingsViewModel : Conductor<Screen>.Collection.OneActive
    {
        public SettingsViewModel()
        {
            Items.Add(new  AlarmInfoSettingsViewModel());
        }
        public int TabIndex { get; set; } = (int)Pages.TabIndex.SETTING;
        public PackIconKind PackIcon { get; set; } = PackIconKind.Settings;
        public string Header { get; set; } = "设置";
        public bool Visible { get; set; } = true;
    }
}
