using RestSharp;
using SP.Parking.Terminal.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Services
{
    /// <summary>
    /// Interface of APMS server functions
    /// </summary>
    public interface IServer
    {
        /// <summary>
        /// Login by username and password
        /// </summary>
        /// <param name="username">Username to login</param>
        /// <param name="password">Password to login</param>
        /// <param name="laneID">Lane ID of login</param>
        /// <param name="complete">Result callback</param>
        void Login(string username, string password, int laneID, Action<ApmsUser, Exception> complete);
        void CreateVoucher(Voucher data, Action<Exception> complete);
        void DeleteVoucher(string Card_Id, DateTime Check_In_Time, Action<Exception> complete);
        void RecallFee(string cardID, int voucherhour, Action<int, Exception> complete);
        void FindAndNotifyBlacklist(int parking_id, string imgPath, string vehiclenumber, int gate, int user, int stateparking, Action<string, Exception> complete);
        void ForcedBarier(ForcedInfo data, Action<ForcedInfo, Exception> complete);
        /// <summary>
        /// Login by card id
        /// </summary>
        /// <param name="cardID">Card ID to login</param>
        /// <param name="laneID">Lane ID of login</param>
        /// <param name="complete">Result callback</param>
        void Login(string cardID, int laneID, Action<ApmsUser, Exception> complete);

        /// <summary>
        /// Logout
        /// </summary>
        /// <param name="shiftID">Shift ID of login session</param>
        /// <param name="userID">User ID</param>
        /// <param name="laneID">Lane ID</param>
        /// <param name="revenue">Revenue of shift</param>
        /// <param name="complete">Result callback</param>
        void Logout(int shiftID, int userID, int laneID, Action<UserShift, Exception> complete, int revenue = 0);

        /// <summary>
        /// Create check in info
        /// </summary>
        /// <param name="data">Data of check in</param>
        /// <param name="complete">Result callback</param>
        void CreateCheckIn(CheckIn data, Action<CheckIn, Exception> complete);

        /// <summary>
        /// Update check in info
        /// </summary>
        /// <param name="data">Data of check in</param>
        /// <param name="complete">Result callback</param>
        void UpdateCheckIn(CheckIn data, Action<CheckIn, Exception> complete);

        /// <summary>
        /// Get check in info by card id
        /// </summary>
        /// <param name="cardID">Card ID of check in</param>
        /// <param name="complete">Result callback</param>
        void GetCheckIn(string cardID, Action<CheckIn, Exception> complete);

        /// <summary>
        /// Create check out info
        /// </summary>
        /// <param name="data">Data of check out</param>
        /// <param name="complete">Result callback</param>
        void CreateCheckOut(CheckOut data, CustomerInfo customerInfo, Action<Exception> complete);

        /// <summary>
        /// Gets the terminals.
        /// </summary>
        /// <param name="complete">The complete.</param>
        void GetTerminals(Action<Models.Terminal[], Exception> complete);

        /// <summary>
        /// Create terminal info
        /// </summary>
        /// <param name="data">Data of terminal</param>
        /// <param name="complete">Result callback</param>
        void CreateTerminal(Models.Terminal data, Action<Models.Terminal, Exception> complete);

        /// <summary>
        /// Update terminal info
        /// </summary>
        /// <param name="data">Data of terminal</param>
        /// <param name="complete">Result callback</param>
        void UpdateTerminal(Models.Terminal data, Action<Models.Terminal, Exception> complete);

        /// <summary>
        /// Create lane info
        /// </summary>
        /// <param name="data">Data of lane</param>
        /// <param name="complete">Result callback</param>
        void CreateLane(Lane data, Action<Lane, Exception> complete);

        /// <summary>
        /// Update lane info
        /// </summary>
        /// <param name="data">Data of lane</param>
        /// <param name="complete">Result callback</param>
        void UpdateLane(Lane data, Action<Lane, Exception> complete);

        /// <summary>
        /// Create lane info
        /// </summary>
        /// <param name="data">Data of lane</param>
        /// <param name="complete">Result callback</param>
        void CreateLane(Lane[] data, Action<Lane[], Exception> complete);

        /// <summary>
        /// Create camera info
        /// </summary>
        /// <param name="data">Data of camera</param>
        /// <param name="complete">Result callback</param>
        void CreateCamera(Camera data, Action<Camera, Exception> complete);

        /// <summary>
        /// Update camera info
        /// </summary>
        /// <param name="data">Data of camera</param>
        /// <param name="complete">Result callback</param>
        void UpdateCamera(Camera data, Action<Camera, Exception> complete);

        /// <summary>
        /// Check the server health
        /// </summary>
        /// <param name="ip">IP address of server to check</param>
        /// <param name="complete">Result callback</param>
        void CheckHealthServer(string ip, Action<Exception> complete);

        /// <summary>
        /// Search check in by card id
        /// </summary>
        /// <param name="cardId">Card ID to search</param>
        /// <param name="complete">Result callback</param>
        void ParkingSessionSearchBasic(string cardId, Action<CheckIn[], Exception> complete);

        /// <summary>
        /// Search check in by advanced information
        /// </summary>
        /// <param name="complete">Result callback</param>
        /// <param name="fromTime">From time</param>
        /// <param name="toTime">To time</param>
        /// <param name="limit">Limit amount of result</param>
        /// <param name="cardLabel">Card label to search. Ignore by set null</param>
        /// <param name="vehicleNumber">Vehicle number to search. Ignore by set null</param>
        /// <param name="vehicleType">Vehicle type to search. Ignore by set VehicleType.None</param>
        /// <param name="vehicleSubType">Vehicle sub type to search. Ignore by set VehicleSubType.None</param>
        //void ParkingSessionSearchAdvance(Action<CheckIn[], Exception> complete, DateTime fromTime, DateTime toTime, int limit = 0, string cardId = null, string cardLabel = null, string vehicleNumber = null, int vehicleTypeId = 0, VehicleSubType vehicleSubType = VehicleSubType.None);

        void ParkingSessionSearchAdvance(ParkingSession data, ParkingSessionEnum mode, Action<ParkingSession[], Exception> complete, int limit = 100);

        void ParkingSessionSearch(ParkingSession data, ParkingSessionEnum mode, int page, int pageSize, Action<SearchResult, Exception> complete);

        /// <summary>
        /// Get global config from server
        /// </summary>
        /// <param name="complete">Result callback</param>
        void GetGlobalConfig(int terminalId, string version, Action<GlobalConfig, Exception> complete);

        /// <summary>
        /// Get card's information
        /// </summary>
        /// <param name="complete">Result callback</param>
        void GetCardInfo(string cardId, Action<Card, Exception> complete);

        /// <summary>
        /// Send images replication command to server
        /// </summary>
        /// <param name="checkInInfo">Check in data to replicate images</param>
        /// <param name="complete">Result callback</param>
        void ReplicateImages(Models.CheckIn checkInInfo, Action<Exception> complete);

        /// <summary>
        /// Create a bulk of cards
        /// </summary>
        /// <param name="data">List of cards to create</param>
        /// <param name="complete">Result callback</param>
        void CreateCards(Models.Card[] data, Action<BulkCreateCardResult, Exception> complete);

        void CreateExceptionalCheckOut(string cardId, int terminalId, int laneId, int operatorId, string notes, bool isLocked, float parkingfee, Action<Exception> complete);

        void GetCardTypes(Action<List<CardType>, Exception> complete);

        void GetVehicleTypes(Action<List<VehicleType>, Exception> complete);

        void CrawlPage(string host, string endpoint, Action<IRestResponse, Exception> callback);

        void GetCards(Action<string, Exception> complete);

        void UpdateParkingSession(ParkingSession parkingSession, Action<Exception> complete);

        void GetLanes(Action<List<Lane>, Exception> complete);

        void GetStatistics(DateTime from, DateTime to, int terminalId, Action<Statistics, Exception> complete);

        void GetTerminalGroups(Action<Models.TerminalGroup[], Exception> complete);
        void GetServerTime(Action<ServerTimeInfo, Exception> complete);
        void GetFarCards(Action<List<FarCards>, Exception> complete);
        //void  GetRegions(Action<List<Region>, Exception> complete);
        void GetBlackList(Action<List<BlackNumber>, Exception> complete);

        void AddRegisteredCard(string cardId, string vehicleNumber, Action<object, Exception> completed);

        void CheckVehicleNumber(string vehicleNumber, string cardId, Action<CheckVehicleNumber, Exception> complete);

		void GetAvailableCards(Action<List<Models.AvailableCard>, Exception> complete);
        void CollectCard(string cardId, string note, Action<Exception> complete);
		void CollectPlate(string cardId, string vehicleNumber, Action<AvailableCard, Exception> complete);

        void RetailInvoice(Action<Exception> complete,long parking_id, long fee, bool completed = true, bool has_buyer = false, string buyer_code = null, 
            string buyer_name = null, string legal_name = null,
            string taxcode = null, string phone = null, string email = null, 
            string address = null, string receiver_name = null, string receiver_emails = null);
    }
}
