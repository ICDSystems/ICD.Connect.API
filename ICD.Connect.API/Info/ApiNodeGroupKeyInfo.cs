#if NETFRAMEWORK
extern alias RealNewtonsoft;
using RealNewtonsoft.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif
using System;
using System.Collections.Generic;
using ICD.Connect.API.Info.Converters;

namespace ICD.Connect.API.Info
{
	[JsonConverter(typeof(ApiNodeGroupKeyInfoConverter))]
	public sealed class ApiNodeGroupKeyInfo : AbstractApiInfo
	{
		private uint m_Key;

		#region Properties

		public uint Key
		{
			get { return m_Key; }
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

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		public ApiNodeGroupKeyInfo()
			: base(null)
		{
		}

		#endregion

		#region Methods

		public static ApiNodeGroupKeyInfo FromClassInfo(uint key, ApiClassInfo classInfo)
		{
			return new ApiNodeGroupKeyInfo
			{
				Key = key,
				Node = classInfo
			};
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

			if (child is ApiClassInfo)
				Node = child as ApiClassInfo;
			else
				throw new ArgumentException(string.Format("{0} can not add child of type {1}", GetType(), child.GetType()));
		}

		/// <summary>
		/// Gets the children attached to this node.
		/// </summary>
		/// <returns></returns>
		protected override IEnumerable<IApiInfo> GetChildren()
		{
			if (Node != null)
				yield return Node;
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

		#endregion
	}
}
