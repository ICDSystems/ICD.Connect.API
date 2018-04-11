using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.EventArguments;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Connect.API.Attributes;
using ICD.Connect.API.Info;
using ICD.Connect.API.Nodes;
using Newtonsoft.Json;

namespace ICD.Connect.API
{
	/// <summary>
	/// ApiHandler represents the top level in the API hierarchy and is responsible for interpreting incoming requests.
	/// </summary>
	[ApiClass("ICD", "Entry point for the Connect API.")]
	public static class ApiHandler
	{
		[ApiNode("ControlSystem", "")]
		public static object ControlSystem { get; set; }

		private static readonly Dictionary<object, Dictionary<string, ApiClassInfo>> s_SubscribedEventsMap;
		private static MethodInfo s_EventCallbackMethod;

		/// <summary>
		/// Gets a reference to the callback method used with API events.
		/// </summary>
		private static MethodInfo EventCallbackMethod
		{
			get
			{
				return s_EventCallbackMethod =
				       s_EventCallbackMethod ?? typeof(ApiHandler)
#if SIMPLSHARP
					                                .GetCType()
#endif
					                                .GetMethod("EventCallback",
					                                           BindingFlags.NonPublic | BindingFlags.Static);
			}
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		static ApiHandler()
		{
			s_SubscribedEventsMap = new Dictionary<object, Dictionary<string, ApiClassInfo>>();
		}

		#region Read Results

		/// <summary>
		/// Executes the given callback for each result in the given command tree.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="readResult"></param>
		public static void ReadResultsRecursive(ApiClassInfo info, Action<ApiResult, Stack<IApiInfo>> readResult)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			if (readResult == null)
				throw new ArgumentNullException("readResult");

			ReadResultsRecursive(info, readResult, new Stack<IApiInfo>());
		}

		/// <summary>
		/// Executes the given callback for each result in the given command tree.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="readResult"></param>
		/// <param name="path"></param>
		private static void ReadResultsRecursive(ApiClassInfo info, Action<ApiResult, Stack<IApiInfo>> readResult, Stack<IApiInfo> path)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			if (readResult == null)
				throw new ArgumentNullException("readResult");

			if (path == null)
				throw new ArgumentNullException("path");

			path.Push(info);
			{
				if (info.Result != null)
					readResult(info.Result, path);

				foreach (ApiEventInfo eventInfo in info.GetEvents())
					ReadResultsRecursive(eventInfo, readResult, path);

				foreach (ApiPropertyInfo property in info.GetProperties())
					ReadResultsRecursive(property, readResult, path);

				foreach (ApiMethodInfo method in info.GetMethods())
					ReadResultsRecursive(method, readResult, path);

				foreach (ApiNodeInfo node in info.GetNodes())
					ReadResultsRecursive(node, readResult, path);

				foreach (ApiNodeGroupInfo nodeGroup in info.GetNodeGroups())
					ReadResultsRecursive(nodeGroup, readResult, path);
			}
			path.Pop();
		}

		/// <summary>
		/// Executes the given callback for each result in the given command tree.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="readResult"></param>
		/// <param name="path"></param>
		private static void ReadResultsRecursive(ApiEventInfo info, Action<ApiResult, Stack<IApiInfo>> readResult, Stack<IApiInfo> path)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			if (readResult == null)
				throw new ArgumentNullException("readResult");

			if (path == null)
				throw new ArgumentNullException("path");

