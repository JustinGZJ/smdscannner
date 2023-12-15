using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using DAQ.Pages;
using DAQ.Properties;
using Stylet;

namespace DAQ.Service
{
    public class QueueProcesser<T>
    {
        ConcurrentQueue<T> Msgs = new ConcurrentQueue<T>();
        public int Capcity { get; set; } = 100;
        public int Interval { get; set; } = 1000;
        Task task;
        int locker = 0;
        Action<List<T>> Todo = new Action<List<T>>((s) => { });
        public QueueProcesser(Action<List<T>> action)
        {
            Todo = action;
            timer = new Timer((o) => BatchProcess(), null, Interval, Interval);
        }


        Timer timer;
        public void Process(T msg)
        {
            Msgs.Enqueue(msg);
            if (Msgs.Count > Capcity)
            {
                task?.Wait();
            }
            if (Interlocked.Increment(ref locker) == 1)
            {
                task = Task.Run(BatchProcess
                ).ContinueWith((x) => Interlocked.Exchange(ref locker, 0));
            }
            else
            {
                timer.Change(0, Interval);
            }
        }

        private void BatchProcess()
        {
            lock (this)
            {
                List<T> vs = new List<T>();
                while (Msgs.TryDequeue(out T v))
                {
                    vs.Add(v);
                }
                try
                {
                    Todo(vs);
                }
                catch (Exception)
                {
                    foreach (var v in vs)
                    {
                        Msgs.Enqueue(v);
                    }
                    throw;
                }

            }
        }
    }

    public class FileSaver<T>
    {
        public string RootFolder { get; set; }
        public string SubPath { get; set; }
        public event EventHandler<Exception> ProcessError;
        public string FileName { get; set; }

        public void Save(T v)
        {
            try
            {
                string fullpath = Path.GetFullPath(RootFolder);
                string path = fullpath;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                var fileName = Path.Combine(path, FileName);
                var propertyInfos = typeof(T).GetProperties();
                if (!File.Exists(fileName))
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    var ms = propertyInfos.Select(p =>
                     {
                         var ns = p.GetCustomAttributes(typeof(DisplayNameAttribute), true);
                         var name = ns.Length > 0 ? (ns[0] as DisplayNameAttribute)?.DisplayName : p.Name;
                         return name;
                     });
                    stringBuilder.AppendLine(string.Join(",", ms));
                    stringBuilder.Append($"{string.Join(",", propertyInfos.Select(x => x.GetValue(v, null) ?? ""))}");
                    stringBuilder.AppendLine();
                    File.AppendAllText(fileName, stringBuilder.ToString());
                }
                else
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append($"{string.Join(",", propertyInfos.Select(x => x.GetValue(v, null) ?? ""))}");
                    stringBuilder.AppendLine();
                    File.AppendAllText(fileName, stringBuilder.ToString());
                }
            }
            catch (Exception ex)
            {
                OnProcessError(ex);
            }
        }
        protected virtual void OnProcessError(Exception e)
        {
            ProcessError?.Invoke(this, e);
        }
    }

    public class FileSaverFactory
    {
        private readonly IEventAggregator _event;

        public FileSaverFactory(IEventAggregator @event)
        {
            _event = @event;
        }
        private string _rootPath = Settings.Default.SaveRootPath;

        public FileSaver<T> GetFileSaver<T>(string tag, string rootpath)
        {
            var saver = new FileSaver<T>()
            {
                RootFolder = rootpath,
            };
            saver.ProcessError += Saver_ProcessError;
            if (typeof(T).IsDefined(typeof(SubFilePathAttribute), false))
            {
                SubFilePathAttribute attribute =
                    (SubFilePathAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(SubFilePathAttribute));
                saver.SubPath ="Line"+ Properties.Settings.Default.LineNo + "_" + attribute.Name;
                string s = $"{DateTime.Now:yyyyMMddHHmmss}_Line{Settings.Default.LineNo}_{attribute.Name}";
                saver.FileName = string.IsNullOrEmpty(tag) ? s + ".csv" : s + "_" + tag + ".csv";
                return saver;
            }
            else
                return null;
        }

        public FileSaver<T> GetFileSaver<T>(string tag)
        {
            return GetFileSaver<T>(tag, Settings.Default.SaveRootPath);
        }
        private void Saver_ProcessError(object sender, Exception e)
        {
            this._event.PostError(e);
        }
    }



}
