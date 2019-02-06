using System;
using ICD.Common.Utils.Extensions;
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
		[Flags]
		public enum eReadWrite
		{
			None = 0,
			Read = 1,
			Write = 2
		}

		/// <summary>
		/// Gets/sets the type for this property.
		/// </summary>
		public Type Type { get; set; }

		/// <summary>
		/// Gets/sets the value for this property.
		/// </summary>
		public object Value { get; set; }

		/// <summary>
		/// Gets/sets the read write flags for the property.
		/// </summary>
		public eReadWrite ReadWrite { get; set; }

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

			if (property != null && property.CanRead)
				ReadWrite |= eReadWrite.Read;

			if (property != null && property.CanWrite)
				ReadWrite |= eReadWrite.Write;

			Value = instance == null || property == null || !ReadWrite.HasFlag(eReadWrite.Read)
				? null
				: property.GetValue(instance, new object[0]);
		}

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
	}
}