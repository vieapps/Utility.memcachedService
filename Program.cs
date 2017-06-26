using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Diagnostics;

namespace net.vieapps.Services.memcachedService
{
	static class Program
	{
		static void Main()
		{
			ServiceBase.Run(new ServiceBase[] { new memcachedService() });
		}
	}
}