using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Models
{
	public class UserShift
	{
        /// <summary>
        /// Gets or sets Id of user shift
        /// </summary>
        [JsonProperty("shift_id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets lane Id of user shift
        /// </summary>
        [JsonProperty("lane_id")]
        public int LaneId { get; set; }

        /// <summary>
        /// Gets or sets user id of user shift
        /// </summary>
        [JsonProperty("user_id")]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets begin timestamp of user shift
        /// </summary>
        [JsonProperty("begin_timestamp")]
        public long BeginTimestamp { get; set; }

        /// <summary>
        /// Gets or sets end timestamp of user shift
        /// </summary>
        [JsonProperty("end_timestamp")]
        public long EndTimestamp { get; set; }

        /// <summary>
        /// Gets or sets number of check in
        /// </summary>
        [JsonProperty("num_check_in")]
        public int NumberOfCheckIn { get; set; }

        /// <summary>
        /// Gets or sets number of check out
        /// </summary>
        [JsonProperty("num_check_out")]
        public int NumberOfCheckOut { get; set; }

        /// <summary>
        /// Gets or sets revenue of user shift
        /// </summary>
        [JsonProperty("revenue")]
        public int Revenue { get; set; }

        /// <summary>
        /// Gets begin time of user shift
        /// </summary>
        [JsonIgnore]
        public DateTime BeginTime { get { return TimestampConverter.Timestamp2DateTime(BeginTimestamp); } }

        /// <summary>
        /// Gets end time of user shift
        /// </summary>
        [JsonIgnore]
        public DateTime EndTime { get { return TimestampConverter.Timestamp2DateTime(EndTimestamp); } }

        /// <summary>
        /// Gets or sets user of user shift
        /// </summary>
        [JsonIgnore]
        public ApmsUser User { get; set; }
	}
}
