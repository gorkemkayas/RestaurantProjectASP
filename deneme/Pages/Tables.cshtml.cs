using deneme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace deneme.Pages
{
    public class TablesModel : PageModel
    {
        private readonly string? _connectionString;

        public List<Tables> Tables { get; set; } = new List<Tables>();

        public TablesModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MSSQL");

            if (_connectionString == null)
            {
                throw new InvalidOperationException("Connection string 'MSSQL' not found.");
            }
        }

        public void OnGet()
        {
            Tables = GetTables(_connectionString);
        }

        public static List<Tables> GetTables(string connectionString)
        {
            List<Tables> tables = new List<Tables>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();  

                string query = "SELECT TableId, TableNumber, Capacity FROM [dbo].[TABLES]";
                SqlCommand command = new SqlCommand(query, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tables.Add(new Tables
                        {
                            TableId = reader.GetInt32(0),
                            TableNumber = reader.GetInt32(1),
                            Capacity = reader.GetInt32(2)
                        });
                    }
                }
            }

            return tables;
        }
    }
}
