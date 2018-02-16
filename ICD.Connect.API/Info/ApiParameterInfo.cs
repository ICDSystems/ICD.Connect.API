using System;
using Newtonsoft.Json;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Connect.API.Attributes;
using ICD.Connect.API.Info.Converters;

namespace ICD.Connect.API.Info
{
	[JsonConverter(typeof(ApiParameterInfoConverter))]
	public sealed class ApiParameterInfo : AbstractApiInfo
	{
		/// <summary>
		/// Gets/sets the data type for the parameter.
		/// </summary>
		public Type Type { get; set; }

		/// <summary>
		/// Gets/sets the value for the parameter.
		/// </summary>
		public object Value { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ApiParameterInfo()
			: this(null, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		/// <param name="parameter"></param>
		public ApiParameterInfo(ApiParameterAttribute attribute, ParameterInfo parameter)
			: this(attribute, parameter, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		/// <param name="parameter"></param>
		/// <param name="instance"></param>
		public ApiParameterInfo(ApiParameterAttribute attribute, ParameterInfo parameter, object instance)
			: this(attribute, parameter, instance, int.MaxValue)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		/// <param name="parameter"></param>
		/// <param name="instance"></param>
		/// <param name="depth"></param>
		public ApiParameterInfo(ApiParameterAttribute attribute, ParameterInfo parameter, object instance, int depth)
			: base(attribute)
		{
			if (depth <= 0)
				return;

			Type = parameter == null ? null : parameter.ParameterType;
		}

		#region Methods

		/// <summary>
		/// Creates a recursive copy of the API info.
		/// </summary>
		/// <returns></returns>
		public ApiParameterInfo DeepCopy()
		{
			ApiParameterInfo output = new ApiParameterInfo
			{
				Type = Type,
				Value = Value
			};

			DeepCopy(output);
			return output;
		}

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

		#endregion
	}
}