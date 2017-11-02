using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorService
{
    public class ServerDetail
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public int ServerId { get; set; }
        public Server Server { get; set; }
        public decimal CpuUtilization { get; set; }
        public decimal RamAvailable { get; set; }
        public decimal RamTotal { get; set; }
        public decimal UpTime { get; set; }
        public decimal BytesReceived { get; set; }
        public decimal BytesSent { get; set; }
        public decimal Handles { get; set; }
        public decimal Processes { get; set; }
    }
}
