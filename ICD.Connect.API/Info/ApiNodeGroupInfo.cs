using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Info.Converters;
using ICD.Connect.API.Nodes;
using Newtonsoft.Json;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Connect.API.Attributes;

namespace ICD.Connect.API.Info
{
	[JsonConverter(typeof(ApiNodeGroupInfoConverter))]
	public sealed class ApiNodeGroupInfo : AbstractApiInfo, IEnumerable<ApiNodeGroupKeyInfo>
	{
		[CanBeNull]
		private Dictionary<uint, ApiNodeGroupKeyInfo> m_Nodes;

		public int NodeCount { get { return m_Nodes == null ? 0 : m_Nodes.Count; } }

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
		public ApiNodeGroupInfo(ApiNodeGroupAttribute attribute, PropertyInfo property, object instance, int depth)
			: base(attribute)
		{
			if (depth <= 0)
				return;

			if (property == null)
				return;

			IEnumerable<ApiNodeGroupKeyInfo> nodes = GetNodes(property, instance, depth - 1);
			SetNodes(nodes);
		}

		/// <summary>
		/// Removes all of the nodes.
		/// </summary>
		public void ClearNodes()
		{
			SetNodes(Enumerable.Empty<ApiNodeGroupKeyInfo>());
		}

		/// <summary>
		/// Gets the nodes class info for the given property.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="instance"></param>
		/// <param name="depth"></param>
		/// <returns></returns>
		[CanBeNull]
		private static IEnumerable<ApiNodeGroupKeyInfo> GetNodes(PropertyInfo property, object instance, int depth)
		{
			if (instance == null)
				yield break;

			if (depth <= 0)
				yield break;

			if (property == null || !property.CanRead)
				yield break;

			IApiNodeGroup nodeGroup = property.GetValue(instance, new object[0]) as IApiNodeGroup;
			if (nodeGroup == null)
				yield break;

			foreach (KeyValuePair<uint, object> kvp in nodeGroup.GetKeyedNodes())
			{
				object value = kvp.Value;
				if (value == null)
					continue;

				Type type = value.GetType();

				ApiClassInfo classInfo = ApiClassAttribute.GetInfo(type, value, depth - 1);
				yield return ApiNodeGroupKeyInfo.FromClassInfo(kvp.Key, classInfo);
			}
		}

		/// <summary>
		/// Gets the nodes from this group.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ApiNodeGroupKeyInfo> GetNodes()
		{
			return m_Nodes == null ? Enumerable.Empty<ApiNodeGroupKeyInfo>() : m_Nodes.Values;
		}

		/// <summary>
		/// Sets the nodes for this group.
		/// </summary>
		/// <param name="nodes"></param>
		public void SetNodes(IEnumerable<ApiNodeGroupKeyInfo> nodes)
		{
			if (nodes == null)
				throw new ArgumentNullException("nodes");

			m_Nodes = null;
			foreach (ApiNodeGroupKeyInfo node in nodes)
				AddNode(node);
		}

		/// <summary>
		/// Adds the node to the group.
		/// </summary>
		/// <param name="node"></param>
		public void AddNode(ApiNodeGroupKeyInfo node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			if (m_Nodes == null)
				m_Nodes = new Dictionary<uint, ApiNodeGroupKeyInfo> { { node.Key, node } };
			else
				m_Nodes.Add(node.Key, node);
		}

		/// <summary>
		/// Removes the node from the group.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool RemoveNode(uint key)
		{
			return m_Nodes != null && m_Nodes.Remove(key);
		}

		public IEnumerator<ApiNodeGroupKeyInfo> GetEnumerator()
		{
			return GetNodes().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Adds the given item as an immediate child to this node.
		/// </summary>
		/// <param name="child"></param>
		protected override void AddChild(IApiInfo child)
		{
			if (child == null)
				throw new ArgumentNullException("child");

			if (child is ApiNodeGroupKeyInfo)
				AddNode(child as ApiNodeGroupKeyInfo);
			else
				throw new ArgumentException(string.Format("{0} can not add child of type {1}", GetType(), child.GetType()));
		}
	}
}
