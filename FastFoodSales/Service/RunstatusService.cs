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
using  DAQ.Pages;

namespace DAQ.Service
{
    public class RunstatusService
    {
        private int _runid = -1;
        readonly StatusDto _status = new StatusDto() { AlarmInfoId = -1 };
        readonly List<StatusInfoDto> alarmInfo = new List<StatusInfoDto>();
        [Inject] public IEventAggregator events;
        public RunstatusService([Inject]DataStorage storage, int stationId)
        {
            OeedbContext.DbMutex.WaitOne();
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
            finally
            {
                OeedbContext.DbMutex.ReleaseMutex();
            }
        }
        public void SetStatus(ushort value)
        {
            if (alarmInfo.Any(x => x.AlarmIndex == value))
            {
                //找到报警信息的索引
                var info = alarmInfo.Find(x => x.AlarmIndex == value);
                if (_status.AlarmInfoId != info.Id)
                {
                    if (_status.AlarmInfoId == -1)
                    {
                        OeedbContext.DbMutex.WaitOne();
                        try
                        {
                            using (var db = new OeedbContext())
                            {
                                db.Alarms.Add(new StatusDto()
                                {
                                    Time = _status.Time,
                                    StatusInfo = _status.StatusInfo,
                                    AlarmInfoId = _status.AlarmInfoId,
                                    Span = _status.Span
                                });
                                db.SaveChangesAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            events.PostError(ex.Message);
                            //  throw;
                        }
                    }
                    _status.AlarmInfoId = info.Id;
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
                var jbxx = lines.SkipWhile(x => x.Contains("基本信息"));
                if (jbxx.Any())
                {
                    var b = jbxx.Skip(3);
                    List<StatusInfoDto> infos = new List<StatusInfoDto>();
                    foreach (var a in b)
                    {
                        var s = from m in a.Split('\t') select m.Trim('"');
                        var v = s as string[] ?? s.ToArray();
                        if (v.Any())
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
                    using (var db = new OeedbContext())
                    {
                        var oldinfos = db.AlarmInfos.Where(x => x.StationId == stationid);
                        db.AlarmInfos.RemoveRange(oldinfos);
                        db.AlarmInfos.AddRange();
                        db.SaveChangesAsync();
                    }
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
