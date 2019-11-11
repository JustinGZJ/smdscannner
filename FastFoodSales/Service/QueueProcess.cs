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

        public void Save<T>(T v)
        {
            try
            {
                string fullpath = Path.GetFullPath(RootFolder);
                string path = Path.Combine(fullpath, SubPath);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                var fileName = Path.Combine(path, FileName);
                var propertyInfos = typeof(T).GetProperties();
                if (!File.Exists(fileName))
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var p in propertyInfos)
                    {
                        var names = p.GetCustomAttributes(typeof(DisplayNameAttribute), true);
                        stringBuilder.Append(names.Length >= 1
                            ? (names[0] as DisplayNameAttribute)?.DisplayName + ","
                            : p.Name + ",");
                    }
                    stringBuilder.AppendLine();
                    stringBuilder.Append($"{string.Join(",", v.GetType().GetProperties().Select(x => x.GetValue(v, null) ?? ""))}");
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
        private string _rootPath = @"\\10.101.30.5\SumidaFile\Monitor";

        public  FileSaver<T> GetFileSaver<T>(string tag)
        {
            var saver = new FileSaver<T>()
            {
                RootFolder = _rootPath,
            };
            saver.ProcessError += Saver_ProcessError;
            if (typeof(T).IsDefined(typeof(SubFilePathAttribute), false))
            {
                SubFilePathAttribute attribute =
                    (SubFilePathAttribute) Attribute.GetCustomAttribute(typeof(T), typeof(SubFilePathAttribute));
                saver.SubPath = Properties.Settings.Default.LineNo + "_" + attribute.Name;
                var s = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Properties.Settings.Default.LineNo+"_"+attribute.Name;
                saver.FileName = string.IsNullOrEmpty(tag) ? s + ".csv" : s + "_"+tag + ".csv";
                return saver;
            }
            else
                return null;
        }

        private void Saver_ProcessError(object sender, Exception e)
        {
            this._event.PostError(e);
        }
    }

    //public class MsgFileSaver<T> : IQueueProcesser<T>
    //{
    //    QueueProcesser<T> processer;
    //    public string RootFolder { get; set; } = "../Data/";
    //    public string SubPath { get; set; } = typeof(T).Name;
    //    public event EventHandler<Exception> ProcessError;
    //    public MsgFileSaver()
    //    {
    //        processer = new QueueProcesser<T>((s) =>
    //          {
    //              try
    //              {
    //                  string fullpath = Path.GetFullPath(RootFolder);
    //                  string path = Path.Combine(fullpath, SubPath);
    //                  if (!Directory.Exists(path))
    //                      Directory.CreateDirectory(path);
    //                  var fileName = Path.Combine(path, DateTime.Today.ToString("yyyy-M-d") + ".csv");
    //                  var propertyInfos = typeof(T).GetProperties();
    //                  if (!File.Exists(fileName))
    //                  {
    //                      StringBuilder stringBuilder = new StringBuilder();
    //                      foreach (var p in propertyInfos)
    //                      {
    //                          var names = p.GetCustomAttributes(typeof(DisplayNameAttribute), true);
    //                          stringBuilder.Append(names.Length >= 1
    //                              ? (names[0] as DisplayNameAttribute)?.DisplayName + ","
    //                              : p.Name + ",");
    //                      }
    //                      stringBuilder.AppendLine();
    //                      File.AppendAllText(fileName, stringBuilder.ToString());
    //                  }
    //                  StringBuilder sb = new StringBuilder();
    //                  foreach (var v in s)
    //                  {
    //                      sb.Append($"{string.Join(",", v.GetType().GetProperties().Select(x => x.GetValue(v, null) ?? ""))}");
    //                      sb.AppendLine();
    //                  }
    //                  File.AppendAllText(fileName, sb.ToString());
    //              }
    //              catch (Exception ex)
    //              {
    //                  OnProcessError(ex);
    //              }
    //          });
    //    }
    //    public void Process(T msg)
    //    {
    //        processer.Process(msg);
    //    }

    //    protected virtual void OnProcessError(Exception e)
    //    {
    //        ProcessError?.Invoke(this, e);
    //    }
    //}


}
