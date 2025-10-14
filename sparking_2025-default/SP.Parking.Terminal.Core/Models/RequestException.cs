using Newtonsoft.Json;
using NLog;
using SP.Parking.Terminal.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Models
{
    public enum RequestExceptionEnum
    {
        CardIsInUse = 0,
        CardIsNotInUse,
        CardIsDisable,
        CardIsLocked,
        BlacklistPlateDetected,
        
        CheckoutSomethingWrong,
        CheckInSomethingWrong,
        MissingField,
        Undefine, 
    }

    public class RequestException
    {
        public string Detail { get; set; }
    }

    public static class RequestExceptionManager
    {
        public static string CARD_IS_IN_USE = "Card is in use";
        public static string CARD_IS_NOT_CHECKED_IN = "Card is not in use";
        public static string CARD_IS_DISABLE = "Card is not enabled";
        public static string CARD_IS_LOCKED = "Card is locked";
        public static string CARD_NOT_FOUND = "Card not found";
        public static string BLACKLIST_PLATE_DETECTED = "Blacklist plate detected"; 

        public static string MISSING_FIELD = "This field is required";
        public static string CARD_IS_NOT_IN_EFFECTIVE_DATE_RANGE = "Card is not in effective range";

        private static StringRes _stringRes = new StringRes();

        public static KeyValuePair<RequestExceptionEnum, string> GetExceptionMessage<T>(string err) where T : class
        {
            try
            {
                RequestException reqEx = JsonConvert.DeserializeObject<RequestException>(err);
                if (reqEx.Detail != null)
                {
                    if (reqEx.Detail.Equals(CARD_IS_IN_USE))
                        return new KeyValuePair<RequestExceptionEnum, string>(RequestExceptionEnum.CardIsInUse, _stringRes.GetText("checkin.already_check_in"));
                    if (reqEx.Detail.Equals(CARD_NOT_FOUND))
                        return new KeyValuePair<RequestExceptionEnum, string>(RequestExceptionEnum.CardIsInUse, _stringRes.GetText("checkin.not_exist_card"));
                    if (reqEx.Detail.Equals(CARD_IS_NOT_CHECKED_IN))
                        return new KeyValuePair<RequestExceptionEnum, string>(RequestExceptionEnum.CardIsNotInUse, _stringRes.GetText("checkin.not_check_in_yet"));
                    if (reqEx.Detail.Equals(CARD_IS_DISABLE))
                        return new KeyValuePair<RequestExceptionEnum, string>(RequestExceptionEnum.CardIsDisable, _stringRes.GetText("checkin.card_is_disable"));
                    if (reqEx.Detail.Equals(CARD_IS_LOCKED))
                        return new KeyValuePair<RequestExceptionEnum, string>(RequestExceptionEnum.CardIsDisable, _stringRes.GetText("checkin.card_is_locked"));
                      if (reqEx.Detail.Equals(BLACKLIST_PLATE_DETECTED))	
	                        return new KeyValuePair<RequestExceptionEnum, string>(RequestExceptionEnum.BlacklistPlateDetected, _stringRes.GetText("checkin.blacklist_plate_detected"));

                    if (reqEx.Detail.Equals(MISSING_FIELD))
                        return new KeyValuePair<RequestExceptionEnum, string>(RequestExceptionEnum.CardIsDisable, _stringRes.GetText("missing_field"));
                    if (reqEx.Detail.Equals(CARD_IS_NOT_IN_EFFECTIVE_DATE_RANGE))
                        return new KeyValuePair<RequestExceptionEnum, string>(RequestExceptionEnum.CardIsDisable, _stringRes.GetText("card_is_not_effective"));

                    if (typeof(T) == typeof(CheckOut))
                        return new KeyValuePair<RequestExceptionEnum, string>(RequestExceptionEnum.CheckoutSomethingWrong, _stringRes.GetText("checkout.something_wrong"));
                    else if (typeof(T) == typeof(CheckIn))
                        return new KeyValuePair<RequestExceptionEnum, string>(RequestExceptionEnum.CheckInSomethingWrong, _stringRes.GetText("checkin.something_wrong"));
                }

                return new KeyValuePair<RequestExceptionEnum, string>(RequestExceptionEnum.Undefine, _stringRes.GetText("something_wrong"));
            }
            catch 
            {
                return new KeyValuePair<RequestExceptionEnum, string>(RequestExceptionEnum.Undefine, _stringRes.GetText("something_wrong"));
            }
        }
    }
}
