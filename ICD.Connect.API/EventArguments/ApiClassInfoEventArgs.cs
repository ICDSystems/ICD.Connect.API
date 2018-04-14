using ICD.Common.Utils.EventArguments;
using ICD.Connect.API.Info;

namespace ICD.Connect.API.EventArguments
{
    public sealed class ApiClassInfoEventArgs : GenericEventArgs<ApiClassInfo>
    {
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="data"></param>
		public ApiClassInfoEventArgs(ApiClassInfo data)
			: base(data)
		{
		}
	}
}
