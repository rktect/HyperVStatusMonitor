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

        public async static Task<int> DoStatusChecks(JArray data, ReplicationSettings repSettings, EmailSettings emailSettings, IMemoryCache memoryCache)
        {
            try
            {
                List<Status> statii;
                memoryCache.TryGetValue("Statii", out statii);
                if (statii == null) statii = new List<Status>();

                // see if we have a host recovery
                bool hostMissed;
                memoryCache.TryGetValue("HostIntervalMissed", out hostMissed);
                if (hostMissed)
                {
                    memoryCache.Set("HostIntervalMissed", false, new MemoryCacheEntryOptions().SetPriority(CacheItemPriority.NeverRemove));
                    statii.Add(new Status() { VmName = "_Primary Host", ProblemType = "HostCommunication", Message = "Primary host is communicating", IsRecovered = true });
                }

                foreach (JObject o in data)
                {
                    string vm = (string)o["VMName"];

                    ReplicationHealth health;
                    Enum.TryParse((string)o["ReplicationHealth"], out health);
                    ReplicationHelpers.ConditionalAddStatus(health != ReplicationHealth.Normal, ref statii, vm, "ReplicationHealth", String.Format("Replication Health is {0}", health.ToString()));

                    ReplicationState state;
                    Enum.TryParse((string)o["ReplicationState"], out state);
                    ReplicationHelpers.ConditionalAddStatus(state != ReplicationState.Replicating, ref statii, vm, "ReplicationState", String.Format("Replication State is {0}", state.ToString()));

                    if (o["AverageReplicationLatency"].HasValues)
                    {
                        int latency = (int)o["AverageReplicationLatency"]["TotalSeconds"];
                        ReplicationHelpers.ConditionalAddStatus(latency > repSettings.LatencyThresholdSeconds, ref statii, vm, "AverageReplicationLatency", String.Format("Average latency {0} above threshold of {1}", latency, repSettings.LatencyThresholdSeconds));
                    }
                    else
                    {
                        ReplicationHelpers.ConditionalAddStatus(true, ref statii, vm, "AverageReplicationLatency", String.Format("Average latency is null indicating replication has not occurred in a while"));
                    }

                    double diffMins = DateTime.Now.Subtract((DateTime)o["LastReplicationTime"]).TotalMinutes;
                    ReplicationHelpers.ConditionalAddStatus(diffMins > repSettings.LastReplicationThresholdMins, ref statii, vm, "LastReplicationTime", String.Format("Last replication {0} mins ago above threshold of {1} mins", Math.Round(diffMins), repSettings.LastReplicationThresholdMins));

                    int missedCount = (int)o["MissedReplicationCount"];
                    ReplicationHelpers.ConditionalAddStatus(missedCount > repSettings.MissedThreshold, ref statii, vm, "MissedReplicationCount", String.Format("Missed {0} replications above threshold of {1}", missedCount, repSettings.MissedThreshold));

                    int errorCount = (int)o["ReplicationErrors"];
                    ReplicationHelpers.ConditionalAddStatus(errorCount > repSettings.ErrorThreshold, ref statii, vm, "ReplicationErrors", String.Format("{0} replication errors above threshold of {1}", errorCount, repSettings.ErrorThreshold));
                }

                statii.Sort((x, y) => x.VmName.CompareTo(y.VmName));

                // send notifications for any new problems (count == 1) or any recoveries - we don't notify multiple times about problems already notified
                var notifications = statii.Where(s => s.Count == 1 || s.IsRecovered == true).ToList();
                if (notifications.Count() > 0)
                {
                    int probsOutstanding = statii.Where(s => s.IsRecovered == false).Count();
                    string msg = "<table style='padding: 5px'><tr>" + string.Join("</tr><tr>", notifications) + "</tr></table>";

                    if (probsOutstanding > 0) msg += String.Format("<br />{0} problems outstanding", probsOutstanding.ToString());

                    await NotificationHelpers.SendEmail(msg, emailSettings);

                    // remove recovery statii
                    statii.RemoveAll(s => s.IsRecovered == true);

                    // set cache since counts have changed and recoveries have been removed
                    memoryCache.Set("Statii", statii, new MemoryCacheEntryOptions().SetPriority(CacheItemPriority.NeverRemove));
                }

                return statii.Count();
            }
            catch (Exception ex)
            {
                string msg = $"ReplicationHelpers.DoStatusChecks exception:<br /><br />{ex}";
                await NotificationHelpers.SendEmail(msg, emailSettings);

                return -1;
            }
        }

        private static void ConditionalAddStatus(bool condition, ref List<Status> statii, string vmname, string problemType, string message)
        {
            var problem = statii.FirstOrDefault(s => s.ProblemType == problemType && s.VmName == vmname);
            if (problem != null)
            {
                if (condition) problem.Count += 1;
                else problem.IsRecovered = true;
            }
            else {
                if (condition) statii.Add(new Status() { VmName = vmname, ProblemType = problemType, Message = message });
            }
        }
        
    }
}
