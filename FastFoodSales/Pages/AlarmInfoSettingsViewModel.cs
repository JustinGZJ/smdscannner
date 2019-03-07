using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using DAQ.Database;
using DAQ.Service;
using Microsoft.Win32;
using Stylet;

namespace DAQ.Pages
{
    public class AlarmInfoSettingsViewModel : Screen
    {
        public IObservableCollection<StatusInfoDto> Items { get; } = new BindableCollection<StatusInfoDto>();

        public IObservableCollection<int> stationIds { get; } = new BindableCollection<int>();


        public AlarmInfoSettingsViewModel()
        {
            using (var db = new OeedbContext())  
            {
                if (db.AlarmInfos.Any())
                {
                    var e = db.AlarmInfos.Select(x => x.StationId);
                    stationIds.AddRange(e.Distinct());
                    Items.AddRange(db.AlarmInfos.Where(x => x.StationId == stationIds.First()));
                }
            }
        }

        public void Filter(object s, SelectionChangedEventArgs e)
        {
            if (s is ComboBox cbo)
            {
                FilterByStationId((int)cbo.SelectedValue);
            }
        }

        public void FilterByStationId(int stationid)
        {
            using (var db = new OeedbContext())
            {
                Items.Clear();
                Items.AddRange(db.AlarmInfos.Where(x => x.StationId == stationid));
            }
        }

        public void LoadFromFile(int stationid)
        {
            OpenFileDialog dlgDialog = new OpenFileDialog();
            dlgDialog.Filter = "All files（*.*）|*.*|csv files(*.*)|*.csv";
            if (dlgDialog.ShowDialog() == true)
            {
                RunstatusService.ReadAlarmFromFile(stationid, dlgDialog.FileName);
            }
            using (var db = new OeedbContext())
            {
                Items.Clear();
                Items.AddRange(db.AlarmInfos);
            }
        }
    }
}
