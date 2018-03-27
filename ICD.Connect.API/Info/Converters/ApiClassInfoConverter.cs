using System;
using ICD.Common.Utils.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace ICD.Connect.API.Info.Converters
{
	public sealed class ApiClassInfoConverter : AbstractApiInfoConverter<ApiClassInfo>
	{
		private const string PROPERTY_PROXYTYPES = "proxyTypes";
		private const string PROPERTY_METHODS = "methods";
		private const string PROPERTY_PROPERTIES = "properties";
		private const string PROPERTY_NODES = "nodes";
		private const string PROPERTY_NODEGROUPS = "nodeGroups";

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

			// Proxy Types
			Type[] types = value.GetProxyTypes().ToArray();
			if (types.Length > 0)
			{
				writer.WritePropertyName(PROPERTY_PROXYTYPES);
				serializer.SerializeArray(writer, types, (s, w, item) => w.WriteType(item));
			}

			// Methods
			ApiMethodInfo[] methods = value.GetMethods().ToArray();
			if (methods.Length > 0)
			{
				writer.WritePropertyName(PROPERTY_METHODS);
				serializer.SerializeArray(writer, methods);
			}

			// Properties
			ApiPropertyInfo[] properties = value.GetProperties().ToArray();
			if (properties.Length > 0)
			{
				writer.WritePropertyName(PROPERTY_PROPERTIES);
				serializer.SerializeArray(writer, properties);
			}

			// Nodes
			ApiNodeInfo[] nodes = value.GetNodes().ToArray();
			if (nodes.Length > 0)
			{
				writer.WritePropertyName(PROPERTY_NODES);
				serializer.SerializeArray(writer, nodes);
			}

			// Node Groups
			ApiNodeGroupInfo[] nodeGroups = value.GetNodeGroups().ToArray();
			if (nodeGroups.Length > 0)
			{
				writer.WritePropertyName(PROPERTY_NODEGROUPS);
				serializer.SerializeArray(writer, nodeGroups);
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
				case PROPERTY_PROXYTYPES:
					IEnumerable<Type> proxyTypes = serializer.DeserializeArray(reader, (s, r) => r.GetValueAsType());
					instance.SetProxyTypes(proxyTypes);
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
			}
		}
	}
}
