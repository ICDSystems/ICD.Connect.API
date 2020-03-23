using System.Collections.Generic;

namespace ICD.Connect.API.Nodes
{
	public interface IConsoleNodeBase : IConsoleCommon
	{
		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IConsoleNodeBase> GetConsoleNodes();
	}
}
