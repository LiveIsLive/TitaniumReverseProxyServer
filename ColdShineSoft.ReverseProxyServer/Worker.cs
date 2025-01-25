namespace ColdShineSoft.ReverseProxyServer
{
	public class Worker : BackgroundService
	{
		protected readonly ILogger<Worker> Logger;

		protected readonly Config Config;

		private Titanium.Web.Proxy.ProxyServer? _ProxyServer;
		public Titanium.Web.Proxy.ProxyServer ProxyServer
		{
			get
			{
				if (this._ProxyServer == null)
				{
					this._ProxyServer = new Titanium.Web.Proxy.ProxyServer();

					//proxyServer.CertificateManager.PfxFilePath = @"C:\Users\Administrator\Desktop\新建文件夹\SHA384withRSA_cold-shine-soft.com.pfx";
					//proxyServer.CertificateManager.PfxPassword = "000000";
					//proxyServer.CertificateManager.OverwritePfxFile = true;
					//proxyServer.CertificateManager.LoadRootCertificate(proxyServer.CertificateManager.PfxFilePath, proxyServer.CertificateManager.PfxPassword, true);
					//proxyServer.CertificateManager.RootCertificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(@"C:\Users\Administrator\Desktop\新建文件夹\SHA384withRSA_cold-shine-soft.com.pfx", "000000");
					//proxyServer.CertificateManager.TrustRootCertificate(true);

					//proxyServer.CertificateManager.CreateRootCertificate(false);
					//proxyServer.CertificateManager.TrustRootCertificate();


					//this._ProxyServer.CertificateManager.CertificateEngine = Titanium.Web.Proxy.Network.CertificateEngine.DefaultWindows;
					//this._ProxyServer.CertificateManager.EnsureRootCertificate();
					// locally trust root certificate used by this proxy 
					//proxyServer.CertificateManager.TrustRootCertificate(true);

					// optionally set the Certificate Engine
					// Under Mono only BouncyCastle will be supported
					//proxyServer.CertificateManager.CertificateEngine = Network.CertificateEngine.BouncyCastle;

					this._ProxyServer.BeforeRequest += ProxyServer_BeforeRequest;
					this._ProxyServer.BeforeResponse += ProxyServer_BeforeResponse;
					//proxyServer.ServerCertificateValidationCallback += ProxyServer_ServerCertificateValidationCallback;
					//proxyServer.ClientCertificateSelectionCallback += ProxyServer_ClientCertificateSelectionCallback;


					//var explicitEndPoint = new Titanium.Web.Proxy.Models.ExplicitProxyEndPoint(System.Net.IPAddress.Any, 8000, true)
					//{
					//	// Use self-issued generic certificate on all https requests
					//	// Optimizes performance by not creating a certificate for each https-enabled domain
					//	// Useful when certificate trust is not required by proxy clients
					//	//GenericCertificate = new X509Certificate2(Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "genericcert.pfx"), "password")
					//};

					//// Fired when a CONNECT request is received
					////explicitEndPoint.BeforeTunnelConnectRequest += OnBeforeTunnelConnectRequest;

					//// An explicit endpoint is where the client knows about the existence of a proxy
					//// So client sends request in a proxy friendly manner
					//proxyServer.AddEndPoint(explicitEndPoint);

					// Transparent endpoint is useful for reverse proxy (client is not aware of the existence of proxy)
					// A transparent endpoint usually requires a network router port forwarding HTTP(S) packets or DNS
					// to send data to this endPoint
					var transparentEndPoint = new Titanium.Web.Proxy.Models.TransparentProxyEndPoint(System.Net.IPAddress.Any, this.Config.EndPointPort, true);
					if (!string.IsNullOrWhiteSpace(this.Config.CertificateFilePath))
						transparentEndPoint.GenericCertificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(this.Config.CertificateFilePath, this.Config.CertificatePassword);

					//var transparentEndPoint = new Titanium.Web.Proxy.Models.TransparentProxyEndPoint(System.Net.IPAddress.Any, this.Config.EndPointPort, true)
					//{
					//	// Generic Certificate hostname to use
					//	// when SNI is disabled by client
					//	//GenericCertificateName= "My Test Certificate",
					//	GenericCertificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(this.Config.CertificateFilePath, this.Config.CertificatePassword),
					//	//GenericCertificateName = "cold-shine-soft.com"
					//};

					this._ProxyServer.AddEndPoint(transparentEndPoint);
					//this._ProxyServer.Start();

					//proxyServer.UpStreamHttpProxy = new ExternalProxy() { HostName = "localhost", Port = 8888 };
					//proxyServer.UpStreamHttpsProxy = new ExternalProxy() { HostName = "localhost", Port = 8888 };

					//foreach (var endPoint in proxyServer.ProxyEndPoints)
					//	Console.WriteLine("Listening on '{0}' endpoint at Ip {1} and port: {2} ",
					//		endPoint.GetType().Name, endPoint.IpAddress, endPoint.Port);

					// Only explicit proxies can be set as system proxy!
					//proxyServer.SetAsSystemHttpProxy(explicitEndPoint);
					//proxyServer.SetAsSystemHttpsProxy(explicitEndPoint);

				}
				return this._ProxyServer;
			}
		}

		private Task ProxyServer_BeforeRequest(object sender, Titanium.Web.Proxy.EventArguments.SessionEventArgs e)
		{
			if (!e.HttpClient.IsHttps&&this.Config.ForceSSL)
			{
				e.Redirect($"https://{e.HttpClient.Request.RequestUri.Authority}{e.HttpClient.Request.RequestUri.PathAndQuery}");
				return Task.CompletedTask;
			}
			e.HttpClient.Request.Host = this.Config.RequestBaseUri.Authority;
			e.HttpClient.Request.RequestUri = new System.Uri(this.Config.RequestBaseUri, e.HttpClient.Request.RequestUri.PathAndQuery);
			return Task.CompletedTask;
		}

		private Task ProxyServer_BeforeResponse(object sender, Titanium.Web.Proxy.EventArguments.SessionEventArgs e)
		{
			if (e.HttpClient.Response.StatusCode == 301)
			{
				string? location = e.HttpClient.Response.Headers.GetFirstHeader("Location")?.Value;
				if (location?.Contains("://") ?? false)
				{
					e.HttpClient.Response.Headers.RemoveHeader("Location");
					e.HttpClient.Response.Headers.AddHeader("Location", new System.Uri(location).PathAndQuery);
				}
			}
			foreach (var header in this.Config.AddResponseHeaders)
				e.HttpClient.Response.Headers.AddHeader(header.Key, header.Value);

			return Task.CompletedTask;
		}

		public Worker(ILogger<Worker> logger, Config config)
		{
			Logger = logger;
			Config = config;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			this.ProxyServer.Start();
			while (!stoppingToken.IsCancellationRequested)
				await System.Threading.Tasks.Task.Delay(1000);
			this.ProxyServer.Stop();
		}
	}
}