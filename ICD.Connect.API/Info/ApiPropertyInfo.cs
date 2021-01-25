using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils;
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

		#region Properties

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

		#endregion

		#region Constructors

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

		/// <summary>
		/// Gets the current value on the given instance.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		/// <returns></returns>
		public object GetValue([NotNull] Type type, [CanBeNull] object instance)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			type = instance == null ? type : instance.GetType();
			PropertyInfo property = ApiPropertyAttribute.GetProperty(this, type);

			return property.GetValue(instance);
		}

		/// <summary>
		/// Interprets the incoming API request.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		/// <param name="path"></param>
		public void HandlePropertyRequest([NotNull] Type type, [CanBeNull] object instance, [NotNull] Stack<IApiInfo> path)
		{
			type = instance == null ? type : instance.GetType();
			PropertyInfo property = ApiPropertyAttribute.GetProperty(this, type);

			// Couldn't find an ApiPropertyAttribute for the given info.
			if (property == null)
			{
				Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.MissingMember };
				Result.SetValue(string.Format("No property with name {0}.", StringUtils.ToRepresentation(Name)));
				return;
			}

			path.Push(this);

			try
			{
				// Set the value
				if (ReadWrite.HasFlag(ApiPropertyInfo.eReadWrite.Write))
				{
					// Trying to write to a readonly property.
					if (!property.CanWrite)
					{
						Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.MissingMember };
						Result.SetValue(string.Format("Property {0} is readonly.", StringUtils.ToRepresentation(Name)));
						return;
					}

					object value;

					try
					{
						value = ReflectionUtils.ChangeType(Value, property.PropertyType);
					}
					// Value is the incorrect type.
					catch (Exception)
					{
						Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.InvalidParameter };
						Result.SetValue(string.Format("Failed to convert to {0}.", property.PropertyType.Name));
						return;
					}

					try
					{
						property.SetValue(instance, value, null);
					}
					// Property failed to execute.
					catch (Exception e)
					{
						Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.Exception };
						Result.SetValue(string.Format("Failed to set property {0} due to {1} - {2}.",
						                              StringUtils.ToRepresentation(Name),
						                              e.GetType().Name, e.Message));
						return;
					}
				}

				// Add the response
				Result = new ApiResult { ErrorCode = ApiResult.eErrorCode.Ok };
				if (property.CanRead)
					Result.SetValue(property.PropertyType, property.GetValue(instance, null));
			}
			finally
			{
				path.Pop();
			}
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

		protected override IEnumerable<IApiInfo> GetChildren()
		{
			yield break;
		}

		/// <summary>
		/// Creates a new instance of the current type.
		/// </summary>
		/// <returns></returns>
		protected override AbstractApiInfo Instantiate()
		{
			return new ApiPropertyInfo();
		}

		#endregion
	}
}