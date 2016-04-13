using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HyperVStatusMon
{
    public class Status
    {
        public string VmName { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return String.Format("<td>{0}</td><td>{1}</td>", VmName, Message);
        }
    }
}
