using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Connect.API.Info;

namespace ICD.Connect.API.Attributes
{
	[AttributeUsage(AttributeTargets.Event, Inherited = true, AllowMultiple = false)]
	public sealed class ApiEventAttribute : AbstractApiAttribute
    {
		private static readonly Dictionary<Type, Dictionary<string, EventInfo>> s_AttributeNameToEvent;
		private static readonly Dictionary<Type, EventInfo[]> s_TypeToEvents;
		private static readonly Dictionary<EventInfo, ApiEventAttribute> s_EventToAttribute;

		private static readonly SafeCriticalSection s_AttributeNameToEventSection;
		private static readonly SafeCriticalSection s_TypeToEventsSection;
		private static readonly SafeCriticalSection s_EventToAttributeSection;

		/// <summary>
		/// Constructor.
		/// </summary>
		static ApiEventAttribute()
		{
			s_AttributeNameToEvent = new Dictionary<Type, Dictionary<string, EventInfo>>();
			s_TypeToEvents = new Dictionary<Type, EventInfo[]>();
			s_EventToAttribute = new Dictionary<EventInfo, ApiEventAttribute>();

			s_AttributeNameToEventSection = new SafeCriticalSection();
			s_TypeToEventsSection = new SafeCriticalSection();
			s_EventToAttributeSection = new SafeCriticalSection();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		public ApiEventAttribute(string name, string help)
			: base(name, help)
		{
		}

		#region Events

		/// <summary>
		/// Gets the event info for the given member.
		/// </summary>
		/// <param name="eventInfo"></param>
		/// <returns></returns>
		public static ApiEventInfo GetInfo(EventInfo eventInfo)
		{
			return GetInfo(eventInfo, null);
		}

		/// <summary>
		/// Gets the event info for the given member.
		/// </summary>
		/// <param name="eventInfo"></param>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static ApiEventInfo GetInfo(EventInfo eventInfo, object instance)
		{
			return GetInfo(eventInfo, instance, int.MaxValue);
		}

		/// <summary>
		/// Gets the event info for the given member.
		/// </summary>
		/// <param name="eventInfo"></param>
		/// <param name="instance"></param>
		/// <param name="depth"></param>
		/// <returns></returns>
		public static ApiEventInfo GetInfo(EventInfo eventInfo, object instance, int depth)
		{
			ApiEventAttribute attribute = eventInfo == null ? null : GetAttribute(eventInfo);
			return new ApiEventInfo(attribute, eventInfo, instance, depth);
		}

		/// <summary>
		/// Gets the EventInfo for the given API info.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		[CanBeNull]
		public static EventInfo GetEvent(ApiEventInfo info, Type type)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			if (type == null)
				throw new ArgumentNullException("type");

			s_AttributeNameToEventSection.Enter();

			try
			{
				return CacheType(type).GetDefault(info.Name, null);
			}
			finally
			{
				s_AttributeNameToEventSection.Leave();
			}
		}

		#endregion

		#region Private Events

		[NotNull]
		private static Dictionary<string, EventInfo> CacheType(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			s_AttributeNameToEventSection.Enter();

			try
			{
				Dictionary<string, EventInfo> eventMap;
				if (!s_AttributeNameToEvent.TryGetValue(type, out eventMap))
				{
					eventMap = new Dictionary<string, EventInfo>();
					s_AttributeNameToEvent.Add(type, eventMap);

					foreach (EventInfo eventInfo in GetEvents(type))
					{
						ApiEventAttribute attribute = GetAttribute(eventInfo);
						if (attribute == null)
							continue;

						eventMap.Add(attribute.Name, eventInfo);
					}
				}

				return eventMap;
			}
			finally
			{
				s_AttributeNameToEventSection.Leave();
			}
		}

		public static IEnumerable<EventInfo> GetEvents(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			s_TypeToEventsSection.Enter();

			try
			{
				EventInfo[] events;
				if (!s_TypeToEvents.TryGetValue(type, out events))
				{
					events =
						type.GetAllTypes()
						    .SelectMany(t =>
#if SIMPLSHARP
						                ((CType)t)
#else
										t.GetTypeInfo()
#endif
							                .GetEvents(BindingFlags))
						    .Where(m => GetAttribute(m) != null)
						    .ToArray();

					s_TypeToEvents.Add(type, events);
				}

				return events;
			}
			finally
			{
				s_TypeToEventsSection.Leave();
			}
		}

		[CanBeNull]
		public static ApiEventAttribute GetAttribute(EventInfo eventInfo)
		{
			if (eventInfo == null)
				throw new ArgumentNullException("eventInfo");

			s_EventToAttributeSection.Enter();

			try
			{
				ApiEventAttribute attribute;
				if (!s_EventToAttribute.TryGetValue(eventInfo, out attribute))
				{
					attribute = eventInfo.GetCustomAttributes<ApiEventAttribute>(true).FirstOrDefault();
					s_EventToAttribute.Add(eventInfo, attribute);
				}

				return attribute;
			}
			finally
			{
				s_EventToAttributeSection.Leave();
			}
		}

		#endregion
	}
}
