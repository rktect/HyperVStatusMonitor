# Hyper-V Replication Monitor #

Monitors the replication health for multiple VMs on a host. 

## Monitoring Features ##

User configurable thresholds to monitor VMs with:

1. Any replication "Health" other than "Normal"
2. Any replication "State" other than "Replicating"
3. An average replication latency of greater than X seconds (300)
4. A last replication time of greater than X minutes (15)
5. More than X missed replications (5)
6. More than X replication errors (0 - 1 error will trigger a notification)
7. Number of minutes without any host communications (10, or 2 x 5 min intervals missed)

## Architecture: ##

To remain as secure as possible, the solution uses a 'push' scenario from the Hyper-V host server. It remains light in processing and has simplistic execution on the host.

The application consists of two components:

1. A **powershell script** on the host which runs on a predetermined interval using Task Scheduler. Since this script calls 'out' on port 80 (or 443 for HTTPS) your firewall will need to permit outgoing requests from the machine running this script. Tested in Powershell 3 and 5. 
2. A **web application** hosted at Azure or another geo-independent location from the host machine. This ensures problems can be detected and notifications sent even if the host goes down. This is built in the new ASP.NET Core 1.0 (1.0.0-rc1-final) so it can literally be compiled and run on any platform.






