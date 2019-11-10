using Stylet;

namespace DAQ.Service
{
    public class TestSpecViewModel : PropertyChangedBase
    {
        public string Source { get; set; }
        public string Name { get; set; }
        public float  Upper { get; set; }
        public float  Lower { get; set; }
        public float Value { get; set; }
        public float Result { get; set; }
    }
}
