using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Connect.API.Attributes;
using ICD.Connect.API.Info;
using ICD.Connect.API.Nodes;

namespace ICD.Connect.API
{
	/// <summary>
	/// ApiHandler represents the top level in the API hierarchy and is responsible for interpreting incoming requests.
	/// </summary>
	[ApiClass("ICD", "Entry point for the Connect API.")]
	public static class ApiHandler
	{
		private static readonly ApiFeedbackCache s_FeedbackCache;

		[ApiNode("ControlSystem", "")]
		public static object ControlSystem { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		static ApiHandler()
		{
			s_FeedbackCache = new ApiFeedbackCache();
		}

		#region Handle Requests

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="info"></param>
		public static void HandleRequest(ApiClassInfo info)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			HandleRequest(null, info);
		}

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="requestor"></param>
		/// <param name="info"></param>
		public static void HandleRequest(IApiRequestor requestor, ApiClassInfo info)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			HandleRequest(requestor, info, typeof(ApiHandler), null, new Stack<IApiInfo>());
		}

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="requestor"></param>
		/// <param name="info"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		/// <param name="path"></param>
		private static void HandleRequest(IApiRequestor requestor, ApiClassInfo info, Type type, object instance, Stack<IApiInfo> path)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			type = instance == null ? type : instance.GetType();

