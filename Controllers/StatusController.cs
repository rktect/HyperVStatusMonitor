﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.OptionsModel;
using System.Threading;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Features;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace HyperVStatusMon.Controllers
{
    [Route("api/[controller]")]
    public class StatusController : Controller
    {
        private IMemoryCache _memoryCache;
        private HyperVSettings _config;

        public StatusController(IMemoryCache memoryCache, IOptions<HyperVSettings> config)
        {
            this._memoryCache = memoryCache;
            this._config = config.Value;
        }

        // GET: only for testing
        [HttpGet]
        public IEnumerable<string> Get()
        {
            DateTime last, timer;
            _memoryCache.TryGetValue("LastUpdated", out last);
            _memoryCache.TryGetValue("Timer", out timer);
            if (last != DateTime.MinValue)
                last = TimeZoneInfo.ConvertTime(last, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

            return new string[] { "HyperV Replication Monitor", String.Format("Last updated {0}", last == DateTime.MinValue ? "never" : last.ToString()), String.Format("Timer {0}", timer == DateTime.MinValue ? "off" : "on") };
        }

        [HttpPost]
        public async Task<string> Post([FromBody]dynamic data)
        {
            if (data == null) return "null";
            JArray jsonData = JArray.Parse(data.ToString());

            var numProblems = await ReplicationHelpers.DoStatusChecks(jsonData, _config.Replication, _config.Email);

            // do a timer for 15s more than configured minutes to see if the status update was NOT received from the host
            // we create the timer by adding an item to the cache and setting a callback on expiry           
            var options = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds((_config.Replication.HostNonCommunicationThresholdMins * 60) + 15))
                .RegisterPostEvictionCallback(
                (echoKey, value, reason, substate) =>
                {
                    if (reason == EvictionReason.Expired)
                        NotificationHelpers.SendEmail(String.Format("Host has not sent an update since {0}. Threshold is {1} mins.", value, _config.Replication.LastReplicationThresholdMins), _config.Email);
                });
            _memoryCache.Set("Timer", DateTime.Now, options);
            _memoryCache.Set("LastUpdated", DateTime.Now, new MemoryCacheEntryOptions().SetPriority(CacheItemPriority.NeverRemove));

            // return ok-{number of VMs processed}-{number of issues found}
            return String.Format("ok-{0}-{1}", jsonData.Count(), numProblems);
        }

    }
}
