using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Connect.API.Info;

namespace ICD.Connect.API
{
	/// <summary>
	/// Describes a single API command path from root to event.
	/// </summary>
	public sealed class ApiEventCommandPath
	{
		private readonly IApiInfo[] m_Path;
		private readonly ApiClassInfo m_Command;
		private readonly ApiEventInfo m_Event;

		/// <summary>
		/// Gets the leaf API event info.
		/// </summary>
		public ApiEventInfo Event { get { return m_Event; } }

		/// <summary>
		/// Gets the root class info.
		/// </summary>
		public ApiClassInfo Root { get { return m_Command; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="rootClassInfo"></param>
		/// <param name="leafEventInfo"></param>
		private ApiEventCommandPath(IEnumerable<IApiInfo> path, ApiClassInfo rootClassInfo, ApiEventInfo leafEventInfo)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			if (rootClassInfo == null)
				throw new ArgumentNullException("rootClasInfo");

			if (leafEventInfo == null)
				throw new ArgumentNullException("leafEventInfo");

			// Feedback shouldn't have a subscription action
			leafEventInfo.SubscribeAction = ApiEventInfo.eSubscribeAction.None;

			m_Path = path.ToArray();
			m_Command = rootClassInfo;
			m_Event = leafEventInfo;
		}

		/// <summary>
		/// Creates a new instance from the given command path.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static ApiEventCommandPath FromPath(Stack<IApiInfo> path)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			return FromPath(path.Reverse());
		}

		/// <summary>
		/// Creates a new instance from the given command path.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static ApiEventCommandPath FromPath(IEnumerable<IApiInfo> path)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			ApiClassInfo root;
			IApiInfo leaf;
			IEnumerable<IApiInfo> pathCopy = ApiCommandBuilder.CopyPath(path, out root, out leaf);

			return new ApiEventCommandPath(pathCopy, root, leaf as ApiEventInfo);
		}

		/// <summary>
		/// Performs a deep copy of the instance.
		/// </summary>
		/// <returns></returns>
		public ApiEventCommandPath DeepCopy()
		{
			return FromPath(m_Path);
		}
	}
}