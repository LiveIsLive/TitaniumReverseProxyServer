using ColdShineSoft.ReverseProxyServer;

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
