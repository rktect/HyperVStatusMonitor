using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HyperVStatusMon
{
    public class Status
    {
        public Status()
        {
            this.IsRecovered = false;
            this.Count = 1;
            this.Start = DateHelpers.GetLocalDateTime(DateTime.Now);
        }

        public string VmName { get; set; }
        public string ProblemType { get; set; }
        public string Message { get; set; }
        public int Count { get; set; }
        public DateTime Start { get; set; }
        public bool IsRecovered { get; set; }

        public override string ToString()
        {
            return String.Format("<td>{0}</td><td>{1}</td><td>{2} since {3}</td>", this.IsRecovered ? "Recovery" : "Problem", VmName, Message, Start.ToString());
        }
    }
}
