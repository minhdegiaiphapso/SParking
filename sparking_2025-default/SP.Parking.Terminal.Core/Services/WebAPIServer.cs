using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using SP.Parking.Terminal.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Core;
using System.Net;
using Serilog;
using SP.Parking.Terminal.Core.Models.Custom;
using System.Configuration;
using System.Windows.Markup;

namespace SP.Parking.Terminal.Core.Services
{
    /// <summary>
    /// Server using Web API
    /// </summary>
    public class WebAPIServer : IServer
    {
        public string DEFAULT_VEHICLE_NUMBER = "    ";
        public string DEFAULT_CHECK_IN = "";
        private IWebClient _restClient;
        private IOptionsSettings _optionSettings;
        private ILogger _logger;
        private IHostSettings _hostSettings;
        private RestClient _apiClient;
        private string _token = "";

        public WebAPIServer(IWebClient restClient, IOptionsSettings optionSettings, ILogger logger,
            IHostSettings hostSettings)
        {
            _restClient = restClient;
            _optionSettings = optionSettings;
            _logger = logger;
            _hostSettings = hostSettings;
            string apiUrl = ConfigurationManager.AppSettings["ApiUrl"];
            _logger.Information(apiUrl);
            _apiClient = new RestClient(apiUrl);
        }

        private void ExecuteAsync(RestRequest request, Action<IRestResponse> callback)
        {
            _restClient.ExecuteAsync(request, callback);
        }

        private void GetToken()
        {
            var restRequest = new RestRequest("/api/auth/authenticate", Method.POST);
			restRequest.AddParameter("userName", "admin");
			restRequest.AddParameter("password", "@sp142536");
			//restRequest.AddJsonBody(new { userName = "admin", password = "@sp142536" });
            var result = _apiClient.Execute<AuthResponse>(restRequest);
            if(result.StatusCode == HttpStatusCode.OK)
            {
                _token = result.Data.AccessToken;
            }
            else
            {
                throw new Exception(result.Content);
            }
        }

        public void CreateVoucher(Voucher data, Action<Exception> complete)
        {
            try
            {
                var request = new RestRequest(string.Format("/api/cards/{0}/voucher/", data.CardId), Method.POST);
                request.AddParameter("voucher_type", data.Voucher_Type);
                request.AddParameter("voucher_amount", data.Voucher_Amount);
                request.AddParameter("parking_fee", data.Parking_Fee);
                request.AddParameter("actual_fee", data.Actual_Fee);
                request.AddParameter("check_in_time", TimeZoneInfo.ConvertTimeToUtc(data.Check_In_Time, TimeZoneInfo.Local).ToString("yyyy-MM-dd HH:mm:ss")); //Get javascript format date
                ExecuteAsync(request, (response) =>
                {
                    if (complete == null) return;
                    complete(GetException(response));
                });
            }
            catch (Exception ex)
            {
                if (complete == null) return;
                complete(ex);
            }
        }
        public void DeleteVoucher(string Card_Id, DateTime Check_In_Time, Action<Exception> complete)
        {
            try
            {
                var request = new RestRequest(string.Format("/api/cards/{0}/deletevoucher/", Card_Id), Method.POST);
                request.AddParameter("check_in_time", TimeZoneInfo.ConvertTimeToUtc(Check_In_Time, TimeZoneInfo.Local).ToString("yyyy-MM-dd HH:mm:ss")); //Get javascript format date
                ExecuteAsync(request, (response) =>
                {
                    if (complete == null) return;
                    complete(GetException(response));
                });
            }
            catch (Exception ex)
            {
                if (complete == null) return;
                complete(ex);
            }
        }
        public void RecallFee(string cardID, int voucherhour, Action<int, Exception> complete)
        {
            if (complete == null) return;
            var request = new RestRequest(string.Format("/api/recallfee/"), Method.GET);
            request.AddParameter("card_id", cardID);
            request.AddParameter("voucher_hour", voucherhour);
            request.Timeout = 3 * 60 * 1000;
            ExecuteAsync(request, (response) =>
            {
                var res = -1;
                Exception exception = GetException(response);
                if (exception == null)
                {
                    res = Convert.ToInt32(response.Content);
                }
                else
                    res = -1;
                complete(res, exception);
            });

        }
        public void ForcedBarier(ForcedInfo data, Action<ForcedInfo, Exception> complete)
        {
            var request = new RestRequest("/api/forcedbarier/", Method.POST);
            //if (data.VehicleType != VehicleType.None)
            request.AddParameter("user", data.User);
            request.AddParameter("terminal", data.PCAddress);
            request.AddParameter("lane", data.Lane);
            request.AddParameter("note", data.Note);

            if (data.ReferenceFrontImage != null)
                request.AddFile("front_thumb", data.ReferenceFrontImage, "front_thumb");
            if (data.ReferenceBackImage != null)
                request.AddFile("back_thumb", data.ReferenceBackImage, "back_thumb");
            ExecuteAsync(request, (response) =>
            {
                if (complete == null) return;
                Exception exception = GetException(response);
                if (exception == null || exception is NotAcceptableException)
                {
                    ForcedInfo resModel = JsonConvert.DeserializeObject<ForcedInfo>(response.Content);
                    resModel.ReferenceBackImage = data.ReferenceBackImage;
                    resModel.ReferenceFrontImage = data.ReferenceFrontImage;
                    complete(resModel, exception);
                }
            });
        }
        public void Login(string username, string password, int laneID, Action<Models.ApmsUser, Exception> complete)
        {
            var request = new RestRequest("/api/users/login/", Method.POST);
            request.AddParameter("username", username);
            request.AddParameter("password", password);
            request.AddParameter("lane_id", laneID);
            ExecuteAsync(request, (response) => OnLoginResult(response, complete));
        }

        public void Login(string cardID, int laneID, Action<Models.ApmsUser, Exception> complete)
        {
            var request = new RestRequest("/api/users/login-by-card/", Method.POST);
            request.AddParameter("card_id", cardID);
            request.AddParameter("lane_id", laneID);
            ExecuteAsync(request, (response) => OnLoginResult(response, complete));
        }

