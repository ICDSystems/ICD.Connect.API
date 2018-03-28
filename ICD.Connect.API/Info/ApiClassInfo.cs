using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Connect.API.Attributes;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Info.Converters;
using Newtonsoft.Json;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.API.Info
{
	[JsonConverter(typeof(ApiClassInfoConverter))]
	public sealed class ApiClassInfo : AbstractApiInfo
	{
		private readonly List<Type> m_ProxyTypes;
		private readonly Dictionary<string, ApiMethodInfo> m_Methods;
		private readonly Dictionary<string, ApiPropertyInfo> m_Properties;
		private readonly Dictionary<string, ApiNodeInfo> m_Nodes;
		private readonly Dictionary<string, ApiNodeGroupInfo> m_NodeGroups; 

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
			: this(attribute, type, instance, int.MaxValue)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		/// <param name="depth"></param>
		public ApiClassInfo(ApiClassAttribute attribute, Type type, object instance, int depth)
			: base(attribute)
		{
			m_ProxyTypes = new List<Type>();
			m_Methods = new Dictionary<string, ApiMethodInfo>();
			m_Properties = new Dictionary<string, ApiPropertyInfo>();
			m_Nodes = new Dictionary<string, ApiNodeInfo>();
			m_NodeGroups = new Dictionary<string, ApiNodeGroupInfo>();

			if (depth <= 0)
				return;

			// Pull the name from the instance
			if ((attribute == null || string.IsNullOrEmpty(attribute.Name)) && instance != null)
				Name = instance.GetType().Name;

			type = instance == null ? type : instance.GetType();

			IEnumerable<Type> proxyTypes = GetProxyTypes(type);
			IEnumerable<ApiMethodInfo> methods = GetMethodInfo(type, instance, depth - 1);
			IEnumerable<ApiPropertyInfo> properties = GetPropertyInfo(type, instance, depth - 1);
			IEnumerable<ApiNodeInfo> nodes = GetNodeInfo(type, instance, depth - 1);
			IEnumerable<ApiNodeGroupInfo> nodeGroups = GetNodeGroupInfo(type, instance, depth - 1);

			SetProxyTypes(proxyTypes);
			SetMethods(methods);
			SetProperties(properties);
			SetNodes(nodes);
			SetNodeGroups(nodeGroups);
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
			output.Update(this);
			return output;
		}

		/// <summary>
		/// Copies the values from the other instance.
		/// </summary>
		/// <param name="info"></param>
		public void Update(ApiClassInfo info)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			SetProxyTypes(info.GetProxyTypes());
			SetMethods(info.GetMethods().Select(m => m.DeepCopy()));
			SetProperties(info.GetProperties().Select(p => p.DeepCopy()));
			SetNodes(info.GetNodes().Select(n => n.DeepCopy()));
			SetNodeGroups(info.GetNodeGroups().Select(n => n.DeepCopy()));
		}

		#region ProxyTypes

		/// <summary>
		/// Clears the proxyTypes for this class.
		/// </summary>
		public void ClearProxyTypes()
		{
			SetProxyTypes(Enumerable.Empty<Type>());
		}

		/// <summary>
		/// Gets the proxyTypes for this class.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<Type> GetProxyTypes()
		{
			return m_ProxyTypes.ToArray(m_ProxyTypes.Count);
		}

		/// <summary>
		/// Sets the proxyTypes for this class.
		/// </summary>
		/// <param name="proxyTypes"></param>
		public void SetProxyTypes(IEnumerable<Type> proxyTypes)
		{
			m_ProxyTypes.Clear();
			m_ProxyTypes.AddRange(proxyTypes.Except((Type)null).Distinct());
		}

		/// <summary>
		/// Adds the proxyType to the collection.
		/// </summary>
		/// <param name="proxyType"></param>
		public void AddProxyType(Type proxyType)
		{
			if (!m_ProxyTypes.Contains(proxyType))
				m_ProxyTypes.Add(proxyType);
		}

		#endregion

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
			return m_Methods.Select(kvp => kvp.Value).ToArray(m_Methods.Count);
		}

		/// <summary>
		/// Sets the methods for this class.
		/// </summary>
		/// <param name="methods"></param>
		public void SetMethods(IEnumerable<ApiMethodInfo> methods)
		{
			m_Methods.Clear();
			m_Methods.AddRange(methods, m => m.Name);
		}

		/// <summary>
		/// Adds the method to the collection.
		/// </summary>
		/// <param name="method"></param>
		public void AddMethod(ApiMethodInfo method)
		{
			m_Methods.Add(method.Name, method);
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
			return m_Properties.Select(kvp => kvp.Value).ToArray(m_Properties.Count);
		}

		/// <summary>
		/// Sets the properties for this class.
		/// </summary>
		/// <param name="properties"></param>
		public void SetProperties(IEnumerable<ApiPropertyInfo> properties)
		{
			m_Properties.Clear();
			m_Properties.AddRange(properties, p => p.Name);
		}

		/// <summary>
		/// Adds the property to the collection.
		/// </summary>
		/// <param name="property"></param>
		public void AddProperty(ApiPropertyInfo property)
		{
			m_Properties.Add(property.Name, property);
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
			return m_Nodes.Select(kvp => kvp.Value).ToArray(m_Nodes.Count);
		}

		/// <summary>
		/// Sets the nodes for this class.
		/// </summary>
		/// <param name="nodes"></param>
		public void SetNodes(IEnumerable<ApiNodeInfo> nodes)
		{
			m_Nodes.Clear();
			m_Nodes.AddRange(nodes, n => n.Name);
		}

		/// <summary>
		/// Adds the node to the collection.
		/// </summary>
		/// <param name="node"></param>
		public void AddNode(ApiNodeInfo node)
		{
			m_Nodes.Add(node.Name, node);
		}

		/// <summary>
		/// Gets the node with the given name, returns null if the node doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		[CanBeNull]
		public ApiNodeInfo GetNode(string name)
		{
			return m_Nodes.GetDefault(name, null);
		}

		/// <summary>
		/// Gets the class info at the node with the given name, returns null if the node or class don't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		[CanBeNull]
		public ApiClassInfo GetNodeClass(string name)
		{
			ApiNodeInfo node = GetNode(name);
			return node == null ? null : node.Node;
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
			return m_NodeGroups.Select(kvp => kvp.Value).ToArray(m_NodeGroups.Count);
		}

		/// <summary>
		/// Sets the node groups for this class.
		/// </summary>
		/// <param name="nodeGroups"></param>
		public void SetNodeGroups(IEnumerable<ApiNodeGroupInfo> nodeGroups)
		{
			m_NodeGroups.Clear();
			m_NodeGroups.AddRange(nodeGroups, n => n.Name);
		}

		/// <summary>
		/// Adds the node group to the collection.
		/// </summary>
		/// <param name="nodeGroup"></param>
		public void AddNodeGroup(ApiNodeGroupInfo nodeGroup)
		{
			m_NodeGroups.Add(nodeGroup.Name, nodeGroup);
		}

		/// <summary>
		/// Gets the node group with the given name, returns null if the node group doesn't exist.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		[CanBeNull]
		public ApiNodeGroupInfo GetNodeGroup(string name)
		{
			return m_NodeGroups.GetDefault(name, null);
		}

		#endregion

		#endregion

		#region Private Methods

		private IEnumerable<Type> GetProxyTypes(Type type)
		{
			if (type == null)
				return Enumerable.Empty<Type>();

			return ApiClassAttribute.GetProxyTypes(type);
		}

		private IEnumerable<ApiPropertyInfo> GetPropertyInfo(Type type, object instance, int depth)
		{
			if (type == null)
				yield break;

			if (depth <= 0)
				yield break;

			foreach (PropertyInfo property in ApiPropertyAttribute.GetProperties(type))
			{
				ApiPropertyAttribute attribute = ApiPropertyAttribute.GetAttribute(property);
				if (attribute != null)
					yield return new ApiPropertyInfo(attribute, property, instance, depth - 1);
			}
		}

		private IEnumerable<ApiMethodInfo> GetMethodInfo(Type type, object instance, int depth)
		{
			if (type == null)
				yield break;

			if (depth <= 0)
				yield break;

			foreach (MethodInfo method in ApiMethodAttribute.GetMethods(type))
			{
				ApiMethodAttribute attribute = ApiMethodAttribute.GetAttribute(method);
				if (attribute != null)
					yield return new ApiMethodInfo(attribute, method, instance, depth - 1);
			}
		}

		private IEnumerable<ApiNodeInfo> GetNodeInfo(Type type, object instance, int depth)
		{
			if (type == null)
				yield break;

			if (depth <= 0)
				yield break;

			foreach (PropertyInfo property in ApiNodeAttribute.GetProperties(type))
			{
				ApiNodeAttribute attribute = ApiNodeAttribute.GetAttribute(property);
				if (attribute != null)
					yield return new ApiNodeInfo(attribute, property, instance, depth - 1);
			}
		}

		private IEnumerable<ApiNodeGroupInfo> GetNodeGroupInfo(Type type, object instance, int depth)
		{
			if (type == null)
				yield break;

			if (depth <= 0)
				yield break;

			foreach (PropertyInfo property in ApiNodeGroupAttribute.GetProperties(type))
			{
				ApiNodeGroupAttribute attribute = ApiNodeGroupAttribute.GetAttribute(property);
				if (attribute != null)
					yield return new ApiNodeGroupInfo(attribute, property, instance, depth - 1);
			}
		}

		#endregion
	}
}