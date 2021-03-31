using System;
using System.Collections.Generic;
using ICD.Common.Utils;
using ICD.Connect.API.Attributes;
using ICD.Connect.API.Info.Converters;
using Newtonsoft.Json;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.API.Info
{
	[JsonConverter(typeof(ApiEventInfoConverter))]
	public sealed class ApiEventInfo : AbstractApiInfo
	{
		public enum eSubscribeAction
		{
			None,
			Subscribe,
			Unsubscribe
		}

		/// <summary>
		/// Gets/sets the subscribe action for this command.
		/// </summary>
		public eSubscribeAction SubscribeAction { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ApiEventInfo()
			: this(null, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		/// <param name="eventInfo"></param>
		public ApiEventInfo(ApiEventAttribute attribute, EventInfo eventInfo)
			: this(attribute, eventInfo, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		/// <param name="eventInfo"></param>
		/// <param name="instance"></param>
		public ApiEventInfo(ApiEventAttribute attribute, EventInfo eventInfo, object instance)
			: this(attribute, eventInfo, instance, int.MaxValue)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		/// <param name="eventInfo"></param>
		/// <param name="instance"></param>
		/// <param name="depth"></param>
		public ApiEventInfo(ApiEventAttribute attribute, EventInfo eventInfo, object instance, int depth)
			: base(attribute)
		{
		}

		/// <summary>
		/// Adds the given item as an immediate child to this node.
		/// </summary>
		/// <param name="child"></param>
		protected override void AddChild(IApiInfo child)
		{
			if (child == null)
				throw new ArgumentNullException("child");

			throw new ArgumentException(string.Format("{0} can not add child of type {1}", GetType(), child.GetType()));
		}

		/// <summary>
		/// Gets the children attached to this node.
		/// </summary>
		/// <returns></returns>
		protected override IEnumerable<IApiInfo> GetChildren()
		{
			yield break;
		}

		/// <summary>
		/// Copies the current state onto the given instance.
		/// </summary>
		/// <param name="info"></param>
		protected override void ShallowCopy(IApiInfo info)
		{
			base.ShallowCopy(info);

			ApiEventInfo apiEventInfo = info as ApiEventInfo;
			if (apiEventInfo == null)
				throw new ArgumentException("info");

			apiEventInfo.SubscribeAction = SubscribeAction;
		}

		/// <summary>
		/// Creates a new instance of the current type.
		/// </summary>
		/// <returns></returns>
		protected override AbstractApiInfo Instantiate()
		{
			return new ApiEventInfo();
		}

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="requestor"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		/// <param name="path"></param>
		public void HandleEventRequest(IApiRequestor requestor, Type type, object instance, Stack<IApiInfo> path)
		{
			type = instance == null ? type : instance.GetType();
			EventInfo eventInfo = ApiEventAttribute.GetEvent(this, type);

			// Couldn't find an ApiEventAttribute for the given info.
			if (eventInfo == null)
			{
				Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.MissingMember };
				Result.SetValue(string.Format("No event with name {0}.", StringUtils.ToRepresentation(Name)));
				return;
			}

			path.Push(this);

			try
			{
				switch (SubscribeAction)
				{
					case eSubscribeAction.None:
						// We're not doing anything with the event so return info.
						ApiEventInfo resultInfo = ApiEventAttribute.GetInfo(eventInfo, instance, 3);
						Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.Ok };
						Result.SetValue(resultInfo);
						return;

					case eSubscribeAction.Subscribe:
						// Subscribe to the event
						Result = Subscribe(requestor, eventInfo, instance, path);
						return;

					case eSubscribeAction.Unsubscribe:
						// Unsubscribe from the event
						Result = Unsubscribe(requestor, instance, path);
						return;

					default:
						throw new ArgumentOutOfRangeException("SubscribeAction", "Unknown subscribe action");
				}
			}
			finally
			{
				path.Pop();
			}
		}

		public ApiResult Subscribe(IApiRequestor requestor, EventInfo eventInfo, object instance, Stack<IApiInfo> path)
		{
			if (eventInfo == null)
				throw new ArgumentNullException("eventInfo");

			if (instance == null)
				throw new ArgumentNullException("instance");

			if (path == null)
				throw new ArgumentNullException("path");

			try
			{
				ApiFeedbackCache.Subscribe(requestor, eventInfo, instance, path);
			}
			catch (Exception e)
			{
				ApiResult output = new ApiResult { ErrorCode = ApiResult.eErrorCode.Exception };
				output.SetValue(string.Format("Failed to subscribe to {0} - {1}", Name, e.Message));
				return output;
			}

			return new ApiResult { ErrorCode = ApiResult.eErrorCode.Ok };
		}

		public ApiResult Unsubscribe(IApiRequestor requestor, object instance, Stack<IApiInfo> path)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			if (path == null)
				throw new ArgumentNullException("path");

			try
			{
				ApiFeedbackCache.Unsubscribe(requestor, instance, path);
			}
			catch (Exception e)
			{
				ApiResult output = new ApiResult { ErrorCode = ApiResult.eErrorCode.Exception };
				output.SetValue(string.Format("Failed to unsubscribe from {0} - {1}", Name, e.Message));
				return output;
			}

			return new ApiResult { ErrorCode = ApiResult.eErrorCode.Ok };
		}
	}
}
