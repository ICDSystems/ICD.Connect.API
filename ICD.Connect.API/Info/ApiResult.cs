#if NETFRAMEWORK
extern alias RealNewtonsoft;
using RealNewtonsoft.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif
using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Connect.API.Info.Converters;

namespace ICD.Connect.API.Info
{
	[JsonConverter(typeof(ApiResultConverter))]
	public sealed class ApiResult
	{
		public enum eErrorCode
		{
			Ok = 0,
			MissingMember = 1,
			MissingNode = 2,
			InvalidParameter = 3,
			Exception = 4
		}

		#region Properties

		/// <summary>
		/// The error code for this response.
		/// </summary>
		public eErrorCode ErrorCode { get; set; }

		/// <summary>
		/// Gets/sets the data type for the parameter.
		/// </summary>
		public Type Type { get; set; }

		/// <summary>
		/// Gets/sets the value for the parameter.
		/// </summary>
		public object Value { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Sets the value and type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		public void SetValue<T>(T value)
		{
// ReSharper disable once CompareNonConstrainedGenericWithNull
			Type type = value == null ? typeof(T) : value.GetType();
			SetValue(type, value);
		}

		/// <summary>
		/// Sets the value and type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		public void SetValue(Type type, object value)
		{
			Type = type;
			Value = value;
		}

		/// <summary>
		/// Gets the value casting to the given type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		[CanBeNull]
		public T GetValue<T>()
		{
			return (T)GetValue(typeof(T));
		}

		/// <summary>
		/// Gets the value casting to the given type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		[CanBeNull]
		public object GetValue(Type type)
		{
			return ReflectionUtils.ChangeType(Value, type);
		}

		/// <summary>
		/// Gets the string representation for this instance.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			ReprBuilder builder = new ReprBuilder(this);

			builder.AppendProperty("ErrorCode", ErrorCode);
			builder.AppendProperty("Type", Type);
			builder.AppendProperty("Value", Value);

			return builder.ToString();
		}

		#endregion

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

				foreach (ApiNodeGroupKeyInfo node in info.GetNodes())
					ReadResultsRecursive(node, readResult, path);
			}
			path.Pop();
		}

		/// <summary>
		/// Executes the given callback for each result in the given command tree.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="readResult"></param>
		/// <param name="path"></param>
		private static void ReadResultsRecursive(ApiNodeGroupKeyInfo info, Action<ApiResult, Stack<IApiInfo>> readResult, Stack<IApiInfo> path)
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

		#endregion
	}
}
