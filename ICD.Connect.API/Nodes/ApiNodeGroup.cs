using System;
using System.Collections.Generic;
using System.Linq;

namespace ICD.Connect.API.Nodes
{
	public sealed class ApiNodeGroup<T> : AbstractApiNodeGroup
	{
		private readonly Func<IEnumerable<T>> m_GetCollection;
		private readonly Func<T, uint> m_GetKey;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="getCollection"></param>
		/// <param name="getKey"></param>
		public ApiNodeGroup(Func<IEnumerable<T>> getCollection, Func<T, uint> getKey)
		{
			m_GetCollection = getCollection;
			m_GetKey = getKey;
		}

		/// <summary>
		/// Gets the keyed nodes for this group.
		/// </summary>
		/// <returns></returns>
		protected override IEnumerable<KeyValuePair<uint, object>> GetNodes()
		{
			return m_GetCollection().Select(item =>
			                                {
				                                uint key = m_GetKey(item);
				                                return new KeyValuePair<uint, object>(key, item);
			                                });
		}
	}
}
