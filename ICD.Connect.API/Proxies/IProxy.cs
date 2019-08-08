using System;
using ICD.Connect.API.Info;

namespace ICD.Connect.API.Proxies
{
	public interface IProxy
	{
		/// <summary>
		/// Raised when the proxy makes an API request.
		/// </summary>
		event EventHandler<ApiClassInfoEventArgs> OnCommand;

		/// <summary>
		/// Called to update the proxy with API info.
		/// </summary>
		/// <param name="info"></param>
		void ParseInfo(ApiClassInfo info);

		/// <summary>
		/// Instructs the proxy to raise commands requesting initial values.
		/// </summary>
		void Initialize();

		/// <summary>
		/// Instructs the proxy to clear any initialized values.
		/// </summary>
		void Deinitialize();
	}
}
