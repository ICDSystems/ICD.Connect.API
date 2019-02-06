using System;
using ICD.Connect.API.Info.Converters;
using Newtonsoft.Json;

namespace ICD.Connect.API.Info
{
	[JsonConverter(typeof(ApiNodeGroupKeyInfoConverter))]
	public sealed class ApiNodeGroupKeyInfo : AbstractApiInfo
	{
		private uint m_Key;

		public uint Key
		{
			get
			{
				return m_Key;
			}
			set
			{
				m_Key = value;
				Name = value.ToString();
			}
		}

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

		/// <summary>
		/// Creates a new instance of the current type.
		/// </summary>
		/// <returns></returns>
		protected override AbstractApiInfo Instantiate()
		{
			return new ApiNodeGroupKeyInfo();
		}
	}
}
