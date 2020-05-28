using System;

namespace spacetrackerb
{
    class EquatorEventArgs : EventArgs
    {
        public double satlatitude { get; set; }
        public string satname { get; set; }
        public EquatorEventArgs(double lat, string name)
        {
            this.satlatitude = lat;
            this.satname = name;
        }
    }
}
