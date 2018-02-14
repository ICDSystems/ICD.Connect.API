using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Connect.API.Attributes;
using ICD.Common.Utils.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.API.Info
{
	public sealed class ApiClassInfo : AbstractApiInfo
	{
		private const string PROPERTY_METHODS = "methods";
		private const string PROPERTY_PROPERTIES = "properties";
		private const string PROPERTY_NODES = "nodes";
		private const string PROPERTY_NODEGROUPS = "nodeGroups";

		private readonly List<ApiMethodInfo> m_Methods;
		private readonly List<ApiPropertyInfo> m_Properties;
		private readonly List<ApiNodeInfo> m_Nodes;
		private readonly List<ApiNodeGroupInfo> m_NodeGroups; 

		/// <summary>
		/// Constructor.
		/// </summary>
		public ApiClassInfo()
			: this(null, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		/// <param name="type"></param>
		public ApiClassInfo(ApiClassAttribute attribute, Type type)
			: this(attribute, type, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		public ApiClassInfo(ApiClassAttribute attribute, Type type, object instance)
			: base(attribute)
		{
			type = instance == null ? type : instance.GetType();

			IEnumerable<ApiMethodInfo> parameters = GetMethodInfo(type, instance);
			m_Methods = new List<ApiMethodInfo>(parameters);

			IEnumerable<ApiPropertyInfo> properties = GetPropertyInfo(type, instance);
			m_Properties = new List<ApiPropertyInfo>(properties);

			IEnumerable<ApiNodeInfo> nodes = GetNodeInfo(type, instance);
			m_Nodes = new List<ApiNodeInfo>(nodes);

			IEnumerable<ApiNodeGroupInfo> nodeGroups = GetNodeGroupInfo(type, instance);
			m_NodeGroups = new List<ApiNodeGroupInfo>(nodeGroups);

			// Pull the name from the instance
			if ((attribute == null || string.IsNullOrEmpty(attribute.Name)) && instance != null)
				Name = instance.GetType().Name;
		}

		#region Methods

		/// <summary>
		/// Clears methods, properties, nodes etc while leaving the name and help intact.
		/// </summary>
		public void ClearChildren()
		{
			ClearMethods();
			ClearProperties();
			ClearNodes();
			ClearNodeGroups();
		}

		/// <summary>
		/// Creates a recursive copy of the API info.
		/// </summary>
		/// <returns></returns>
		public ApiClassInfo DeepCopy()
		{
			ApiClassInfo output = new ApiClassInfo();
			DeepCopy(output);

			output.SetMethods(GetMethods().Select(m => m.DeepCopy()));
			output.SetProperties(GetProperties().Select(p => p.DeepCopy()));
			output.SetNodes(GetNodes().Select(n => n.DeepCopy()));
			output.SetNodeGroups(GetNodeGroups().Select(n => n.DeepCopy()));

			return output;
		}

		#region Methods

		/// <summary>
		/// Clears the methods for this class.
		/// </summary>
		public void ClearMethods()
		{
			SetMethods(Enumerable.Empty<ApiMethodInfo>());
		}

		/// <summary>
		/// Gets the methods for this class.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ApiMethodInfo> GetMethods()
		{
			return m_Methods.ToArray(m_Methods.Count);
		}

		/// <summary>
		/// Sets the methods for this class.
		/// </summary>
		/// <param name="methods"></param>
		public void SetMethods(IEnumerable<ApiMethodInfo> methods)
		{
			m_Methods.Clear();
			m_Methods.AddRange(methods);
		}

		/// <summary>
		/// Adds the method to the collection.
		/// </summary>
		/// <param name="method"></param>
		public void AddMethod(ApiMethodInfo method)
		{
			m_Methods.Add(method);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Clears the properties for this class.
		/// </summary>
		public void ClearProperties()
		{
			SetProperties(Enumerable.Empty<ApiPropertyInfo>());
		}

		/// <summary>
		/// Gets the properties for this class.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ApiPropertyInfo> GetProperties()
		{
			return m_Properties.ToArray(m_Properties.Count);
		}

		/// <summary>
		/// Sets the properties for this class.
		/// </summary>
		/// <param name="properties"></param>
		public void SetProperties(IEnumerable<ApiPropertyInfo> properties)
		{
			m_Properties.Clear();
			m_Properties.AddRange(properties);
		}

		/// <summary>
		/// Adds the property to the collection.
		/// </summary>
		/// <param name="property"></param>
		public void AddProperty(ApiPropertyInfo property)
		{
			m_Properties.Add(property);
		}

		#endregion

		#region Nodes

		/// <summary>
		/// Clears the nodes for this class.
		/// </summary>
		public void ClearNodes()
		{
			SetNodes(Enumerable.Empty<ApiNodeInfo>());
		}

		/// <summary>
		/// Gets the nodes for this class.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ApiNodeInfo> GetNodes()
		{
			return m_Nodes.ToArray(m_Nodes.Count);
		}

		/// <summary>
		/// Sets the nodes for this class.
		/// </summary>
		/// <param name="nodes"></param>
		public void SetNodes(IEnumerable<ApiNodeInfo> nodes)
		{
			m_Nodes.Clear();
			m_Nodes.AddRange(nodes);
		}

		/// <summary>
		/// Adds the node to the collection.
		/// </summary>
		/// <param name="node"></param>
		public void AddNode(ApiNodeInfo node)
		{
			m_Nodes.Add(node);
		}

		#endregion

		#region Node Groups

		/// <summary>
		/// Clears the node groups for this class.
		/// </summary>
		public void ClearNodeGroups()
		{
			SetNodeGroups(Enumerable.Empty<ApiNodeGroupInfo>());
		}

		/// <summary>
		/// Gets the node groups for this class.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ApiNodeGroupInfo> GetNodeGroups()
		{
			return m_NodeGroups.ToArray(m_NodeGroups.Count);
		}

		/// <summary>
		/// Sets the node groups for this class.
		/// </summary>
		/// <param name="nodeGroups"></param>
		public void SetNodeGroups(IEnumerable<ApiNodeGroupInfo> nodeGroups)
		{
			m_NodeGroups.Clear();
			m_NodeGroups.AddRange(nodeGroups);
		}

		/// <summary>
		/// Adds the node group to the collection.
		/// </summary>
		/// <param name="nodeGroup"></param>
		public void AddNodeGroup(ApiNodeGroupInfo nodeGroup)
		{
			m_NodeGroups.Add(nodeGroup);
		}

		#endregion

		#endregion

		#region Private Methods

		private IEnumerable<ApiPropertyInfo> GetPropertyInfo(Type type, object instance)
		{
			if (type == null)
				yield break;

			foreach (PropertyInfo property in ApiPropertyAttribute.GetProperties(type))
			{
				ApiPropertyAttribute attribute = ApiPropertyAttribute.GetAttribute(property);
				if (attribute != null)
					yield return new ApiPropertyInfo(attribute, property, instance);
			}
		}

		private IEnumerable<ApiMethodInfo> GetMethodInfo(Type type, object instance)
		{
			if (type == null)
				yield break;

			foreach (MethodInfo method in ApiMethodAttribute.GetMethods(type))
			{
				ApiMethodAttribute attribute = ApiMethodAttribute.GetAttribute(method);
				if (attribute != null)
					yield return new ApiMethodInfo(attribute, method, instance);
			}
		}

		private IEnumerable<ApiNodeInfo> GetNodeInfo(Type type, object instance)
		{
			if (type == null)
				yield break;

			foreach (PropertyInfo property in ApiNodeAttribute.GetProperties(type))
			{
				ApiNodeAttribute attribute = ApiNodeAttribute.GetAttribute(property);
				if (attribute != null)
					yield return new ApiNodeInfo(attribute, property, instance);
			}
		}

		private IEnumerable<ApiNodeGroupInfo> GetNodeGroupInfo(Type type, object instance)
		{
			if (type == null)
				yield break;

			foreach (PropertyInfo property in ApiNodeGroupAttribute.GetProperties(type))
			{
				ApiNodeGroupAttribute attribute = ApiNodeGroupAttribute.GetAttribute(property);
				if (attribute != null)
					yield return new ApiNodeGroupInfo(attribute, property, instance);
			}
		}

		#endregion

		#region Serialization

		/// <summary>
		/// Override to serialize additional properties to the JSON.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteProperties(JsonWriter writer)
		{
			base.WriteProperties(writer);

			// Methods
			if (m_Methods.Count > 0)
			{
				writer.WritePropertyName(PROPERTY_METHODS);
				writer.WriteStartArray();
				{
					foreach (ApiMethodInfo method in m_Methods)
						method.Serialize(writer);
				}
				writer.WriteEndArray();
			}

			// Properties
			if (m_Properties.Count > 0)
			{
				writer.WritePropertyName(PROPERTY_PROPERTIES);
				writer.WriteStartArray();
				{
					foreach (ApiPropertyInfo property in m_Properties)
						property.Serialize(writer);
				}
				writer.WriteEndArray();
			}

			// Nodes
			if (m_Nodes.Count > 0)
			{
				writer.WritePropertyName(PROPERTY_NODES);
				writer.WriteStartArray();
				{
					foreach (ApiNodeInfo node in m_Nodes)
						node.Serialize(writer);
				}
				writer.WriteEndArray();
			}

			// Node Groups
			if (m_NodeGroups.Count > 0)
			{
				writer.WritePropertyName(PROPERTY_NODEGROUPS);
				writer.WriteStartArray();
				{
					foreach (ApiNodeGroupInfo nodeGroup in m_NodeGroups)
						nodeGroup.Serialize(writer);
				}
				writer.WriteEndArray();
			}
		}

		/// <summary>
		/// Deserializes the JSON string to an ApiClassInfo instance.
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		public static ApiClassInfo Deserialize(string json)
		{
			JObject jObject = JObject.Parse(json);
			return Deserialize(jObject);
		}

		/// <summary>
		/// Instanties a new instance and applies the JSON object.
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public static ApiClassInfo Deserialize(JToken token)
		{
			ApiClassInfo instance = new ApiClassInfo();
			Deserialize(instance, token);
			return instance;
		}

		/// <summary>
		/// Applies the JSON object info to the given instance.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		public static void Deserialize(ApiClassInfo instance, JToken token)
		{
			// Methods
			JToken methods = token[PROPERTY_METHODS];
			if (methods != null)
			{
				IEnumerable<ApiMethodInfo> methodInfo = methods.Select(m => ApiMethodInfo.Deserialize(m));
				instance.SetMethods(methodInfo);
			}

			// Properties
			JToken properties = token[PROPERTY_PROPERTIES];
			if (properties != null)
			{
				IEnumerable<ApiPropertyInfo> propertyInfo = properties.Select(p => ApiPropertyInfo.Deserialize(p));
				instance.SetProperties(propertyInfo);
			}

			// Nodes
			JToken nodes = token[PROPERTY_NODES];
			if (nodes != null)
			{
				IEnumerable<ApiNodeInfo> nodeInfo = nodes.Select(m => ApiNodeInfo.Deserialize(m));
				instance.SetNodes(nodeInfo);
			}

			// Node Groups
			JToken nodeGroups = token[PROPERTY_NODEGROUPS];
			if (nodeGroups != null)
			{
				IEnumerable<ApiNodeGroupInfo> nodeGroupInfo = nodeGroups.Select(m => ApiNodeGroupInfo.Deserialize(m));
				instance.SetNodeGroups(nodeGroupInfo);
			}

			AbstractApiInfo.Deserialize(instance, token);
		}

		#endregion
	}
}