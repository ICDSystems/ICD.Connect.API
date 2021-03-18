using System;
using System.Collections.Generic;
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
		#region Properties

		/// <summary>
		/// Gets/sets the data type for the parameter.
		/// </summary>
		public Type Type { get; set; }

		/// <summary>
		/// Gets/sets the value for the parameter.
		/// </summary>
		public object Value { get; set; }

		#endregion

		#region Constructors

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

			throw new ArgumentException(string.Format("{0} can not add child of type {1}", GetType(), child.GetType()));
		}

		/// <summary>
		/// Gets the children attached to this node.
		/// </summary>
		/// <returns></returns>
		protected override IEnumerable<IApiInfo> GetChildren()
		{
			yield break;
		}

		/// <summary>
		/// Copies the current state onto the given instance.
		/// </summary>
		/// <param name="info"></param>
		protected override void ShallowCopy(IApiInfo info)
		{
			base.ShallowCopy(info);

			ApiParameterInfo apiParameterInfo = info as ApiParameterInfo;
			if (apiParameterInfo == null)
				throw new ArgumentException("info");

			apiParameterInfo.Type = Type;
			apiParameterInfo.Value = Value;
		}

		/// <summary>
		/// Creates a new instance of the current type.
		/// </summary>
		/// <returns></returns>
		protected override AbstractApiInfo Instantiate()
		{
			return new ApiParameterInfo();
		}

		#endregion
	}
}