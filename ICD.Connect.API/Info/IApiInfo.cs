namespace ICD.Connect.API.Info
{
	public interface IApiInfo
	{
		/// <summary>
		/// Gets the name for the API attribute.
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Gets the help for the API attribute.
		/// </summary>
		string Help { get; set; }

		/// <summary>
		/// Gets/sets the response message for this request.
		/// </summary>
		ApiResult Result { get; set; }
	}
}
