using System;
using System.Linq;
using ICD.Common.Properties;
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

		[CanBeNull]
		public static ApiParameterAttribute GetAttribute(ParameterInfo parameter)
		{
			if (parameter == null)
				throw new ArgumentNullException("parameter");

			return parameter.GetCustomAttributes<ApiParameterAttribute>(true).FirstOrDefault();
		}
	}
}