using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace WpfApp4.Repositories
{
    public abstract class LoginRepositoryBase
    {
        private readonly string _connectionString;
        public LoginRepositoryBase()
        {
            _connectionString = "Server=(local); Database=MVVMLoginDB; Integrated Security=true; Encrypt = True; TrustServerCertificate = True";
        }
        protected SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
