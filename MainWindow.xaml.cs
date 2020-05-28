using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace spacetrackerb
{
    public partial class MainWindow : Window
    {
        UserLocation userLocation;
        Satellite satellite;
        List<SpaceCraft> spaceCraftsLive;
        Brush loading = new SolidColorBrush(Color.FromRgb(232, 62, 62));
        Brush ready = new SolidColorBrush(Color.FromRgb(62, 232, 62));
        bool isValid;

        public MainWindow()
        {
            InitializeComponent();

            spaceCraftsLive = new List<SpaceCraft>();
            BtnAddToList.IsEnabled = false;
            BtnDeleteFromList.IsEnabled = false;
        }

        private void BtnMore_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.n2yo.com/satellites/");
        }

        private async void BtnGetData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BtnAddToList.IsEnabled = false;
                TxtBlckData.Text = "Checking ID...";
                TxtBlckEquator.Text = string.Empty;
                TxtBlckMessages.Text = string.Empty;
                BtnGetData.Background = loading;

                int id = Convert.ToInt32(TxtBxGetID.Text); // Input controleren.
                satellite = await Satellite.GetData(id); // Get request (via ID).

                BtnGetData.Background = ready;

                double lat = Convert.ToDouble(satellite.GetLatLon().satlatitude); 
                double lon = Convert.ToDouble(satellite.GetLatLon().satlongitude);
                string name = Convert.ToString(satellite.info.satname);

                satellite.AboveEquator += SpaceStation_AboveEquator;
                satellite.BelowEquator += SpaceStation_BelowEquator;

                satellite.CheckEquator(); // Locatie vergelijken met evenaar.

                if (name != null && lat != 0 && lon != 0) // Satelliet in rotatie.
                {
                    TxtBlckData.Text = satellite.ToString();
                    isValid = true;
                    BtnAddToList.IsEnabled = true;
                }
                else if (name != null && lat == 0 && lon == 0) // Satelliet niet in rotatie.
                {
                    TxtBlckData.Text = satellite.NotInOrbit();
                    TxtBlckEquator.Text = string.Empty;
                    isValid = false;
                    BtnAddToList.IsEnabled = false;
                }
                else // Niet bestaande satelliet.
                {
                    TxtBlckData.Text = satellite.NoCraft();
                    TxtBlckEquator.Text = string.Empty;
                    isValid = false;
                    BtnAddToList.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                TxtBlckData.Text = $"Please fill in all fields correctly.";
                TxtBlckEquator.Text = $"Message: {ex.Message}";
                isValid = false;
            }
        }

        private void SpaceStation_AboveEquator(object sender, EquatorEventArgs e)
        {
            if (e != null)
            {
                TxtBlckEquator.Text = $"{e.satname} is flying above the equator at latitude: {e.satlatitude}.";
            }
            else
            {
                TxtBlckEquator.Text = "Something went wrong";
            }
        }

        private void SpaceStation_BelowEquator(object sender, EquatorEventArgs e)
        {
            if (e != null)
            {
                TxtBlckEquator.Text = $"{e.satname} is flying below the equator at latitude: {e.satlatitude}.";
            }
            else
            {
                TxtBlckEquator.Text = "Something went wrong";
            }
        }

        private async void MapView_Loaded(object sender, RoutedEventArgs e)
        {
            // Specificaties van map configureren.
            MapView.MapProvider = GMapProviders.GoogleMap;

            MapView.DragButton = MouseButton.Left;

            MapView.MinZoom = 2;
            MapView.MaxZoom = 15;


            MapView.Zoom = 15;

            MapView.ShowCenter = false;

            userLocation = await UserLocation.GetUserLocation(); // Wachten op locatie gebruiker.
            // Instellen als startpositie en een marker op plaatsen.
            MapView.Position = new PointLatLng(userLocation.latitude, userLocation.longitude);
            DrawMarker(userLocation.latitude, userLocation.longitude, TypeOfMarker.UserM);
        }

        private void BtnAddToList_Click(object sender, RoutedEventArgs e)
        {
            BtnDeleteFromList.IsEnabled = false;
            TxtBlckMessages.Text = string.Empty;

            if (isValid == true) // Controle waardigheid.
            {
                spaceCraftsLive.Add(satellite);
            }

            LbxTracking.Items.Clear();
            foreach (var craft in spaceCraftsLive)
            {
                LbxTracking.Items.Add(craft.ToString());
            }
        }

        private void BtnDeleteFromList_Click(object sender, RoutedEventArgs e)
        {
            TxtBlckMessages.Text = string.Empty;

            try
            {
                // Geselecteerde satelliet verwijderen.
                spaceCraftsLive.Remove(spaceCraftsLive[LbxTracking.SelectedIndex]);
                LbxTracking.Items.Clear();
                foreach (var craft in spaceCraftsLive)
                {
                    LbxTracking.Items.Add(craft.ToString());
                }
                BtnDeleteFromList.IsEnabled = false;
            }
            catch (Exception ex)
            {
                BtnDeleteFromList.IsEnabled = false;
                Console.WriteLine($"Can't delete non existing item. " +
                    $"Message: {ex.Message}");
            }
        }

        private void LbxTracking_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MapView.Zoom = 0; // Uitzoomen.
            MapView.Markers.Clear();
            BtnDeleteFromList.IsEnabled = true;

            try
            {
                int craftIndex = LbxTracking.SelectedIndex;

                double lat1 = userLocation.latitude;
                double lon1 = userLocation.longitude;

                double lat2 = spaceCraftsLive[craftIndex].GetLatLon().satlatitude;
                double lon2 = spaceCraftsLive[craftIndex].GetLatLon().satlongitude;
                DateTime t = spaceCraftsLive[craftIndex].time;

                // Gepaste marker tekenen op gepaste locatie.
                DrawMarker(lat1, lon1, TypeOfMarker.UserM);
                DrawMarker(lat2, lon2, TypeOfMarker.SatelliteM);

                // Berekenen van afstand tussen gebruiker en satelliet.
                double distance = GetDistance(lat1, lon1, lat2, lon2);

                TxtBlckMessages.Foreground = Brushes.Black;
                TxtBlckMessages.Text = $"Around {t.Hour}h{t.Minute}, you were about {distance} km away from " +
                    $"{spaceCraftsLive[craftIndex].info.satname}.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Waiting for listbox to refill. " +
                    $"Message: {ex.Message}");
            }
        }

        private double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Afstand berekenen tussen 2 punten.
            int earthRadius = 6371;
            double dLat = degToRad(lat2 - lat1);
            double dLon = degToRad(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(degToRad(lat1)) * Math.Cos(degToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distance = earthRadius * c;
            return Math.Round(distance, 2);
        }

        private double degToRad(double deg)
        {
            return deg * (Math.PI / 180); // Graden naar radialen omzetten.
        }

        private void BtnShowAll_Click(object sender, RoutedEventArgs e)
        {
            if (spaceCraftsLive.Count > 0) // Controle of lijst niet leeg is.
            {
                TxtBlckMessages.Text = string.Empty;
                // Voor elke satelliet een marker tekenen.
                foreach (var craft in spaceCraftsLive)
                {
                    double lat = craft.GetLatLon().satlatitude;
                    double lon = craft.GetLatLon().satlongitude;

                    DrawMarker(lat, lon, TypeOfMarker.SatelliteM);
                }
            }
            else
            {
                TxtBlckMessages.Foreground = Brushes.Red;
                TxtBlckMessages.Text = "There are no tracked points to display.";
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Opslaan in textbestand.
            if (spaceCraftsLive.Count > 0)
            {
                SaveFileDialog sFDialog = new SaveFileDialog();
                sFDialog.Filter = "Text file | *.txt";
                sFDialog.DefaultExt = "txt";
                sFDialog.InitialDirectory = Environment.GetFolderPath(
                    Environment.SpecialFolder.Desktop);
                if (sFDialog.ShowDialog() == true)
                {
                    StreamWriter sWriter = new StreamWriter(sFDialog.OpenFile());
                    sWriter.WriteLine("---ALL TRACKED POINTS---\n\n");

                    foreach (var point in LbxTracking.Items)
                    {
                        sWriter.WriteLine(point);
                    }
                    sWriter.Dispose();
                    sWriter.Close();
                }
            }
            else
            {
                TxtBlckMessages.Foreground = Brushes.Red;
                TxtBlckMessages.Text = "There are no tracked points to save.";
            }
        }
        
        public enum TypeOfMarker // Type marker (User of Satellite).
        {
            UserM,
            SatelliteM
        }

        public void DrawMarker(double lat, double lon, TypeOfMarker type)
        {
            PointLatLng latlon = new PointLatLng(lat, lon);

            // Rode cirkel voor gebruiker.
            Ellipse UserIcon = new Ellipse
            {
                Width = 15,
                Height = 15,
                Stroke = Brushes.Red,
                StrokeThickness = 2
            };

            // Satelliet icoon voor satelliet.
            UCSatellite SatIcon = new UCSatellite()
            {
                Width = 20,
                Height = 20
            };

            GMapMarker marker = new GMapMarker(latlon); // Marker on position.

            if (type == TypeOfMarker.UserM)
            {
                marker.Shape = UserIcon;
                MapView.Markers.Add(marker);
            }
            else if (type == TypeOfMarker.SatelliteM)
            {
                marker.Shape = SatIcon;
                MapView.Markers.Add(marker);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(Environment.ExitCode); // Alle threads stoppen bij closed.
        }
    }
}
