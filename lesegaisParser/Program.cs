using lesegaisParser.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace lesegaisParser
{
    internal class Program
    {
        private static Timer _timer = null;
        private static LoaderDB _loaderDB = null;
        static void Main(string[] args)
        {
            var uri = "https://www.lesegais.ru/open-area/graphql";
            var jsonDealsCountPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\post_wood_deal_count.json");
            var jsonDealsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\post_wood_deal.json");
            var parser = new Parser(uri, jsonDealsCountPath, jsonDealsPath, 10000);
            _loaderDB = new LoaderDB(parser);
            _timer = new Timer(UpdateDB, null, 0, 600000);
            Console.ReadLine();
        }

        private static void UpdateDB(Object o)
        {
            _loaderDB.LoadData();
        }
    }
}