        public void Logout(int shiftID, int userID, int laneID, Action<UserShift, Exception> complete, int revenue = 0)
        {
            var request = new RestRequest("/api/users/logout/", Method.POST);
            request.AddParameter("shift_id", shiftID);
            request.AddParameter("user_id", userID);
            request.AddParameter("lane_id", laneID);
            request.AddParameter("revenue", revenue);
            ExecuteAsync(request, (response) => OnLogoutResult(response, complete));
        }

        private void OnLoginResult(IRestResponse response, Action<Models.ApmsUser, Exception> complete)
        {
            if (response != null)
            {
                ApmsUser user = null;
                Exception exception = GetLoginException(response);
                if (exception == null)
                {
                    user = JsonConvert.DeserializeObject<ApmsUser>(response.Content);
                }
                if (complete != null)
                    complete(user, exception);
            }
        }

        private void OnLogoutResult(IRestResponse response, Action<Models.UserShift, Exception> complete)
        {
            if (response != null)
            {
                UserShift shift = null;
                Exception exception = GetLoginException(response);
                if (exception == null)
                {
                    shift = JsonConvert.DeserializeObject<UserShift>(response.Content);
                }
                if (complete != null)
                    complete(shift, exception);
            }
        }

        private Exception GetLoginException(IRestResponse response)
        {
            if (response.ResponseStatus == ResponseStatus.Completed)
            {
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.OK:
                        return null;
                    case System.Net.HttpStatusCode.BadRequest:
                        if (response.Content.Contains("Login fail"))
                            return new LoginInvalidException(response.Content);
                        else
                            return new ServerErrorException(response.Content);
                    default:
                        if (response.ErrorException != null)
                            return new ServerErrorException(response.ErrorException.Message);
                        else
                            return new ServerErrorException(response.Content);
                }
            }
            else
            {
                return new ServerDisconnectException(response.ErrorMessage);
            }
        }

        private Exception GetException(IRestResponse response)
        {
            if (response != null)
            {
                if (response.ResponseStatus == ResponseStatus.Completed)
                {
                    switch (response.StatusCode)
                    {
                        case System.Net.HttpStatusCode.OK:
                        case System.Net.HttpStatusCode.Created:
                            {
                                _logger.Information(response.Content);
                                return null;
                            }
                        case System.Net.HttpStatusCode.NotFound:
                            return new NotFoundException(response.Content);
                        case System.Net.HttpStatusCode.InternalServerError:
                            {
                                _logger.Error(response.ErrorException, response.Content);
                                if (response.ErrorException != null)
                                    return new InternalServerErrorException(response.ErrorException.Message);
                                else
                                    return new InternalServerErrorException(response.ToString());
                            }
                        case HttpStatusCode.NotAcceptable:
                            return new NotAcceptableException(response.Content);
                        default:
                            if (response.ErrorException != null)
                                return new ServerErrorException(response.ErrorException.Message);
                            else
                                return new ServerErrorException(response.Content);
                    }
                }
                else
                {
                    _logger.Error(response.ErrorException, response.Content);
                    return new ServerDisconnectException(response.ErrorMessage);
                }
            }
            else
            {
                return new ServerDisconnectException("Disconnect");
            }
        }
        public void FindAndNotifyBlacklist(int parking_id, string imgPath, string vehiclenumber, int gate, int user, int stateparking, Action<string, Exception> complete)
        {
            var request = new RestRequest(string.Format("/api/findandnotifyblacklist/"), Method.POST);
            request.AddParameter("parking_session_id", parking_id);
            request.AddParameter("image_path", imgPath);
            request.AddParameter("vehiclenumber", vehiclenumber);
            request.AddParameter("gate", gate);
            request.AddParameter("user", user);
            request.AddParameter("stateparking", stateparking);
            ExecuteAsync(request, (response) =>
            {
                if (complete == null) return;
                Exception exception = GetException(response);
                complete(response.Content, exception);
            });
        }
        public void CreateCheckIn(Models.CheckIn data, Action<Models.CheckIn, Exception> complete)
        {
            _logger.Information($"[{nameof(CreateCheckIn)}] Begin check in");
            var request = new RestRequest(string.Format("/api/cards/{0}/checkin/", data.CardId), Method.POST);
            request.AddParameter("terminal_id", data.TerminalId);
            request.AddParameter("lane_id", data.LaneId);
            request.AddParameter("operator_id", data.OperatorId);
            request.AddParameter("vehicle_type", data.VehicleTypeId);
            request.AddParameter("entry_check", data.EntryCheck);
            request.AddParameter("use_vehicle_type_from_card", _optionSettings.UseVehicleTypeFromCard);


            //request.AddParameter("prefix_vehicle_number", "51-N1");
            //request.AddParameter("vehicle_sub_type", (int)data.VehicleSubType);
            if (string.IsNullOrEmpty(data.PrefixNumberVehicle))
            {
                request.AddParameter("prefix_vehicle_number", DEFAULT_VEHICLE_NUMBER);
            }
            else
            {
                request.AddParameter("prefix_vehicle_number", data.PrefixNumberVehicle);
            }
            if (string.IsNullOrEmpty(data.VehicleNumber))
            {

                request.AddParameter("alpr_vehicle_number", DEFAULT_VEHICLE_NUMBER);
                request.AddParameter("vehicle_number", DEFAULT_VEHICLE_NUMBER);
            }
            else
            {

                request.AddParameter("vehicle_number", data.VehicleNumber);
                request.AddParameter("alpr_vehicle_number", string.Format("{0}-{1}", string.IsNullOrEmpty(data.PrefixNumberVehicle) ? "" : data.PrefixNumberVehicle, data.VehicleNumber));
            }
            //request.AddParameter("vehicle_number", data.VehicleNumber);
            //request.AddParameter("alpr_vehicle_number", data.AlprVehicleNumber);
            if (data.FrontImage != null)
                request.AddFile("front_thumb", data.FrontImage, "front_thumb");
            if (data.BackImage != null)
                request.AddFile("back_thumb", data.BackImage, "back_thumb");
            if (data.Extra1Image != null)
            {
                request.AddFile("extra1_thumb", data.Extra1Image, "extra1_thumb");
            }
            if (data.Extra2Image != null)
            {
                request.AddFile("extra2_thumb", data.Extra2Image, "extra2_thumb");
            }

            ExecuteAsync(request, (response) =>
            {
                if (complete == null) return;
                CheckIn model = null;
                Exception exception = GetException(response);

                _logger.Information($"[{nameof(CreateCheckIn)}] End check in");
                if (exception == null || exception is NotAcceptableException)
                {
                    model = data;
                    CheckIn resModel = JsonConvert.DeserializeObject<CheckIn>(response.Content);

                    model.CardLabel = resModel.CardLabel;
                    model.FrontImagePath = resModel.FrontImagePath;
                    model.BackImagePath = resModel.BackImagePath;
                    if (!string.IsNullOrEmpty(resModel.Extra1ImagePath))
                        model.Extra1ImagePath = resModel.Extra1ImagePath;
                    if (!string.IsNullOrEmpty(resModel.Extra2ImagePath))
                        model.Extra2ImagePath = resModel.Extra2ImagePath;
                    model.VehicleNumberExists = resModel.VehicleNumberExists;
                    model.LimitNumSlots = resModel.LimitNumSlots;
                    model.CurrentNumSlots = resModel.CurrentNumSlots;
                    model.CheckInTimestamp = resModel.CheckInTimestamp;
                    model.CustomerInfo = resModel.CustomerInfo;
                    model.CardTypeId = resModel.CardTypeId;
                    model.EntryCount = resModel.EntryCount;
                    model.Entries = resModel.Entries;
                    model.ParkingSessionId = resModel.ParkingSessionId;

                    if (_optionSettings.UseVehicleTypeFromCard)
                    {
                        model.VehicleTypeId = model.CustomerInfo.VehicleTypeFromCard;
                    }
                }

                complete(model, exception);

            });
        }

