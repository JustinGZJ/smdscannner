using DAQ.Service;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Stylet;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Windows;

namespace DAQ.Pages
{
    public class LaserStaticViewModel : PropertyChangedBase
    {
        public class kv
        {
            public string key { get; set; }
            public double value { get; set; }
        }
        public class kvp
        {
            public string 项目 { get; set; }
            public double 数量 { get; set; }

            public string 比率 { get; set; }
        }
        public Func<ChartPoint, string> PointLabel { get; set; }
        private readonly LaserService laser;
        private string filename = "LaserStatic.json";
        private BindableCollection<kvp> statistics = new BindableCollection<kvp>();
        private int total;

        public LaserStaticViewModel([Inject] LaserService laser)
        {
            this.laser = laser;
            PointLabel = (p) => string.Format("{0} ({1:P})", p.Y, p.Participation);
            laser.LaserHandler += _laser_LaserHandler;
            if (File.Exists(filename))
            {
                var file = File.ReadAllText(filename);
                var kvs = JsonConvert.DeserializeObject<List<kv>>(file);
                SeriesCollection = new SeriesCollection();
                foreach (var kv in kvs)
                {
                    SeriesCollection.Add(new PieSeries
                    {
                        Title = kv.key,
                        Values = new ChartValues<ObservableValue> { new ObservableValue(kv.value) },
                        DataLabels = true,
                        LabelPoint = PointLabel
                    });
                }

            }
            else
            {
                SeriesCollection = new SeriesCollection()             {
                new PieSeries
                {
                    Title = "A",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(1) },
                     LabelPoint = PointLabel,
                    DataLabels = true
                },
                new PieSeries
                {
                    Title = "B",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(0) },
                      LabelPoint = PointLabel,
                    DataLabels = true
                },
                new PieSeries
                {
                    Title = "C",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(0) },
                      LabelPoint = PointLabel,
                    DataLabels = true
                },
                new PieSeries
                {
                    Title = "D",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(0) },
                      LabelPoint = PointLabel,
                    DataLabels = true
                }
                ,
                new PieSeries
                {
                    Title = "E",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(0) },
                      LabelPoint = PointLabel,
                    DataLabels = true
                },
                new PieSeries
                {
                    Title = "F",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(0) },
                    LabelPoint = PointLabel,
                    DataLabels = true
                }
            };
            }
            getStatistic();
        }

        public SeriesCollection SeriesCollection { get; set; }

        public void resetcnt()
        {

            foreach (var series in SeriesCollection)
            {
                foreach (var value in series.Values.Cast<ObservableValue>())
                {
                    value.Value = 0;
                }
            }
            save();
        }
        public void test()
        {
            _laser_LaserHandler(null, new Laser() { CodeQuality = "A" });

        }

        private void save()
        {
            var q = SeriesCollection.Select(x =>
           new kv
           {
               key = x.Title,
               value = x.Values.Cast<ObservableValue>().First().Value
           }).ToList();
            File.WriteAllText(filename, JsonConvert.SerializeObject(q));
        }

        public int Total { get => total; set => SetAndNotify(ref total, value); }
        private void getStatistic()
        {
            var sum = SeriesCollection.Sum(x => x.Values.Cast<ObservableValue>().First().Value);
            var q = SeriesCollection.Select(x =>
           new kvp
           {
               项目 = x.Title,
               数量 = x.Values.Cast<ObservableValue>().First().Value,
               比率 = (x.Values.Cast<ObservableValue>().First().Value / sum).ToString("P")
           }).ToList();
            Total = (int)sum;
            statistics.Clear();
            statistics.AddRange(q);
        }

        public BindableCollection<kvp> Statistics { get => statistics; set => statistics = value; }

        private void _laser_LaserHandler(object sender, Laser e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var series = SeriesCollection.FirstOrDefault(x => e.CodeQuality.Contains(x.Title));
                if (series != null)
                {
                    foreach (var value in series.Values.Cast<ObservableValue>())
                    {
                        value.Value += 1;
                    }
                }
                else
                {
                    SeriesCollection.Add(new PieSeries
                    {
                        Title = e.CodeQuality,
                        Values = new ChartValues<ObservableValue> { new ObservableValue(1) },
                        DataLabels = true
                    });
                }
                save();
                getStatistic();
            });

        }
    }
}
