using System;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.ComponentModel;

namespace SP.Parking.Terminal.Core.Utilities
{
	/// <summary>
	/// Attribute for localization.
	/// </summary>
	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
	public sealed class LocalizableDescriptionAttribute : DescriptionAttribute
	{
		private static StringRes _stringRes = new StringRes();


		#region Public methods.
		// ------------------------------------------------------------------

		/// <summary>
		/// Initializes a new instance of the 
		/// <see cref="LocalizableDescriptionAttribute"/> class.
		/// </summary>
		/// <param name="description">The description.</param>
		/// <param name="resourcesType">Type of the resources.</param>
		public LocalizableDescriptionAttribute(string description)
			: base(description)
		{
		}

		#endregion

		#region Public properties.

		/// <summary>
		/// Get the string value from the resources.
		/// </summary>
		/// <value></value>
		/// <returns>The description stored in this attribute.</returns>
		public override string Description
		{
			get
			{
				if (!_isLocalized)
				{
					_isLocalized = true;

					if (_stringRes != null)
					{
						DescriptionValue = _stringRes.GetText(DescriptionValue);
					}
				}

				return DescriptionValue;
			}
		}
		#endregion

		#region Private variables.

		private bool _isLocalized;

		#endregion
	}
}