			// If there was nothing to handle we provide a response describing the features on this class
			if (info.IsEmpty)
			{
				ApiClassInfo resultData = ApiClassAttribute.GetInfo(type, instance, 3);
				info.Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.Ok };
				info.Result.SetValue(resultData);
				return;
			}

			path.Push(info);

			try
			{
				foreach (ApiEventInfo eventInfo in info.GetEvents())
					HandleRequest(requestor, eventInfo, type, instance, path);

				foreach (ApiMethodInfo method in info.GetMethods())
					HandleRequest(requestor, method, type, instance, path);

				foreach (ApiPropertyInfo property in info.GetProperties())
					HandleRequest(requestor, property, type, instance, path);

				foreach (ApiNodeInfo node in info.GetNodes())
					HandleRequest(requestor, node, type, instance, path);

				foreach (ApiNodeGroupInfo nodeGroup in info.GetNodeGroups())
					HandleRequest(requestor, nodeGroup, type, instance, path);
			}
			finally
			{
				path.Pop();
			}
		}

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="requestor"></param>
		/// <param name="info"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		/// <param name="path"></param>
		private static void HandleRequest(IApiRequestor requestor, ApiEventInfo info, Type type, object instance, Stack<IApiInfo> path)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			type = instance == null ? type : instance.GetType();
			EventInfo eventInfo = ApiEventAttribute.GetEvent(info, type);

			// Couldn't find an ApiEventAttribute for the given info.
			if (eventInfo == null)
			{
				info.Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.MissingMember };
				info.Result.SetValue(string.Format("No event with name {0}.", StringUtils.ToRepresentation(info.Name)));
				return;
			}

			path.Push(info);

			try
			{
				switch (info.SubscribeAction)
				{
					case ApiEventInfo.eSubscribeAction.None:
						// We're not doing anything with the event so return info.
						ApiEventInfo resultInfo = ApiEventAttribute.GetInfo(eventInfo, instance, 3);
						info.Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.Ok };
						info.Result.SetValue(resultInfo);
						return;

					case ApiEventInfo.eSubscribeAction.Subscribe:
						// Subscribe to the event
						info.Result = Subscribe(requestor, eventInfo, instance, path);
						return;

					case ApiEventInfo.eSubscribeAction.Unsubscribe:
						// Unsubscribe from the event
						info.Result = Unsubscribe(requestor, eventInfo, instance, path);
						return;

					default:
						throw new ArgumentOutOfRangeException("info", "Unknown subscribe action");
				}
			}
			finally
			{
				path.Pop();
			}
		}

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="requestor"></param>
		/// <param name="node"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		/// <param name="path"></param>
		private static void HandleRequest(IApiRequestor requestor, ApiNodeInfo node, Type type, object instance, Stack<IApiInfo> path)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			type = instance == null ? type : instance.GetType();
			PropertyInfo property = ApiNodeAttribute.GetProperty(node, type);

            // Couldn't find an ApiNodeAttribute for the given info
			if (property == null)
			{
				node.Result = new ApiResult {ErrorCode = ApiResult.eErrorCode.MissingMember};
				node.Result.SetValue(string.Format("No node property with name {0}.", StringUtils.ToRepresentation(node.Name)));
				node.Node = null;
				return;
			}

			path.Push(node);

			try
			{
				object nodeValue = property.GetValue(instance, null);

				// Found the ApiNodeAttribute but the property value was null
				if (nodeValue == null)
				{
					node.Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.MissingNode };
					node.Result.SetValue(string.Format("The node at property {0} is null.", StringUtils.ToRepresentation(node.Name)));
					node.Node = null;
					return;
				}

				Type nodeType = nodeValue.GetType();
				HandleRequest(requestor, node.Node, nodeType, nodeValue, path);
			}
			finally
			{
				path.Pop();
			}
		}

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="requestor"></param>
		/// <param name="nodeGroup"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		/// <param name="path"></param>
		private static void HandleRequest(IApiRequestor requestor, ApiNodeGroupInfo nodeGroup, Type type, object instance, Stack<IApiInfo> path)
		{
			if (nodeGroup == null)
				throw new ArgumentNullException("nodeGroup");

			type = instance == null ? type : instance.GetType();
			PropertyInfo property = ApiNodeGroupAttribute.GetProperty(nodeGroup, type);

			// Couldn't find an ApiNodeGroupAttribute for the given info
			if (property == null)
			{
				nodeGroup.Result = new ApiResult {ErrorCode = ApiResult.eErrorCode.MissingMember};
				nodeGroup.Result.SetValue(string.Format("No node group property with name {0}.",
				                                        StringUtils.ToRepresentation(nodeGroup.Name)));
				nodeGroup.ClearNodes();
				return;
			}

			path.Push(nodeGroup);

			try
			{
				IApiNodeGroup group = property.GetValue(instance, null) as IApiNodeGroup;

				// Found the ApiNodeGroupAttribute but the property value was null
				if (group == null)
				{
					nodeGroup.Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.MissingNode };
					nodeGroup.Result.SetValue(string.Format("The node group at property {0} is null.",
															StringUtils.ToRepresentation(nodeGroup.Name)));
					nodeGroup.ClearNodes();
					return;
				}

				bool handled = false;

				foreach (ApiNodeGroupKeyInfo node in nodeGroup)
				{
					handled = true;

					// The key for the group is invalid
					if (!group.ContainsKey(node.Key))
					{
						node.Node = null;
						node.Result = new ApiResult {ErrorCode = ApiResult.eErrorCode.MissingNode};
						node.Result.SetValue(string.Format("The node group at property {0} does not contain a key at {1}.",
						                                   StringUtils.ToRepresentation(nodeGroup.Name), node.Key));
						continue;
					}

					object classInstance = group[node.Key];

					// The instance at the given key is null
					if (classInstance == null)
					{
						node.Node = null;
						node.Result = new ApiResult {ErrorCode = ApiResult.eErrorCode.MissingNode};
						node.Result.SetValue(string.Format("The node group at property {0} key {1} is null.",
						                                   StringUtils.ToRepresentation(nodeGroup.Name), node.Key));
						continue;
					}

					Type classType = classInstance.GetType();

					path.Push(node);

					try
					{
						HandleRequest(requestor, node.Node, classType, classInstance, path);
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
				nodeGroup.Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.Ok };
				nodeGroup.Result.SetValue(nodeGroupInfo);
			}
			finally
			{
				path.Pop();
			}
		}

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="requestor"></param>
		/// <param name="info"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		/// <param name="path"></param>
		private static void HandleRequest(IApiRequestor requestor, ApiMethodInfo info, Type type, object instance, Stack<IApiInfo> path)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			type = instance == null ? type : instance.GetType();
			MethodInfo method = ApiMethodAttribute.GetMethod(info, type);

			// Couldn't find an ApiMethodAttribute for the given info.
			if (method == null)
			{
				info.Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.MissingMember };
				info.Result.SetValue(string.Format("No method with name {0}.", StringUtils.ToRepresentation(info.Name)));
				info.ClearParameters();
				return;
			}

			path.Push(info);

			try
			{
				// We're not executing the method so return info about the parameters.
				if (!info.Execute)
				{
					ApiMethodInfo methodInfo = ApiMethodAttribute.GetInfo(method, instance, 3);
					info.Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.Ok };
					info.Result.SetValue(methodInfo);
					return;
				}

				ApiParameterInfo[] parameterInfos = info.GetParameters().ToArray();
				object[] parameters = parameterInfos.Select(p => p.Value).ToArray(parameterInfos.Length);
				Type[] types = method.GetParameters()
				                     .Select(p =>
#if SIMPLSHARP
				                             (Type)
#endif
				                             p.ParameterType)
				                     .ToArray();

				// Wrong number of parameters.
				if (parameters.Length != types.Length)
				{
					info.Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.InvalidParameter };
					info.Result.SetValue(string.Format("Parameters length {0} does not match method parameters length {1}.",
													   parameters.Length, types.Length));
					info.ClearParameters();
					return;
				}

				bool converted = true;
				for (int index = 0; index < parameters.Length; index++)
				{
					try
					{
						parameters[index] = ReflectionUtils.ChangeType(parameters[index], types[index]);
					}
					// Parameter is the incorrect type.
					catch (Exception)
					{
						ApiParameterInfo parameterInfo = parameterInfos[index];
						parameterInfo.Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.InvalidParameter };
						parameterInfo.Result.SetValue(string.Format("Failed to convert to {0}.", types[index].Name));
						converted = false;
					}
				}

				// Failed to convert all of the parameters.
				if (!converted)
				{
					info.Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.InvalidParameter };
					info.Result.SetValue(string.Format("Failed to execute method {0} due to one or more invalid parameters.",
													   StringUtils.ToRepresentation(info.Name)));
					return;
				}

				object value;

				try
				{
					value = method.Invoke(instance, parameters);
				}
				// Method failed to execute.
				catch (Exception e)
				{
					info.Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.Exception };
					info.Result.SetValue(string.Format("Failed to execute method {0} due to {1} - {2}.",
													   StringUtils.ToRepresentation(info.Name),
													   e.GetType().Name, e.Message));
					return;
				}

				info.Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.Ok };
				info.Result.SetValue(value);
			}
			finally
			{
				path.Pop();
			}
		}

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="requestor"></param>
		/// <param name="info"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		/// <param name="path"></param>
		private static void HandleRequest(IApiRequestor requestor, ApiPropertyInfo info, Type type, object instance, Stack<IApiInfo> path)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			type = instance == null ? type : instance.GetType();
			PropertyInfo property = ApiPropertyAttribute.GetProperty(info, type);

			// Couldn't find an ApiPropertyAttribute for the given info.
			if (property == null)
			{
				info.Result = new ApiResult {ErrorCode = ApiResult.eErrorCode.MissingMember};
				info.Result.SetValue(string.Format("No property with name {0}.", StringUtils.ToRepresentation(info.Name)));
				return;
			}

			path.Push(info);

			try
			{
				// Set the value
				if (info.Write)
				{
					// Trying to write to a readonly property.
					if (!property.CanWrite)
					{
						info.Result = new ApiResult {ErrorCode = ApiResult.eErrorCode.MissingMember};
						info.Result.SetValue(string.Format("Property {0} is readonly.", StringUtils.ToRepresentation(info.Name)));
						return;
					}

					object value;

					try
					{
						value = ReflectionUtils.ChangeType(info.Value, property.PropertyType);
					}
						// Value is the incorrect type.
					catch (Exception)
					{
						info.Result = new ApiResult {ErrorCode = ApiResult.eErrorCode.InvalidParameter};
						info.Result.SetValue(string.Format("Failed to convert to {0}.", property.PropertyType.Name));
						return;
					}

					try
					{
						property.SetValue(instance, value, null);
					}
						// Property failed to execute.
					catch (Exception e)
					{
						info.Result = new ApiResult {ErrorCode = ApiResult.eErrorCode.Exception};
						info.Result.SetValue(string.Format("Failed to set property {0} due to {1} - {2}.",
						                                   StringUtils.ToRepresentation(info.Name),
						                                   e.GetType().Name, e.Message));
						return;
					}
				}

				// Add the response
				info.Result = new ApiResult {ErrorCode = ApiResult.eErrorCode.Ok};
				if (property.CanRead)
					info.Result.SetValue(property.PropertyType, property.GetValue(instance, null));
			}
			finally
			{
				path.Pop();
			}
		}

		#endregion

		#region Event Callbacks

		private static ApiResult Subscribe(IApiRequestor requestor, EventInfo eventInfo, object instance, Stack<IApiInfo> path)
		{
			if (eventInfo == null)
				throw new ArgumentNullException("eventInfo");

			if (instance == null)
				throw new ArgumentNullException("instance");

			if (path == null)
				throw new ArgumentNullException("path");

			try
			{
				s_FeedbackCache.Subscribe(requestor, eventInfo, instance, path);
			}
			catch (Exception e)
			{
				ApiResult output = new ApiResult { ErrorCode = ApiResult.eErrorCode.Exception };
				output.SetValue(string.Format("Failed to subscribe to {0} - {1}", eventInfo.Name, e.Message));
				return output;
			}

			return new ApiResult { ErrorCode = ApiResult.eErrorCode.Ok };
		}

		private static ApiResult Unsubscribe(IApiRequestor requestor, EventInfo eventInfo, object instance, Stack<IApiInfo> path)
		{
			if (eventInfo == null)
				throw new ArgumentNullException("eventInfo");

			if (instance == null)
				throw new ArgumentNullException("instance");

			if (path == null)
				throw new ArgumentNullException("path");

			try
			{
				s_FeedbackCache.Unsubscribe(requestor, eventInfo, instance, path);
			}
			catch (Exception e)
			{
				ApiResult output = new ApiResult { ErrorCode = ApiResult.eErrorCode.Exception };
				output.SetValue(string.Format("Failed to unsubscribe from {0} - {1}", eventInfo.Name, e.Message));
				return output;
			}

			return new ApiResult { ErrorCode = ApiResult.eErrorCode.Ok };
		}

		#endregion
	}
}
