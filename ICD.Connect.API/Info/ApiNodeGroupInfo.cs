using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Nodes;
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
	public sealed class ApiNodeGroupInfo : AbstractApiInfo
	{
		private const string PROPERTY_NODES = "nodes";

		private readonly Dictionary<uint, ApiClassInfo> m_Nodes;

		public ApiClassInfo this[uint key] { get { return m_Nodes[key]; } set { m_Nodes[key] = value; } }

		public int Count { get { return m_Nodes.Count; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ApiNodeGroupInfo()
			: this(null, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		/// <param name="property"></param>
		public ApiNodeGroupInfo(ApiNodeGroupAttribute attribute, PropertyInfo property)
			: this(attribute, property, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		/// <param name="property"></param>
		/// <param name="instance"></param>
		public ApiNodeGroupInfo(ApiNodeGroupAttribute attribute, PropertyInfo property, object instance)
			: base(attribute)
		{
			m_Nodes = new Dictionary<uint, ApiClassInfo>();

			IEnumerable<KeyValuePair<uint, ApiClassInfo>> nodes = GetNodes(property, instance);
			SetNodes(nodes);
		}

		/// <summary>
		/// Gets the nodes class info for the given property.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="instance"></param>
		/// <returns></returns>
		[CanBeNull]
		private IEnumerable<KeyValuePair<uint, ApiClassInfo>> GetNodes(PropertyInfo property, object instance)
		{
			if (instance == null)
				return Enumerable.Empty<KeyValuePair<uint, ApiClassInfo>>();

			if (property == null || !property.CanRead)
				return Enumerable.Empty<KeyValuePair<uint, ApiClassInfo>>();

			IApiNodeGroup nodeGroup = property.GetValue(instance, new object[0]) as IApiNodeGroup;
			if (nodeGroup == null)
				return Enumerable.Empty<KeyValuePair<uint, ApiClassInfo>>();

			return nodeGroup.Select(kvp =>
			                        {
				                        uint key = kvp.Key;
				                        object value = kvp.Value;
				                        Type type = value == null ? null : value.GetType();

				                        ApiClassAttribute attribute = ApiClassAttribute.GetClassAttributeForType(type);
				                        ApiClassInfo info = attribute == null ? null : attribute.GetInfo(type, value);

				                        return new KeyValuePair<uint, ApiClassInfo>(key, info);
			                        })
			                .Where(kvp => kvp.Value != null);
		}

		/// <summary>
		/// Gets the nodes from this group.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<KeyValuePair<uint, ApiClassInfo>> GetNodes()
		{
			return m_Nodes.ToArray(m_Nodes.Count);
		}

		/// <summary>
		/// Sets the nodes for this group.
		/// </summary>
		/// <param name="nodes"></param>
		public void SetNodes(IEnumerable<KeyValuePair<uint, ApiClassInfo>> nodes)
		{
			m_Nodes.Clear();
			m_Nodes.AddRange(nodes);
		}

		/// <summary>
		/// Adds the node to the group.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="node"></param>
		public void AddNode(uint key, ApiClassInfo node)
		{
			m_Nodes.Add(key, node);
		}

		#region Serialization

		/// <summary>
		/// Override to serialize additional properties to the JSON.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteProperties(JsonWriter writer)
		{
			base.WriteProperties(writer);

			// Nodes
			if (m_Nodes.Count != 0)
			{
				writer.WritePropertyName(PROPERTY_NODES);
				writer.WriteStartObject();
				{
					foreach (KeyValuePair<uint, ApiClassInfo> kvp in m_Nodes.OrderByKey())
					{
						writer.WritePropertyName(kvp.Key.ToString());
						kvp.Value.Serialize(writer);
					}
				}
				writer.WriteEndObject();
			}
		}

		/// <summary>
		/// Deserializes the JSON string to an ApiNodeGroupInfo instance.
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		public static ApiNodeGroupInfo Deserialize(string json)
		{
			JObject jObject = JObject.Parse(json);
			return Deserialize(jObject);
		}

		/// <summary>
		/// Instanties a new instance and applies the JSON object.
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public static ApiNodeGroupInfo Deserialize(JToken token)
		{
			ApiNodeGroupInfo instance = new ApiNodeGroupInfo();
			Deserialize(instance, token);
			return instance;
		}

		/// <summary>
		/// Applies the JSON object info to the given instance.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		public static void Deserialize(ApiNodeGroupInfo instance, JToken token)
		{
			// Nodes
			JObject nodes = token[PROPERTY_NODES] as JObject;
			if (nodes != null)
			{
				foreach (KeyValuePair<string, JToken> kvp in nodes)
				{
					uint key = uint.Parse(kvp.Key);
					ApiClassInfo value = ApiClassInfo.Deserialize(kvp.Value);

					instance.AddNode(key, value);
				}
			}

			AbstractApiInfo.Deserialize(instance, token);
		}

		#endregion
	}
}
