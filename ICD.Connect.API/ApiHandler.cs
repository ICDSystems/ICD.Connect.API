using System;
using System.Collections.Generic;
using System.Linq;
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
			ApiClassInfo info = ApiClassInfo.Deserialize(serialized);
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

			foreach (ApiMethodInfo method in info.GetMethods())
				HandleRequest(method, type, instance);

			foreach (ApiPropertyInfo property in info.GetProperties())
				HandleRequest(property, type, instance);

			foreach (ApiNodeInfo node in info.GetNodes())
				HandleRequest(node, type, instance);

			foreach (ApiNodeGroupInfo nodeGroup in info.GetNodeGroups())
				HandleRequest(nodeGroup, type, instance);
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

			object nodeValue = property.GetValue(instance, new object [0]);
			Type nodeType = nodeValue == null
				                ?
#if SIMPLSHARP
				                (Type)
#endif
				                property.PropertyType
				                : nodeValue.GetType();

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
			IApiNodeGroup group = property.GetValue(instance, new object[0]) as IApiNodeGroup;

			foreach (KeyValuePair<uint, ApiClassInfo> kvp in nodeGroup)
			{
				ApiClassInfo classInfo = kvp.Value;
				object classInstance = group[kvp.Key];
				Type classType = classInstance.GetType();

				HandleRequest(classInfo, classType, classInstance);
			}
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
			object[] parameters = info.GetParameters()
			                          .Select(p => p.Value)
			                          .ToArray();
			Type[] types = method.GetParameters()
			                     .Select(p =>
#if SIMPLSHARP
			                             (Type)
#endif
			                             p.ParameterType)
			                     .ToArray();

			for (int index = 0; index < parameters.Length; index++)
				parameters[index] = ChangeType(parameters[index], types[index]);

			method.Invoke(instance, parameters);
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

			if (info.Write)
			{
				object value = ChangeType(info.Value, property.PropertyType);
				property.SetValue(instance, value, new object[0]);
			}

			if (info.Read)
			{
				// TODO
			}
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
