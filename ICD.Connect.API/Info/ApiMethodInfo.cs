using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Attributes;
using ICD.Connect.API.Info.Converters;
using Newtonsoft.Json;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.API.Info
{
	[JsonConverter(typeof(ApiMethodInfoConverter))]
	public sealed class ApiMethodInfo : AbstractApiInfo
	{
		[CanBeNull]
		private List<ApiParameterInfo> m_Parameters;

		#region Properties

		/// <summary>
		/// Gets/sets whether or not this method is to be executed.
		/// </summary>
		public bool Execute { get; set; }

		/// <summary>
		/// Gets the number of parameters.
		/// </summary>
		public int ParameterCount { get { return m_Parameters == null ? 0 : m_Parameters.Count; } }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		public ApiMethodInfo()
			: this(null, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		/// <param name="method"></param>
		public ApiMethodInfo(ApiMethodAttribute attribute, MethodInfo method)
			: this(attribute, method, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		/// <param name="method"></param>
		/// <param name="instance"></param>
		public ApiMethodInfo(ApiMethodAttribute attribute, MethodInfo method, object instance)
			: this(attribute, method, instance, int.MaxValue)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		/// <param name="method"></param>
		/// <param name="instance"></param>
		/// <param name="depth"></param>
		public ApiMethodInfo(ApiMethodAttribute attribute, MethodInfo method, object instance, int depth)
			: base(attribute)
		{
			if (depth <= 0)
				return;

			if (method == null)
				return;

			IEnumerable<ApiParameterInfo> parameters = GetParameterInfo(method, instance, depth - 1);
			SetParameters(parameters);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Clears the parameters for the method.
		/// </summary>
		public void ClearParameters()
		{
			SetParameters(Enumerable.Empty<ApiParameterInfo>());
		}

		/// <summary>
		/// Gets the parameters for the method.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ApiParameterInfo> GetParameters()
		{
			return m_Parameters ?? Enumerable.Empty<ApiParameterInfo>();
		}

		/// <summary>
		/// Sets the parameters for the method.
		/// </summary>
		/// <param name="parameters"></param>
		public void SetParameters(IEnumerable<ApiParameterInfo> parameters)
		{
			if (parameters == null)
				throw new ArgumentNullException("parameters");

			m_Parameters = null;
			foreach (ApiParameterInfo parameter in parameters)
				AddParameter(parameter);
		}

		/// <summary>
		/// Adds the parameter to the collection.
		/// </summary>
		/// <param name="parameter"></param>
		public void AddParameter(ApiParameterInfo parameter)
		{
			if (parameter == null)
				throw new ArgumentNullException("parameter");

			if (m_Parameters == null)
				m_Parameters = new List<ApiParameterInfo> {parameter};
			else
				m_Parameters.Add(parameter);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Adds the given item as an immediate child to this node.
		/// </summary>
		/// <param name="child"></param>
		protected override void AddChild(IApiInfo child)
		{
			if (child == null)
				throw new ArgumentNullException("child");

			if (child is ApiParameterInfo)
				AddParameter(child as ApiParameterInfo);
			else
				throw new ArgumentException(string.Format("{0} can not add child of type {1}", GetType(), child.GetType()));
		}

		/// <summary>
		/// Gets the children attached to this node.
		/// </summary>
		/// <returns></returns>
		protected override IEnumerable<IApiInfo> GetChildren()
		{
			return GetParameters();
		}

		/// <summary>
		/// Copies the current state onto the given instance.
		/// </summary>
		/// <param name="info"></param>
		protected override void ShallowCopy(IApiInfo info)
		{
			base.ShallowCopy(info);

			ApiMethodInfo apiMethodInfo = info as ApiMethodInfo;
			if (apiMethodInfo == null)
				throw new ArgumentException("info");

			apiMethodInfo.Execute = Execute;
		}

		/// <summary>
		/// Creates a new instance of the current type.
		/// </summary>
		/// <returns></returns>
		protected override AbstractApiInfo Instantiate()
		{
			return new ApiMethodInfo();
		}

		private static IEnumerable<ApiParameterInfo> GetParameterInfo(MethodInfo method, object instance, int depth)
		{
			if (method == null)
				yield break;

			if (depth <= 0)
				yield break;

			foreach (ParameterInfo parameter in ApiParameterAttribute.GetParameters(method))
			{
				ApiParameterAttribute attribute = ApiParameterAttribute.GetAttribute(parameter);
				if (attribute != null)
					yield return new ApiParameterInfo(attribute, parameter, instance, depth - 1);
			}
		}

		#endregion

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		/// <param name="path"></param>
		public void HandleMethodRequest(Type type, object instance, Stack<IApiInfo> path)
		{
			type = instance == null ? type : instance.GetType();
			MethodInfo method = ApiMethodAttribute.GetMethod(this, type);

			// Couldn't find an ApiMethodAttribute for the given info.
			if (method == null)
			{
				Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.MissingMember };
				Result.SetValue(string.Format("No method with name {0}.", StringUtils.ToRepresentation(Name)));
				ClearParameters();
				return;
			}

			path.Push(this);

			try
			{
				// We're not executing the method so return info about the parameters.
				if (!Execute)
				{
					ApiMethodInfo methodInfo = ApiMethodAttribute.GetInfo(method, instance, 3);
					Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.Ok };
					Result.SetValue(methodInfo);
					return;
				}

				ApiParameterInfo[] parameterInfos = GetParameters().ToArray();
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
					Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.InvalidParameter };
					Result.SetValue(string.Format("Parameters length {0} does not match method parameters length {1}.",
					                              parameters.Length, types.Length));
					ClearParameters();
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
					Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.InvalidParameter };
					Result.SetValue(string.Format("Failed to execute method {0} due to one or more invalid parameters.",
					                              StringUtils.ToRepresentation(Name)));
					return;
				}

				object value;

				try
				{
					value = method.Invoke(instance, parameters);
				}
				// Method failed to execute.
				catch (TargetInvocationException e)
				{
					Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.Exception };
					Result.SetValue(string.Format("Failed to execute method {0} due to {1} - {2}.",
					                              StringUtils.ToRepresentation(Name),
					                              e.InnerException.GetType().Name, e.InnerException.Message));
					return;
				}
				catch (Exception e)
				{
					Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.Exception };
					Result.SetValue(string.Format("Failed to execute method {0} due to {1} - {2}.",
					                              StringUtils.ToRepresentation(Name),
					                              e.GetType().Name, e.Message));
					return;
				}

				Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.Ok };
				Result.SetValue(value);
			}
			finally
			{
				path.Pop();
			}
		}
	}
}