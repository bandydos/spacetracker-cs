namespace spacetrackerb
{
    class LatLons
    {
        public double satlatitude { get; set; }
        public double satlongitude { get; set; }
        public LatLons(double lat, double lon)
        {
            this.satlatitude = lat;
            this.satlongitude = lon;
        }
    }
}
