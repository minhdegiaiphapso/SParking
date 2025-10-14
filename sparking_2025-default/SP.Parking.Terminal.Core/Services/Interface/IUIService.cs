using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Parking.Terminal.Core.Services
{
    public enum MessageIcon
    {
        None,
        Success,
        Failed,
        Rating,
    }

    public class NoticeToUser
    {
		public string Color { get; set; }

		public string Icon { get; set; }
        public string Message { get; set; }

		public NoticeToUser(string icon, string message)
		{
			this.Message = message;
			this.Icon = icon;

            if (this.Icon == IconEnums.Card)
                Color = "Red";
            else if (this.Icon == IconEnums.Check)
                Color = "LightGreen";
            else if (this.Icon == IconEnums.update_ok)
                Color = "LightBlue";
            else if (this.Icon == IconEnums.Close)
                Color = "Red";
            else if (this.Icon == IconEnums.Duplicate)
                Color = "Yellow";
            else if (this.Icon == IconEnums.Error)
                Color = "Red";
            else if (this.Icon == IconEnums.Parking)
                Color = "Cyan";
            else if (this.Icon == IconEnums.Guide)
                Color = "White";
            else if (this.Icon == IconEnums.Warning)
                Color = "Yellow";
		}
    }

	public class Notices : ObservableCollection<NoticeToUser>
    {
        int _timeOut = 5000; // default
        public int TimeOut
        {
            get { return _timeOut; }
            set
            {
                _timeOut = value;
            }
        }
    }

    /// <summary>
    /// Message that will be display to user.
    /// </summary>
    public class MessageToUser
    {
        // Option that allow user to choose
        public class Option
        {
            // Title of the button
            public string Title { get; set; }

            // Tap event
            public Func<bool> Handler;

            // Status of the button
            public OptionStatus Status { get; set; }
        }

        // Enum present status of that option
        public enum OptionStatus
        {
            Negative = -1,
            Neutral = 0,
            Positive = 1,
        }

        // Caption title 
        public string Title { get; set; }

        // Description string
        public string Message { get; set; }

        // Options
        public List<Option> Options { get; private set; }

        // Icon
        public MessageIcon Icon { get; set; }

        // Add an option
        public void AddOption(string optTitle, Func<bool> optHandler)
        {
            if (Options == null)
                Options = new List<Option>();

            Options.Add(new Option()
            {
                Title = optTitle,
                Handler = optHandler,
                Status = OptionStatus.Neutral,
            });
        }

        // Add an option with status
        public void AddOption(string optTitle, Func<bool> optHandler, OptionStatus status)
        {
            if (Options == null)
                Options = new List<Option>();

            Options.Add(new Option()
            {
                Title = optTitle,
                Handler = optHandler,
                Status = status,
            });
        }

        public MessageToUser()
        {
        }

        public MessageToUser(string title, string message)
        {
            this.Title = title;
            this.Message = message;
        }

        public MessageToUser(string title, string message, string optTitle, Func<bool> optHandler)
            : this(title, message)
        {
            this.AddOption(optTitle, optHandler);
        }

        public MessageToUser(string title, string message, string optTitle1, Func<bool> optHandler1, string optTitle2, Func<bool> optHandler2)
            : this(title, message, optTitle1, optHandler1)
        {
            this.AddOption(optTitle2, optHandler2);
        }
    }

    public interface IUIService
    {
        void ShowMessage(MessageToUser message);

        void OpenUrl(string url);

        void ChangeColor(string color);
    }
}
