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
	[JsonConverter(typeof(ApiPropertyInfoConverter))]
	public sealed class ApiPropertyInfo : AbstractApiInfo
	{
		/// <summary>
		/// Gets/sets the type for this property.
		/// </summary>
		public Type Type { get; set; }

		/// <summary>
		/// Gets/sets the value for this property.
		/// </summary>
		public object Value { get; set; }

		/// <summary>
		/// Gets/sets if the property can be read.
		/// </summary>
		public bool Read { get; set; }

		/// <summary>
		/// Gets/sets if the property can be written.
		/// </summary>
		public bool Write { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ApiPropertyInfo()
			: this(null, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		/// <param name="property"></param>
		public ApiPropertyInfo(ApiPropertyAttribute attribute, PropertyInfo property)
			: this(attribute, property, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		/// <param name="property"></param>
		/// <param name="instance"></param>
		public ApiPropertyInfo(ApiPropertyAttribute attribute, PropertyInfo property, object instance)
			: this(attribute, property, instance, int.MaxValue)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		/// <param name="property"></param>
		/// <param name="instance"></param>
		/// <param name="depth"></param>
		public ApiPropertyInfo(ApiPropertyAttribute attribute, PropertyInfo property, object instance, int depth)
			: base(attribute)
		{
			if (depth <= 0)
				return;

			Type = property == null ? null : property.PropertyType;
			Read = property != null && property.CanRead;
			Write = property != null && property.CanWrite;
			Value = instance == null || property == null || !Read ? null : property.GetValue(instance, new object[0]);
		}

		#region Methods

		/// <summary>
		/// Creates a recursive copy of the API info.
		/// </summary>
		/// <returns></returns>
		public ApiPropertyInfo DeepCopy()
		{
			ApiPropertyInfo output = new ApiPropertyInfo
			{
				Type = Type,
				Value = Value,
				Read = Read,
				Write = Write
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