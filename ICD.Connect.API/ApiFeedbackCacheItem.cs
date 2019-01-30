using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Collections;
using ICD.Connect.API.Info;

namespace ICD.Connect.API
{
	public sealed class ApiFeedbackCacheItem
	{
		private readonly IApiInfo[] m_Path;
		private readonly ApiClassInfo m_Command;
		private readonly ApiEventInfo m_Event;
		private readonly IcdHashSet<IApiRequestor> m_Requestors;
		private readonly Delegate m_Callback;

		#region Properties

		/// <summary>
		/// Gets the root command from the path.
		/// </summary>
		public ApiClassInfo Command { get { return m_Command; } }

		/// <summary>
		/// Gets the leaf event from the path.
		/// </summary>
		public ApiEventInfo Event { get { return m_Event; } }

		/// <summary>
		/// Gets the delegate for the event subscription.
		/// </summary>
		public Delegate Callback { get { return m_Callback; } }

		/// <summary>
		/// Gets the number of requestors that are currently registered.
		/// </summary>
		public int Count { get { return m_Requestors.Count; } }

		#endregion

		#region Factories

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="command"></param>
		/// <param name="eventInfo"></param>
		/// <param name="callback"></param>
		private ApiFeedbackCacheItem(IEnumerable<IApiInfo> path, ApiClassInfo command, ApiEventInfo eventInfo,
		                             Delegate callback)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			if (command == null)
				throw new ArgumentNullException("command");

			if (eventInfo == null)
				throw new ArgumentNullException("eventInfo");

			// Feedback shouldn't have a subscription action
			eventInfo.SubscribeAction = ApiEventInfo.eSubscribeAction.None;

			m_Path = path.ToArray();
			m_Command = command;
			m_Event = eventInfo;
			m_Requestors = new IcdHashSet<IApiRequestor>();
			m_Callback = callback;
		}

		/// <summary>
		/// Creates a new instance from the given command path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		public static ApiFeedbackCacheItem FromPath(Stack<IApiInfo> path, Delegate callback)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			if (callback == null)
				throw new ArgumentNullException("callback");

			return FromPath(path.Reverse(), callback);
		}

		/// <summary>
		/// Creates a new instance from the given command path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		public static ApiFeedbackCacheItem FromPath(IEnumerable<IApiInfo> path, Delegate callback)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			if (callback == null)
				throw new ArgumentNullException("callback");

			ApiClassInfo root;
			IApiInfo leaf;
			IEnumerable<IApiInfo> pathCopy = ApiCommandBuilder.CopyPath(path, out root, out leaf);

			return new ApiFeedbackCacheItem(pathCopy, root, leaf as ApiEventInfo, callback);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Performs a copy of the instance command info.
		/// </summary>
		/// <returns></returns>
		public ApiFeedbackCacheItem CommandCopy()
		{
			return FromPath(m_Path, m_Callback);
		}

		/// <summary>
		/// Adds the requestor to the collection for callback.
		/// </summary>
		/// <param name="requestor"></param>
		public void AddRequestor(IApiRequestor requestor)
		{
			m_Requestors.Add(requestor);
		}

		/// <summary>
		/// Removes the requestor from the collection for callback.
		/// </summary>
		/// <param name="requestor"></param>
		public void RemoveRequestor(IApiRequestor requestor)
		{
			m_Requestors.Remove(requestor);
		}

		/// <summary>
		/// Gets the subscribed requestors.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IApiRequestor> GetRequestors()
		{
			return m_Requestors;
		}

		#endregion
	}
}
