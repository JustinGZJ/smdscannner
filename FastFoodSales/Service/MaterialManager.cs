using System;
using System.Linq;
using DAQ.Properties;
using Newtonsoft.Json;

namespace DAQ.Service
{
    public class MaterialManager
    {

        public string[] FlyWires { get; set; } = Enumerable.Repeat("",8).ToArray();
        public string[] Tubes { get; set; } = Enumerable.Repeat("", 8).ToArray();

        public MaterialManager Save()
        {
            Settings.Default.Materials = JsonConvert.SerializeObject(this);
            Settings.Default.Save();
            return this;
        }

        public (string, string) GetMaterial(int index)
        {
            if (index > 7 || index < 0)
                throw new OutOfMemoryException("index mush be between 0 and 7");
            return (FlyWires[index], Tubes[index]);
        }

        public static MaterialManager Load()
        {
            MaterialManager m;
            try
            {
                  m = JsonConvert.DeserializeObject<MaterialManager>(Settings.Default.Materials) ?? new MaterialManager();
               // m = new MaterialManager();
            }
            catch (Exception e)
            {
                m = new MaterialManager();
            }
            return m;
        }
    }
}