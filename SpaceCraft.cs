using System;
using System.Collections.Generic;

namespace spacetrackerb
{
    class SpaceCraft
    {
        public List<LatLons> positions { get; set; }

        public CraftInfo info { get; set; }

        public DateTime time { get; set; }

        public SpaceCraft(List<LatLons> pos, CraftInfo specs)
        {
            this.positions = pos;
            this.info = specs;
            this.time = DateTime.Now;
        }

        public LatLons GetLatLon()
        {
            // Meest recente positie returnen.
            return new LatLons(positions[0].satlatitude, positions[0].satlongitude);
        }

        public void CheckEquator()
        {
            if (GetLatLon().satlatitude >= 0)
            {
                OnAboveEquator();
            }
            else
            {
                OnBelowEquator();
            }
        }

        public event EventHandler<EquatorEventArgs> AboveEquator;
        public event EventHandler<EquatorEventArgs> BelowEquator;

        public void OnAboveEquator()
        {
            AboveEquator?.Invoke(this, new EquatorEventArgs(
                GetLatLon().satlatitude, info.satname));
        }

        public void OnBelowEquator()
        {
            BelowEquator?.Invoke(this, new EquatorEventArgs(
                GetLatLon().satlatitude, info.satname));
        }

        public override string ToString()
        {
            return $"{info.satname} (NORAD ID: {info.satid}), " +
                $"lat: {GetLatLon().satlatitude}, lon: {GetLatLon().satlongitude} ({time}).";
        }

        public string NotInOrbit()
        {
            return $"{info.satname} (NORAD ID: {info.satid}) is not in orbit at the moment.";
        }

        public string NoCraft()
        {
            return $"There is no spacecraft with NORAD ID: {info.satid}.";
        }
    }
}
