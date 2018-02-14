using System.Collections;
using System.Collections.Generic;

namespace ICD.Connect.API.Nodes
{
	public abstract class AbstractApiNodeGroup : IApiNodeGroup
	{
		/// <summary>
		/// Gets the node at the given key.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public abstract object this[uint key] { get; }

		/// <summary>
		/// Returns true if the group contains an instance for the given key.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public abstract bool ContainsKey(uint key);

		/// <summary>
		/// Gets the keyed nodes for this group.
		/// </summary>
		/// <returns></returns>
		protected abstract IEnumerable<KeyValuePair<uint, object>> GetNodes();

		public IEnumerator<KeyValuePair<uint, object>> GetEnumerator()
		{
			return GetNodes().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
