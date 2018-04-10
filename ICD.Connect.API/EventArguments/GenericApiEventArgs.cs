namespace ICD.Connect.API.EventArguments
{
	public sealed class GenericApiEventArgs<T> : AbstractGenericApiEventArgs<T>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="eventName"></param>
		/// <param name="data"></param>
		public GenericApiEventArgs(string eventName, T data)
			: base(eventName, data)
		{
		}
	}
}
