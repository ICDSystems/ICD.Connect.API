﻿using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Extensions;
using Newtonsoft.Json;

namespace ICD.Connect.API.Info.Converters
{
	public sealed class ApiNodeGroupInfoConverter : AbstractApiInfoConverter<ApiNodeGroupInfo>
	{
		private const string PROPERTY_NODES = "nodes";

		/// <summary>
		/// Creates a new instance of ApiNodeInfo.
		/// </summary>
		/// <returns></returns>
		protected override ApiNodeGroupInfo Instantiate()
		{
			return new ApiNodeGroupInfo();
		}

		/// <summary>
		/// Override to serialize additional properties to the JSON.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		protected override void WriteProperties(JsonWriter writer, ApiNodeGroupInfo value, JsonSerializer serializer)
		{
			base.WriteProperties(writer, value, serializer);

			// Nodes
			KeyValuePair<uint, ApiClassInfo>[] nodes = value.GetNodes().ToArray();
			if (nodes.Length != 0)
			{
				writer.WritePropertyName(PROPERTY_NODES);
				serializer.SerializeDictionary(writer, nodes.OrderByKey());
			}
		}

		/// <summary>
		/// Override to handle the current property value with the given name.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="reader"></param>
		/// <param name="instance"></param>
		/// <param name="serializer"></param>
		protected override void ReadProperty(string property, JsonReader reader, ApiNodeGroupInfo instance,
		                                     JsonSerializer serializer)
		{
			switch (property)
			{
				case PROPERTY_NODES:
					IEnumerable<KeyValuePair<uint, ApiClassInfo>> nodes = serializer.DeserializeDictionary<uint, ApiClassInfo>(reader);
					instance.SetNodes(nodes);
					break;
			}
		}
	}
}
