using System;
using System.Linq;
using ICD.Common.Properties;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Info;

namespace ICD.Connect.API.Attributes
{
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public sealed class ApiClassAttribute : AbstractApiAttribute
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public ApiClassAttribute()
			: this(null, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		public ApiClassAttribute(string name, string help)
			: base(name, help)
		{
		}

		/// <summary>
		/// Gets the class info for the given type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static ApiClassInfo GetInfo(Type type)
		{
			ApiClassAttribute attribute = type == null ? null : GetAttribute(type);
			return new ApiClassInfo(attribute, type);
		}

		/// <summary>
		/// Gets the class info for the given type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static ApiClassInfo GetInfo(Type type, object instance)
		{
			ApiClassAttribute attribute = type == null ? null : GetAttribute(type);
			return new ApiClassInfo(attribute, type, instance);
		}

		[CanBeNull]
		public static ApiClassAttribute GetAttribute(Type type)
		{
			return
#if SIMPLSHARP
				((CType)type)
#else
				type
#endif
					.GetCustomAttributes<ApiClassAttribute>(true).FirstOrDefault();
		}
	}
}
