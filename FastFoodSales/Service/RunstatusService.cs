using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DAQ.Database;
using DAQ.Service;
using Stylet;
using StyletIoC;
using DAQ.Pages;
using Microsoft.EntityFrameworkCore;
using Timer = System.Timers.Timer;

namespace DAQ.Service
{


    public class SimpleMonitor
    {
        private int _busyCount;

        public void Enter()
        {
            Interlocked.Exchange(ref _busyCount, 1);
        }

        public void Leave()
        {
            Interlocked.Exchange(ref _busyCount, 0);
        }
        public bool Busy => this._busyCount > 0;
    }
    public class RunstatusService
    {
        private int _runid = -1;
        readonly StatusDto _status = new StatusDto() { StatusInfoId = -1 };
        public StatusDto Status => _status;
        readonly List<StatusInfoDto> alarmInfo = new List<StatusInfoDto>();
        [Inject] public IEventAggregator events;
        public static SimpleMonitor Monitor { get; } = new SimpleMonitor();
        private int _stationid;
        public RunstatusService(int stationId)
        {
            _stationid = stationId;
            LoadAlarmInfos(stationId);
        }

        private void LoadAlarmInfos(int stationId)
        {
            try
            {
                using (var db = new OeedbContext())
                {
                    var q = db.AlarmInfos.Where(x => x.StationId == stationId);
                    if (q.Any())
                    {
                        alarmInfo.AddRange(q);
                        if (alarmInfo.Any(x => x.AlarmContent == "运行中"))
                        {
                            _runid = alarmInfo.Find(x => x.AlarmContent == "运行中").AlarmIndex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                    events.PostError(ex.Message);
            }
        }


        public void SetStatus(ushort value)
        {
            if (Monitor.Busy)
            {
                SpinWait.SpinUntil(() => Monitor.Busy == false);
                LoadAlarmInfos(_stationid);
            }
            if (alarmInfo.Any(x => x.AlarmIndex == value))
            {
                //找到报警信息的索引
                var info = alarmInfo.Find(x => x.AlarmIndex == value);
                if (_status.StatusInfoId != info.Id)
                {
                    _status.StatusInfoId = info.Id;
                    _status.Time = DateTime.Now;
                    _status.StatusInfo = info;
                }
                else
                {
                    _status.Span = DateTime.Now - _status.Time;
                }
            }
        }

        public static void ReadAlarmFromFile(int stationid, string filename)
        {
            try
            {
                var lines = File.ReadAllLines(filename);
                int index = 0;
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("基本信息_Y39"))
                    {
                        index = i;
                        break;
                    }
                }
                var jbxx = lines.Skip(index);
                if (jbxx.Any())
                {
                    var b = jbxx.Skip(3).ToArray();
                    List<StatusInfoDto> infos = new List<StatusInfoDto>();
                    foreach (var a in b)
                    {
                        var s = from m in a.Split('\t') select m.Trim('"');
                        var v = s as string[] ?? s.ToArray();
                        if (v.Length > 2 && !string.IsNullOrEmpty(v[1]))
                        {
                            var infoDto = new StatusInfoDto()
                            {
                                AlarmContent = v[1],
                                AlarmIndex = int.Parse(v[0]),
                                StationId = stationid
                            };
                            infos.Add(infoDto);
                        }
                    }

                    OeedbContext.DbMutex.WaitOne();
                    using (var db = new OeedbContext())
                    {
                        var oldinfos = db.AlarmInfos.Where(x => x.StationId == stationid);
                        foreach (var info in infos)
                        {
                            if (oldinfos.Any(x => x.AlarmContent == info.AlarmContent))
                            {
                                oldinfos.Single(x => x.AlarmContent == info.AlarmContent).AlarmIndex = info.AlarmIndex;
                            }
                            else
                            {
                                db.AlarmInfos.Add(info);
                            }
                        }
                        db.SaveChangesAsync();
                    }
                    OeedbContext.DbMutex.ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //        throw;
            }
        }

    }
}
