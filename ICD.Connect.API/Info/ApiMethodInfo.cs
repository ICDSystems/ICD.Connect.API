using System;
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
	}
}