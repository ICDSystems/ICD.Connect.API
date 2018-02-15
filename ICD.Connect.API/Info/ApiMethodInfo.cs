﻿using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.API.Info
{
	public sealed class ApiMethodInfo : AbstractApiInfo
	{
		private const string PROPERTY_PARAMETERS = "params";

		private readonly List<ApiParameterInfo> m_Parameters;

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
			m_Parameters = new List<ApiParameterInfo>();

			if (depth <= 0)
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
			return m_Parameters.ToArray(m_Parameters.Count);
		}

		/// <summary>
		/// Sets the parameters for the method.
		/// </summary>
		/// <param name="parameters"></param>
		public void SetParameters(IEnumerable<ApiParameterInfo> parameters)
		{
			m_Parameters.Clear();
			m_Parameters.AddRange(parameters);
		}

		/// <summary>
		/// Adds the parameter to the collection.
		/// </summary>
		/// <param name="parameter"></param>
		public void AddParameter(ApiParameterInfo parameter)
		{
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

			foreach (ParameterInfo parameter in method.GetParameters())
			{
				ApiParameterAttribute attribute = GetParameterAttributeForParameter(parameter);
				yield return new ApiParameterInfo(attribute, parameter, instance, depth - 1);
			}
		}

		private ApiParameterAttribute GetParameterAttributeForParameter(ParameterInfo parameter)
		{
			return parameter.GetCustomAttributes<ApiParameterAttribute>(true).FirstOrDefault() ??
			       new ApiParameterAttribute(parameter.Name, null);
		}

		#endregion

		#region Serialization

		/// <summary>
		/// Override to serialize additional properties to the JSON.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteProperties(JsonWriter writer)
		{
			base.WriteProperties(writer);

			// Parameters
			if (m_Parameters.Count > 0)
			{
				writer.WritePropertyName(PROPERTY_PARAMETERS);
				writer.WriteStartArray();
				{
					foreach (ApiParameterInfo parameter in m_Parameters)
						parameter.Serialize(writer);
				}
				writer.WriteEndArray();
			}
		}

		/// <summary>
		/// Deserializes the JSON string to an ApiMethodInfo instance.
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		public static ApiMethodInfo Deserialize(string json)
		{
			JObject jObject = JObject.Parse(json);
			return Deserialize(jObject);
		}

		/// <summary>
		/// Instanties a new instance and applies the JSON object.
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public static ApiMethodInfo Deserialize(JToken token)
		{
			ApiMethodInfo instance = new ApiMethodInfo();
			Deserialize(instance, token);
			return instance;
		}

		/// <summary>
		/// Applies the JSON object info to the given instance.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		public static void Deserialize(ApiMethodInfo instance, JToken token)
		{
			// Parameters
			JToken parameters = token[PROPERTY_PARAMETERS];
			if (parameters != null)
			{
				IEnumerable<ApiParameterInfo> parameterInfo = parameters.Select(m => ApiParameterInfo.Deserialize(m));
				instance.SetParameters(parameterInfo);
			}

			AbstractApiInfo.Deserialize(instance, token);
		}

		#endregion
	}
}