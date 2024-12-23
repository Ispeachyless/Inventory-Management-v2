using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FinalProject
{
    public sealed class DBManager
    {
        private static string _connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog = IMS;Integrated Security = True";
        private static SqlConnection connection = new SqlConnection(_connectionString);
        public static SqlConnection Connection()
        {
            return connection;
        }
    }
}
