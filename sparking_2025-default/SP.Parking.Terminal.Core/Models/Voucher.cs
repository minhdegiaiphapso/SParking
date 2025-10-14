using Cirrious.MvvmCross.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Models
{
    public class Voucher
    {
        [JsonProperty("card_id")]
        public string CardId { get; set; }
        [JsonProperty("voucher_type")]
        public string Voucher_Type { get; set; }
        [JsonProperty("voucher_amount")]
        public float Voucher_Amount { get; set; }
        [JsonProperty("parking_fee")]
        public float Parking_Fee { get; set; }
        [JsonProperty("actual_fee")]
        public float Actual_Fee { get; set; }
        [JsonProperty("check_in_time")]
        public DateTime Check_In_Time { get; set; }
    }
    public class VoucherSearch
    {
        [JsonProperty("vouchertype")]
        public string VoucherType { get; set; }
        [JsonProperty("amountvoucher")]
        public float VoucherAmount { get; set; }
        [JsonProperty("actualfee")]
        public float ActualFee { get; set; }
        [JsonProperty("fee")]
        public float ParkingFee { get; set; }
        [JsonIgnore]
        public string StrVoucherAmount { get {
                if(VoucherAmount>0)
                    return VoucherAmount.ToString("#,###");
                return "0";
            }
        }
        [JsonIgnore]
        public string StrActualFee { get
            {
                if (ActualFee > 0)
                    return ActualFee.ToString("#,###");
                return "0";
            }
        }
    }
}

