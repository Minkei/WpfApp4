using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp4.Models
{
    public class QRCodeModel
    {
        public string? QRCodeValue { get; set; }
        public DateTime ScanDate { get; set; }  
        public TimeSpan ScanTime { get; set; }
        public string? PIC { get; set; }    
    }
}
