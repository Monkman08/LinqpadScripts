<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Net.NetworkInformation</Namespace>
</Query>

int[] servers = 
{
123,12345,12345
};

void Main()
{
	Console.WriteLine("Scanning Local Network Addresses...");
	
	Dictionary<string, Task<PingReply>> pingTasks = new Dictionary<string, Task<PingReply>>();
	
	var addresses = servers.Select(s => string.Format("the{0}.server", s)).ToArray();
	
	foreach (var address in addresses)
		pingTasks.Add(address, PingAsync(address));
		
	Task.WaitAll(pingTasks.Values.ToArray());
	
	var ips = new Dictionary<string, string>();
	foreach (var pTask in pingTasks)
		if(pTask.Value.Result.Status == IPStatus.Success)
		{
				// infer Other machine address from server IP	
				var octets = pTask.Value.Result.Address.ToString().Split('.');
				octets[3] = "6";
				var compIp = String.Join(".", octets);
				
				ips.Add(pTask.Key, compIp);
		}
	
	ips.Dump(" ComputerIp + Server Name");
	
	Console.WriteLine("ARE YOU SURE? This modify the Servers!!!");
	var input = Console.ReadLine();
	if (!String.Equals(input, "y", StringComparison.InvariantCultureIgnoreCase))
		return;
	
	int count = 0;
	foreach(var ip in ips.Values)
	{
		Console.WriteLine("[Exec{0}] Executing Command for '{1}'", count++, ip);
		
		var processInfo = new ProcessStartInfo
		{
			WorkingDirectory = "C:\\Data",
			FileName = "C:\\Data\\psexec.exe",
			CreateNoWindow = true,
			UseShellExecute = false,
			WindowStyle = ProcessWindowStyle.Hidden,
			Arguments = String.Format(@"\\{0} -u Administrator -p supersecretpassword -c -f C:\DATA\ProgramToRun.exe", ip),
			RedirectStandardOutput = false
		};	
		
		Process.Start(processInfo);		
	}
}

// Define other methods and classes here

static Task<PingReply> PingAsync(string address)
{
	var tcs = new TaskCompletionSource<PingReply>();
	Ping ping = new Ping();
	ping.PingCompleted += (obj, sender) => { tcs.SetResult(sender.Reply); };
	ping.SendAsync(address, new object());
	return tcs.Task;
}