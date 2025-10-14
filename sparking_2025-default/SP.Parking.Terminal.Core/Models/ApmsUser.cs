using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Models
{
	public class ApmsUser
	{
        /// <summary>
        /// Gets or sets Id of user
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets username of user
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets flag indicate user is a staff or not
        /// </summary>
        [JsonProperty("is_staff")]
        public bool IsStaff { get; set; }

        /// <summary>
        /// Gets or sets flag indicate user is a administrator or not
        /// </summary>
        [JsonProperty("is_admin")]
        public bool IsAdmin { get; set; }

        /// <summary>
        /// Gets or sets display name of user
        /// </summary>
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets shift id of login session
        /// </summary>
        [JsonProperty("shift_id")]
        public int ShiftID { get; set; }

        /// <summary>
        /// Gets or sets staff id
        /// </summary>
        [JsonProperty("staff_id")]
        public string StaffID { get; set; }
	}
}
