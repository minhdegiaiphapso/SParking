using Cirrious.CrossCore;
using SP.Parking.Terminal.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.ViewModels
{
    public class BaseTestViewModel : BaseViewModel
    {
        private readonly Stopwatch Timer = new Stopwatch();
        
        IRunModeManager _manager;

        public BaseTestViewModel(IViewModelServiceLocator services)
            : base(services)
        {
            _manager = Mvx.Resolve<IRunModeManager>();
        }

        /// <summary>
        /// Start Test ViewModel with a specified running times
        /// </summary>
        public override void Start()
        {
            base.Start();
        }

        /// <summary>
        /// Executes the method.
        /// </summary>
        /// <param name="action">The action.</param>
        public void ExecuteMethod(Action action)
        {
            Timer.Restart();
            action();
            Timer.Stop();            
            Console.WriteLine(Timer.ElapsedMilliseconds);
        }

        public virtual void Run() { }
    }
}
