using System;
using ICD.Common.Utils.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Connect.API.Attributes;

namespace ICD.Connect.API.Info
{
	public sealed class ApiPropertyInfo : AbstractApiInfo
	{
		private const string PROPERTY_TYPE = "type";
		private const string PROPERTY_VALUE = "value";
		private const string PROPERTY_READ = "read";
		private const string PROPERTY_WRITE = "write";

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
			: base(attribute)
		{
			Type = property == null ? null : property.PropertyType;
			Read = property != null && property.CanRead;
			Write = property != null && property.CanWrite;
			Value = instance == null || property == null || !Read ? null : property.GetValue(instance, new object[0]);
		}

		#region Methods

		/// <summary>
		/// Sets the value and type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		public void SetValue<T>(T value)
		{
			SetValue(typeof(T), value);
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

		#region Serialization

		/// <summary>
		/// Override to serialize additional properties to the JSON.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteProperties(JsonWriter writer)
		{
			base.WriteProperties(writer);

			if (Type != null)
			{
				writer.WritePropertyName(PROPERTY_TYPE);
				writer.WriteValue(Type.FullName);
			}

			// We want to allow serializing null values in a write context
			if (Write)
			{
				writer.WritePropertyName(PROPERTY_VALUE);
				writer.WriteValue(Value);
			}

			if (Read)
			{
				writer.WritePropertyName(PROPERTY_READ);
				writer.WriteValue(Read);
			}

			if (Write)
			{
				writer.WritePropertyName(PROPERTY_WRITE);
				writer.WriteValue(Write);
			}
		}

		/// <summary>
		/// Deserializes the JSON string to an ApiPropertyInfo instance.
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		public static ApiPropertyInfo Deserialize(string json)
		{
			JObject jObject = JObject.Parse(json);
			return Deserialize(jObject);
		}

		/// <summary>
		/// Instanties a new instance and applies the JSON object.
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public static ApiPropertyInfo Deserialize(JToken token)
		{
			ApiPropertyInfo instance = new ApiPropertyInfo();
			Deserialize(instance, token);
			return instance;
		}

		/// <summary>
		/// Applies the JSON object info to the given instance.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		public static void Deserialize(ApiPropertyInfo instance, JToken token)
		{
			// Type
			string typeName = (string)token[PROPERTY_TYPE];
			instance.Type = typeName == null ? null : Type.GetType(typeName, false, true);

			// Value
			instance.Value = JsonUtils.Deserialize(instance.Type, token[PROPERTY_VALUE]);

			// Read
			JToken read = token[PROPERTY_READ];
			instance.Read = read != null && (bool)read;

			// Write
			JToken write = token[PROPERTY_WRITE];
			instance.Write = write != null && (bool)write;

			AbstractApiInfo.Deserialize(instance, token);
		}

		#endregion
	}
}