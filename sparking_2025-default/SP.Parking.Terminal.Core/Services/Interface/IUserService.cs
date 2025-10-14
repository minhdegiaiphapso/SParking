using SP.Parking.Terminal.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Services
{
    /// <summary>
    /// Validate on login fail exception
    /// </summary>
    public class LoginInvalidException : Exception
    {
        public LoginInvalidException() : base() { }
        public LoginInvalidException(string message) : base(message) { }
    }

    public class InternalServerErrorException : ServerErrorException
    {
        public InternalServerErrorException() : base() { }
        public InternalServerErrorException(string message) : base(message) { }
    }

    public class NotAcceptableException : Exception
    {
        public NotAcceptableException() : base() { }
        public NotAcceptableException(string message) : base(message) { }
    }

    /// <summary>
    /// Error that handled and thrown by server
    /// </summary>
    public class ServerErrorException : Exception
    {
        public ServerErrorException() : base() { }
        public ServerErrorException(string message) : base(message) { }
    }

    /// <summary>
    /// Error that thrown when interacts with server
    /// </summary>
    public class ServerDisconnectException : Exception
    {
        public ServerDisconnectException() : base() { }
        public ServerDisconnectException(string message) : base(message) { }
    }

    /// <summary>
    /// User service interface
    /// </summary>
	public interface IUserService
	{
        /// <summary>
        /// Current active user
        /// </summary>
        ApmsUser CurrentUser { get; set; }

        /// <summary>
        /// Login by username and password
        /// </summary>
        /// <param name="username">Username to login</param>
        /// <param name="password">Password to login</param>
        /// <param name="laneID">Lane ID of login</param>
        /// <param name="complete">Result callback</param>
        void Login(string username, string password, int laneID, Action<Exception> complete);

        /// <summary>
        /// Login by card ID
        /// </summary>
        /// <param name="cardId">Card ID to login</param>
        /// <param name="laneID">Lane ID of login</param>
        /// <param name="complete">Result callback</param>
        void Login(string cardId, int laneID, Action<Exception> complete);

        /// <summary>
        /// Logout
        /// </summary>
        /// <param name="laneID">Lane ID of login</param>
        /// <param name="complete">Result callback</param>
        /// <param name="revenue">Revenue of shift</param>
        void Logout(int laneID, Action<UserShift, Exception> complete, int revenue = 0);

        /// <summary>
        /// Update logout info
        /// </summary>
        /// <param name="userShift">User shift info</param>
        /// <param name="complete">Result callback</param>
        void UpdateLogout(UserShift userShift, Action<UserShift, Exception> complete);

        /// <summary>
        /// Already login or not
        /// </summary>
        bool IsLogin { get; }
	}
}
