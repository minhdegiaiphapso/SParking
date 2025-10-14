using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ND.ANPR.Service.Server
{
	public class AnprEngine
	{
		private string _serverPath;
		private bool isStarted = false;
		private System.Diagnostics.Process procServer = null;
		private readonly string procName = "python ../../manage.py runserver 8192";
		private string WorkingDirectory;
		int numstart = 0;
		int tickCount = 0;
		Task task;
		APICaller myCaller;
		public string ServerPath => _serverPath;
		public AnprEngine()
		{
			myCaller = new APICaller();
		}
		public void Start(string serverPath)
		{
			Stop();
			_serverPath = serverPath;
			WorkingDirectory = Path.Combine(serverPath, "Python3.9.4\\ANPRV8\\venv\\Scripts");
			isStarted = true;
			task = Task.Factory.StartNew(() => MaintainProcess(), TaskCreationOptions.LongRunning);
		}
		private async void MaintainProcess()
		{
			while (isStarted)
			{
				if(numstart == 0)
				{
					var b = await myCaller.CheckHealth();
					if (!b)
					{
						StartProcess();
						numstart++;
						tickCount = Environment.TickCount;
					}
					else
					{
						numstart++;
						tickCount = Environment.TickCount;
					}
					
					
				}
				else if ((Environment.TickCount - tickCount) > 30000)
				{
					var b = await myCaller.CheckHealth();
					if (!b)
					{
						StartProcess();
						numstart++;
						tickCount = Environment.TickCount;
					}
					else
					{
						tickCount = Environment.TickCount;
					}
				}
				await Task.Delay(3000);
			}
		}
		private async void StartProcess()
		{
			try
			{
				FixConfiguration();
				// create the ProcessStartInfo using "cmd" as the program to be run,
				// and "/c " as the parameters.
				// Incidentally, /c tells cmd that we want it to execute the command that follows,
				// and then exit.

				System.Diagnostics.ProcessStartInfo procStartInfo =
						new System.Diagnostics.ProcessStartInfo("cmd.exe", "/c " + procName);

				procStartInfo.WorkingDirectory = WorkingDirectory;
				// The following commands are needed to redirect the standard output.
				// This means that it will be redirected to the Process.StandardOutput StreamReader.
				//procStartInfo.RedirectStandardOutput = true;
				procStartInfo.UseShellExecute = false;
				// Do not create the black window.
				procStartInfo.CreateNoWindow = true;

				// Now we create a process, assign its ProcessStartInfo and start it
				procServer = new System.Diagnostics.Process();
				procServer.StartInfo = procStartInfo;

				procServer.Start();
			}
			catch (Exception objException)
			{
				//Can log here!
			}
		}
		private void FixConfiguration()
		{
			var fieConfig = Path.Combine(ServerPath, "Python3.9.4\\ANPRV8\\venv\\pyvenv.cfg");
			var pythonPath = Path.Combine(ServerPath, "Python3.9.4");
			if (File.Exists(fieConfig))
			{
				var result = File.ReadAllLines(fieConfig);
				if (result.Length >= 3)
				{
					result[0] = $"home = {pythonPath}";
				}
				File.WriteAllLines(fieConfig, result);
			}
		}
		public void Stop()
		{
			isStarted = false;
			try
			{
				if (procServer != null)
				{
					SendCtrlC(procServer);

					if (!procServer.WaitForExit(1000))
					{
						procServer.Kill();
						procServer.WaitForExit();
					}

					if (procServer != null)
					{
						procServer.Dispose();
						procServer = null;
					}
				}
			}
			catch (Exception objException)
			{
				//
			}
		}
		#region send Ctrl + C to process
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool AttachConsole(uint dwProcessId);
		[DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
		private static extern bool FreeConsole();
		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GenerateConsoleCtrlEvent(CtrlTypes dwCtrlEvent, uint dwProcessGroupId);
		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetConsoleCtrlHandler(EventHandler HandlerRoutine, bool Add);
		public static void SendCtrlC(Process proc)
		{
			if (AttachConsole((uint)proc.Id))
			{
				SetConsoleCtrlHandler(null, true);
				GenerateConsoleCtrlEvent(CtrlTypes.CTRL_C_EVENT, 0);
				proc.WaitForExit(100);
				FreeConsole();
				SetConsoleCtrlHandler(null, false);
			}
		}

		public enum CtrlTypes : uint
		{
			CTRL_C_EVENT,
			CTRL_BREAK_EVENT,
			CTRL_CLOSE_EVENT,
			CTRL_LOGOFF_EVENT = 5u,
			CTRL_SHUTDOWN_EVENT
		}
		#endregion
	}
}
