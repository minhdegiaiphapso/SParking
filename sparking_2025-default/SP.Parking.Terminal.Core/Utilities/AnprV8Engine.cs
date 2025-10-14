using ND.ANPR.Service.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Utilities
{
	public class AnprV8Engine
	{
		private static AnprV8Engine _instance;
		private ND.ANPR.Service.Server.AnprEngine _engine;
		private bool _isStart;
		private string _serverPath;
		private AnprV8Engine()
		{
			_engine = new AnprEngine();
			_isStart = false;
			_serverPath = string.Empty;
		}
		public static AnprV8Engine Instance
		{
			get
			{
				if (_instance == null)
					_instance = new AnprV8Engine();
				return _instance;
			}
		}
		public void Start(string ServerPath)
		{
			if (!string.IsNullOrEmpty(ServerPath) && (!_isStart || _serverPath != ServerPath))
			{
				_engine.Start(ServerPath);
				_isStart = true;
				_serverPath = ServerPath;
			}
		}
		public void Stop()
		{
			_engine?.Stop();
		}
	}
}
