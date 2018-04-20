using ICD.Common.Utils.EventArguments;
using ICD.Connect.API.Info;

namespace ICD.Connect.API
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
