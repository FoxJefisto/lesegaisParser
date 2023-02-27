using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace lesegaisParser.Services
{
    internal class Parser
    {
        private Requester _requester;
        public string Uri { get; set; }
        public string JsonDealsCountPath { get; set; }
        public string JsonDealsPath { get; set; }
        private int _dealsInOnePage;

        public int DealsInOnePage
        {
            get { return _dealsInOnePage; }
            set
            {
                _dealsInOnePage = value;
                var jsonPost = File.ReadAllText(JsonDealsPath);
                var jObject = JObject.Parse(jsonPost);
                jObject["variables"]["size"] = _dealsInOnePage;
                var jsonStr = JsonConvert.SerializeObject(jObject);
                File.WriteAllText(JsonDealsPath, jsonStr);
            }
        }


        public Parser(string uri, string jsonDealsCountPath, string jsonDealsPath, int dealsCountInOnePage)
        {
            _requester = Requester.GetInstance();
            Uri = uri;
            JsonDealsCountPath = jsonDealsCountPath;
            JsonDealsPath = jsonDealsPath;
            DealsInOnePage = dealsCountInOnePage;
        }

        public JToken GetDealJToken(int page)
        {
            var jsonPost = File.ReadAllText(JsonDealsPath);
            var jObject = JObject.Parse(jsonPost);
            jObject["variables"]["number"] = page;
            jsonPost = JsonConvert.SerializeObject(jObject);
            var jsonStr = _requester.PostJsonAsync(Uri, jsonPost);
            var table = JObject.Parse(jsonStr);
            return table["data"]["searchReportWoodDeal"]["content"];
        }

        public int GetDealCount()
        {
            var jsonPost = File.ReadAllText(JsonDealsCountPath);
            var jsonStr = _requester.PostJsonAsync(Uri, jsonPost);
            var jObject = JObject.Parse(jsonStr);
            return jObject["data"]["searchReportWoodDeal"]["total"].ToObject<int>();
        }
    }
}
