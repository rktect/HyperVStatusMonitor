using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HyperVStatusMon
{
    public class HyperVSettings
    {
        public ReplicationSettings Replication { get; set; }
        public EmailSettings Email { get; set; }
    }

    public class ReplicationSettings
    {
        public int LatencyThresholdSeconds { get; set; }
        public int LastReplicationThresholdMins { get; set; }
        public int MissedThreshold { get; set; }
        public int ErrorThreshold { get; set; }
        public int HostNonCommunicationThresholdMins { get; set; }
    }

    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPassword { get; set; }
        public EmailRecipient From { get; set; }
        public string Subject { get; set; }
        public List<EmailRecipient> To { get; set; }
    }
}
