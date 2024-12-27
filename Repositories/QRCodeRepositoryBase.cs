using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace WpfApp4.Repositories
{
    public abstract class QRCodeRepositoryBase
    {
        private readonly string _connectionString;
        public QRCodeRepositoryBase()
        {
            _connectionString = "Server=(local); Database=QRCodeDB; Integrated Security=true; Encrypt = True; TrustServerCertificate = True";
        }
        protected SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
