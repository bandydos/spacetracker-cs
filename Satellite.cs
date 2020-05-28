using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace spacetrackerb
{
    class Satellite : SpaceCraft
    {
        public Satellite(List<LatLons> pos, CraftInfo specs)
            : base(pos, specs)
        {

        }

        public static async Task<Satellite> GetData(int id)
        {
            HttpClient httpClient = new HttpClient();
            Satellite station;
            string api_key = "XZ6XPA-9C9XZQ-YBDRKD-4FM8";
            string api_url = $"https://www.n2yo.com/rest/" +
                $"v1/satellite/positions/{id}/0/0/0/1/&apiKey={api_key}";

            string response = await httpClient.GetStringAsync(api_url);

            station = JsonConvert.DeserializeObject<Satellite>(response);
            return station;
        }
    }
}
