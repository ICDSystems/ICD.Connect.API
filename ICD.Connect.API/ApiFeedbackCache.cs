using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
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
	public static class ApiFeedbackCache
	{
		private static readonly WeakKeyDictionary<object, Dictionary<string, ApiFeedbackCacheItem>> s_SubscribedEventsMap;
		private static readonly SafeCriticalSection s_SubscribedEventsSection;

		/// <summary>
		/// Logger for the originator.
		/// </summary>
		private static ILoggerService Logger
		{
			get { return ServiceProvider.TryGetService<ILoggerService>(); }
		}

		/// <summary>
		/// Static constructor.
		/// </summary>
		static ApiFeedbackCache()
		{
			s_SubscribedEventsMap = new WeakKeyDictionary<object, Dictionary<string, ApiFeedbackCacheItem>>();
			s_SubscribedEventsSection = new SafeCriticalSection();
		}

		#region Methods

		/// <summary>
		/// Subscribes to the given event on the given instance.
		/// </summary>
		/// <param name="requestor"></param>
		/// <param name="eventInfo"></param>
		/// <param name="instance"></param>
		/// <param name="path"></param>
		public static void Subscribe([NotNull] IApiRequestor requestor, [NotNull] EventInfo eventInfo,
		                             [NotNull] object instance, [NotNull] Stack<IApiInfo> path)
		{
			if (requestor == null)
				throw new ArgumentNullException("requestor");

			if (eventInfo == null)
				throw new ArgumentNullException("eventInfo");

			if (instance == null)
				throw new ArgumentNullException("instance");

			if (path == null)
				throw new ArgumentNullException("path");

			s_SubscribedEventsSection.Enter();

			try
			{
				Dictionary<string, ApiFeedbackCacheItem> events;
				if (!s_SubscribedEventsMap.TryGetValue(instance, out events))
				{
					events = new Dictionary<string, ApiFeedbackCacheItem>();
					s_SubscribedEventsMap.Add(instance, events);
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
				s_SubscribedEventsSection.Leave();
			}
		}

		/// <summary>
		/// Unsubscribes from the given event on the given instance.
		/// </summary>
		/// <param name="requestor"></param>
		/// <param name="instance"></param>
		/// <param name="path"></param>
		public static void Unsubscribe([NotNull] IApiRequestor requestor, [NotNull] object instance, [NotNull] Stack<IApiInfo> path)
		{
			if (requestor == null)
				throw new ArgumentNullException("requestor");

			if (instance == null)
				throw new ArgumentNullException("instance");

			if (path == null)
				throw new ArgumentNullException("path");

			string key = path.Peek().Name;

			s_SubscribedEventsSection.Enter();

			try
			{
				Dictionary<string, ApiFeedbackCacheItem> events;
				if (!s_SubscribedEventsMap.TryGetValue(instance, out events))
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
					s_SubscribedEventsMap.Remove(instance);
			}
			finally
			{
				s_SubscribedEventsSection.Leave();
			}
		}

		/// <summary>
		/// Removes the requestor from all of the callback infos.
		/// </summary>
		/// <param name="requestor"></param>
		public static void UnsubscribeAll([NotNull] IApiRequestor requestor)
		{
			if (requestor == null)
				throw new ArgumentNullException("requestor");

			s_SubscribedEventsSection.Enter();

			try
			{
				foreach (KeyValuePair<object, Dictionary<string, ApiFeedbackCacheItem>> instanceToEventNames in s_SubscribedEventsMap.ToArray())
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
						s_SubscribedEventsMap.Remove(instanceToEventNames.Key);
				}
			}
			finally
			{
				s_SubscribedEventsSection.Leave();
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Only support subscribing to events matching the signature of this method.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private static void EventCallback([NotNull] object sender, [NotNull] IApiEventArgs args)
		{
			if (sender == null)
				throw new ArgumentNullException("sender");

			if (args == null)
				throw new ArgumentNullException("args");

			ApiFeedbackCacheItem callbackInfo;

			s_SubscribedEventsSection.Enter();

			try
			{
				Dictionary<string, ApiFeedbackCacheItem> map;
				if (!s_SubscribedEventsMap.TryGetValue(sender, out map))
					return;

				if (!map.TryGetValue(args.EventName, out callbackInfo))
					return;
			}
			finally
			{
				s_SubscribedEventsSection.Leave();
			}

			// Create a copy and build the result
			ApiEventCommandPath copy = callbackInfo.CommandPath.DeepCopy();
			args.BuildResult(sender, copy.Event);

			foreach (IApiRequestor requestor in callbackInfo.GetRequestors())
				requestor.HandleFeedback(copy.Root);
		}

		#endregion
	}
}
