using System;

namespace DAQ.Pages
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SubFilePathAttribute : Attribute  //类名是特性的名称
    {
        public string Name { get; }

        public SubFilePathAttribute(string name) //name为定位参数
        {
            this.Name = name;
        }
    }
}