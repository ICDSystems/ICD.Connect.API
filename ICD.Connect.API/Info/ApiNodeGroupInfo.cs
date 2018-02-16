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
			m_Nodes = new Dictionary<uint, ApiClassInfo>();

			if (depth <= 0)
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
		private IEnumerable<KeyValuePair<uint, ApiClassInfo>> GetNodes(PropertyInfo property, object instance, int depth)
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

			foreach (KeyValuePair<uint, object> kvp in nodeGroup)
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

		/// <summary>
		/// Removes the node from the group.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool RemoveNode(uint key)
		{
			return m_Nodes.Remove(key);
		}

		public IEnumerator<KeyValuePair<uint, ApiClassInfo>> GetEnumerator()
		{
			return m_Nodes.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
