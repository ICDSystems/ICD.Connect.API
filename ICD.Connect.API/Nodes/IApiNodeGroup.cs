using System.Collections.Generic;

namespace ICD.Connect.API.Nodes
{
	public interface IApiNodeGroup : IEnumerable<KeyValuePair<uint, object>>
	{
		object this[uint key] { get; }
	}
}
