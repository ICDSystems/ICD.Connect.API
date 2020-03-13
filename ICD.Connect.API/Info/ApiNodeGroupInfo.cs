using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
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

		#region Properties

		public int NodeCount { get { return m_Nodes == null ? 0 : m_Nodes.Count; } }

		#endregion

		#region Constructors

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

		#endregion

		#region Methods

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

		#endregion

		#region Private Methods

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

		/// <summary>
		/// Gets the children attached to this node.
		/// </summary>
		/// <returns></returns>
		protected override IEnumerable<IApiInfo> GetChildren()
		{
			return GetNodes();
		}

		/// <summary>
		/// Creates a new instance of the current type.
		/// </summary>
		/// <returns></returns>
		protected override AbstractApiInfo Instantiate()
		{
			return new ApiNodeGroupInfo();
		}

		#endregion

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="requestor"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		/// <param name="path"></param>
		public void HandleNodeGroupRequest(IApiRequestor requestor, Type type, object instance, Stack<IApiInfo> path)
		{
			type = instance == null ? type : instance.GetType();
			PropertyInfo property = ApiNodeGroupAttribute.GetProperty(this, type);

			// Couldn't find an ApiNodeGroupAttribute for the given info
			if (property == null)
			{
				Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.MissingMember };
				Result.SetValue(string.Format("No node group property with name {0}.",
				                              StringUtils.ToRepresentation(Name)));
				ClearNodes();
				return;
			}

			path.Push(this);

			try
			{
				IApiNodeGroup group = property.GetValue(instance, null) as IApiNodeGroup;

				// Found the ApiNodeGroupAttribute but the property value was null
				if (group == null)
				{
					Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.MissingNode };
					Result.SetValue(string.Format("The node group at property {0} is null.",
															StringUtils.ToRepresentation(Name)));
					ClearNodes();
					return;
				}

				bool handled = false;

				foreach (ApiNodeGroupKeyInfo node in GetNodes())
				{
					handled = true;

					// The key for the group is invalid
					if (!group.ContainsKey(node.Key))
					{
						node.Node = null;
						node.Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.MissingNode };
						node.Result.SetValue(string.Format("The node group at property {0} does not contain a key at {1}.",
														   StringUtils.ToRepresentation(Name), node.Key));
						continue;
					}

					object classInstance = group[node.Key];

					// The instance at the given key is null
					if (classInstance == null)
					{
						node.Node = null;
						node.Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.MissingNode };
						node.Result.SetValue(string.Format("The node group at property {0} key {1} is null.",
														   StringUtils.ToRepresentation(Name), node.Key));
						continue;
					}

					Type classType = classInstance.GetType();

					path.Push(node);

					try
					{
						node.Node.HandleClassRequest(requestor, classType, classInstance, path);
					}
					finally
					{
						path.Pop();
					}
				}

				if (handled)
					return;

				// If there was nothing to handle we provide a response describing the features on this node group
				ApiNodeGroupInfo nodeGroupInfo = ApiNodeGroupAttribute.GetInfo(property, instance, 3);
				Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.Ok };
				Result.SetValue(nodeGroupInfo);
			}
			finally
			{
				path.Pop();
			}
		}
	}
}
