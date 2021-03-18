using System.Collections.Generic;

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

		/// <summary>
		/// Creates a copy of the instance containing none of the child nodes.
		/// </summary>
		/// <returns></returns>
		IApiInfo ShallowCopy();

		/// <summary>
		/// Adds the given item as an immediate child to this node.
		/// </summary>
		/// <param name="child"></param>
		void AddChild(IApiInfo child);

		/// <summary>
		/// Gets the children attached to this node.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IApiInfo> GetChildren();
	}
}
