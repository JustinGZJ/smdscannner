using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using DAQ.Database;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using StyletIoC;

namespace DAQ.Pages
{
    public class RuntimeHistoryViewModel : Screen
    {
        public IObservableCollection<StatusDto> StatusCollection { get; } = new BindableCollection<StatusDto>();

        public string[] StationNames { get; }
        public int StationIndex { get; set; }
        public string DisplayHeader => $"N{StationIndex + 1}故障停机时间分布";
        public string FromTo => FromDateTime.ToString("G") + "-" + ToDateTime.ToString("G");

        private IEventAggregator Event;

        public RuntimeHistoryViewModel([Inject]IEventAggregator Event,StatusViewModel status)
        {
            this.Event = Event;
            StationIndex = status.StationId-1;
            List<string> sList = new List<string>();
            for (int i = 0; i < 12; i++)
            {
                sList.Add($"N{i + 1}");
            }
            StationNames = sList.ToArray();
            Query();
        }
        protected override void OnActivate()
        {
            base.OnActivate();
        }

        public void GoBack()
        {
            Event.Publish("BACK","STATUS");
        }
        public void Query()
        {
            try
            {
                GetStatus();
                GetTop10Alarms();
                GetStatckRows();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                // throw;
            }
        }

        void GetStatckRows()
        {
            StackSeries.Clear();
            foreach (var status in StatusCollection)
            {
                Brush brush = Brushes.Red;
                if (status.StatusInfo.AlarmContent.Contains("准备中") || status.StatusInfo.AlarmContent.Contains("原点复位"))
                {
                    brush = Brushes.Yellow;
                }
                else if (status.StatusInfo.AlarmContent.Contains("运行中"))
                {
                    brush = Brushes.SpringGreen;
                }
                StackSeries.Add(new StackedRowSeries()
                {
                    Values = new ChartValues<double> { status.Span.TotalMinutes },
                    Fill = brush,
                    Title = $"{status.StatusInfo.AlarmContent}\n{status.Time}-{status.Time + status.Span}"
                });
            }
        }
        void GetStatus()
        {
            try
            {
                using (var db = new OeedbContext())
                {
                    StatusCollection.Clear();
                    StatusCollection.AddRange(
                        db.Alarms
                            .Include(m => m.StatusInfo)
                            .Where(x => x.StatusInfo.StationId == StationIndex + 1)
                            .Where(x=>x.Time>FromDateTime&&x.Time<ToDateTime)
                            .OrderBy(x => x.Time));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void GetTop10Alarms()
        {
            var z = StatusCollection.Where(m => m.StatusInfo.AlarmContent != "运行中").Where(w => w.StatusInfoId != -1)
                .GroupBy(x => x.StatusInfo.Id)
                .Select(x => new
                {
                    Key = x.First().StatusInfo.AlarmContent,
                    Value = x.Sum(y => y.Span.TotalSeconds)
                }).OrderByDescending(x => x.Value);
            if (z.Any())
            {
                var q = z.ToList();
                if (q.Count() > 10)
                {
                    var top10 = q.Take(10);
                    var other = q.Skip(10).Sum(x => x.Value);
                    Top10SeriesCollection[0].Values.Clear();
                    foreach (var v in top10)
                    {
                        Top10SeriesCollection[0].Values.Add(new ObservableValue(v.Value));
                    }
                    Top10SeriesCollection[0].Values.Add(new ObservableValue(other));
                    var l = new List<string>();
                    l.AddRange(top10.Select(x => x.Key).ToArray());
                    l.Add("Others");
                    _top10Labels.Clear();
                    _top10Labels.AddRange(l.ToArray());
                }
                else
                {
                    Top10SeriesCollection[0].Values.Clear();
                    foreach (var v in q)
                    {
                        Top10SeriesCollection[0].Values.Add(new ObservableValue(v.Value));
                    }
                    _top10Labels.Clear();
                    _top10Labels.AddRange(q.Select(x => x.Key));
                }
            }
            else
            {
                Top10SeriesCollection[0].Values.Clear();
                _top10Labels.Clear();

                //   return;
            }

        }

        public SeriesCollection StackSeries { get; } = new SeriesCollection()
        {
        };

        public BindableCollection<string> _top10Labels { get; } = new BindableCollection<string>();
        public SeriesCollection Top10SeriesCollection { get; set; } = new SeriesCollection()
        {
            new ColumnSeries()
            {
                Values = new ChartValues<ObservableValue>()
            }
        };

        public DateTime ToDateTime { get; set; } = DateTime.Today.AddDays(1);

        public DateTime FromDateTime { get; set; } = DateTime.Today;

        //public int TabIndex { get; set; } = (int)Pages.TabIndex.HISTORY;
        //public PackIconKind PackIcon { get; set; } = PackIconKind.History;
        //public string Header { get; set; } = "History";
        //public bool Visiable { get; set; } = true;
    }
}
