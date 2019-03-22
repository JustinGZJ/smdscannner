using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DAQ.Database;
using DAQ.Service;
using Microsoft.EntityFrameworkCore;
using Modbus.Data;
using Stylet;
using StyletIoC;

namespace DAQ.Pages
{
    public class RunStatusCollectionViewModel : Screen
    {


        public OeedbContext db { get; set; } = new OeedbContext();
        [Inject]
        public IEventAggregator _event { get; set; }
        public void SaveRunStatus()
        {


            {
                var gp = db.Alarms
                    .Include(x => x.StatusInfo)
                    .OrderBy(x => x.Time)
                    .GroupBy(x => x.StatusInfo.StationId).Select(g => g.Last()).ToList();
                foreach (var item in _items)
                {
                    var status = item.Service1.Status;
                    if (status.StatusInfoId != -1)
                    {
                        var sId = status.StatusInfo.StationId;

                        if (gp.Any(x => x.StatusInfo.StationId == sId))
                        {
                            var c = gp.Single(x => x.StatusInfo.StationId == sId);
                            if (c.StatusInfoId == status.StatusInfoId && c.Time == status.Time)
                            {
                                c.Span = status.Span;
                                continue;
                            }
                        }
                        db.Alarms.Add(new StatusDto
                        {
                            Time = status.Time,
                            Span = status.Span,
                            StatusInfoId = status.StatusInfoId
                        });
                    }
                }
            }

            if (OeedbContext.DbMutex.WaitOne(100))
            {
                try
                {
                    db.SaveChanges();
                }
                catch (Exception EX)
                {
                    _event.PostError(EX.StackTrace);
                   
                }
                finally
                {
                    OeedbContext.DbMutex.ReleaseMutex();
                }
            }
        }


        public RunStatusCollectionViewModel([Inject] DataStorage dataStore, [Inject] IEventAggregator Event)
        {
            for (int i = 0; i < 12; i++)
            {
                _items.Add(new StatusViewModel(dataStore, i + 1, Event) { Parent = this.Parent });
            }

            _radioCnt = _items.Count / _unit + (_items.Count % _unit > 0 ? 1 : 0);
            for (int i = 0; i < _radioCnt; i++)
            {
                Selector.Add(new OeeCollectionViewModel.Ts());
            }
            _index = 0;
            var l = _items.Take(_unit);
            Items.AddRange(l);

            Task.Run(() =>
            {
                while (true)
                {
                    lock (this)
                    {
                        SaveRunStatus();
                        Thread.Sleep(1000);
                    }
                }
            });
        }

        public BindableCollection<StatusViewModel> Items { get; set; } = new BindableCollection<StatusViewModel>();
        private BindableCollection<StatusViewModel> _items = new BindableCollection<StatusViewModel>();

        public void ShowHistory(int StationId)
        {

        }
        public BindableCollection<OeeCollectionViewModel.Ts> Selector { get; set; } = new BindableCollection<OeeCollectionViewModel.Ts>();
        public int _unit = 9;
        private int _radioCnt;
        private int _index;
        public int Index
        {
            get => _index;
            set
            {
                if (value >= _radioCnt || value < 0) return;
                _index = value;
                foreach (var v in Selector)
                {
                    v.Selected = false;
                }
                Selector[_index].Selected = true;
                var l = _items.Skip(Index * _unit).Take(_unit);
                Items.Clear();
                Items.AddRange(l);
            }
        }
    }
}
