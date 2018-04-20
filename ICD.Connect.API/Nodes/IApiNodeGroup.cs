using System.Collections.Generic;

namespace ICD.Connect.API.Nodes
{
	public interface IApiNodeGroup
	{
		/// <summary>
		/// Gets the instance for the given key.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		object this[uint key] { get; }

		/// <summary>
		/// Returns true if the group contains an instance for the given key.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		bool ContainsKey(uint key);

		/// <summary>
		/// Gets a sequence of keyed nodes.
		/// </summary>
		/// <returns></returns>
		IEnumerable<KeyValuePair<uint, object>> GetKeyedNodes();
	}
}
