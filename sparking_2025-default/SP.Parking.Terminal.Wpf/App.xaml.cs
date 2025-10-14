using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Wpf.Views;
using Cirrious.CrossCore;
using SP.Parking.Terminal.Wpf.Views;
using SP.Parking.Terminal.Core.Services;
using SP.Parking.Terminal.Core.Utilities;
using NLog;
using System.Reflection;
using System.Windows.Threading;
using SP.Parking.Terminal.Wpf.Utility;
using Microsoft.Shell;
using Serilog;
//using Bosch.VideoSDK.AxCameoLib;
//using Bosch.VideoSDK.CameoLib;

namespace SP.Parking.Terminal.Wpf
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        // TODO: Make this unique!
        private const string Unique = "Change this to something that uniquely identifies your program.";

        [STAThread]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                var application = new App();
                application.InitializeComponent();

                Log.Logger = new LoggerConfiguration()
                                .WriteTo.File("serilog.log", outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                                .CreateLogger();

                application.Run();
               
                // Allow single instance code to perform cleanup operations
                //SingleInstance<App>.Cleanup();
            }
        }

      

        #region ISingleInstanceApp Members
        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            // Bring window to foreground
            if (this.MainWindow.WindowState == WindowState.Minimized)
            {
                this.MainWindow.WindowState = WindowState.Normal;
            }

            this.MainWindow.Activate();

            return true;
        }
        #endregion

        private bool _setupComplete = false;

        ArgumentParameterManager _params;

        string _version = string.Empty;

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        protected override void OnStartup(StartupEventArgs e)
        {
            // Setup unhandled exception
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            //this.Dispatcher.UnhandledException += Dispatcher_UnhandledException;

            _version = OtherUtilities.GetVersion();
            _params = new ArgumentParameterManager();
            CommandLine.Parser.Default.ParseArgumentsStrict(e.Args, _params);
            _mailSender = new MailSender();
            _mailSender.SendMailComplete = SendMailCompleted;

            base.OnStartup(e);
        }
		protected override void OnExit(ExitEventArgs e)
		{
			var anprV8 = AnprV8Engine.Instance;
			anprV8?.Stop();
			base.OnExit(e);
		}
		private void SendMailCompleted(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        MailSender _mailSender = null;
        void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.Fatal("{0} - Current_DispatcherUnhandledException occur. \n Object = {1}. \n Exception = {2}", _version, sender, e.Exception);
            e.Handled = true;

            if (LocalLogService.LastException != null)
                _mailSender.SendAsync("[SP.Parking.Terminal.Wpf] Crash Report", e.Exception.ToString() + Environment.NewLine + "Last exception: " + LocalLogService.LastException.ToString(), null);
            else
                _mailSender.SendAsync("[SP.Parking.Terminal.Wpf] Crash Report", e.Exception.ToString(), null);

            CrashDump.MiniDumpToFile();
            string msg = e.Exception.ToString();
            if (msg.Length > 200) msg = msg.Substring(0, 400) + "...";
            MessageBox.Show(msg);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Fatal("{0} - UnhandledException occur. \n Object = {1}. \n Exception = {2}", _version, sender, e.ExceptionObject.ToString());

            //if (LocalLogService.LastException != null)
            //    _mailSender.SendAsync("[SP.Parking.Terminal.Wpf] Crash Report", e.ExceptionObject.ToString() + Environment.NewLine + "Last exception: " + LocalLogService.LastException.ToString(), null);
            //else
            //    _mailSender.SendAsync("[SP.Parking.Terminal.Wpf] Crash Report", e.ExceptionObject.ToString(), null);
            CrashDump.MiniDumpToFile();
            string msg = e.ExceptionObject.ToString();
            if (msg.Length > 200) msg = msg.Substring(0, 400) + "...";
            MessageBox.Show(msg);
        }
       
        private void DoSetup()
        {
            var presenter = new ApmsPresenter(MainWindow);

            var setup = new Setup(_params, Dispatcher, presenter);
            setup.Initialize();

            var start = Mvx.Resolve<IMvxAppStart>();
            start.Start();

            _setupComplete = true;
            _logger.Fatal("App start");
        }

        protected override void OnActivated(System.EventArgs e)
        {
            if (!_setupComplete)
                DoSetup();
            
            base.OnActivated(e);
        }
    }
}