			path.Push(info);
			{
				if (info.Result != null)
					readResult(info.Result, path);
			}
			path.Pop();
		}

		/// <summary>
		/// Executes the given callback for each result in the given command tree.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="readResult"></param>
		/// <param name="path"></param>
		private static void ReadResultsRecursive(ApiPropertyInfo info, Action<ApiResult, Stack<IApiInfo>> readResult, Stack<IApiInfo> path)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			if (readResult == null)
				throw new ArgumentNullException("readResult");

			if (path == null)
				throw new ArgumentNullException("path");

			path.Push(info);
			{
				if (info.Result != null)
					readResult(info.Result, path);
			}
			path.Pop();
		}

		/// <summary>
		/// Executes the given callback for each result in the given command tree.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="readResult"></param>
		/// <param name="path"></param>
		private static void ReadResultsRecursive(ApiMethodInfo info, Action<ApiResult, Stack<IApiInfo>> readResult, Stack<IApiInfo> path)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			if (readResult == null)
				throw new ArgumentNullException("readResult");

			if (path == null)
				throw new ArgumentNullException("path");

			path.Push(info);
			{
				if (info.Result != null)
					readResult(info.Result, path);

				foreach (ApiParameterInfo parameter in info.GetParameters())
					ReadResultsRecursive(parameter, readResult, path);
			}
			path.Pop();
		}

		/// <summary>
		/// Executes the given callback for each result in the given command tree.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="readResult"></param>
		/// <param name="path"></param>
		private static void ReadResultsRecursive(ApiParameterInfo info, Action<ApiResult, Stack<IApiInfo>> readResult, Stack<IApiInfo> path)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			if (readResult == null)
				throw new ArgumentNullException("readResult");

			if (path == null)
				throw new ArgumentNullException("path");

			path.Push(info);
			{
				if (info.Result != null)
					readResult(info.Result, path);
			}
			path.Pop();
		}

		/// <summary>
		/// Executes the given callback for each result in the given command tree.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="readResult"></param>
		/// <param name="path"></param>
		private static void ReadResultsRecursive(ApiNodeInfo info, Action<ApiResult, Stack<IApiInfo>> readResult, Stack<IApiInfo> path)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			if (readResult == null)
				throw new ArgumentNullException("readResult");

			if (path == null)
				throw new ArgumentNullException("path");

			path.Push(info);
			{
				if (info.Result != null)
					readResult(info.Result, path);

				if (info.Node != null)
					ReadResultsRecursive(info.Node, readResult, path);
			}
			path.Pop();
		}

		/// <summary>
		/// Executes the given callback for each result in the given command tree.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="readResult"></param>
		/// <param name="path"></param>
		private static void ReadResultsRecursive(ApiNodeGroupInfo info, Action<ApiResult, Stack<IApiInfo>> readResult, Stack<IApiInfo> path)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			if (readResult == null)
				throw new ArgumentNullException("readResult");

			if (path == null)
				throw new ArgumentNullException("path");

			path.Push(info);
			{
				if (info.Result != null)
					readResult(info.Result, path);

				foreach (KeyValuePair<uint, ApiClassInfo> item in info.GetNodes().Where(kvp => kvp.Value != null))
					ReadResultsRecursive(item.Value, readResult, path);
			}
			path.Pop();
		}

		#endregion

		#region Handle Requests

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="serialized"></param>
		/// <returns></returns>
		public static ApiClassInfo HandleRequest(string serialized)
		{
			ApiClassInfo info = JsonConvert.DeserializeObject<ApiClassInfo>(serialized);
			HandleRequest(info);
			return info;
		}

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="info"></param>
		public static void HandleRequest(ApiClassInfo info)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			HandleRequest(info, typeof(ApiHandler), null, new Stack<IApiInfo>());
		}

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		/// <param name="path"></param>
		private static void HandleRequest(ApiClassInfo info, Type type, object instance, Stack<IApiInfo> path)
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
					HandleRequest(eventInfo, type, instance, path);

				foreach (ApiMethodInfo method in info.GetMethods())
					HandleRequest(method, type, instance, path);

				foreach (ApiPropertyInfo property in info.GetProperties())
					HandleRequest(property, type, instance, path);

				foreach (ApiNodeInfo node in info.GetNodes())
					HandleRequest(node, type, instance, path);

				foreach (ApiNodeGroupInfo nodeGroup in info.GetNodeGroups())
					HandleRequest(nodeGroup, type, instance, path);
			}
			finally
			{
				path.Pop();
			}
		}

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		/// <param name="path"></param>
		private static void HandleRequest(ApiEventInfo info, Type type, object instance, Stack<IApiInfo> path)
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
						info.Result = Subscribe(eventInfo, instance, path);
						return;

					case ApiEventInfo.eSubscribeAction.Unsubscribe:
						// Unsubscribe from the event
						info.Result = Unsubscribe(eventInfo, instance, path);
						return;

					default:
						throw new ArgumentOutOfRangeException();
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
		/// <param name="node"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		/// <param name="path"></param>
		private static void HandleRequest(ApiNodeInfo node, Type type, object instance, Stack<IApiInfo> path)
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
				HandleRequest(node.Node, nodeType, nodeValue, path);
			}
			finally
			{
				path.Pop();
			}
		}

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="nodeGroup"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		/// <param name="path"></param>
		private static void HandleRequest(ApiNodeGroupInfo nodeGroup, Type type, object instance, Stack<IApiInfo> path)
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

				foreach (KeyValuePair<uint, ApiClassInfo> kvp in nodeGroup)
				{
					handled = true;

					ApiClassInfo classInfo = kvp.Value;

					// The key for the group is invalid
					if (!group.ContainsKey(kvp.Key))
					{
						classInfo.ClearChildren();
						classInfo.Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.MissingNode };
						classInfo.Result.SetValue(string.Format("The node group at property {0} does not contain a key at {1}.",
																StringUtils.ToRepresentation(nodeGroup.Name), kvp.Key));
						continue;
					}

					object classInstance = group[kvp.Key];

					// The instance at the given key is null
					if (classInstance == null)
					{
						classInfo.ClearChildren();
						classInfo.Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.MissingNode };
						classInfo.Result.SetValue(string.Format("The node group at property {0} key {1} is null.",
																StringUtils.ToRepresentation(nodeGroup.Name), kvp.Key));
						continue;
					}

					Type classType = classInstance.GetType();

					HandleRequest(classInfo, classType, classInstance, path);
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
		/// <param name="info"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		/// <param name="path"></param>
		private static void HandleRequest(ApiMethodInfo info, Type type, object instance, Stack<IApiInfo> path)
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
		/// <param name="info"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		/// <param name="path"></param>
		private static void HandleRequest(ApiPropertyInfo info, Type type, object instance, Stack<IApiInfo> path)
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

		private static ApiResult Subscribe(EventInfo eventInfo, object instance, Stack<IApiInfo> path)
		{
			if (eventInfo == null)
				throw new ArgumentNullException("eventInfo");

			if (instance == null)
				throw new ArgumentNullException("instance");

			if (path == null)
				throw new ArgumentNullException("path");

			try
			{
				Delegate handler = ReflectionUtils.CreateDelegate(eventInfo.EventHandlerType, null, EventCallbackMethod);
				eventInfo.AddEventHandler(instance, handler);

				if (!s_SubscribedEventsMap.ContainsKey(instance))
					s_SubscribedEventsMap.Add(instance, new Dictionary<string, ApiClassInfo>());

				ApiClassInfo command = ApiCommandBuilder.CommandFromPath(path);

				s_SubscribedEventsMap[instance].Add(eventInfo.Name, command);

			}
			catch (Exception e)
			{
				ApiResult output = new ApiResult { ErrorCode = ApiResult.eErrorCode.Exception };
				output.SetValue(string.Format("Failed to subscribe to {0} - {1}", eventInfo.Name, e.Message));
				return output;
			}

			return new ApiResult { ErrorCode = ApiResult.eErrorCode.Ok };
		}

		private static ApiResult Unsubscribe(EventInfo eventInfo, object instance, Stack<IApiInfo> path)
		{
			try
			{
				// TODO - Build a lookup table
				throw new NotImplementedException();
				//Delegate handler = null;
				//eventInfo.RemoveEventHandler(null, handler);
			}
			catch (Exception e)
			{
				ApiResult output = new ApiResult { ErrorCode = ApiResult.eErrorCode.Exception };
				output.SetValue(string.Format("Failed to unsubscribe from {0} - {1}", eventInfo.Name, e.Message));
				return output;
			}

			return new ApiResult { ErrorCode = ApiResult.eErrorCode.Ok };
		}

		/// <summary>
		/// Only support subscribing to events matching the siganture of this method.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		[UsedImplicitly]
		private static void EventCallback(object sender, IApiEventArgs args)
		{
			if (sender == null)
				throw new ArgumentNullException("sender");

			if (args == null)
				throw new ArgumentNullException("args");

			Dictionary<string, ApiClassInfo> map;
			if (!s_SubscribedEventsMap.TryGetValue(sender, out map))
				return;

			ApiClassInfo command;
			if (!map.TryGetValue(args.EventName, out command))
				return;

			IcdConsole.PrintLine(eConsoleColor.Magenta, "API event raised - {0} - {1}", args, command);
		}

		#endregion
	}
}
