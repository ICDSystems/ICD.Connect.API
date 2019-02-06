using System;
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
	}
}
