using System;
using ICD.Common.Utils;
using ICD.Connect.API.Info;

namespace ICD.Connect.API.EventArguments
{
	public abstract class AbstractApiEventArgs : EventArgs, IApiEventArgs
	{
		private readonly string m_EventName;

		/// <summary>
		/// Gets the name of the API event associated with these arguments.
		/// </summary>
		public string EventName { get { return m_EventName; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="eventName"></param>
		protected AbstractApiEventArgs(string eventName)
		{
			m_EventName = eventName;
		}

		/// <summary>
		/// Builds an API result for the args.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public abstract void BuildResult(object sender, ApiResult result);

		/// <summary>
		/// Get the string representation for this instance.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			ReprBuilder builder = new ReprBuilder(this);

			builder.AppendProperty("EventName", m_EventName);
			ToString(builder.AppendProperty);

			return builder.ToString();
		}

		/// <summary>
		/// Override to add additional properties to the string representation.
		/// </summary>
		/// <param name="addProperty"></param>
		protected virtual void ToString(AddReprPropertyDelegate addProperty)
		{
		}
	}
}
