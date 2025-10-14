using System;
using System.Windows;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Wpf.Views;
using SP.Parking.Terminal.Core.ViewModels;
using Cirrious.CrossCore.Core;
using SP.Parking.Terminal.Core.Services;
using Cirrious.CrossCore.IoC;
using Green.Devices.Dal;
using Green.Devices.Vivotek;
using SP.Parking.Terminal.Core.Models;
using Green.Devices.CardReader;

namespace SP.Parking.Terminal.Core
{
    public partial class ApmsApp : MvxApplication
    {
        public override void Initialize()
        {        
            RFIDCardReaderService crd = new RFIDCardReaderService();
            Mvx.ConstructAndRegisterSingleton<IRFIDCardReaderService, RFIDCardReaderService>();
            Mvx.ConstructAndRegisterSingleton<IProlificCardReaderFactory, ProlificCardReaderFactory>();

            CreatableTypes()
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();
            //CreatableTypes()
            //    .EndingWith("Settings")
            //    .AsInterfaces()
            //    .RegisterAsLazySingleton();

            Mvx.LazyConstructAndRegisterSingleton<ANPRService>(() => new ANPRService());
            Mvx.LazyConstructAndRegisterSingleton(() => new PlateRecognizeService());
            ArgumentParameter argParams = Mvx.Resolve<IRunModeManager>().ArgumentParams;
            Mvx.LazyConstructAndRegisterSingleton<IOptionsSettings, OptionsSettings>();
            Mvx.LazyConstructAndRegisterSingleton<ITestingSettings>(() => new TestingSettings(argParams));
            Mvx.LazyConstructAndRegisterSingleton<IHostSettings>(() => new HostSettings(argParams));
            Mvx.LazyConstructAndRegisterSingleton<ISystemSettings>(() => new SystemSettings(argParams));
            //IBugSenderService service = Mvx.Resolve<IBugSenderService>();

            InitialiseServices();

            //RunMode mode = Mvx.Resolve<IRunModeManager>().ArgumentParams.Mode;

            //if (mode == Services.RunMode.Production)
            //{
            //    IUserPreferenceService us = Mvx.Resolve<IUserPreferenceService>();
            //    if (!us.HasLocal)
            //        RegisterAppStart<ConfigViewModel>();
            //    else
            //        us.SyncToServer((exception) =>
            //        {
            //            if (exception == null)
            //                RegisterAppStart<LaneContainerViewModel>();
            //            else
            //                Console.WriteLine("Error");
            //        });
            //}

            //else if (mode == Services.RunMode.Testing)
            //    RegisterAppStart<TestViewModel>();

            RegisterAppStart<BootstrapViewModel>();

            //Card[] cards = new Card[2];
            //cards[0] = new Card() { Id = "abcds", Label = "C1111", CardType = CardType.Guest, Status = CardStatus.Free, VehicleType = VehicleType.Bike };
            //cards[1] = new Card() { Id = "abcds1", Label = "C1111", CardType = CardType.Guest, Status = CardStatus.Free, VehicleType = VehicleType.Bike };
            //IServer server = Mvx.Resolve<IServer>();
            //server.CreateCards(cards, (rs, ex) =>
            //{
            //    if(ex != null)
            //    {
            //        Console.WriteLine(ex.Message);
            //    }
            //    else
            //    {
            //        Console.WriteLine(rs.NumCreated);
            //    }
            //});
        }       
        private void InitialiseServices()
        {
            // View model service locator
			Mvx.LazyConstructAndRegisterSingleton<IBarrierDeviceManager, BarrierDeviceManager>();

            Mvx.LazyConstructAndRegisterSingleton<IWebClient, WebClient>();
            Mvx.LazyConstructAndRegisterSingleton<IViewModelServiceLocator, ViewModelServiceLocator>();
            Mvx.LazyConstructAndRegisterSingleton<IServer, WebAPIServer>();
            Mvx.LazyConstructAndRegisterSingleton<IWebApiTestingServer, WebApiTestingServer>();
            Mvx.LazyConstructAndRegisterSingleton<IUserServiceLocator, UserServiceLocator>();
        }
    }
}