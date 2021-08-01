using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Connection
    {
        public string Ip { get; set; }
        public int Port { get; set; }
        public int LimitConectors { get; set; }

        public string ConnectionString => $"{Ip}:{Port}";
    }
}
