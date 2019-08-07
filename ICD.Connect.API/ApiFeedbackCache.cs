﻿using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
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
		private readonly SafeCriticalSection m_SubscribedEventsSection;

		private ILoggerService m_CachedLogger;

		/// <summary>
		/// Logger for the originator.
		/// </summary>
		private ILoggerService Logger
		{
			get { return m_CachedLogger = m_CachedLogger ?? ServiceProvider.TryGetService<ILoggerService>(); }
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public ApiFeedbackCache()
		{
			m_SubscribedEventsMap = new WeakKeyDictionary<object, Dictionary<string, ApiFeedbackCacheItem>>();
			m_SubscribedEventsSection = new SafeCriticalSection();
		}

		#region Methods

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

			m_SubscribedEventsSection.Enter();

			try
			{
				Dictionary<string, ApiFeedbackCacheItem> events;
				if (!m_SubscribedEventsMap.TryGetValue(instance, out events))
				{
					events = new Dictionary<string, ApiFeedbackCacheItem>();
					m_SubscribedEventsMap.Add(instance, events);
				}

				string key = path.Peek().Name;

				ApiFeedbackCacheItem callbackInfo;
				if (!events.TryGetValue(key, out callbackInfo))
				{
					// Create a new subscription
					Delegate callback = ReflectionUtils.SubscribeEvent<IApiEventArgs>(instance, eventInfo, EventCallback);

					callbackInfo = ApiFeedbackCacheItem.FromPath(path, eventInfo, callback);
					events.Add(key, callbackInfo);
				}

				callbackInfo.AddRequestor(requestor);
			}
			finally
			{
				m_SubscribedEventsSection.Leave();
			}
		}

		/// <summary>
		/// Unsubscribes from the given event on the given instance.
		/// </summary>
		/// <param name="requestor"></param>
		/// <param name="instance"></param>
		/// <param name="path"></param>
		public void Unsubscribe(IApiRequestor requestor, object instance, Stack<IApiInfo> path)
		{
			if (requestor == null)
				throw new ArgumentNullException("requestor");

			if (instance == null)
				throw new ArgumentNullException("instance");

			if (path == null)
				throw new ArgumentNullException("path");

			string key = path.Peek().Name;

			m_SubscribedEventsSection.Enter();

			try
			{
				Dictionary<string, ApiFeedbackCacheItem> events;
				if (!m_SubscribedEventsMap.TryGetValue(instance, out events))
					return;

				ApiFeedbackCacheItem callbackInfo;
				if (!events.TryGetValue(key, out callbackInfo))
					return;

				callbackInfo.RemoveRequestor(requestor);
				if (callbackInfo.Count > 0)
					return;

				// Remove the subscription
				ReflectionUtils.UnsubscribeEvent(instance, callbackInfo.EventInfo, callbackInfo.Callback);
				Logger.AddEntry(eSeverity.Debug, "{0} unsubscribed from {1} event {2}", requestor, instance, key);

				events.Remove(key);

				if (events.Count == 0)
					m_SubscribedEventsMap.Remove(instance);
			}
			finally
			{
				m_SubscribedEventsSection.Leave();
			}
		}

		/// <summary>
		/// Removes the requestor from all of the callback infos.
		/// </summary>
		/// <param name="requestor"></param>
		public void UnsubscribeAll(IApiRequestor requestor)
		{
			if (requestor == null)
				throw new ArgumentNullException("requestor");

			m_SubscribedEventsSection.Enter();

			try
			{
				foreach (KeyValuePair<object, Dictionary<string, ApiFeedbackCacheItem>> instanceToEventNames in m_SubscribedEventsMap.ToArray())
				{
					foreach (KeyValuePair<string, ApiFeedbackCacheItem> eventNameToItem in instanceToEventNames.Value.ToArray())
					{
						ApiFeedbackCacheItem item = eventNameToItem.Value;

						item.RemoveRequestor(requestor);
						if (item.Count != 0)
							continue;

						instanceToEventNames.Value.Remove(eventNameToItem.Key);

						// Remove the subscription
						ReflectionUtils.UnsubscribeEvent(instanceToEventNames.Key, item.EventInfo, item.Callback);
						Logger.AddEntry(eSeverity.Debug, "{0} unsubscribed from {1} event {2}", requestor, instanceToEventNames.Key,
						                eventNameToItem.Key);
					}

					if (instanceToEventNames.Value.Count == 0)
						m_SubscribedEventsMap.Remove(instanceToEventNames.Key);
				}
			}
			finally
			{
				m_SubscribedEventsSection.Leave();
			}
		}

		#endregion

		/// <summary>
		/// Only support subscribing to events matching the signature of this method.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void EventCallback(object sender, IApiEventArgs args)
		{
			if (sender == null)
				throw new ArgumentNullException("sender");

			if (args == null)
				throw new ArgumentNullException("args");

			ApiFeedbackCacheItem callbackInfo;

			m_SubscribedEventsSection.Enter();

			try
			{
				Dictionary<string, ApiFeedbackCacheItem> map;
				if (!m_SubscribedEventsMap.TryGetValue(sender, out map))
					return;

				if (!map.TryGetValue(args.EventName, out callbackInfo))
					return;
			}
			finally
			{
				m_SubscribedEventsSection.Leave();
			}

			// Create a copy and build the result
			ApiEventCommandPath copy = callbackInfo.CommandPath.DeepCopy();
			args.BuildResult(sender, copy.Event);

			foreach (IApiRequestor requestor in callbackInfo.GetRequestors())
				requestor.HandleFeedback(copy.Root);
		}
	}
}
