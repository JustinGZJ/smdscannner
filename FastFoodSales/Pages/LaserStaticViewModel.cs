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

namespace DAQ.Pages
{
    public class LaserStaticViewModel : PropertyChangedBase
    {
        public class kv
        {
            public string key { get; set; }
            public double value { get; set; }
        }
        private readonly LaserService laser;
        private string filename = "LaserStatic.json";
        public LaserStaticViewModel([Inject] LaserService laser)
        {
            this.laser = laser;
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
                        DataLabels = true
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
                    DataLabels = true
                },
                new PieSeries
                {
                    Title = "B",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(0) },
                    DataLabels = true
                },
                new PieSeries
                {
                    Title = "C",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(0) },
                    DataLabels = true
                },
                new PieSeries
                {
                    Title = "D",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(0) },
                    DataLabels = true
                }
                ,
                new PieSeries
                {
                    Title = "E",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(0) },
                    DataLabels = true
                },
                new PieSeries
                {
                    Title = "F",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(0) },
                    DataLabels = true
                }
            };
            }
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

        private void _laser_LaserHandler(object sender, Laser e)
        {
            var series = SeriesCollection.FirstOrDefault(x => x.Title == e.CodeQuality);
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
        }
    }
}
