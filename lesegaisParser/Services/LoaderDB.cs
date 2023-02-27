using Newtonsoft.Json.Linq;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace lesegaisParser.Services
{
    internal class LoaderDB
    {
        public Parser parser;
        public string ConnectionString { get; private set; }
        public LoaderDB(Parser parser)
        {
            this.ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=wood_db;Integrated Security=True";
            this.parser = parser;
        }

        public LoaderDB(string connectionString, Parser parser)
        {
            this.ConnectionString = connectionString;
            this.parser = parser;
        }

        public void LoadData()
        {
            int dealCount = parser.GetDealCount();
            int dealsInOnePage = parser.DealsInOnePage;
            var pages = dealCount / dealsInOnePage + 1;
            var dealNumbersInDB = GetAllDealNumbers();
            Console.WriteLine("Началась загрузка базы данных");
            Console.WriteLine($"Количество данных в БД: {dealNumbersInDB.Count}");
            Console.WriteLine($"Всего данных для считывания: {dealCount}");
            Console.WriteLine($"Количество данных на одной странице: {dealsInOnePage}");
            Console.WriteLine($"Количество страниц: {pages}");
            for (int page = 0; page < pages; page++)
            {
                var dealsJToken = parser.GetDealJToken(page);
                LoadJTokenData(dealsJToken, dealNumbersInDB, dealCount);
                Console.WriteLine($"Загружено {(page + 1)}/{pages} страниц");
            }
            Console.WriteLine("База данных полностью загружена");
        }

        private List<string> GetAllDealNumbers()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                var sqlSelectExpr = "SELECT deal_number FROM deal";
                var commandSelect = new SqlCommand(sqlSelectExpr, connection);
                var reader = commandSelect.ExecuteReader();
                var dealNumbersInDB = new List<string>();
                while (reader.Read())
                {
                    dealNumbersInDB.Add(reader.GetString(0));
                }
                dealNumbersInDB = dealNumbersInDB.Distinct().ToList();
                reader.Close();
                return dealNumbersInDB;
            }
        }

        private void InsertJTokenRow(JToken row)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var sqlInsertExpr = "INSERT INTO deal(deal_number, seller_name, seller_inn, buyer_name" +
                ", buyer_inn, deal_date, wood_volume_buyer, wood_volume_seller) VALUES" +
                "(@dealNumber, @sellerName, @sellerInn, @buyerName, @buyerInn, @dealDate, @woodVolumeBuyer" +
                ", @woodVolumeSeller)";
                connection.Open();
                var command = new SqlCommand(sqlInsertExpr, connection);
                command.Parameters.Add(new SqlParameter("@dealNumber", row["dealNumber"].ToObject<string>() ?? (object)DBNull.Value));
                command.Parameters.Add(new SqlParameter("@sellerName", row["sellerName"].ToObject<string>() ?? (object)DBNull.Value));
                command.Parameters.Add(new SqlParameter("@sellerInn", row["sellerInn"].ToObject<string>() ?? (object)DBNull.Value));
                command.Parameters.Add(new SqlParameter("@buyerName", row["buyerName"].ToObject<string>() ?? (object)DBNull.Value));
                command.Parameters.Add(new SqlParameter("@buyerInn", row["buyerInn"].ToObject<string>() ?? (object)DBNull.Value));
                command.Parameters.Add(new SqlParameter("@dealDate", row["dealDate"].ToObject<DateTime?>() ?? (object)DBNull.Value) { SqlDbType = SqlDbType.Date });
                command.Parameters.Add(new SqlParameter("@woodVolumeBuyer", Math.Round(row["woodVolumeBuyer"].ToObject<float>(), 1)));
                command.Parameters.Add(new SqlParameter("@woodVolumeSeller", Math.Round(row["woodVolumeSeller"].ToObject<float>(), 1)));
                command.ExecuteNonQuery();
            }
        }

        private void UpdateJTokenRow(JToken row)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var sqlUpdateExpr = "UPDATE deal SET seller_name = @sellerName, seller_inn = @sellerInn" +
                ", buyer_name = @buyerName, buyer_inn = @buyerInn, deal_date = @dealDate " +
                ", wood_volume_buyer = @woodVolumeBuyer, wood_volume_seller = @woodVolumeSeller " +
                "WHERE deal_number = @dealNumber";
                connection.Open();
                var command = new SqlCommand(sqlUpdateExpr, connection);
                command.Parameters.Add(new SqlParameter("@dealNumber", row["dealNumber"].ToObject<string>() ?? (object)DBNull.Value));
                command.Parameters.Add(new SqlParameter("@sellerName", row["sellerName"].ToObject<string>() ?? (object)DBNull.Value));
                command.Parameters.Add(new SqlParameter("@sellerInn", row["sellerInn"].ToObject<string>() ?? (object)DBNull.Value));
                command.Parameters.Add(new SqlParameter("@buyerName", row["buyerName"].ToObject<string>() ?? (object)DBNull.Value));
                command.Parameters.Add(new SqlParameter("@buyerInn", row["buyerInn"].ToObject<string>() ?? (object)DBNull.Value));
                command.Parameters.Add(new SqlParameter("@dealDate", row["dealDate"].ToObject<DateTime?>() ?? (object)DBNull.Value) { SqlDbType = SqlDbType.Date });
                command.Parameters.Add(new SqlParameter("@woodVolumeBuyer", Math.Round(row["woodVolumeBuyer"].ToObject<float>(), 1)));
                command.Parameters.Add(new SqlParameter("@woodVolumeSeller", Math.Round(row["woodVolumeSeller"].ToObject<float>(), 1)));
                command.ExecuteNonQuery();
            }
        }

        private DateTime GetDealDateByDealNumber(string dealNumber)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                var sqlSelectExpr = "SELECT deal_date FROM deal WHERE deal_number = @dealNumber";
                var commandSelect = new SqlCommand(sqlSelectExpr, connection);
                commandSelect.Parameters.Add(new SqlParameter("@dealNumber", dealNumber));
                var reader = commandSelect.ExecuteReader();
                reader.Read();
                var date = reader.GetDateTime(0);
                reader.Close();
                return date;
            }
        }

        private bool CheckData(JToken row)
        {
            var sellerInn = row["sellerInn"].ToObject<string>();
            var buyerInn = row["buyerInn"].ToObject<string>();
            return !string.IsNullOrEmpty(sellerInn) && !string.IsNullOrEmpty(buyerInn) && sellerInn.All(x => char.IsDigit(x)) && buyerInn.All(x => char.IsDigit(x));
        }

        private void LoadJTokenData(JToken jTokenData, List<string> dealNumbersInDB, int dealCount)
        {
            foreach (var row in jTokenData)
            {
                if (CheckData(row))
                {
                    var dealNumber = row["dealNumber"].ToObject<string>();
                    if (dealNumbersInDB.Contains(dealNumber))
                    {
                        //Console.WriteLine($"Обновление {row["dealNumber"]}");
                        var dateInDB = GetDealDateByDealNumber(dealNumber);
                        var dateInJson = row["dealDate"].ToObject<DateTime?>();
                        if (dateInDB < dateInJson)
                        {
                            UpdateJTokenRow(row);
                        }
                    }
                    else
                    {
                        //Console.WriteLine($"Добавление {row["dealNumber"]} {dealNumbersInDB.Count}/{dealCount}");
                        InsertJTokenRow(row);
                        dealNumbersInDB.Add(row["dealNumber"].ToObject<string>());
                    }
                }
                else
                {
                    //Console.WriteLine($"Данные не прошли проверку.\nsellerInn = {row["sellerInn"]}\nbuyerInn = {row["buyerInn"]}");
                }
            }
        }
    }
}
