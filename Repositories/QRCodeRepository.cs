using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp4.Models;
using WpfApp4.Repositories;
using System.Net;
using System.Data;
namespace WpfApp4.Repositories
{
    public class QRCodeRepository: QRCodeRepositoryBase
    {
        public async Task AddQRCodeAsync(QRCodeModel qRCodeModel)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var connection = GetConnection())
                    using (var command = new SqlCommand())
                    {
                        connection.Open();
                        command.Connection = connection;
                        command.CommandText = "INSERT INTO QRCodeData (QRCodeValue, ScanDate, ScanTime, PIC) " +
                                              "VALUES (@QRCodeValue, @ScanDate, @ScanTime, @PIC)";

                        command.Parameters.Add("@QRCodeValue", SqlDbType.NVarChar, 255).Value = qRCodeModel.QRCodeValue;
                        command.Parameters.Add("@ScanDate", SqlDbType.Date).Value = qRCodeModel.ScanDate;
                        command.Parameters.Add("@ScanTime", SqlDbType.Time,7).Value = qRCodeModel.ScanTime;
                        command.Parameters.Add("@PIC", SqlDbType.NVarChar, 100).Value = qRCodeModel.PIC;

                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi nếu có
                    Console.WriteLine("Error: " + ex.Message);
                }
            });
        }

    }
}
