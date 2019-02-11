using System;
using Stylet;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
namespace DAQ.Service
{
    public class DataStore : PropertyChangedBase
    {
        private const string DEFAULT_PATH = "./Settings/DataValues.json";
        IPLCService Modbus;
        private object _enable = new object();
        public void Load(string FilePath = DEFAULT_PATH)
        {
            lock (_enable)
            {
                if (!File.Exists(FilePath))
                    return;
                var str = File.ReadAllText(FilePath);
                try
                {
                    var obj = JsonConvert.DeserializeObject<IEnumerable<VAR>>(str);
                    DataValues.Clear();
                    foreach(var a in obj)
                    {
                        DataValues.Add(a);
                    }
                    
                }
                catch (Exception)
                {

                //    throw;
                }
          
            }
        }
        public async void Save(string FilePath = DEFAULT_PATH)
        {
            await Task.Run(() =>
             {
                 string path = Path.GetDirectoryName(FilePath);
                 if (!Directory.Exists(path))
                     Directory.CreateDirectory(path);
               var v = DataValues.Select(x => new
                 {
                     x.Name,
                     x.StartIndex,
                     x.Type,
                     x.tag
                 });

                 File.WriteAllText(FilePath, JsonConvert.SerializeObject(v));
             });
        }
        public DataStore(IPLCService modbus)
        {
            Modbus = modbus;
            Load();
            Task.Run(() =>
            {
                while (true)
                {
                    lock (_enable)
                    {
                        foreach (var v in DataValues)
                        {
                            if (v.Type == TYPE.BOOL)
                            {
                                var r = Modbus.ReadBool(v.StartIndex, v.tag);
                                if (r.HasValue)
                                    v.Value = r.Value;
                            }
                            else if (v.Type == TYPE.SHORT)
                            {
                                var r = Modbus.ReadUInt16(v.StartIndex);
                                if (r.HasValue)
                                    v.Value = r.Value;
                            }
                            else if (v.Type == TYPE.STRING)
                            {
                                var r = Modbus.ReadString(v.StartIndex, v.tag);
                                v.Value = r ?? "";
                            }
                            else if (v.Type == TYPE.INT)
                            {
                                var r = Modbus.ReadInt(v.StartIndex);
                                if (r.HasValue)
                                    v.Value = r.Value;
                            }
                            v.Time = DateTime.Now;
                        }
                        Refresh();
                    }
                    System.Threading.Thread.Sleep(1);

                }
            });
        }
        public void AddItem(VAR var)
        {
            lock (_enable)
            {
                DataValues.Add(var);
            }
            Save();
        }
        public void RemoveItem(VAR var)
        {
            lock (_enable)
            {
                DataValues.Remove(var);
            }
            Save();
        }
        public object this[string Name]
        {
            set
            {
                DataValues.FirstOrDefault(x => x.Name == Name).Value = value;
            }
            get
            {
                return DataValues.FirstOrDefault(x => x.Name == Name).Value;
            }
        }
        public BindableCollection<VAR> DataValues { get; set; } = new BindableCollection<VAR>();

    }
}