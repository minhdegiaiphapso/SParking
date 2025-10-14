using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Models
{
	public class BulkCreateCardResult
	{
        /// <summary>
        /// Gets or sets number of successfully created cards
        /// </summary>
        [JsonProperty("created")]
        public int NumCreated { get; set; }

        /// <summary>
        /// Gets or sets list of error card ids
        /// </summary>
        [JsonProperty("error_cards")]
        public string[] ErrorCards { get; set; }
	}
}
