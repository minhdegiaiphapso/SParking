using SP.Parking.Terminal.Core;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Wpf.Platform;
using Cirrious.MvvmCross.Wpf.Views;
using System;
using System.Windows.Controls;
using System.Windows.Threading;
using Cirrious.CrossCore;
using SP.Parking.Terminal.Core.Services;
using SP.Parking.Terminal.Wpf.Services;
using Cirrious.MvvmCross.BindingEx.WindowsShared;
using SP.Parking.Terminal.Core.Utilities;
using Green.Devices.Dal;
using SP.Parking.Terminal.Wpf.UI;
using Cirrious.CrossCore.Converters;
using System.Collections.Generic;
using System.Reflection;
using SP.Parking.Terminal.Wpf.Devices;
using Serilog;

namespace SP.Parking.Terminal.Wpf
{
    public class Setup : MvxWpfSetup
    {
        private ArgumentParameterManager _params;

        public Setup(ArgumentParameterManager param, Dispatcher dispatcher, IMvxWpfViewPresenter presenter)
            : base(dispatcher, presenter)
        {
            _params = param;
        }

        protected override IMvxApplication CreateApp()
        {
            return new SP.Parking.Terminal.Core.ApmsApp();
        }

        protected override IMvxTrace CreateDebugTrace()
        {
            return new DebugTrace();
        }

		protected override void InitializeFirstChance()
		{
			base.InitializeFirstChance();

            Mvx.RegisterSingleton<IRunModeManager>(new RunModeManager());
            Mvx.RegisterSingleton<ILogger>(Log.Logger);

            Mvx.Resolve<IRunModeManager>().ArgumentParams = _params.Parameters;

            Mvx.RegisterType<ICamera, VitaminCamera>();
            
            Mvx.LazyConstructAndRegisterSingleton<ILogService, LogService>();

			Mvx.LazyConstructAndRegisterSingleton<IUIService>(() => new UIService());

            Mvx.LazyConstructAndRegisterSingleton<ILocalizeService>(() => new LocalizeService());
		}

        protected override void InitializeLastChance()
        {
            base.InitializeLastChance();

            var builder = new MvxWindowsBindingBuilder();
            builder.DoRegistration();

            Mvx.CallbackWhenRegistered<IMvxValueConverterRegistry>(FillValueConverters);
        }

        private void FillValueConverters(IMvxValueConverterRegistry registry)
        {            
            registry.AddOrOverwrite("ByteImage", new ByteImageValueConverter());
        }

        //protected virtual List<Assembly> ValueConverterAssemblies
        //{
        //    get
        //    {
        //        var toReturn = new List<Assembly>();
        //        toReturn.AddRange(GetViewModelAssemblies());
        //        toReturn.AddRange(GetViewAssemblies());
        //        return toReturn;
        //    }
        //}
    }
}
