using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Common.Utils.Collections;
using ICD.Connect.API.Info;

namespace ICD.Connect.API
{
	public sealed class ApiFeedbackCacheItem
	{
		private readonly WeakKeyDictionary<IApiRequestor, object> m_Requestors;
		private readonly SafeCriticalSection m_RequestorsSection;

		private readonly ApiEventCommandPath m_CommandPath;
		private readonly EventInfo m_EventInfo;
		private readonly Delegate m_Callback;

		#region Properties

		/// <summary>
		/// Gets the event command.
		/// </summary>
		public ApiEventCommandPath CommandPath { get { return m_CommandPath; } }

		/// <summary>
		/// Gets the info for the subscribed event.
		/// </summary>
		public EventInfo EventInfo { get { return m_EventInfo; } }

		/// <summary>
		/// Gets the delegate for the event subscription.
		/// </summary>
		public Delegate Callback { get { return m_Callback; } }

		/// <summary>
		/// Gets the number of requestors that are currently registered.
		/// </summary>
		public int Count { get { return m_RequestorsSection.Execute(() => m_Requestors.Count); } }

		#endregion

		#region Factories

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="commandPath"></param>
		/// <param name="eventInfo"></param>
		/// <param name="callback"></param>
		private ApiFeedbackCacheItem(ApiEventCommandPath commandPath, EventInfo eventInfo, Delegate callback)
		{
			if (commandPath == null)
				throw new ArgumentNullException("commandPath");

			if (callback == null)
				throw new ArgumentNullException("callback");

			// Easier than making a WeakKeyHashSet from scratch
			m_Requestors = new WeakKeyDictionary<IApiRequestor, object>();
			m_RequestorsSection = new SafeCriticalSection();

			m_CommandPath = commandPath;
			m_EventInfo = eventInfo;
			m_Callback = callback;
		}

		public static ApiFeedbackCacheItem FromPath(Stack<IApiInfo> path, EventInfo eventInfo, Delegate callback)
		{
			ApiEventCommandPath commandPath = ApiEventCommandPath.FromPath(path);
			return new ApiFeedbackCacheItem(commandPath, eventInfo, callback);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Adds the requestor to the collection for callback.
		/// </summary>
		/// <param name="requestor"></param>
		public void AddRequestor(IApiRequestor requestor)
		{
			m_RequestorsSection.Execute(() => m_Requestors[requestor] = null);
		}

		/// <summary>
		/// Removes the requestor from the collection for callback.
		/// </summary>
		/// <param name="requestor"></param>
		public void RemoveRequestor(IApiRequestor requestor)
		{
			m_RequestorsSection.Execute(() => m_Requestors.Remove(requestor));
		}

		/// <summary>
		/// Gets the subscribed requestors.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IApiRequestor> GetRequestors()
		{
			return m_RequestorsSection.Execute(() => m_Requestors.Keys.ToArray());
		}

		#endregion
	}
}
