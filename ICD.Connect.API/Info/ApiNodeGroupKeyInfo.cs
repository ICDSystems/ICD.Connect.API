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

		/// <summary>
		/// Creates a recursive copy of the API info.
		/// </summary>
		/// <returns></returns>
		public ApiNodeGroupKeyInfo DeepCopy()
		{
			ApiNodeGroupKeyInfo output = new ApiNodeGroupKeyInfo
			{
				Node = Node == null ? null : Node.DeepCopy()
			};

			DeepCopy(output);
			return output;
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
	}
}
