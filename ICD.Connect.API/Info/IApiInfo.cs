using Newtonsoft.Json;

namespace ICD.Connect.API.Info
{
	public interface IApiInfo
	{
		/// <summary>
		/// Gets the name for the API attribute.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the help for the API attribute.
		/// </summary>
		string Help { get; }

		/// <summary>
		/// Serializes the instance to JSON.
		/// </summary>
		/// <returns></returns>
		string Serialize();

		/// <summary>
		/// Serializes the instance to JSON.
		/// </summary>
		/// <param name="writer"></param>
		void Serialize(JsonWriter writer);
	}
}