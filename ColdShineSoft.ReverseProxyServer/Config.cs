using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColdShineSoft.ReverseProxyServer
{
	public class Config
	{
		public string RequestBaseUrl { get; set; } = null!;
		public int EndPointPort { get; set; }
		public string CertificateFilePath { get; set; } = null!;
		public string CertificatePassword { get; set; } = null!;

		public bool ForceSSL { get; set; }

		private System.Uri _RequestBaseUri=null!;
		public System.Uri RequestBaseUri
		{
			get
			{
				if (this._RequestBaseUri == null)
					this._RequestBaseUri = new Uri(this.RequestBaseUrl);
				return this._RequestBaseUri;
			}
		}

		private System.Collections.Generic.Dictionary<string, string> _AddResponseHeaders = null!;
		public System.Collections.Generic.Dictionary<string, string> AddResponseHeaders
		{
			get
			{
				if (this._AddResponseHeaders == null)
					this._AddResponseHeaders = new Dictionary<string, string>();
				return this._AddResponseHeaders;
			}
			set
			{
				this._AddResponseHeaders = value;
			}
		}
	}
}