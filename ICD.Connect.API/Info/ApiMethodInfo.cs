﻿using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
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

		/// <summary>
		/// Gets/sets whether or not this method is to be executed.
		/// </summary>
		public bool Execute { get; set; }

		public int ParameterCount { get { return m_Parameters == null ? 0 : m_Parameters.Count; } }

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

		#region Methods

		/// <summary>
		/// Creates a recursive copy of the API info.
		/// </summary>
		/// <returns></returns>
		public ApiMethodInfo DeepCopy()
		{
			ApiMethodInfo output = new ApiMethodInfo();
			DeepCopy(output);

			output.SetParameters(GetParameters().Select(p => p.DeepCopy()));

			return output;
		}

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
			return m_Parameters == null
				? Enumerable.Empty<ApiParameterInfo>()
				: m_Parameters.ToArray(m_Parameters.Count);
		}

		/// <summary>
		/// Sets the parameters for the method.
		/// </summary>
		/// <param name="parameters"></param>
		public void SetParameters(IEnumerable<ApiParameterInfo> parameters)
		{
			if (parameters == null)
				throw new ArgumentNullException("parameters");

			m_Parameters = parameters.ToList();
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

		private IEnumerable<ApiParameterInfo> GetParameterInfo(MethodInfo method, object instance, int depth)
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
	}
}