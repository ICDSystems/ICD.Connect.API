using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Connect.API.EventArguments;
using ICD.Connect.API.Info;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.API
{
	public sealed class ApiFeedbackCache
	{
		private readonly WeakKeyDictionary<object, Dictionary<string, ApiFeedbackCacheItem>> m_SubscribedEventsMap;
		private static MethodInfo s_EventCallbackMethod;

		/// <summary>
		/// Gets a reference to the callback method used with API events.
		/// </summary>
		private static MethodInfo EventCallbackMethod
		{
			get
			{
				return s_EventCallbackMethod =
				       s_EventCallbackMethod ?? typeof(ApiFeedbackCache)
#if SIMPLSHARP
					                                .GetCType()
#endif
					                                .GetMethod("EventCallback",
					                                           BindingFlags.NonPublic | BindingFlags.Instance);
			}
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public ApiFeedbackCache()
		{
			m_SubscribedEventsMap = new WeakKeyDictionary<object, Dictionary<string, ApiFeedbackCacheItem>>();
		}

		/// <summary>
		/// Subscribes to the given event on the given instance.
		/// </summary>
		/// <param name="requestor"></param>
		/// <param name="eventInfo"></param>
		/// <param name="instance"></param>
		/// <param name="path"></param>
		public void Subscribe(IApiRequestor requestor, EventInfo eventInfo, object instance, Stack<IApiInfo> path)
		{
			if (requestor == null)
				throw new ArgumentNullException("requestor");

			if (eventInfo == null)
				throw new ArgumentNullException("eventInfo");

			if (instance == null)
				throw new ArgumentNullException("instance");

			if (path == null)
				throw new ArgumentNullException("path");

			string key = path.Peek().Name;

			if (!m_SubscribedEventsMap.ContainsKey(instance))
				m_SubscribedEventsMap.Add(instance, new Dictionary<string, ApiFeedbackCacheItem>());

			if (!m_SubscribedEventsMap[instance].ContainsKey(key))
			{
				// Create a new subscription
				ReflectionUtils.SubscribeEvent(instance, eventInfo, this, EventCallbackMethod);
				ApiFeedbackCacheItem info = ApiFeedbackCacheItem.FromPath(path);
				m_SubscribedEventsMap[instance].Add(key, info);
			}

			ApiFeedbackCacheItem callbackInfo = m_SubscribedEventsMap[instance][key];
			callbackInfo.AddRequestor(requestor);
		}

		/// <summary>
		/// Unsubscribes from the given event on the given instance.
		/// </summary>
		/// <param name="requestor"></param>
		/// <param name="eventInfo"></param>
		/// <param name="instance"></param>
		/// <param name="path"></param>
		public void Unsubscribe(IApiRequestor requestor, EventInfo eventInfo, object instance, Stack<IApiInfo> path)
		{
			if (requestor == null)
				throw new ArgumentNullException("requestor");

			if (eventInfo == null)
				throw new ArgumentNullException("eventInfo");

			if (instance == null)
				throw new ArgumentNullException("instance");

			if (path == null)
				throw new ArgumentNullException("path");

			string key = path.Peek().Name;

			if (!m_SubscribedEventsMap.ContainsKey(instance))
				return;

			if (!m_SubscribedEventsMap[instance].ContainsKey(key))
				return;

			ApiFeedbackCacheItem callbackInfo = m_SubscribedEventsMap[instance][key];
			callbackInfo.RemoveRequestor(requestor);
		}

		/// <summary>
		/// Only support subscribing to events matching the siganture of this method.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		[UsedImplicitly]
		private void EventCallback(object sender, IApiEventArgs args)
		{
			if (sender == null)
				throw new ArgumentNullException("sender");

			if (args == null)
				throw new ArgumentNullException("args");

			Dictionary<string, ApiFeedbackCacheItem> map;
			if (!m_SubscribedEventsMap.TryGetValue(sender, out map))
				return;

			ApiFeedbackCacheItem callbackInfo;
			if (!map.TryGetValue(args.EventName, out callbackInfo))
				return;

			// Create a copy and build the result
			ApiFeedbackCacheItem copy = callbackInfo.CommandCopy();
			copy.Event.Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.Ok };
			args.BuildResult(sender, copy.Event.Result);

			foreach (IApiRequestor requestor in callbackInfo.GetRequestors())
				requestor.HandleFeedback(copy.Command);
		}
	}
}
