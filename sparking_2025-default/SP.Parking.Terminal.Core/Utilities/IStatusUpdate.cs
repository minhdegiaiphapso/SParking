using System;

namespace SP.Parking.Terminal.Core
{
	public enum ProgressStatus
	{
        Pending,
        Started,
		Running,
		Ended,

	}
	
	public interface IStatusUpdate
	{
		/// <summary>
		/// Gets or sets a value indicating whether this instance is busy.
		/// </summary>
		void StatusChanged(ProgressStatus status, string message = null, float value = 0);

		/// <summary>
		/// Indicate that exception has occur during the processing
		/// </summary>
		void HandleError(Exception ex);
	}
 

	public class NullStatusUpdate : IStatusUpdate
	{
		public static readonly NullStatusUpdate Instance = new NullStatusUpdate();

		#region IStatusUpdate implementation

		public void StatusChanged (ProgressStatus status, string message, float value)
		{
		}

		public void HandleError (Exception ex)
		{
		}

		#endregion
	}
}

