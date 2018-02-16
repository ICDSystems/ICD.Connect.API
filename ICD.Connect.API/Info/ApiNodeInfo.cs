using ICD.Common.Properties;
using Newtonsoft.Json;
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
		/// <summary>
		/// Gets/sets the node.
		/// </summary>
		public ApiClassInfo Node { get; set; }

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

		/// <summary>
		/// Creates a recursive copy of the API info.
		/// </summary>
		/// <returns></returns>
		public ApiNodeInfo DeepCopy()
		{
			ApiNodeInfo output = new ApiNodeInfo
			{
				Node = Node == null ? null : Node.DeepCopy()
			};

			DeepCopy(output);
			return output;
		}

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
	}
}
