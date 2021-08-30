#if NETFRAMEWORK
extern alias RealNewtonsoft;
using RealNewtonsoft.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif
using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Connect.API.Attributes;
using ICD.Connect.API.Info.Converters;

namespace ICD.Connect.API.Info
{
	[JsonConverter(typeof(ApiNodeInfoConverter))]
	public sealed class ApiNodeInfo : AbstractApiInfo
	{
		#region Properties

		/// <summary>
		/// Gets/sets the node.
		/// </summary>
		public ApiClassInfo Node { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		public ApiNodeInfo()
			: this(null, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		/// <param name="property"></param>
		public ApiNodeInfo(ApiNodeAttribute attribute, PropertyInfo property)
			: this(attribute, property, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		/// <param name="property"></param>
		/// <param name="instance"></param>
		public ApiNodeInfo(ApiNodeAttribute attribute, PropertyInfo property, object instance)
			: this(attribute, property, instance, int.MaxValue)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		/// <param name="property"></param>
		/// <param name="instance"></param>
		/// <param name="depth"></param>
		public ApiNodeInfo(ApiNodeAttribute attribute, PropertyInfo property, object instance, int depth)
			: base(attribute)
		{
			if (depth <= 0)
				return;

			Node = GetClassInfo(property, instance, depth - 1);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets the class info for the given properties value.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="instance"></param>
		/// <param name="depth"></param>
		/// <returns></returns>
		[CanBeNull]
		private ApiClassInfo GetClassInfo(PropertyInfo property, object instance, int depth)
		{
			if (instance == null)
				return null;

			if (depth <= 0)
				return null;

			if (property == null || !property.CanRead)
				return null;

			return ApiClassAttribute.GetInfo(property.PropertyType, property.GetValue(instance, new object[0]), depth - 1);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Adds the given item as an immediate child to this node.
		/// </summary>
		/// <param name="child"></param>
		protected override void AddChild(IApiInfo child)
		{
			if (child == null)
				throw new ArgumentNullException("child");

			if (child is ApiClassInfo)
				Node = child as ApiClassInfo;
			else
				throw new ArgumentException(string.Format("{0} can not add child of type {1}", GetType(), child.GetType()));
		}

		protected override IEnumerable<IApiInfo> GetChildren()
		{
			if (Node != null)
				yield return Node;
		}

		/// <summary>
		/// Creates a new instance of the current type.
		/// </summary>
		/// <returns></returns>
		protected override AbstractApiInfo Instantiate()
		{
			return new ApiNodeInfo();
		}

		#endregion

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="requestor"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		/// <param name="path"></param>
		public void HandleNodeRequest(IApiRequestor requestor, Type type, object instance, Stack<IApiInfo> path)
		{
			type = instance == null ? type : instance.GetType();
			PropertyInfo property = ApiNodeAttribute.GetProperty(this, type);

			// Couldn't find an ApiNodeAttribute for the given info
			if (property == null)
			{
				Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.MissingMember };
				Result.SetValue(string.Format("No node property with name {0}.", StringUtils.ToRepresentation(Name)));
				Node = null;
				return;
			}

			path.Push(this);

			try
			{
				object nodeValue = property.GetValue(instance, null);

				// Found the ApiNodeAttribute but the property value was null
				if (nodeValue == null)
				{
					Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.MissingNode };
					Result.SetValue(string.Format("The node at property {0} is null.", StringUtils.ToRepresentation(Name)));
					Node = null;
					return;
				}

				Type nodeType = nodeValue.GetType();
				Node.HandleClassRequest(requestor, nodeType, nodeValue, path);
			}
			finally
			{
				path.Pop();
			}
		}
	}
}
