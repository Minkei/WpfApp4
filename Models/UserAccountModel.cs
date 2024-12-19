using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp4.Models
{
    public class UserAccountModel
    {
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = "User not logged in";
        public byte[] ProfilePicture { get; set; } = Array.Empty<byte>();
    }
}
