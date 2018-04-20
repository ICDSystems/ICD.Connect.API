﻿using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Connect.API.Attributes;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Info.Converters;
using ICD.Connect.API.Proxies;
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
		[CanBeNull]
		private List<Type> m_ProxyTypes;

		[CanBeNull]
		private Dictionary<string, ApiEventInfo> m_Events;
		[CanBeNull]
		private Dictionary<string, ApiMethodInfo> m_Methods;
		[CanBeNull]
		private Dictionary<string, ApiPropertyInfo> m_Properties;
		[CanBeNull]
		private Dictionary<string, ApiNodeInfo> m_Nodes;
		[CanBeNull]
		private Dictionary<string, ApiNodeGroupInfo> m_NodeGroups;

		#region Properties

		public int ProxyTypeCount { get { return m_ProxyTypes == null ? 0 : m_ProxyTypes.Count; } }

		public int EventCount { get { return m_Events == null ? 0 : m_Events.Count; } }

		public int MethodCount { get { return m_Methods == null ? 0 : m_Methods.Count; } }

		public int PropertyCount { get { return m_Properties == null ? 0 : m_Properties.Count; } }

		public int NodeCount { get { return m_Nodes == null ? 0 : m_Nodes.Count; } }

		public int NodeGroupCount { get { return m_NodeGroups == null ? 0 : m_NodeGroups.Count; } }

		/// <summary>
		/// Returns true if there are no child info instances.
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return ProxyTypeCount +
					   EventCount +
					   MethodCount +
					   PropertyCount +
					   NodeCount +
					   NodeGroupCount == 0;
			}
		}

		/// <summary>
		/// Returns true if this class info represents a proxy instance.
		/// </summary>
		public bool IsProxy { get; set; }

		#endregion

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
			if (depth <= 0)
				return;

			// Pull the name from the instance
			if ((attribute == null || string.IsNullOrEmpty(attribute.Name)) && instance != null)
				Name = instance.GetType().Name;

			type = instance == null ? type : instance.GetType();

			if (type == null)
				return;

			IsProxy = type.IsAssignableTo(typeof(IProxy));

			IEnumerable<Type> proxyTypes = GetProxyTypes(type);
			IEnumerable<ApiEventInfo> events = GetEventInfo(type, instance, depth - 1);
			IEnumerable<ApiMethodInfo> methods = GetMethodInfo(type, instance, depth - 1);
			IEnumerable<ApiPropertyInfo> properties = GetPropertyInfo(type, instance, depth - 1);
			IEnumerable<ApiNodeInfo> nodes = GetNodeInfo(type, instance, depth - 1);
			IEnumerable<ApiNodeGroupInfo> nodeGroups = GetNodeGroupInfo(type, instance, depth - 1);

			SetProxyTypes(proxyTypes);
			SetEvents(events);
			SetMethods(methods);
			SetProperties(properties);
			SetNodes(nodes);
			SetNodeGroups(nodeGroups);
		}

		#region Methods

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
			return m_ProxyTypes == null
				       ? Enumerable.Empty<Type>()
				       : m_ProxyTypes.ToArray(m_ProxyTypes.Count);
		}

		/// <summary>
		/// Sets the proxyTypes for this class.
		/// </summary>
		/// <param name="proxyTypes"></param>
		public void SetProxyTypes(IEnumerable<Type> proxyTypes)
		{
			if (proxyTypes == null)
				throw new ArgumentNullException("proxyTypes");

			m_ProxyTypes = null;
			foreach (Type type in proxyTypes)
				AddProxyType(type);
		}

		/// <summary>
		/// Adds the proxyType to the collection.
		/// </summary>
		/// <param name="proxyType"></param>
		public void AddProxyType(Type proxyType)
		{
			if (proxyType == null)
				throw new ArgumentNullException("proxyType");

			if (m_ProxyTypes == null)
				m_ProxyTypes = new List<Type> {proxyType};
			else if (!m_ProxyTypes.Contains(proxyType))
				m_ProxyTypes.Add(proxyType);
		}

		#endregion

		#region Events

		/// <summary>
		/// Clears the events for this class.
		/// </summary>
		public void ClearEvents()
		{
			SetEvents(Enumerable.Empty<ApiEventInfo>());
		}

		/// <summary>
		/// Gets the events for this class.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ApiEventInfo> GetEvents()
		{
			return m_Events == null
					   ? Enumerable.Empty<ApiEventInfo>()
					   : m_Events.Select(kvp => kvp.Value).ToArray(m_Events.Count);
		}

		/// <summary>
		/// Sets the events for this class.
		/// </summary>
		/// <param name="events"></param>
		public void SetEvents(IEnumerable<ApiEventInfo> events)
		{
			if (events == null)
				throw new ArgumentNullException("events");

			m_Events = null;
			foreach (ApiEventInfo eventInfo in events)
				AddEvent(eventInfo);
		}

		/// <summary>
		/// Adds the event to the collection.
		/// </summary>
		/// <param name="eventInfo"></param>
		public void AddEvent(ApiEventInfo eventInfo)
		{
			if (eventInfo == null)
				throw new ArgumentNullException("eventInfo");

			if (m_Events == null)
				m_Events = new Dictionary<string, ApiEventInfo> { { eventInfo.Name, eventInfo } };
			else
				m_Events.Add(eventInfo.Name, eventInfo);
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
			return m_Methods == null
				       ? Enumerable.Empty<ApiMethodInfo>()
				       : m_Methods.Select(kvp => kvp.Value).ToArray(m_Methods.Count);
		}

		/// <summary>
		/// Sets the methods for this class.
		/// </summary>
		/// <param name="methods"></param>
		public void SetMethods(IEnumerable<ApiMethodInfo> methods)
		{
			if (methods == null)
				throw new ArgumentNullException("methods");

			m_Methods = null;
			foreach (ApiMethodInfo method in methods)
				AddMethod(method);
		}

		/// <summary>
		/// Adds the method to the collection.
		/// </summary>
		/// <param name="method"></param>
		public void AddMethod(ApiMethodInfo method)
		{
			if (method == null)
				throw new ArgumentNullException("method");

			if (m_Methods == null)
				m_Methods = new Dictionary<string, ApiMethodInfo> {{method.Name, method}};
			else
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
			return m_Properties == null
				       ? Enumerable.Empty<ApiPropertyInfo>()
				       : m_Properties.Select(kvp => kvp.Value).ToArray(m_Properties.Count);
		}

		/// <summary>
		/// Sets the properties for this class.
		/// </summary>
		/// <param name="properties"></param>
		public void SetProperties(IEnumerable<ApiPropertyInfo> properties)
		{
			if (properties == null)
				throw new ArgumentNullException("properties");

			m_Properties = null;
			foreach (ApiPropertyInfo property in properties)
				AddProperty(property);
		}

		/// <summary>
		/// Adds the property to the collection.
		/// </summary>
		/// <param name="property"></param>
		public void AddProperty(ApiPropertyInfo property)
		{
			if (property == null)
				throw new ArgumentNullException("property");

			if (m_Properties == null)
				m_Properties = new Dictionary<string, ApiPropertyInfo> { { property.Name, property } };
			else
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
			return m_Nodes == null
				       ? Enumerable.Empty<ApiNodeInfo>()
				       : m_Nodes.Select(kvp => kvp.Value).ToArray(m_Nodes.Count);
		}

		/// <summary>
		/// Sets the nodes for this class.
		/// </summary>
		/// <param name="nodes"></param>
		public void SetNodes(IEnumerable<ApiNodeInfo> nodes)
		{
			if (nodes == null)
				throw new ArgumentNullException("nodes");

			m_Nodes = null;
			foreach (ApiNodeInfo node in nodes)
				AddNode(node);
		}

		/// <summary>
		/// Adds the node to the collection.
		/// </summary>
		/// <param name="node"></param>
		public void AddNode(ApiNodeInfo node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			if (m_Nodes == null)
				m_Nodes = new Dictionary<string, ApiNodeInfo> { { node.Name, node } };
			else
				m_Nodes.Add(node.Name, node);
		}

		/// <summary>
		/// Tries to get the node with the given name.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="node"></param>
		/// <returns></returns>
		public bool TryGetNode(string name, out ApiNodeInfo node)
		{
			if (name == null)
				throw new ArgumentNullException("name");

			node = null;

			return m_Nodes != null && m_Nodes.TryGetValue(name, out node);
		}

		/// <summary>
		/// Tries to get the contents of the node with the given name.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="info"></param>
		/// <returns></returns>
		public bool TryGetNodeContents(string name, out ApiClassInfo info)
		{
			if (name == null)
				throw new ArgumentNullException("name");

			info = null;

			ApiNodeInfo node;
			if (!TryGetNode(name, out node))
				return false;

			info = node == null ? null : node.Node;
			return true;
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
			return m_NodeGroups == null
				       ? Enumerable.Empty<ApiNodeGroupInfo>()
				       : m_NodeGroups.Select(kvp => kvp.Value).ToArray(m_NodeGroups.Count);
		}

		/// <summary>
		/// Sets the node groups for this class.
		/// </summary>
		/// <param name="nodeGroups"></param>
		public void SetNodeGroups(IEnumerable<ApiNodeGroupInfo> nodeGroups)
		{
			if (nodeGroups == null)
				throw new ArgumentNullException("nodeGroups");

			m_NodeGroups = null;
			foreach (ApiNodeGroupInfo nodeGroup in nodeGroups)
				AddNodeGroup(nodeGroup);
		}

		/// <summary>
		/// Adds the node group to the collection.
		/// </summary>
		/// <param name="nodeGroup"></param>
		public void AddNodeGroup(ApiNodeGroupInfo nodeGroup)
		{
			if (nodeGroup == null)
				throw new ArgumentNullException("nodeGroup");

			if (m_NodeGroups == null)
				m_NodeGroups = new Dictionary<string, ApiNodeGroupInfo> { { nodeGroup.Name, nodeGroup } };
			else
				m_NodeGroups.Add(nodeGroup.Name, nodeGroup);
		}

		/// <summary>
		/// Tries to get the node group with the given name.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="nodeGroup"></param>
		/// <returns></returns>
		public bool TryGetNodeGroup(string name, out ApiNodeGroupInfo nodeGroup)
		{
			if (name == null)
				throw new ArgumentNullException("name");

			nodeGroup = null;

			return m_NodeGroups != null && m_NodeGroups.TryGetValue(name, out nodeGroup);
		}

		#endregion

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

			if (child is ApiEventInfo)
				AddEvent(child as ApiEventInfo);
			else if (child is ApiMethodInfo)
				AddMethod(child as ApiMethodInfo);
			else if (child is ApiPropertyInfo)
				AddProperty(child as ApiPropertyInfo);
			else if (child is ApiNodeInfo)
				AddNode(child as ApiNodeInfo);
			else if (child is ApiNodeGroupInfo)
				AddNodeGroup(child as ApiNodeGroupInfo);
			else
				throw new ArgumentException(string.Format("{0} can not add child of type {1}", GetType(), child.GetType()));
		}

		private IEnumerable<Type> GetProxyTypes(Type type)
		{
			return type == null ? Enumerable.Empty<Type>() : ApiClassAttribute.GetProxyTypes(type);
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

		private IEnumerable<ApiEventInfo> GetEventInfo(Type type, object instance, int depth)
		{
			if (type == null)
				yield break;

			if (depth <= 0)
				yield break;

			foreach (EventInfo eventInfo in ApiEventAttribute.GetEvents(type))
			{
				ApiEventAttribute attribute = ApiEventAttribute.GetAttribute(eventInfo);
				if (attribute != null)
					yield return new ApiEventInfo(attribute, eventInfo, instance, depth - 1);
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
