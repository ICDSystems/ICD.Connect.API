#if NETFRAMEWORK
extern alias RealNewtonsoft;
using RealNewtonsoft.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif
using ICD.Common.Utils.Extensions;
using System.Collections.Generic;

namespace ICD.Connect.API.Info.Converters
{
	public sealed class ApiClassInfoConverter : AbstractApiInfoConverter<ApiClassInfo>
	{
		private const string PROPERTY_IS_PROXY = "ip";
		private const string PROPERTY_PROXYTYPES = "pts";
		private const string PROPERTY_EVENTS = "es";
		private const string PROPERTY_METHODS = "ms";
		private const string PROPERTY_PROPERTIES = "ps";
		private const string PROPERTY_NODES = "ns";
		private const string PROPERTY_NODEGROUPS = "ngs";

		/// <summary>
		/// Creates a new instance of ApiParameterInfo.
		/// </summary>
		/// <returns></returns>
		protected override ApiClassInfo Instantiate()
		{
			return new ApiClassInfo();
		}

		/// <summary>
		/// Override to serialize additional properties to the JSON.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		protected override void WriteProperties(JsonWriter writer, ApiClassInfo value, JsonSerializer serializer)
		{
			base.WriteProperties(writer, value, serializer);

			// IsProxy
			if (value.IsProxy)
			{
				writer.WritePropertyName(PROPERTY_IS_PROXY);
				writer.WriteValue(value.IsProxy);
			}

			/*// Proxy Types
			if (value.ProxyTypeCount > 0)
			{
				writer.WritePropertyName(PROPERTY_PROXYTYPES);
				serializer.SerializeArray(writer, value.GetProxyTypes(), (s, w, item) => w.WriteType(item));
			}*/

			// Events
			if (value.EventCount > 0)
			{
				writer.WritePropertyName(PROPERTY_EVENTS);
				serializer.SerializeArray(writer, value.GetEvents());
			}

			// Methods
			if (value.MethodCount > 0)
			{
				writer.WritePropertyName(PROPERTY_METHODS);
				serializer.SerializeArray(writer, value.GetMethods());
			}

			// Properties
			if (value.PropertyCount > 0)
			{
				writer.WritePropertyName(PROPERTY_PROPERTIES);
				serializer.SerializeArray(writer, value.GetProperties());
			}

			// Nodes
			if (value.NodeCount > 0)
			{
				writer.WritePropertyName(PROPERTY_NODES);
				serializer.SerializeArray(writer, value.GetNodes());
			}

			// Node Groups
			if (value.NodeGroupCount > 0)
			{
				writer.WritePropertyName(PROPERTY_NODEGROUPS);
				serializer.SerializeArray(writer, value.GetNodeGroups());
			}
		}

		/// <summary>
		/// Override to handle the current property value with the given name.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="reader"></param>
		/// <param name="instance"></param>
		/// <param name="serializer"></param>
		protected override void ReadProperty(string property, JsonReader reader, ApiClassInfo instance,
		                                     JsonSerializer serializer)
		{
			switch (property)
			{
				case PROPERTY_IS_PROXY:
					instance.IsProxy = reader.GetValueAsBool();
					break;

				/*case PROPERTY_PROXYTYPES:
					// Skip proxy types that can't be deserialized.
					// This is by design - we fall back to the best known compatible type.
					IEnumerable<Type> proxyTypes = serializer.DeserializeArray(reader, (s, r) => r.GetValueAsType())
					                                         .Where(t => t != null);
					instance.SetProxyTypes(proxyTypes);
					break;*/

				case PROPERTY_EVENTS:
					IEnumerable<ApiEventInfo> events = serializer.DeserializeArray<ApiEventInfo>(reader);
					instance.SetEvents(events);
					break;

				case PROPERTY_METHODS:
					IEnumerable<ApiMethodInfo> methods = serializer.DeserializeArray<ApiMethodInfo>(reader);
					instance.SetMethods(methods);
					break;

				case PROPERTY_PROPERTIES:
					IEnumerable<ApiPropertyInfo> properties = serializer.DeserializeArray<ApiPropertyInfo>(reader);
					instance.SetProperties(properties);
					break;

				case PROPERTY_NODES:
					IEnumerable<ApiNodeInfo> nodes = serializer.DeserializeArray<ApiNodeInfo>(reader);
					instance.SetNodes(nodes);
					break;

				case PROPERTY_NODEGROUPS:
					IEnumerable<ApiNodeGroupInfo> nodeGroups = serializer.DeserializeArray<ApiNodeGroupInfo>(reader);
					instance.SetNodeGroups(nodeGroups);
					break;

				default:
					base.ReadProperty(property, reader, instance, serializer);
					break;
			}
		}
	}
}
