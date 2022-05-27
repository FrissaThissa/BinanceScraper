using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceScraper
{
    abstract class Database
    {
        private static string connectionString = "Host=localhost;Username=postgres;Password=poster162;Database=coinbay";
        private static NpgsqlConnection connection;

        /// <summary>
        /// Tries to open the database connection
        /// </summary>
        /// <returns>True if database connection is open, else false</returns>
        public static bool OpenConnection()
        {
            if (connection != null && connection.State == System.Data.ConnectionState.Open)
                return true;

            Console.WriteLine("Opening database connection");
            connection = new NpgsqlConnection(connectionString);
            connection.Open();
            if (connection.State == System.Data.ConnectionState.Open)
                return true;

            return false;
        }

        public static void CloseConnection()
        {
            if (connection == null || connection.State == System.Data.ConnectionState.Closed || connection.State == System.Data.ConnectionState.Broken)
                return;

            connection.Close();
        }

        /// <summary>
        /// Retrieve all symbols from database
        /// </summary>
        /// <returns>List of symbols</returns>
        public static List<string> RetrieveSymbols()
        {
            if (!OpenConnection())
                return new List<string>();

            List<string> symbols = new List<string>();
            NpgsqlCommand cmd = new NpgsqlCommand("SELECT symbol FROM symbol", connection);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                symbols.Add(reader[0].ToString());
            }
            reader.Close();
            cmd.Dispose();
            return symbols;
        }

        public static void CreateDataTable(string symbol)
        {
            if (!OpenConnection())
                return;

            string sql = "CREATE TABLE IF NOT EXISTS dps_" + symbol + " (" +
                         "opentime BIGINT PRIMARY KEY," +
                         "openprice DOUBLE PRECISION NOT NULL," +
                         "highprice DOUBLE PRECISION NOT NULL," +
                         "lowprice DOUBLE PRECISION NOT NULL," +
                         "closeprice DOUBLE PRECISION NOT NULL," +
                         "volume DOUBLE PRECISION NOT NULL," +
                         "closetime BIGINT NOT NULL," +
                         "numberoftrades INT NOT NULL" +
                         "); ";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }

        public static int PushDatapoints(string symbol, List<Datapoint> datapoints)
        {
            if (!OpenConnection())
                return 0;

            string sql = "INSERT INTO dps_" + symbol + " VALUES";
            foreach (Datapoint datapoint in datapoints)
            {
                sql += "(" + datapoint.OpenTime + "," +
                    datapoint.Open + "," +
                    datapoint.High + "," +
                    datapoint.Low + "," +
                    datapoint.Close + "," +
                    datapoint.Volume + "," +
                    datapoint.CloseTime + "," +
                    datapoint.NumberOfTrades + "),";
            }
            sql = sql.Remove(sql.Length - 1) + " ON CONFLICT DO NOTHING";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
            int newrows = cmd.ExecuteNonQuery();
            cmd.Dispose();
            return newrows;
        }

        public static void CreateResultTable(string symbol, int month, int year)
        {
            if (!OpenConnection())
                return;

            string sql = "CREATE TABLE IF NOT EXISTS result_" + symbol + "_" + year.ToString() + month.ToString() + " (" +
                         "strategyid INT PRIMARY KEY," +
                         "result DOUBLE PRECISION NOT NULL," +
                         "FOREIGN KEY (strategyid) REFERENCES strategy(id)" +
                         "); ";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }

        public static void PushResults(string symbol, int month, int year, Dictionary<string, double> results)
        {
            if (!OpenConnection())
                return;

            //Insert strategies
            string sql = "INSERT INTO strategy (hash) VALUES ";
            foreach (KeyValuePair<string, double> result in results)
                sql += "('" + result.Key + "'),";
            sql = sql.Remove(sql.Length - 1) + " ON CONFLICT DO NOTHING";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
            cmd.ExecuteNonQuery();
            cmd.Dispose();

            //Insert results
            sql = "INSERT INTO result_" + symbol + "_" + year.ToString() + month.ToString() + " VALUES ";
            foreach (KeyValuePair<string, double> result in results)
                sql += "((SELECT id FROM strategy WHERE hash = '" + result.Key + "'), " + result.Value.ToString() + "),";
            sql = sql.Remove(sql.Length - 1) + " ON CONFLICT DO NOTHING";
            cmd = new NpgsqlCommand(sql, connection);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }
    }
}
