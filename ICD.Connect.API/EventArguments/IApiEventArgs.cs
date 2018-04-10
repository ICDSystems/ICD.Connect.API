namespace ICD.Connect.API.EventArguments
{
	public interface IApiEventArgs
	{
		/// <summary>
		/// Gets the name of the API event associated with these arguments.
		/// </summary>
		string EventName { get; }
	}
}
