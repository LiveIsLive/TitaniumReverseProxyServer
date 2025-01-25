using ColdShineSoft.ReverseProxyServer;

System.Net.ServicePointManager.SecurityProtocol = 0;
foreach (System.Net.SecurityProtocolType type in System.Enum.GetValues(typeof(System.Net.SecurityProtocolType)))
	try
	{
		System.Net.ServicePointManager.SecurityProtocol |= type;
	}
	catch (System.NotSupportedException exception)
	{
		System.Console.WriteLine(type + "£∫" + exception.Message);
	}
System.Console.WriteLine(System.Net.ServicePointManager.SecurityProtocol);


IHost host = Host.CreateDefaultBuilder(args)
	.ConfigureAppConfiguration((hostContext, configBuilder)=>
	{
		configBuilder.SetBasePath(System.AppContext.BaseDirectory)
		.AddJsonFile("appsettings.json")
		.Build();
	})
	.UseWindowsService()
	.ConfigureServices((hostContext, services) =>
	{

		services.AddHostedService<Worker>();
		services.AddSingleton<ColdShineSoft.ReverseProxyServer.Config>(hostContext.Configuration.GetSection("Config").Get<ColdShineSoft.ReverseProxyServer.Config>()??throw new System.ArgumentException("appsettings’“≤ªµΩ∂•≤„Config"));
	})
	.Build();

Directory.SetCurrentDirectory(System.AppContext.BaseDirectory);
await host.RunAsync();
