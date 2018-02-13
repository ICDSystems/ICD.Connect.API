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
using ICD.Common.Utils.Extensions;

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
		/// <param name="json"></param>
		/// <returns></returns>
		public static ApiClassInfo HandleRequest(string json)
		{
			ApiClassInfo info = ApiClassInfo.Deserialize(json);
			return HandleRequest(info);
		}

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public static ApiClassInfo HandleRequest(ApiClassInfo request)
		{
			if (request == null)
				throw new ArgumentNullException("info");

			ApiClassInfo response = request.DeepCopy();
			HandleRequest(request, response, typeof(ApiHandler), null);

			return response;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="request"></param>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		private static void HandleRequest(ApiClassInfo request, ApiClassInfo output, Type type, object instance)
		{
			if (request == null)
				throw new ArgumentNullException("info");

			type = instance == null ? type : instance.GetType();
			bool handled = false;

			handled |= ZipHandleRequest(request.GetMethods(), output.GetMethods(), (a, b) => HandleRequest(a, b, type, instance));
			handled |= ZipHandleRequest(request.GetProperties(), output.GetProperties(), (a, b) => HandleRequest(a, b, type, instance));
			handled |= ZipHandleRequest(request.GetNodes(), output.GetNodes(), (a, b) => HandleRequest(a, b, type, instance));
			handled |= ZipHandleRequest(request.GetNodeGroups(), output.GetNodeGroups(), (a, b) => HandleRequest(a, b, type, instance));

			if (handled)
				return;

			// If the command wasn't handled at this stage we respond with info about the features on this class
			throw new NotImplementedException();
		}

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		private static void HandleRequest(ApiNodeInfo node, ApiNodeInfo output, Type type, object instance)
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

			HandleRequest(node.Node, output.Node, nodeType, nodeValue);
		}

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="nodeGroup"></param>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		private static void HandleRequest(ApiNodeGroupInfo nodeGroup, ApiNodeGroupInfo output, Type type, object instance)
		{
			type = instance == null ? type : instance.GetType();

			PropertyInfo property = ApiNodeGroupAttribute.GetProperty(nodeGroup, type);
			IApiNodeGroup group = property.GetValue(instance, new object[0]) as IApiNodeGroup;

			ZipHandleRequest(nodeGroup, output, (a, b) =>
			                                    {
				                                    object classInstance = group[a.Key];
				                                    Type classType = classInstance.GetType();

				                                    HandleRequest(a.Value, b.Value, classType, classInstance);
			                                    });
		}

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		private static void HandleRequest(ApiMethodInfo info, ApiMethodInfo output, Type type, object instance)
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
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		private static void HandleRequest(ApiPropertyInfo info, ApiPropertyInfo output, Type type, object instance)
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

		/// <summary>
		/// Returns true if there was anything in the sequence.
		/// </summary>
		/// <typeparam name="TFirst"></typeparam>
		/// <typeparam name="TSecond"></typeparam>
		/// <param name="first"></param>
		/// <param name="second"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		private static bool ZipHandleRequest<TFirst, TSecond>(this IEnumerable<TFirst> first,
		                                                      IEnumerable<TSecond> second,
		                                                      Action<TFirst, TSecond> callback)
		{
			bool output = false;

			first.Zip(second, (a, b) =>
			                  {
				                  output = true;
				                  callback(a, b);
			                  });

			return output;
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
