using System;
using System.Net.Http;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace lesegaisParser.Services
{
    internal class Requester
    {
        private static Requester _instance;
        private readonly HttpClient _client;

        public static Requester GetInstance()
        {
            if (_instance == null)
            {
                _instance = new Requester();
            }
            return _instance;
        }
        private Requester()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.174 YaBrowser/22.1.5.810 Yowser/2.5 Safari/537.36");
            _client.DefaultRequestHeaders.Accept.ParseAdd("*/*");
            _client.DefaultRequestHeaders.Add("Referer", "https://www.lesegais.ru/open-area/deal");
        }

        public string GetStringAsync(string uri)
        {
            var result = _client.GetAsync(uri).Result;
            for (int i = 0; i < 200 && !result.IsSuccessStatusCode; i++)
            {
                Thread.Sleep (i * 1000);
                result = _client.GetAsync(uri).Result;
                if (result.IsSuccessStatusCode)
                    break;
            }
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception($"Get request failed: {uri}");
            }
            var resultedContent = result.Content.ReadAsStringAsync().Result;
            if (string.IsNullOrEmpty(resultedContent))
            {
                throw new Exception("Nothing to return");
            }
            return resultedContent;
        }

        public string PostJsonAsync(string uri, string jsonStr)
        {
            var content = new StringContent(jsonStr, Encoding.UTF8, "application/json");
            var result = _client.PostAsync(uri, content).Result;
            return result.Content.ReadAsStringAsync().Result;
        }
    }
}
