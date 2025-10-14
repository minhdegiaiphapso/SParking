using SP.Parking.Terminal.Core.Services;
using Cirrious.MvvmCross.ViewModels;
using System;
using System.Diagnostics;

namespace SP.Parking.Terminal.Core.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        //public IMvxCommandCollection Commands { get; private set; }

        public HomeViewModel(IViewModelServiceLocator services) : base(services)
        {

        }

        public void Init(ParameterKey key)
        {
           
        }
    }
}