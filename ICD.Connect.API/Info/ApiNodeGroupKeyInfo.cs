﻿using System;
using ICD.Connect.API.Info.Converters;
using Newtonsoft.Json;

namespace ICD.Connect.API.Info
{
	[JsonConverter(typeof(ApiNodeGroupKeyInfoConverter))]
	public sealed class ApiNodeGroupKeyInfo : AbstractApiInfo
	{
		public uint Key { get; set; }

		/// <summary>
		/// Gets/sets the node.
		/// </summary>
		public ApiClassInfo Node { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ApiNodeGroupKeyInfo()
			: base(null)
		{
		}

		public static ApiNodeGroupKeyInfo FromClassInfo(uint key, ApiClassInfo classInfo)
		{
			return new ApiNodeGroupKeyInfo
			{
				Key = key,
				Name = key.ToString(),
				Node = classInfo
			};
		}

		/// <summary>
		/// Adds the given item as an immediate child to this node.
		/// </summary>
		/// <param name="child"></param>
		protected override void AddChild(IApiInfo child)
		{
			if (child == null)
				throw new ArgumentNullException("child");

			if (child is ApiClassInfo)
				Node = child as ApiClassInfo;
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

			ApiNodeGroupKeyInfo apiNodeGroupKeyInfo = info as ApiNodeGroupKeyInfo;
			if (apiNodeGroupKeyInfo == null)
				throw new ArgumentException("info");

			apiNodeGroupKeyInfo.Key = Key;
		}
	}
}