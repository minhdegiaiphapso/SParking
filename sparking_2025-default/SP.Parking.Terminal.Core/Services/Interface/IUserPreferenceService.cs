using SP.Parking.Terminal.Core.Models;
using System;

namespace SP.Parking.Terminal.Core.Services
{
    public interface IUserPreferenceService
    {
        bool HasLocal { get; }
        /// <summary>
        /// System settings of client
        /// </summary>
        ISystemSettings SystemSettings { get; }

        IHostSettings HostSettings { get; }

        IOptionsSettings OptionsSettings { get; }

        ITestingSettings TestSettings { get; }

        void SyncToServer(Action<Exception> complete);
        ServerTimeInfo GetServerTime();
    }
}
