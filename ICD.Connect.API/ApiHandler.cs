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
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
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

		#region Methods

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

			HandleRequest(info, typeof(ApiHandler), null);
		}

		#endregion

		#region Results

		/// <summary>
		/// Executes the given callback for each result in the given command tree.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="readResult"></param>
		public static void ReadResultsRecursive(ApiClassInfo info, Action<ApiResult> readResult)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			if (readResult == null)
				throw new ArgumentNullException("readResult");

			if (info.Result != null)
				readResult(info.Result);

			foreach (ApiPropertyInfo property in info.GetProperties())
				ReadResultsRecursive(property, readResult);

			foreach (ApiMethodInfo method in info.GetMethods())
				ReadResultsRecursive(method, readResult);

			foreach (ApiNodeInfo node in info.GetNodes())
				ReadResultsRecursive(node, readResult);

			foreach (ApiNodeGroupInfo nodeGroup in info.GetNodeGroups())
				ReadResultsRecursive(nodeGroup, readResult);
		}

		/// <summary>
		/// Executes the given callback for each result in the given command tree.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="readResult"></param>
		private static void ReadResultsRecursive(ApiPropertyInfo info, Action<ApiResult> readResult)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			if (readResult == null)
				throw new ArgumentNullException("readResult");

			if (info.Result != null)
				readResult(info.Result);
		}

		/// <summary>
		/// Executes the given callback for each result in the given command tree.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="readResult"></param>
		private static void ReadResultsRecursive(ApiMethodInfo info, Action<ApiResult> readResult)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			if (readResult == null)
				throw new ArgumentNullException("readResult");

			if (info.Result != null)
				readResult(info.Result);

			foreach (ApiParameterInfo parameter in info.GetParameters())
				ReadResultsRecursive(parameter, readResult);
		}

		/// <summary>
		/// Executes the given callback for each result in the given command tree.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="readResult"></param>
		private static void ReadResultsRecursive(ApiParameterInfo info, Action<ApiResult> readResult)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			if (readResult == null)
				throw new ArgumentNullException("readResult");

			if (info.Result != null)
				readResult(info.Result);
		}

		/// <summary>
		/// Executes the given callback for each result in the given command tree.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="readResult"></param>
		private static void ReadResultsRecursive(ApiNodeInfo info, Action<ApiResult> readResult)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			if (readResult == null)
				throw new ArgumentNullException("readResult");

			if (info.Result != null)
				readResult(info.Result);

			if (info.Node != null)
				ReadResultsRecursive(info.Node, readResult);
		}

		/// <summary>
		/// Executes the given callback for each result in the given command tree.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="readResult"></param>
		private static void ReadResultsRecursive(ApiNodeGroupInfo info, Action<ApiResult> readResult)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			if (readResult == null)
				throw new ArgumentNullException("readResult");

			if (info.Result != null)
				readResult(info.Result);

			foreach (KeyValuePair<uint, ApiClassInfo> item in info.GetNodes().Where(kvp => kvp.Value != null))
				ReadResultsRecursive(item.Value, readResult);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		private static void HandleRequest(ApiClassInfo info, Type type, object instance)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			type = instance == null ? type : instance.GetType();
			bool handled = false;

			foreach (ApiMethodInfo method in info.GetMethods())
			{
				handled = true;
				HandleRequest(method, type, instance);
			}

			foreach (ApiPropertyInfo property in info.GetProperties())
			{
				handled = true;
				HandleRequest(property, type, instance);
			}

			foreach (ApiNodeInfo node in info.GetNodes())
			{
				handled = true;
				HandleRequest(node, type, instance);
			}

			foreach (ApiNodeGroupInfo nodeGroup in info.GetNodeGroups())
			{
				handled = true;
				HandleRequest(nodeGroup, type, instance);
			}

			if (handled)
				return;

			// If there was nothing to handle we provide a response describing the features on this class
			ApiClassInfo resultData = ApiClassAttribute.GetInfo(type, instance, 3);
			info.Result = new ApiResult {ErrorCode = ApiResult.eErrorCode.Ok};
			info.Result.SetValue(resultData);
		}

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		private static void HandleRequest(ApiNodeInfo node, Type type, object instance)
		{
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

			object nodeValue = property.GetValue(instance, new object [0]);

			// Found the ApiNodeAttribute but the property value was null
			if (nodeValue == null)
			{
				node.Result = new ApiResult {ErrorCode = ApiResult.eErrorCode.MissingNode};
				node.Result.SetValue(string.Format("The node at property {0} is null.", StringUtils.ToRepresentation(node.Name)));
				node.Node = null;
				return;
			}

			Type nodeType = nodeValue.GetType();
			HandleRequest(node.Node, nodeType, nodeValue);
		}

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="nodeGroup"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		private static void HandleRequest(ApiNodeGroupInfo nodeGroup, Type type, object instance)
		{
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

			IApiNodeGroup group = property.GetValue(instance, new object[0]) as IApiNodeGroup;

			// Found the ApiNodeGroupAttribute but the property value was null
			if (group == null)
			{
				nodeGroup.Result = new ApiResult {ErrorCode = ApiResult.eErrorCode.MissingNode};
				nodeGroup.Result.SetValue(string.Format("The node group at property {0} is null.",
				                                        StringUtils.ToRepresentation(nodeGroup.Name)));
				nodeGroup.ClearNodes();
				return;
			}

			bool handled = false;

			foreach (KeyValuePair<uint, ApiClassInfo> kvp in nodeGroup.ToArray(nodeGroup.NodeCount))
			{
				handled = true;

				ApiClassInfo classInfo = kvp.Value;

				// The key for the group is invalid
				if (!group.ContainsKey(kvp.Key))
				{
					classInfo.ClearChildren();
					classInfo.Result = new ApiResult {ErrorCode = ApiResult.eErrorCode.MissingNode};
					classInfo.Result.SetValue(string.Format("The node group at property {0} does not contain a key at {1}.",
					                                        StringUtils.ToRepresentation(nodeGroup.Name), kvp.Key));
					continue;
				}

				object classInstance = group[kvp.Key];

				// The instance at the given key is null
				if (classInstance == null)
				{
					classInfo.ClearChildren();
					classInfo.Result = new ApiResult {ErrorCode = ApiResult.eErrorCode.MissingNode};
					classInfo.Result.SetValue(string.Format("The node group at property {0} key {1} is null.",
					                                        StringUtils.ToRepresentation(nodeGroup.Name), kvp.Key));
					continue;
				}

				Type classType = classInstance.GetType();

				HandleRequest(classInfo, classType, classInstance);
			}

			if (handled)
				return;

			// If there was nothing to handle we provide a response describing the features on this node group
			ApiNodeGroupInfo nodeGroupInfo = ApiNodeGroupAttribute.GetInfo(property, instance, 3);
			nodeGroup.Result = new ApiResult {ErrorCode = ApiResult.eErrorCode.Ok};
			nodeGroup.Result.SetValue(nodeGroupInfo);
		}

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		private static void HandleRequest(ApiMethodInfo info, Type type, object instance)
		{
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
					parameters[index] = ChangeType(parameters[index], types[index]);
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
				info.Result = new ApiResult {ErrorCode = ApiResult.eErrorCode.InvalidParameter};
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

			info.Result = new ApiResult {ErrorCode = ApiResult.eErrorCode.Ok};
			info.Result.SetValue(value);
		}

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		private static void HandleRequest(ApiPropertyInfo info, Type type, object instance)
		{
			type = instance == null ? type : instance.GetType();

			PropertyInfo property = ApiPropertyAttribute.GetProperty(info, type);

			// Couldn't find an ApiPropertyAttribute for the given info.
			if (property == null)
			{
				info.Result = new ApiResult {ErrorCode = ApiResult.eErrorCode.MissingMember};
				info.Result.SetValue(string.Format("No property with name {0}.", StringUtils.ToRepresentation(info.Name)));
				return;
			}

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
					value = ChangeType(info.Value, property.PropertyType);
				}
				// Value is the incorrect type.
				catch (Exception)
				{
					info.Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.InvalidParameter };
					info.Result.SetValue(string.Format("Failed to convert to {0}.", property.PropertyType.Name));
					return;
				}

				try
				{
					property.SetValue(instance, value, new object[0]);
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
				info.Result.SetValue(property.GetValue(instance, new object[0]));
		}

		/// <summary>
		/// Changes the given value to the given type.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private static object ChangeType(object value, Type type)
		{
			if (type.IsEnum)
				return Enum.ToObject(type, value);

			return Convert.ChangeType(value, type, null);
		}

		#endregion

		#region API

		[ApiMethod("SetLoggingSeverity", "Sets the severity level for the logging service.")]
		private static void SetLoggingSeverity(eSeverity severity)
		{
			ServiceProvider.GetService<ILoggerService>().SeverityLevel = severity;
		}

		#endregion
	}
}
