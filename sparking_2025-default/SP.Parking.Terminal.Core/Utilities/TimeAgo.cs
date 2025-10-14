using SP.Parking.Terminal.Core.Models;
using System;

namespace SP.Parking.Terminal.Core
{
	public class TimeAgo
	{
		public static bool IsDailog;
		public static string ToString(DateTime date, bool isUtc = true)
		{
            var now = TimeMapInfo.Current.LocalTime;
            var utcnow = TimeMapInfo.Current.UtcTime;
            TimeSpan timeSince = isUtc ? utcnow.Subtract(date) : now.Subtract(date);
            //TimeSpan timeSince = isUtc ? DateTime.UtcNow.Subtract(date) : DateTime.Now.Subtract(date);
            //if (timeSince.TotalMilliseconds < 1)
            //	return localeService.GetText("const.TimeAgo.NotYet");
			if (timeSince.TotalMinutes < 1)
				return "vừa mới đây";
			if (timeSince.TotalMinutes < 2)
				return "1 phút trước";
			if (timeSince.TotalMinutes < 60)
				return timeSince.Minutes + " phút trước";
			if (timeSince.TotalMinutes < 120)
				return "1 giờ trước";
			if (timeSince.TotalHours < 24)
				return timeSince.Hours + " giờ trước";
			if (timeSince.TotalDays == 1)
				return "hôm qua";
			if (timeSince.TotalDays < 7)
				return timeSince.Days + " ngày trước";
			if (timeSince.TotalDays < 14)
				return "tuần trước";
			if (timeSince.TotalDays < 21)
				return "2 tuần trước";
			if (timeSince.TotalDays < 28)
				return "3 tuần trước";
			if (timeSince.TotalDays < 60)
				return "tháng trước";
			if (timeSince.TotalDays < 365)
				return Math.Round(timeSince.TotalDays / 30) + " tháng trước";
			if (timeSince.TotalDays < 730)
				return "năm trước";

			//last but not least...
			return Math.Round(timeSince.TotalDays / 365) + " năm trước";

		}
	}


}

