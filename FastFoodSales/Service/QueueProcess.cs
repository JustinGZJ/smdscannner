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

    public class MsgFileSaver<T> : IQueueProcesser<T>
    {
        QueueProcesser<T> processer;
        public string RootFolder { get; set; } = "../Data/";
        public string SubPath { get; set; } = typeof(T).Name;
        public event EventHandler<Exception> ProcessError;
        public MsgFileSaver()
        {
            processer = new QueueProcesser<T>((s) =>
              {
                  try
                  {
                      string fullpath = Path.GetFullPath(RootFolder);
                      string path = Path.Combine(fullpath, SubPath);
                      if (!Directory.Exists(path))
                          Directory.CreateDirectory(path);
                      var fileName = Path.Combine(path, DateTime.Today.ToString("yyyy-M-d") + ".csv");
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
                          File.AppendAllText(fileName, stringBuilder.ToString());
                      }
                      StringBuilder sb = new StringBuilder();
                      foreach (var v in s)
                      {
                          sb.Append($"{string.Join(",", v.GetType().GetProperties().Select(x => x.GetValue(v, null) ?? ""))}");
                          sb.AppendLine();
                      }
                      File.AppendAllText(fileName, sb.ToString());
                  }
                  catch (Exception ex)
                  {
                      OnProcessError(ex);
                  }
              });
        }
        public void Process(T msg)
        {
            processer.Process(msg);
        }

        protected virtual void OnProcessError(Exception e)
        {
            ProcessError?.Invoke(this, e);
        }
    }

    public class SaveMsg<T>
    {
        public string Source { get; set; }
        public T Msg { get; set; }

        public static SaveMsg<T> Create(string source, T msg)
        {
            var m = new SaveMsg<T>() { Msg = msg, Source = source };
            return m;
        }
    }

}
