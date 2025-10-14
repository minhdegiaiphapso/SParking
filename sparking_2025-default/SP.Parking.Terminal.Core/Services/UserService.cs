using SP.Parking.Terminal.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Services
{
	public class UserService : IUserService
	{
        private IServer _server;
        private Action<Exception> _onCompleted;

        public ApmsUser CurrentUser
        {
            get;
            set;
        }

        public bool IsLogin
        {
            get { return CurrentUser != null; }
        }

        public UserService(IServer server)
        {
            this._server = server;
        }

        public void Login(string username, string password, int laneID, Action<Exception> complete)
        {
            _onCompleted = complete;
            _server.Login(username, password, laneID, OnLoginResult);
        }

        public void Login(string cardId, int laneID, Action<Exception> complete)
        {
            _onCompleted = complete;
            _server.Login(cardId, laneID, OnLoginResult);
        }

        private void OnLoginResult(ApmsUser user, Exception exception)
        {
            if (exception == null)
            {
                CurrentUser = user;
            }
            if (_onCompleted != null)
            {
                _onCompleted(exception);
            }
        }

        public void Logout(int laneID, Action<UserShift, Exception> complete, int revenue = 0)
        {
            if (CurrentUser == null) return;
            _server.Logout(CurrentUser.ShiftID, CurrentUser.Id, laneID, (shift, exception) => 
            {
                if (exception == null)
                {
                    shift.User = CurrentUser;
                    //CurrentUser = null;
                }
                complete(shift, exception);
            }, revenue);
        }

        public void UpdateLogout(UserShift userShift, Action<UserShift, Exception> complete)
        {
            _server.Logout(userShift.Id, userShift.UserId, userShift.LaneId, (shift, exception) =>
            {
                if (exception == null)
                {
                    shift.User = CurrentUser;
                    CurrentUser = null;
                }
                complete(shift, exception);
            }, userShift.Revenue);
        }
    }
}
