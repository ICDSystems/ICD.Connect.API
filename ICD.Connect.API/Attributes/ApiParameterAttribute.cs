using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Connect.API.Info;

namespace ICD.Connect.API.Attributes
{
	[AttributeUsage(AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
	public sealed class ApiParameterAttribute : AbstractApiAttribute
	{
		private static readonly Dictionary<ParameterInfo, ApiParameterAttribute> s_ParameterToAttribute;
		private static readonly Dictionary<MethodInfo, ParameterInfo[]> s_MethodToParameters;

		private static readonly SafeCriticalSection s_ParameterToAttributeSection;
		private static readonly SafeCriticalSection s_MethodToParametersSection;

		/// <summary>
		/// Static constructor.
		/// </summary>
		static ApiParameterAttribute()
		{
			s_ParameterToAttribute = new Dictionary<ParameterInfo, ApiParameterAttribute>();
			s_MethodToParameters = new Dictionary<MethodInfo, ParameterInfo[]>();

			s_ParameterToAttributeSection = new SafeCriticalSection();
			s_MethodToParametersSection = new SafeCriticalSection();	
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		public ApiParameterAttribute(string name, string help)
			: base(name, help)
		{
		}

		/// <summary>
		/// Returns the info for the attribute.
		/// </summary>
		/// <returns></returns>
		public static ApiParameterInfo GetInfo(ParameterInfo parameter)
		{
			ApiParameterAttribute attribute = parameter == null ? null : GetAttribute(parameter);
			return new ApiParameterInfo(attribute, parameter);
		}

		public static IEnumerable<ParameterInfo> GetParameters(MethodInfo method)
		{
			if (method == null)
				throw new ArgumentNullException("method");

			s_MethodToParametersSection.Enter();

			try
			{
				ParameterInfo[] parameters;
				if (!s_MethodToParameters.TryGetValue(method, out parameters))
				{
					parameters = method.GetParameters();
					s_MethodToParameters.Add(method, parameters);
				}

				return parameters;
			}
			finally
			{
				s_MethodToParametersSection.Enter();
			}
		}

		[CanBeNull]
		public static ApiParameterAttribute GetAttribute(ParameterInfo parameter)
		{
			if (parameter == null)
				throw new ArgumentNullException("parameter");

			s_ParameterToAttributeSection.Enter();

			try
			{
				ApiParameterAttribute attribute;
				if (!s_ParameterToAttribute.TryGetValue(parameter, out attribute))
				{
					// Parameter attributes are optional
					attribute = parameter.GetCustomAttributes<ApiParameterAttribute>(true).FirstOrDefault() ??
					                                  new ApiParameterAttribute(parameter.Name, string.Empty);

					s_ParameterToAttribute.Add(parameter, attribute);
				}

				return attribute;
			}
			finally
			{
				s_ParameterToAttributeSection.Leave();
			}
		}
	}
}