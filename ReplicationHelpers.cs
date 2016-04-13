using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HyperVStatusMon
{
    public class ReplicationHelpers
    {
        public async static Task<int> DoStatusChecks(JArray data, ReplicationSettings repSettings, EmailSettings emailSettings)
        {
            try
            {
                List<Status> statii = new List<Status>();

                foreach (JObject o in data)
                {
                    string vm = (string)o["VMName"];

                    ReplicationState state;
                    ReplicationHealth health;
                    Enum.TryParse((string)o["ReplicationState"], out state);
                    Enum.TryParse((string)o["ReplicationHealth"], out health);

                    if (health != ReplicationHealth.Normal) statii.Add(new Status() { VmName = vm, Message = String.Format("ReplicationHealth is {0}", health.ToString()) });
                    if (state != ReplicationState.Replicating) statii.Add(new Status() { VmName = vm, Message = String.Format("ReplicationState is {0}", state.ToString()) });

                    int latency = (int)o["AverageReplicationLatency"]["TotalSeconds"];
                    if (latency > repSettings.LatencyThresholdSeconds) statii.Add(new Status() { VmName = vm, Message = String.Format("Avg latency {0} above threshold of {1}", latency, repSettings.LatencyThresholdSeconds) });

                    double diffMins = DateTime.Now.Subtract((DateTime)o["LastReplicationTime"]).TotalMinutes;
                    if (diffMins > repSettings.LastReplicationThresholdMins) statii.Add(new Status() { VmName = vm, Message = String.Format("Last replication {0} mins ago above threshold of {1} mins", Math.Round(diffMins), repSettings.LastReplicationThresholdMins) });

                    int missedCount = (int)o["MissedReplicationCount"];
                    if (missedCount > repSettings.MissedThreshold) statii.Add(new Status() { VmName = vm, Message = String.Format("Missed {0} replications above threshold of {1}", missedCount, repSettings.MissedThreshold) });

                    int errorCount = (int)o["ReplicationErrors"];
                    if (errorCount > repSettings.ErrorThreshold) statii.Add(new Status() { VmName = vm, Message = String.Format("{0} replication errors above threshold of {1}", errorCount, repSettings.ErrorThreshold) });

                }

                if (statii.Count() > 0)
                {
                    // send notifications
                    string msg = "<table style='padding: 5px'><tr>" + string.Join("</tr><tr>", statii) + "</tr></table>";
                    await NotificationHelpers.SendEmail(msg, emailSettings);
                }
                Console.WriteLine(String.Format("ok-{0}-{1}", data.Count, statii.Count));
                return statii.Count();
            }
            catch (Exception ex)
            {
                return -1;
            }
        }
        
    }
}
