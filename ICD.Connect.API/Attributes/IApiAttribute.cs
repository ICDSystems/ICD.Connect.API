using ICD.Connect.API.Info;

namespace ICD.Connect.API.Attributes
{
	public interface IApiAttribute
	{
		/// <summary>
		/// Gets the name for the decorated item. 
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the help string for the decorated item.
		/// </summary>
		string Help { get; }

		/// <summary>
		/// Returns the info for the attribute.
		/// </summary>
		/// <param name="memberInfo"></param>
		/// <returns></returns>
		IApiInfo GetInfo(object memberInfo);
	}
}
