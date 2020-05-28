using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace spacetrackerb
{
    class UserLocation
    {
        public double latitude { get; set; }
        public double longitude { get; set; }

        public UserLocation(double lat, double lon)
        {
            this.latitude = lat;
            this.longitude = lon;
        }

        public static async Task<UserLocation> GetUserLocation()
        {
            UserLocation userInfo;
            HttpClient httpClient = new HttpClient();

            string api_url = "http://ip-api.com/json";

            string response = await httpClient.GetStringAsync(api_url);

            userInfo = JsonConvert.DeserializeObject<UserLocation>(response);
            Console.WriteLine(userInfo.longitude);
            return userInfo;
        }
    }
}
