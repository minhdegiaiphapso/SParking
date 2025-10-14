using SP.Parking.Terminal.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.ViewModels
{
    public class FindImagesViewModel : BaseViewModel
    {
        public FindImagesViewModel(IViewModelServiceLocator service)
            : base(service)
        {
            //_userPreferenceService = Mvx.Resolve<IUserPreferenceService>();
        }

        public override void Start()
        {
            base.Start();
            Setup();
        }
        public void Setup()
        {
            ShowViewModelExt<GetCheckedInformationViewModel>(null, null, null);
            //ShowViewModelExt<GetCheckedInformationViewModel>(null, null, null);
        }
    }
}
