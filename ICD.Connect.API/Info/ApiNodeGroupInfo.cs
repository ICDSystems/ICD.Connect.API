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
	public sealed class ApiNodeGroupInfo : AbstractApiInfo, IEnumerable<KeyValuePair<uint, ApiClassInfo>>
	{
		[CanBeNull]
		private Dictionary<uint, ApiClassInfo> m_Nodes;

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

			IEnumerable<KeyValuePair<uint, ApiClassInfo>> nodes = GetNodes(property, instance, depth - 1);
			SetNodes(nodes);
		}

		/// <summary>
		/// Creates a recursive copy of the API info.
		/// </summary>
		/// <returns></returns>
		public ApiNodeGroupInfo DeepCopy()
		{
			ApiNodeGroupInfo output = new ApiNodeGroupInfo();

			IEnumerable<KeyValuePair<uint, ApiClassInfo>> nodesCopy =
				GetNodes().Select(kvp => new KeyValuePair<uint, ApiClassInfo>(kvp.Key, kvp.Value.DeepCopy()));

			output.SetNodes(nodesCopy);

			DeepCopy(output);
			return output;
		}

		/// <summary>
		/// Removes all of the nodes.
		/// </summary>
		public void ClearNodes()
		{
			SetNodes(Enumerable.Empty<KeyValuePair<uint, ApiClassInfo>>());
		}

		/// <summary>
		/// Gets the nodes class info for the given property.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="instance"></param>
		/// <param name="depth"></param>
		/// <returns></returns>
		[CanBeNull]
		private static IEnumerable<KeyValuePair<uint, ApiClassInfo>> GetNodes(PropertyInfo property, object instance, int depth)
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
				uint key = kvp.Key;

				object value = kvp.Value;
				if (value == null)
					continue;

				Type type = value.GetType();
				ApiClassInfo info = ApiClassAttribute.GetInfo(type, value, depth - 1);

				yield return new KeyValuePair<uint, ApiClassInfo>(key, info);
			}
		}

		/// <summary>
		/// Gets the nodes from this group.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<KeyValuePair<uint, ApiClassInfo>> GetNodes()
		{
			return m_Nodes == null ? Enumerable.Empty<KeyValuePair<uint, ApiClassInfo>>() : m_Nodes.ToArray(m_Nodes.Count);
		}

		/// <summary>
		/// Sets the nodes for this group.
		/// </summary>
		/// <param name="nodes"></param>
		public void SetNodes(IEnumerable<KeyValuePair<uint, ApiClassInfo>> nodes)
		{
			if (nodes == null)
				throw new ArgumentNullException("nodes");

			m_Nodes = null;
			foreach (KeyValuePair<uint, ApiClassInfo> kvp in nodes)
				AddNode(kvp.Key, kvp.Value);
		}

		/// <summary>
		/// Adds the node to the group.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="node"></param>
		public void AddNode(uint key, ApiClassInfo node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			if (m_Nodes == null)
				m_Nodes = new Dictionary<uint, ApiClassInfo> {{key, node}};
			else
				m_Nodes.Add(key, node);
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

		public IEnumerator<KeyValuePair<uint, ApiClassInfo>> GetEnumerator()
		{
			return m_Nodes == null
				       ? Enumerable.Empty<KeyValuePair<uint, ApiClassInfo>>().GetEnumerator()
				       : m_Nodes.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
