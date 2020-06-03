using System;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.API.Info;

namespace ICD.Connect.API.EventArguments
{
	public abstract class AbstractGenericApiEventArgs<T> : AbstractApiEventArgs, IGenericEventArgs<T>
	{
		private readonly T m_Data;

		/// <summary>
		/// Gets the wrapped data associated with the event.
		/// </summary>
		object IGenericEventArgs.Data { get { return Data; } }

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
		/// Builds an API result for the args.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public override void BuildResult(object sender, ApiResult result)
		{
// ReSharper disable once CompareNonConstrainedGenericWithNull
			Type type = Data == null ? typeof(T) : Data.GetType();
			result.SetValue(type, Data);
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