        public void AddRegisteredCard(string cardId, string vehicleNumber, Action<object, Exception> completed)
        {
            Execute("/api/customer/add-registered-card", Method.POST, new { cardId = cardId, vehicleNumber = vehicleNumber }, completed);
        }

        private void Execute(string aiUrl, Method method, object data, Action<object, Exception> completed)
        {
            if (string.IsNullOrEmpty(_token))
            {
                GetToken();
            }

            RestRequest restRequest = CreateRequest(aiUrl, method, data);
            var result = _apiClient.Execute<ApiResponseModel<object>>(restRequest);
            if (result.StatusCode == HttpStatusCode.OK)
            {
                if (completed != null)
                {
                    completed(result.Data, result.ErrorException);
                }
            }
            else if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                GetToken();
                restRequest = CreateRequest(aiUrl, method, data);
                result = _apiClient.Execute<ApiResponseModel<object>>(restRequest);
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    if (completed != null)
                    {
                        completed(result.Data, result.ErrorException);
                    }
                }
            }

            if (result.StatusCode != HttpStatusCode.OK)
            {
                _logger.Error(result.ErrorException, $"Call api failed: {result.Content}");
            }
        }

        private RestRequest CreateRequest(string apiUrl, Method method, object data)
        {
            var restRequest = new RestRequest(apiUrl, method);
			restRequest.AddBody( data);
			//restRequest.AddJsonBody(data);
            restRequest.AddHeader("Authorization", $"Bearer {_token}");
            _logger.Information($"[{nameof(CreateRequest)}] {JsonConvert.SerializeObject(data)}");
            return restRequest;
        }

        public void UpdateCheckIn(Models.CheckIn data, Action<Models.CheckIn, Exception> complete)
        {
            var request = new RestRequest(string.Format("/api/cards/{0}/checkin/", data.CardId), Method.PUT);
            //if (data.VehicleType != VehicleType.None)    
            request.AddParameter("vehicle_type", data.VehicleTypeId);
            request.AddParameter("terminal_id", data.TerminalId);
            request.AddParameter("lane_id", data.LaneId);
            request.AddParameter("operator_id", data.OperatorId);
            request.AddParameter("vehicle_type", data.VehicleTypeId);
            request.AddParameter("entry_check", data.EntryCheck);
            request.AddParameter("use_vehicle_type_from_card", _optionSettings.UseVehicleTypeFromCard);

            if (string.IsNullOrEmpty(data.PrefixNumberVehicle))
            {
                request.AddParameter("prefix_vehicle_number", DEFAULT_VEHICLE_NUMBER);
            }
            else
            {
                request.AddParameter("prefix_vehicle_number", data.PrefixNumberVehicle);
            }

            if (string.IsNullOrEmpty(data.VehicleNumber))
            {
                request.AddParameter("alpr_vehicle_number", DEFAULT_VEHICLE_NUMBER);
                request.AddParameter("vehicle_number", DEFAULT_VEHICLE_NUMBER);
            }
            else
            {
                request.AddParameter("vehicle_number", data.VehicleNumber);
                request.AddParameter("alpr_vehicle_number", string.Format("{0}-{1}", string.IsNullOrEmpty(data.PrefixNumberVehicle) ? "" : data.PrefixNumberVehicle, data.VehicleNumber));
            }
            if (data.FrontImage != null)
                request.AddFile("front_thumb", data.FrontImage, "front_thumb");
            if (data.BackImage != null)
                request.AddFile("back_thumb", data.BackImage, "back_thumb");
            if (data.Extra1Image != null)
                request.AddFile("extra1_thumb", data.Extra1Image, "extra1_thumb");
            if (data.Extra2Image != null)
                request.AddFile("extra2_thumb", data.Extra2Image, "extra2_thumb");

            ExecuteAsync(request, (response) =>
            {
                if (complete == null) return;
                CheckIn model = null;
                Exception exception = GetException(response);
                if (exception == null)
                {
                    model = data;
                    CheckIn resModel = JsonConvert.DeserializeObject<CheckIn>(response.Content);
                    model.CardLabel = resModel.CardLabel;
                    model.FrontImagePath = resModel.FrontImagePath;
                    model.BackImagePath = resModel.BackImagePath;
                    if (!string.IsNullOrEmpty(resModel.Extra1ImagePath))
                        model.Extra1ImagePath = resModel.Extra1ImagePath;
                    if (!string.IsNullOrEmpty(resModel.Extra2ImagePath))
                        model.Extra2ImagePath = resModel.Extra2ImagePath;
                    model.VehicleNumberExists = resModel.VehicleNumberExists;
                    //model.LimitNumSlots = resModel.LimitNumSlots;
                    //model.CurrentNumSlots = resModel.CurrentNumSlots;
                    model.CheckInTimestamp = resModel.CheckInTimestamp;
                    //model.EntryCount = resModel.EntryCount;
                    //model.Entries = resModel.Entries;
                    model.ParkingSessionId = resModel.ParkingSessionId;

                    if (_optionSettings.UseVehicleTypeFromCard && model.CustomerInfo != null)
                    {
                        model.VehicleTypeId = model.CustomerInfo.VehicleTypeFromCard;
                    }
                }
                complete(model, exception);
            });
        }

        public void GetCheckIn(string cardID, Action<Models.CheckIn, Exception> complete)
        {
            if (complete == null) return;
            var request = new RestRequest(string.Format("/api/cards/{0}/checkin/", cardID), Method.GET);
            ExecuteAsync(request, (response) =>
            {
                CheckIn model = null;
                Exception exception = GetException(response);
                if (exception == null)
                {
                    model = JsonConvert.DeserializeObject<CheckIn>(response.Content);
                }
                complete(model, exception);
            });

        }

        public void CreateCheckOut(Models.CheckOut data, CustomerInfo customerInfo, Action<Exception> complete)
        {
            try
            {
                var request = new RestRequest(string.Format("/api/cards/{0}/checkout/", data.CardId), Method.POST);
                request.AddParameter("terminal_id", data.TerminalId);
                request.AddParameter("lane_id", data.LaneId);
                request.AddParameter("operator_id", data.OperatorId);
                request.AddParameter("check_out_time", TimeZoneInfo.ConvertTimeToUtc(data.CheckOutTime, TimeZoneInfo.Local).ToString("o")); //Get javascript format date
                request.AddParameter("parking_fee", string.IsNullOrEmpty(data.is_cancel) ? (long)customerInfo.ParkingFee : 0);
                //request.AddParameter("parking_fee", customerInfo.ParkingFee);
                request.AddParameter("parking_fee_details", customerInfo.ParkingFeeDetail);
                request.AddParameter("alpr_vehicle_number", string.IsNullOrEmpty(data.AlprVehicleNumber) ? DEFAULT_VEHICLE_NUMBER : data.AlprVehicleNumber);
                request.AddFile("front_thumb", data.FrontImage, "front_thumb");
                request.AddFile("back_thumb", data.BackImage, "back_thumb");
                if (data.Extra1Image != null)
                {
                    request.AddFile("extra1_thumb", data.Extra1Image, "extra1_thumb");
                }
                if (data.Extra2Image != null)
                {
                    request.AddFile("extra2_thumb", data.Extra1Image, "extra2_thumb");
                }
                request.AddParameter("is_cancel", data.is_cancel == null ? " " : data.is_cancel);
                //request.AddParameter("is_cancel", string.IsNullOrEmpty(data.is_cancel) ? DEFAULT_CHECK_IN : data.is_cancel);

                ExecuteAsync(request, (response) =>
                {
                    if (complete == null) return;
                    Exception exception = GetException(response);
                    if (exception == null)
                    {
                        CheckOut model = JsonConvert.DeserializeObject<CheckOut>(response.Content);
                        data.BackImagePath = model.BackImagePath;
                        data.FrontImagePath = model.FrontImagePath;
                        if (!string.IsNullOrEmpty(model.Extra1ImagePath))
                            data.Extra1ImagePath = model.Extra1ImagePath;
                        if (!string.IsNullOrEmpty(model.Extra2ImagePath))
                            data.Extra2ImagePath = model.Extra2ImagePath;
                    }
                    complete(GetException(response));
                });
            }
            catch (Exception ex)
            {
                if (complete == null) return;
                complete(ex);
            }
        }
        public void GetTerminals(Action<Models.Terminal[], Exception> complete)
        {
            var request = new RestRequest("/api/terminals/", Method.GET);
            ExecuteAsync(request, (response) => OnTerminalResponse(response, complete));
        }

        public void CreateTerminal(Models.Terminal data, Action<Models.Terminal, Exception> complete)
        {
            var request = new RestRequest("/api/terminals/", Method.POST);
            request.AddParameter("terminal_id", data.TerminalId);
            request.AddParameter("name", data.Name);
            request.AddParameter("status", (int)data.Status);
            ExecuteAsync(request, (response) => OnTerminalResponse(response, complete));
        }

        public void UpdateTerminal(Models.Terminal data, Action<Models.Terminal, Exception> complete)
        {
            var request = new RestRequest(string.Format("/api/terminals/{0}", data.Id), Method.PUT);
            request.AddParameter("name", data.Name);
            request.AddParameter("status", (int)data.Status);
            ExecuteAsync(request, (response) => OnTerminalResponse(response, complete));
        }

        private void OnTerminalResponse(IRestResponse response, Action<Models.Terminal[], Exception> complete)
        {
            if (complete == null) return;
            Models.Terminal[] model = null;
            Exception exception = GetException(response);
            if (exception == null)
            {
                model = JsonConvert.DeserializeObject<Models.Terminal[]>(response.Content);
            }
            complete(model, exception);
        }

        private void OnTerminalResponse(IRestResponse response, Action<Models.Terminal, Exception> complete)
        {
            if (complete == null) return;
            Models.Terminal model = null;
            Exception exception = GetException(response);
            if (exception == null)
            {
                model = JsonConvert.DeserializeObject<Models.Terminal>(response.Content);
            }
            complete(model, exception);
        }

        public void CreateLane(Models.Lane data, Action<Models.Lane, Exception> complete)
        {
            var request = new RestRequest("/api/lanes/", Method.POST);
            request.AddParameter("name", data.Name);
            request.AddParameter("vehicle_type", data.VehicleTypeId);
            request.AddParameter("direction", (int)data.Direction);
            request.AddParameter("enabled", data.Enabled);
            request.AddParameter("terminal_id", data.TerminalId);
            ExecuteAsync(request, (response) => OnLaneResponse(response, complete));
        }

        public void UpdateLane(Models.Lane data, Action<Models.Lane, Exception> complete)
        {
            var request = new RestRequest(string.Format("/api/lanes/{0}", data.Id), Method.PUT);
            request.AddParameter("name", data.Name);
            request.AddParameter("vehicle_type", data.VehicleTypeId);
            request.AddParameter("direction", (int)data.Direction);
            request.AddParameter("enabled", data.Enabled);
            request.AddParameter("terminal_id", data.TerminalId);
            ExecuteAsync(request, (response) => OnLaneResponse(response, complete));
        }

        private void OnLaneResponse(IRestResponse response, Action<Lane, Exception> complete)
        {
            if (complete == null) return;
            Lane model = null;
            Exception exception = GetException(response);
            if (exception == null)
            {
                model = JsonConvert.DeserializeObject<Lane>(response.Content);
            }
            complete(model, exception);
        }

        public void CreateLane(Models.Lane[] data, Action<Models.Lane[], Exception> complete)
        {
            var request = new RestRequest("/api/lanes/", Method.POST);
            Dictionary<string, object>[] request_lst = new Dictionary<string, object>[data.Length];
            int idx = 0;
            foreach (Models.Lane lane in data)
            {
                if (lane == null) continue;
                Dictionary<string, object> request_obj = new Dictionary<string, object>();
                request_obj["name"] = lane.Name;
                request_obj["vehicle_type"] = lane.VehicleTypeId;
                request_obj["direction"] = (int)lane.Direction;
                request_obj["enabled"] = lane.Enabled;
                request_obj["terminal_id"] = lane.TerminalId;
                request_lst[idx] = request_obj;
                idx++;
            }
            request.AddParameter("bulk", JsonConvert.SerializeObject(request_lst));
            ExecuteAsync(request, (response) => OnLanesResponse(response, complete));
        }

        public void GetLanes(Action<List<Lane>, Exception> complete)
        {
            var request = new RestRequest("/api/lanes/", Method.GET);
            ExecuteAsync(request, response =>
            {
                List<Lane> rs = null;
                Exception exception = GetException(response);
                if (exception == null)
                {
                    rs = JsonConvert.DeserializeObject<List<Lane>>(response.Content);
                }
                if (complete != null) complete(rs, exception);
            });
        }

        private void OnLanesResponse(IRestResponse response, Action<Lane[], Exception> complete)
        {
            if (complete == null) return;
            Lane[] rs = null;
            Exception exception = GetException(response);
            if (exception == null)
            {
                rs = JsonConvert.DeserializeObject<Lane[]>(response.Content);
            }
            complete(rs, exception);
        }

        public void CreateCamera(Models.Camera data, Action<Models.Camera, Exception> complete)
        {
            var request = new RestRequest("/api/cameras/", Method.POST);
            request.AddParameter("name", data.Name);
            request.AddParameter("ip", data.IP);
            request.AddParameter("direction", (int)data.Direction);
            request.AddParameter("position", (int)data.Position);
            request.AddParameter("serial_number", data.SerialNumber);
            request.AddParameter("lane_id", data.LaneId);
            ExecuteAsync(request, (response) => OnCameraResponse(response, complete));
        }

        public void UpdateCamera(Models.Camera data, Action<Models.Camera, Exception> complete)
        {
            var request = new RestRequest(string.Format("/api/cameras/{0}", data.Id), Method.PUT);
            request.AddParameter("name", data.Name);
            request.AddParameter("ip", data.IP);
            request.AddParameter("direction", (int)data.Direction);
            request.AddParameter("position", (int)data.Position);
            request.AddParameter("serial_number", data.SerialNumber);
            request.AddParameter("lane_id", data.LaneId);
            ExecuteAsync(request, (response) => OnCameraResponse(response, complete));
        }

        private void OnCameraResponse(IRestResponse response, Action<Camera, Exception> complete)
        {
            if (complete == null) return;
            Camera model = null;
            Exception exception = GetException(response);
            if (exception == null)
            {
                model = JsonConvert.DeserializeObject<Camera>(response.Content);
            }
            complete(model, exception);
        }

        public void CheckHealthServer(string ip, Action<Exception> complete)
        {
            if (complete == null) return;
            var request = new RestRequest("/api/health/", Method.GET);
            _restClient.ExecuteAsync(ip, request, (response) => OnCheckHealthResponse(response, complete));
        }

        private void OnCheckHealthResponse(IRestResponse response, Action<Exception> complete)
        {
            complete(GetException(response));
        }

        public void ParkingSessionSearchBasic(string cardId, Action<CheckIn[], Exception> complete)
        {
            if (complete == null) return;
            var request = new RestRequest("/api/parking-sessions/", Method.GET);
            request.AddParameter("card_id", cardId);
            ExecuteAsync(request, (response) => OnSearchResponse(response, complete));
        }

        public void ParkingSessionSearchAdvance(ParkingSession data, ParkingSessionEnum mode, Action<ParkingSession[], Exception> complete, int limit = 100)
        {
            var request = new RestRequest("/api/parking-sessions/", Method.GET);
            request.AddParameter("from_time", TimestampConverter.DateTime2Timestamp(data.StartDate));
            request.AddParameter("to_time", TimestampConverter.DateTime2Timestamp(data.EndDate));
            //if (limit > 0)
            request.AddParameter("limit", limit);
            if (!string.IsNullOrEmpty(data.CardLabel))
                request.AddParameter("card_label", data.CardLabel.Trim());
            if (!string.IsNullOrEmpty(data.CardId))
                request.AddParameter("card_id", data.CardId);
            if (!string.IsNullOrEmpty(data.VehicleNumber))
                request.AddParameter("vehicle_number", data.VehicleNumber.Trim());

            request.AddParameter("mode", (int)mode);
            request.AddParameter("vehicle_type", data.VehicleType.Id);

            ExecuteAsync(request, (response) => OnSearchResponse(response, complete));
        }

        public void ParkingSessionSearch(ParkingSession data, ParkingSessionEnum mode, int page, int pageSize, Action<SearchResult, Exception> complete)
        {
            var request = new RestRequest("/api/parking-sessions/search/", Method.GET);
            request.AddParameter("from_time", TimestampConverter.DateTime2Timestamp(data.StartDate));
            request.AddParameter("to_time", TimestampConverter.DateTime2Timestamp(data.EndDate));
            if (!string.IsNullOrEmpty(data.CardLabel))
                request.AddParameter("card_label", data.CardLabel.Trim());
            if (!string.IsNullOrEmpty(data.CardId))
                request.AddParameter("card_id", data.CardId);
            if (!string.IsNullOrEmpty(data.VehicleNumber))
                request.AddParameter("vehicle_number", data.VehicleNumber.Trim());

            if (data.CurrentUserId.HasValue)
            {
                request.AddParameter("operator_id", data.CurrentUserId);
            }

            request.AddParameter("page", page);
            request.AddParameter("page_size", pageSize);
            request.AddParameter("mode", (int)mode);
            if (data.TerminalGroup != null)
                request.AddParameter("terminal_group", data.TerminalGroup.Id);
            request.AddParameter("vehicle_type", data.VehicleType.Id);
            request.Timeout = 10 * 60 * 1000;
            ExecuteAsync(request, (response) => OnSearchResponse(response, complete));
        }

        private void OnSearchResponse(IRestResponse response, Action<SearchResult, Exception> complete)
        {
            SearchResult rs = null;
            Exception exception = GetException(response);
            if (exception == null)
            {
                try
                {
                    List<ParkingSession> lst = JsonConvert.DeserializeObject<List<ParkingSession>>(response.Content);
                    int total = 0;
                    if (lst != null && lst.Count > 0)
                        total = lst[0].Total;
                    rs = new SearchResult() { Total = total, ParkingSessions = lst };
                }
                catch
                {
                    rs = JsonConvert.DeserializeObject<SearchResult>(response.Content);
                }
            }
            complete(rs, exception);
        }

        private void OnSearchResponse(IRestResponse response, Action<ParkingSession[], Exception> complete)
        {
            ParkingSession[] rs = null;
            Exception exception = GetException(response);
            if (exception == null)
            {
                rs = JsonConvert.DeserializeObject<ParkingSession[]>(response.Content);
            }
            complete(rs, exception);
        }

        //public void ParkingSessionSearchAdvance(Action<CheckIn[], Exception> complete, DateTime fromTime, DateTime toTime, int limit = 0, string cardId = null, string cardLabel = null, string vehicleNumber = null, int vehicleTypeId = 0, VehicleSubType vehicleSubType = VehicleSubType.None)
        //{
        //    if (complete == null) return;
        //    var request = new RestRequest("/api/parking-sessions/", Method.GET);
        //    request.AddParameter("from_time", TimestampConverter.DateTime2Timestamp(fromTime));
        //    request.AddParameter("to_time", TimestampConverter.DateTime2Timestamp(toTime));
        //    if (limit > 0)
        //        request.AddParameter("limit", limit);
        //    if (cardLabel != null)
        //        request.AddParameter("card_label", cardLabel);
        //    if (cardId != null)
        //        request.AddParameter("card_id", cardId);
        //    if (vehicleNumber != null)
        //        request.AddParameter("vehicle_number", vehicleNumber);
        //    //if (vehicleType != VehicleType.None)
        //    request.AddParameter("vehicle_type", vehicleTypeId);
        //    if (vehicleSubType != VehicleSubType.None)
        //        request.AddParameter("vehicle_sub_type", (int)vehicleSubType);
        //    ExecuteAsync(request, (response) => OnSearchResponse(response, complete));
        //}

        private void OnSearchResponse(IRestResponse response, Action<CheckIn[], Exception> complete)
        {
            CheckIn[] rs = null;
            Exception exception = GetException(response);
            if (exception == null)
            {
                rs = JsonConvert.DeserializeObject<CheckIn[]>(response.Content);
            }
            complete(rs, exception);
        }

        public void GetGlobalConfig(int terminalId, string version, Action<GlobalConfig, Exception> complete)
        {
            if (complete == null) return;
            var request = new RestRequest("/api/global-config/", Method.GET);
            request.AddParameter("terminal_id", terminalId);
            request.AddParameter("version", version);
            ExecuteAsync(request, (response) => OnGetGlobalConfigReceived(response, complete));
        }

        private void OnGetGlobalConfigReceived(IRestResponse response, Action<GlobalConfig, Exception> complete)
        {
            GlobalConfig rs = null;
            Exception exception = GetException(response);
            if (exception == null)
            {
                rs = JsonConvert.DeserializeObject<GlobalConfig>(response.Content);
            }
            complete(rs, exception);
        }

        public void GetCardInfo(string cardId, Action<Card, Exception> complete)
        {
            if (complete == null) return;
            var request = new RestRequest(string.Format("/api/cards/{0}", cardId), Method.GET);
            ExecuteAsync(request, (response) => OnCardInfoReceived(response, complete));
        }

        private void OnCardInfoReceived(IRestResponse response, Action<Card, Exception> complete)
        {
            Card rs = null;
            Exception exception = GetException(response);
            if (exception == null)
            {
                rs = JsonConvert.DeserializeObject<Card>(response.Content);
            }
            complete(rs, exception);
        }

        public void ReplicateImages(Models.CheckIn checkInInfo, Action<Exception> complete)
        {
            var request = new RestRequest(string.Format("/api/image-replication/"), Method.POST);
            request.AddParameter("card_id", checkInInfo.CardId);
            request.AddParameter("front_image", checkInInfo.FrontImagePath);
            request.AddParameter("back_image", checkInInfo.BackImagePath);
            if (!string.IsNullOrEmpty(checkInInfo.Extra1ImagePath))
                request.AddParameter("extra1_image", checkInInfo.Extra1ImagePath);
            if (!string.IsNullOrEmpty(checkInInfo.Extra2ImagePath))
                request.AddParameter("extra2_image", checkInInfo.Extra2ImagePath);
            ExecuteAsync(request, (response) =>
            {
                if (complete != null) complete(GetException(response));
            });
        }

        public void CreateCards(Models.Card[] data, Action<Models.BulkCreateCardResult, Exception> complete)
        {
            List<Dictionary<string, object>> request_lst = new List<Dictionary<string, object>>();
            foreach (Models.Card item in data)
            {
                if (item == null) continue;
                Dictionary<string, object> request_obj = new Dictionary<string, object>();
                request_obj["card_id"] = item.Id;

                request_obj["card_label"] = item.Label.Trim();
                request_obj["status"] = item.Status;
                request_obj["vehicle_type"] = item.VehicleTypeId;
                request_obj["card_type"] = item.CardType.Id;
                request_lst.Add(request_obj);
            }
            var request = new RestRequest(string.Format("/api/cards/"), Method.POST);
            request.AddParameter("bulk", JsonConvert.SerializeObject(request_lst));
            ExecuteAsync(request, (response) =>
            {
                Models.BulkCreateCardResult rs = null;
                Exception exception = GetException(response);
                if (exception == null)
                {
                    rs = JsonConvert.DeserializeObject<BulkCreateCardResult>(response.Content);
                }
                if (complete != null) complete(rs, exception);
            });
        }

        public void GetCards(Action<string, Exception> complete)
        {
            var request = new RestRequest("/api/cards/", Method.GET);
            ExecuteAsync(request, response =>
            {
                if (complete != null) complete(response.Content, GetException(response));
            });
        }

        public void UpdateParkingSession(ParkingSession parkingSession, Action<Exception> complete)
        {
            var request = new RestRequest(string.Format("/api/parking-sessions/{0}/", parkingSession.Id), Method.PUT);
            request.AddParameter("vehicle_number", parkingSession.VehicleNumber);
            ExecuteAsync(request, (response) =>
            {
                if (complete != null) complete(GetException(response));
            });
        }

        public void CreateExceptionalCheckOut(string cardId, int terminalId, int laneId, int operatorId, string notes, bool isLocked, float parkingFee, Action<Exception> complete)
        {
            //POST 
            var request = new RestRequest(string.Format("/api/cards/{0}/exception-checkout/", cardId), Method.POST);
            request.AddParameter("terminal_id", terminalId);
            request.AddParameter("lane_id", laneId);
            request.AddParameter("operator_id", operatorId);
            request.AddParameter("notes", notes);
            request.AddParameter("parkingfee", (long)parkingFee);
            request.AddParameter("lock_card", isLocked ? 1 : 0);

            ExecuteAsync(request, (response) =>
            {

                if (complete != null) complete(GetException(response));
            });
        }

        public void GetCardTypes(Action<List<CardType>, Exception> complete)
        {
            var request = new RestRequest("/api/card-types/", Method.GET);
            _restClient.ExecuteAsync(request, (response) =>
            {

                List<CardType> rs = null;
                Exception exception = GetException(response);
                if (exception == null)
                {
                    rs = JsonConvert.DeserializeObject<List<CardType>>(response.Content);
                }
                if (complete != null) complete(rs, exception);
            });
        }

        public void GetVehicleTypes(Action<List<VehicleType>, Exception> complete)
        {
            var request = new RestRequest("/api/vehicle-types/", Method.GET);
            _restClient.ExecuteAsync(request, (response) =>
            {

                List<VehicleType> rs = null;
                Exception exception = GetException(response);
                if (exception == null)
                {
                    rs = JsonConvert.DeserializeObject<List<VehicleType>>(response.Content);
                }
                if (complete != null) complete(rs, exception);
            });
        }

        public void CrawlPage(string host, string endpoint, Action<IRestResponse, Exception> callback)
        {
            var request = new RestRequest(endpoint, Method.GET);
            _restClient.ExecuteAsync(host, request, response =>
            {
                //var exception = GetException(response);
                if (callback != null)
                    callback(response, null);
            });
        }

        public void GetStatistics(DateTime from, DateTime to, int terminalId, Action<Statistics, Exception> complete)
        {
            var request = new RestRequest("/api/statistics/", Method.GET);
            request.AddParameter("time_from", TimestampConverter.DateTime2Timestamp(from));
            request.AddParameter("time_to", TimestampConverter.DateTime2Timestamp(to));
            request.AddParameter("terminal_id", terminalId);
            _restClient.ExecuteAsync(request, (response) =>
            {

                Statistics rs = null;
                Exception exception = GetException(response);
                if (exception == null)
                {
                    rs = JsonConvert.DeserializeObject<Statistics>(response.Content);
                }
                if (complete != null) complete(rs, exception);
            });
        }

        public void GetTerminalGroups(Action<Models.TerminalGroup[], Exception> complete)
        {
            var request = new RestRequest("/api/terminal-groups/", Method.GET);
            ExecuteAsync(request, (response) => OnTerminalGroupResponse(response, complete));
        }

        private void OnTerminalGroupResponse(IRestResponse response, Action<Models.TerminalGroup[], Exception> complete)
        {
            if (complete == null) return;
            Models.TerminalGroup[] model = null;
            Exception exception = GetException(response);
            if (exception == null)
            {
                model = JsonConvert.DeserializeObject<Models.TerminalGroup[]>(response.Content);
            }
            complete(model, exception);
        }
        public void GetServerTime(Action<ServerTimeInfo, Exception> complete)
        {
            var request = new RestRequest("/api/time-info/", Method.GET);
            _restClient.ExecuteAsync(request, (response) =>
            {

                ServerTimeInfo rs = null;
                Exception exception = GetException(response);
                if (exception == null)
                {
                    rs = JsonConvert.DeserializeObject<ServerTimeInfo>(response.Content);
                    var localtime = new DateTime(rs.LocalTime.Year, rs.LocalTime.Month, rs.LocalTime.Day, rs.LocalTime.Hour, rs.LocalTime.Minute, rs.LocalTime.Second, DateTimeKind.Local);
                    rs.LocalTime = localtime;
                }
                if (complete != null) complete(rs, exception);
            });
        }
        public void GetFarCards(Action<List<FarCards>, Exception> complete)
        {
            var request = new RestRequest("/api/farcards/", Method.GET);
            _restClient.ExecuteAsync(request, (response) =>
            {

                List<FarCards> rs = null;
                Exception exception = GetException(response);
                if (exception == null)
                {
                    List<FarCards> res = JsonConvert.DeserializeObject<List<FarCards>>(response.Content);
                    if (res != null && res.Count > 0)
                        rs = res;
                }
                if (complete != null) complete(rs, exception);
            });
        }
        //public void GetRegions(Action<List<Region>, Exception> complete)
        //{
        //    var request = new RestRequest("/api/regions/", Method.GET);
        //    _restClient.ExecuteAsync(request, (response) =>
        //    {

        //        List<Region> rs = null;
        //        Exception exception = GetException(response);
        //        if (exception == null)
        //        {
        //            List<Region> res = JsonConvert.DeserializeObject<List<Region>>(response.Content);
        //            if (res != null && res.Count > 0)
        //                rs = res;
        //        }
        //        if (complete != null) complete(rs, exception);
        //    });
        //}
        public void GetBlackList(Action<List<BlackNumber>, Exception> complete)
        {
            var request = new RestRequest("/api/blacklist/", Method.GET);
            _restClient.ExecuteAsync(request, (response) =>
            {
                List<BlackNumber> rs = null;
                Exception exception = GetException(response);
                if (exception == null)
                {
                    List<BlackNumber> res = JsonConvert.DeserializeObject<List<BlackNumber>>(response.Content);
                    if (res != null && res.Count > 0)
                        rs = res;
                }
                if (complete != null) complete(rs, exception);
            });
        }

        public void CheckVehicleNumber(string vehicleNumber, string cardId, Action<CheckVehicleNumber, Exception> complete)
        {
            var request = new RestRequest("/api/check-vehiclenumber/", Method.POST);
            //if (data.VehicleType != VehicleType.None)
            request.AddParameter("vehicle_number", string.IsNullOrEmpty(vehicleNumber) ? "-1": vehicleNumber.Trim());

            ExecuteAsync(request, (response) =>
            {
                if (complete == null) return;
                Exception exception = GetException(response);
                if (exception == null || exception is NotAcceptableException)
                {
                    CheckVehicleNumber result = JsonConvert.DeserializeObject<CheckVehicleNumber>(response.Content);
                    complete(result, exception);
                }
            });
        }
		public void GetAvailableCards(Action<List<AvailableCard>, Exception> complete)
		{
			var request = new RestRequest("/api/cards/available/", Method.GET);
			ExecuteAsync(request, response =>
			{
				List<AvailableCard> rs = null;
				Exception exception = GetException(response);
				if (exception == null)
				{
					rs = JsonConvert.DeserializeObject<List<AvailableCard>>(response.Content);
				}
				if (complete != null) complete(rs, exception);
			});
		}
		public void CollectCard(string cardId, string note, Action<Exception> complete)
        {
			var request = new RestRequest("/api/cards/collect/", Method.POST);
			request.AddParameter("card_id", cardId);
			request.AddParameter("note", note);
			ExecuteAsync(request, response =>
			{
				Exception exception = GetException(response);
				
				if (complete != null) complete(exception);
			});
		}
		public void CollectPlate(string cardId, string vehicleNumber, Action<AvailableCard, Exception> complete)
		{
			var request = new RestRequest("/api/cards/plate-collect/", Method.POST);
			request.AddParameter("card_id", cardId);
			request.AddParameter("note", vehicleNumber);
			ExecuteAsync(request, response =>
			{
				AvailableCard rs = null;
				Exception exception = GetException(response);
				if (exception == null)
				{
					rs = JsonConvert.DeserializeObject<AvailableCard>(response.Content);
				}
				if (complete != null) complete(rs, exception);
			});
		}
        public void  RetailInvoice(Action<Exception> complete, long parking_id, long fee, bool completed = true, 
            bool has_buyer = false, string buyer_code = null,
			string buyer_name = null, string legal_name = null,
			string taxcode = null, string phone = null, string email = null,
			string address = null, string receiver_name = null, string receiver_emails = null)
        {
			var request = new RestRequest("/api/mobile/retail-invoice/", Method.POST);
			request.AddParameter("parking_id", parking_id);
			request.AddParameter("fee", fee);
			request.AddParameter("completed", completed);
			request.AddParameter("has_buyer", has_buyer);
            if (has_buyer) { 
                if(!string.IsNullOrEmpty(buyer_code))
					request.AddParameter("buyer_code", buyer_code);
				if (!string.IsNullOrEmpty(buyer_name))
					request.AddParameter("buyer_name", buyer_name);
				if (!string.IsNullOrEmpty(legal_name))
					request.AddParameter("legal_name", legal_name);
				if (!string.IsNullOrEmpty(taxcode))
					request.AddParameter("taxcode", taxcode);
				if (!string.IsNullOrEmpty(phone))
					request.AddParameter("phone", phone);
				if (!string.IsNullOrEmpty(email))
					request.AddParameter("email", email);
				if (!string.IsNullOrEmpty(address))
					request.AddParameter("address", address);
				if (!string.IsNullOrEmpty(receiver_name))
					request.AddParameter("receiver_name", receiver_name);
				if (!string.IsNullOrEmpty(receiver_emails))
					request.AddParameter("receiver_emails", receiver_emails);
			}
			ExecuteAsync(request, response =>
			{
				Exception exception = GetException(response);
				if (complete != null) complete(exception);
			});
		}
	}
}
