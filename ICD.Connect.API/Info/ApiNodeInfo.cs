using ICD.Common.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Connect.API.Attributes;

namespace ICD.Connect.API.Info
{
	public sealed class ApiNodeInfo : AbstractApiInfo
	{
		private const string PROPERTY_NODE = "node";

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
			: base(attribute)
		{
			Node = GetClassInfo(property, instance);
		}

		/// <summary>
		/// Gets the class info for the given properties value.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="instance"></param>
		/// <returns></returns>
		[CanBeNull]
		private ApiClassInfo GetClassInfo(PropertyInfo property, object instance)
		{
			if (instance == null)
				return null;

			if (property == null || !property.CanRead)
				return null;

			ApiClassAttribute classAttribute = ApiClassAttribute.GetClassAttributeForType(property.PropertyType);
			return classAttribute == null
				       ? null
				       : classAttribute.GetInfo(property.PropertyType, property.GetValue(instance, new object[0]));
		}

		#region Serialization

		/// <summary>
		/// Override to serialize additional properties to the JSON.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteProperties(JsonWriter writer)
		{
			base.WriteProperties(writer);

			if (Node != null)
			{
				writer.WritePropertyName(PROPERTY_NODE);
				Node.Serialize(writer);
			}
		}

		/// <summary>
		/// Deserializes the JSON string to an ApiNodeInfo instance.
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		public static ApiNodeInfo Deserialize(string json)
		{
			JObject jObject = JObject.Parse(json);
			return Deserialize(jObject);
		}

		/// <summary>
		/// Instanties a new instance and applies the JSON object.
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public static ApiNodeInfo Deserialize(JToken token)
		{
			ApiNodeInfo instance = new ApiNodeInfo();
			Deserialize(instance, token);
			return instance;
		}

		/// <summary>
		/// Applies the JSON object info to the given instance.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		public static void Deserialize(ApiNodeInfo instance, JToken token)
		{
			// Node
			JToken node = token[PROPERTY_NODE];
			if (node != null)
				instance.Node = ApiClassInfo.Deserialize(node);

			AbstractApiInfo.Deserialize(instance, token);
		}

		#endregion
	}
}
