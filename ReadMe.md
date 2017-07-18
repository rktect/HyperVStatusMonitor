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

The default parameters are set for a replication primary and replica in the same hosting environment. If you are replicating over a WAN you should adjust these as necessary for your scenario.

## Architecture ##

To remain as secure as possible, the solution uses a 'push' scenario from the Hyper-V host server. It remains light in processing and has simplistic execution on the host. Status details are processed in a web application hosted in an external environment with no dependancies on the Hyper-V hosting.

The application consists of two components:

1. A **powershell script** on the host which runs on a predetermined interval using Task Scheduler. Since this script calls 'out' on port 80 (or 443 for HTTPS) your firewall will need to permit outgoing requests from the machine running this script. Tested in Powershell 3+. 
2. A **web application** hosted at Azure or another geo-independent location from the host machine. This ensures problems can be detected and notifications sent even if the host goes down. This is built in the new ASP.NET Core 1.0 (1.0.0-rc1-final) so it can be compiled and run on any platform.

The notification structure uses "Problem" and "Recovery" logic. So you are only notified of each problem once, instead of repeatedly getting "Problem" emails every status check interval. The system will send you a "Recovery" as each problem is resolved - including a note about how many problems are unresolved.

## Instructions ##

1. Clone the repo.
2. Edit the appsettings.json file to set up:
		
	a. Your desired replication condition thresholds based on your Hyper-V environment.

	b. Configure the SMTP parameters for an email service. Values for SMTP user and password *can* be put in appsettings.json, but it is recommended to set these in the Environment Variables for more secure storage. This also makes it easy to have different development and production values based on the environment. Note that this service should not be dependent on your Hyper-V hosting environment - so that notifications can still be sent out if that data center is offline. For email service - [SendGrid.com](https://sendgrid.com) offers a free developer account with 25,000 emails per month and great metrics built-in.

	c. Set up the email subject as well as from and to addresses for your monitoring needs.
3. Build and publish the app to Azure or your preferred location. You can do this in Visual Studio 2015 (Community Edition is free!) or using the [dnx command prompt](https://docs.asp.net/en/latest/dnx/commands.html "dnx").
4. The host will write a new entry into the Application log on the host if there are any issues connecting to the web API. In order to do this you need to add a new "Source" for this log using Powershell (run as Administrator):

		New-EventLog –LogName Application –Source "Hyper-V Replication Monitor"

	You can easily view these with Event Viewer or this Powershell script:

		Get-EventLog -LogName Application -Source "Hyper-V Replication Monitor"

4. Edit the 'PostReplicationStatusHttp.ps1' file (or PostReplicationStatusHttps.ps1 if you plan to use HTTPS with a self-signed certificate). You only need to set the $endPoint parameter for the web application URI you set up in step 3, and the $eventSource to the "Source" string you set up in step 4.
6. Copy the edited Powershell script to the Hyper-V host (or a machine that has permissions to access Replication statistics). 

	If you are running the script from another machine than the primary replication host, then you will have to tell the script what server the primary is on:

		Change:
		$json = Measure-VMReplication | ConvertTo-Json -Compress
		To:
		$json = Measure-VMReplication -ComputerName {primary-server-name} | ConvertTo-Json -Compress

	You can test this script by running it with appropriate permissions in Powershell. In your Powershell test, you will see the Response data which shows a 200-OK status if it submitted successfully. Under "Content", you will see "Host ok. {x} VMs checked. {y} problems found." to tell you what was processed and the outcome of the status condition logic. 
7. Set the script to run on an interval using Task Scheduler > Add a new task. 

	You can open Task Scheduler on a remote machine by right-clicking Task Scheduler (local) and select "Connect to a remote computer". If you have problems with connecting, it is likely your firewall preventing it - try running this in Powershell: 

		Set-NetFirewallRule -DisplayGroup 'Remote Event Log Management' -Enabled True -PassThru

	You will need to select "Run whether user is logged on or not" and "Run with highest privileges". Make sure a user with sufficient privileges is configured to run the task. 

	Under triggers, add a new daily schedule with "Repeat task every" 5 minute intervals (or whatever duration you are comfortable monitoring the replication status) and choose "Indefinitely".  

	Under actions, create a new action (changing the script path to your file location):

		Program/script:	c:\windows\system32\WindowsPowerShell\v1.0\powershell.exe
		Add arguments: -WindowStyle Hidden -NonInteractive -Executionpolicy unrestricted -file c:\{your path to file}\PostReplicationStatusHttp.ps1

	You can configure any other task properties you wish based on your needs and make sure the trigger and task are enabled.



And that is it. If your web app is hosted at Azure, you can see the API being called on the duration you configured in the portal. You can also see the task status in Task Scheduler but I've noticed that it can say "successful" when running powershell scripts that actually didn't fully run for whatever permission or connectivity issue.

**Note:** If you are hosting your web app in Azure or another location where the timezone is UTC or other, you can adjust the notification timezone setting in DateHelpers.cs to what zone you are in. 


## Requirements ##

Tested with Hyper-V Server 2012 R1 and R2, Powershell 3+, Azure App Service and IIS 7.5+

## Future Features ##

- Ability to kick off an unplanned failover to the replica server for all VMs.
- Update to latest .NET core
