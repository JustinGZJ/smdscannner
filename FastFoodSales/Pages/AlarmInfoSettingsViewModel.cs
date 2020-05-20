using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
            DisplayName = "报警信息";
            using (var db = new OeedbContext())  
            {
                db.Database.EnsureCreated();
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
            try
            {
                if (s is ComboBox cbo)
                {
                    FilterByStationId((int)cbo.SelectedValue);
                }
            }
            catch (Exception)
            {
               // throw;
            }
        }

        public void FilterByStationId(int stationid)
        {
            using (var db = new OeedbContext())
            {
                Items.Clear();
                Items.AddRange(db.AlarmInfos.Where(x => x.StationId == stationid).OrderBy(x=>x.AlarmIndex));
            }
        }

        public void LoadFromFile(string Stationid)
        {
           if(!int.TryParse(Stationid,out int stationid))
           {
               MessageBox.Show("Station id must be integer");
               return;
           }
           
            OpenFileDialog dlgDialog = new OpenFileDialog();
            dlgDialog.Filter = "All files（*.*）|*.*|csv files(*.*)|*.csv";
            if (dlgDialog.ShowDialog() == true)
            {
                RunstatusService.Monitor.Enter();
                RunstatusService.ReadAlarmFromFile(stationid, dlgDialog.FileName);
                RunstatusService.Monitor.Leave();
            }
            using (var db = new OeedbContext())
            {
                Items.Clear();
                foreach (var d in db.AlarmInfos)
                {
                   Items.Add(d);
                }
            }
        }
    }
}
