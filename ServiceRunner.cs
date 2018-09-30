using System;
using System.ServiceProcess;

namespace net.vieapps.Services.Utility.Memcached
{
	public partial class ServiceRunner : ServiceBase
	{
		ServiceComponent Component { get; } = new ServiceComponent();

		public ServiceRunner() => this.InitializeComponent();

		protected override void OnStart(string[] args) => this.Component.Start(args);

		protected override void OnStop() => this.Component.Stop();
	}
}