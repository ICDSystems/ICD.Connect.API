using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Connect.API.Info;

namespace ICD.Connect.API
{
	public sealed class ApiHandlerEventCallbackInfo
	{
		private IApiInfo[] m_Path;
		private readonly ApiClassInfo m_Command;
		private readonly ApiEventInfo m_Event;

		public ApiClassInfo Command { get { return m_Command; } }

		public ApiEventInfo Event { get { return m_Event; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="command"></param>
		/// <param name="eventInfo"></param>
		private ApiHandlerEventCallbackInfo(IEnumerable<IApiInfo> path, ApiClassInfo command, ApiEventInfo eventInfo)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			if (command == null)
				throw new ArgumentNullException("command");

			if (eventInfo == null)
				throw new ArgumentNullException("eventInfo");

			m_Path = path.ToArray();
			m_Command = command;
			m_Event = eventInfo;
		}

		/// <summary>
		/// Creates a new instance from the given command path.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static ApiHandlerEventCallbackInfo FromPath(Stack<IApiInfo> path)
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
		public static ApiHandlerEventCallbackInfo FromPath(IEnumerable<IApiInfo> path)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			ApiClassInfo root;
			IApiInfo leaf;
			IEnumerable<IApiInfo> pathCopy = ApiCommandBuilder.CopyPath(path, out root, out leaf);

			return new ApiHandlerEventCallbackInfo(pathCopy, root, leaf as ApiEventInfo);
		}

		/// <summary>
		/// Performs a deep copy of the instance.
		/// </summary>
		/// <returns></returns>
		public ApiHandlerEventCallbackInfo DeepCopy()
		{
			return FromPath(m_Path);
		}
	}
}
