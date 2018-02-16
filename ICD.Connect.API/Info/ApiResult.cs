﻿using System;
using ICD.Connect.API.Info.Converters;
using Newtonsoft.Json;

namespace ICD.Connect.API.Info
{
	[JsonConverter(typeof(ApiResultConverter))]
	public sealed class ApiResult
	{
		public enum eErrorCode
		{
			Ok = 0,
			MissingMember = 1,
			MissingNode = 2,
			InvalidParameter = 3,
			Exception = 4
		}

		#region Properties

		/// <summary>
		/// The error code for this response.
		/// </summary>
		public eErrorCode ErrorCode { get; set; }

		/// <summary>
		/// Gets/sets the data type for the parameter.
		/// </summary>
		public Type Type { get; set; }

		/// <summary>
		/// Gets/sets the value for the parameter.
		/// </summary>
		public object Value { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Creates a recursive copy of the API response.
		/// </summary>
		/// <returns></returns>
		public ApiResult DeepCopy()
		{
			return new ApiResult
			{
				ErrorCode = ErrorCode,
				Type = Type,
				Value = Value
			};
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
