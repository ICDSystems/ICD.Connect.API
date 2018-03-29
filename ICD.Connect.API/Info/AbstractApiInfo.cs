using ICD.Common.Utils;
using ICD.Connect.API.Attributes;

namespace ICD.Connect.API.Info
{
	public abstract class AbstractApiInfo : IApiInfo
	{
		/// <summary>
		/// Gets/sets the name for the API attribute.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets/sets the help for the API attribute.
		/// </summary>
		public string Help { get; set; }

		/// <summary>
		/// Gets/sets the response message for this request.
		/// </summary>
		public ApiResult Result { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		protected AbstractApiInfo(IApiAttribute attribute)
		{
			Name = attribute == null ? null : attribute.Name;
			Help = attribute == null ? null : attribute.Help;
		}

		/// <summary>
		/// Creates a recursive copy of the API info.
		/// </summary>
		/// <returns></returns>
		protected void DeepCopy(IApiInfo info)
		{
			info.Name = Name;
			info.Help = Help;
			info.Result = Result == null ? null : Result.DeepCopy();
		}

		/// <summary>
		/// Gets the string representation for this instance.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			ReprBuilder builder = new ReprBuilder(this);

			builder.AppendProperty("Name", Name);

			return builder.ToString();
		}
	}
}
