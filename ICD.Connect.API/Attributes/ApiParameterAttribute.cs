using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
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

		/// <summary>
		/// Static constructor.
		/// </summary>
		static ApiParameterAttribute()
		{
			s_ParameterToAttribute = new Dictionary<ParameterInfo, ApiParameterAttribute>();
			s_MethodToParameters = new Dictionary<MethodInfo, ParameterInfo[]>();
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

			if (!s_MethodToParameters.ContainsKey(method))
			{
				ParameterInfo[] parameters = method.GetParameters().ToArray();
				s_MethodToParameters.Add(method, parameters);
			}

			return s_MethodToParameters[method];
		}

		[CanBeNull]
		public static ApiParameterAttribute GetAttribute(ParameterInfo parameter)
		{
			if (parameter == null)
				throw new ArgumentNullException("parameter");

			if (!s_ParameterToAttribute.ContainsKey(parameter))
			{
				// Parameter attributes are optional
				ApiParameterAttribute attribute = parameter.GetCustomAttributes<ApiParameterAttribute>(true).FirstOrDefault() ??
												  new ApiParameterAttribute(parameter.Name, string.Empty);

				s_ParameterToAttribute.Add(parameter, attribute);
			}

			return s_ParameterToAttribute[parameter];
		}
	}
}