using System.Collections;
using System.Collections.Generic;

namespace ICD.Connect.API.Nodes
{
	public abstract class AbstractApiNodeGroup : IApiNodeGroup
	{
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
