using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BinanceScraper
{
    public class Binance
    {
        private HttpClient client = new HttpClient();

        /// <summary>
        /// Set up variables
        /// </summary>
        public Binance()
        {
            client.BaseAddress = new Uri("https://api.binance.com/api/v3/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// A function to retrieve a batch of max. 1000 binance datapoints
        /// </summary>
        /// <param name="amount">The amount of datapoints to retrieve</param>
        /// <param name="startTime">The starttime of the data</param>
        /// <returns>A list of datapoints retrieved with specified variables</returns>
        public async Task<List<Datapoint>?> RetrieveBatch(string symbol, int amount, long startTime = 0)
        {
            List<Datapoint> datapoints = new();

            string request = "klines?symbol=" + symbol + "EUR&interval=1m";

            //If starttime = 0 (first retrieve) the starttime for the request should not be set to retrieve most recent data
            request += "&limit=" + (Math.Min(1000, amount));
            if (startTime != 0) request = request + "&startTime=" + startTime;

            HttpResponseMessage response = await client.GetAsync(request);
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                List<List<double>>? result = JsonConvert.DeserializeObject<List<List<double>>>(json);
                if (result == null)
                    return null;

                foreach (List<double> dp in result)
                {
                    datapoints.Add(new Datapoint(
                        Convert.ToInt64(dp[0]),
                        dp[1],
                        dp[2],
                        dp[3],
                        dp[4],
                        dp[5],
                        Convert.ToInt64(dp[6]),
                        Convert.ToInt32(dp[8])
                    ));
                }
            }

            return datapoints;
        }
    }
}
