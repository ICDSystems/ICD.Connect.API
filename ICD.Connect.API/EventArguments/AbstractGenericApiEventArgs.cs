using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;

namespace ICD.Connect.API.EventArguments
{
	public abstract class AbstractGenericApiEventArgs<T> : AbstractApiEventArgs, IGenericEventArgs<T>
	{
		private readonly T m_Data;

		/// <summary>
		/// Gets the wrapped data associated with the event.
		/// </summary>
		public T Data { get { return m_Data; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="eventName"></param>
		/// <param name="data"></param>
		protected AbstractGenericApiEventArgs(string eventName, T data)
			: base(eventName)
		{
			m_Data = data;
		}

		/// <summary>
		/// Override to add additional properties to the string representation.
		/// </summary>
		/// <param name="addProperty"></param>
		protected override void ToString(AddReprPropertyDelegate addProperty)
		{
			base.ToString(addProperty);

			addProperty("Data", m_Data);
		}
	}
}
