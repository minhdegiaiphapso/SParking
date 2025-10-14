using Cirrious.CrossCore;
using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.ViewModels
{
    public class StatisticsViewModel : BaseViewModel
    {
        IServer _server;

        IHostSettings _hostSettings;

        private Statistics _statistics;
        public Statistics Statistics
        {
            get { return _statistics; }
            set
            {
                _statistics = value;
                RaisePropertyChanged(() => Statistics);
            }
        }

        public StatisticsViewModel(IViewModelServiceLocator service)
            : base(service)
        {
            _hostSettings = Mvx.Resolve<IHostSettings>();
            _server = Mvx.Resolve<IServer>();
            var now = TimeMapInfo.Current.LocalTime;
            DateTime from = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            //DateTime from = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            _server.GetStatistics(from, now, _hostSettings.Terminal.Id, (result, ex) => {
                this.Statistics = result;
            });
            //_server.GetStatistics(from, DateTime.Now, _hostSettings.Terminal.Id, (result, ex) => {
            //    this.Statistics = result;
            //});
        }

        public override void Start()
        {
            base.Start();
        }
    }
}
