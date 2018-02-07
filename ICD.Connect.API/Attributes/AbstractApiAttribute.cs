using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Attributes;
using ICD.Connect.API.Info;

namespace ICD.Connect.API.Attributes
{
	[MeansImplicitUse]
	public abstract class AbstractApiAttribute : AbstractIcdAttribute, IApiAttribute
	{
		private readonly string m_Name;
		private readonly string m_Help;

		/// <summary>
		/// Gets the name for the decorated item. 
		/// </summary>
		public string Name { get { return m_Name; } }

		/// <summary>
		/// Gets the help string for the decorated item.
		/// </summary>
		public string Help { get { return m_Help; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		protected AbstractApiAttribute(string name, string help)
		{
			m_Name = FormatName(name);
			m_Help = FormatHelp(help);
		}

		/// <summary>
		/// Returns the info for the attribute.
		/// </summary>
		/// <param name="memberInfo"></param>
		/// <returns></returns>
		public abstract IApiInfo GetInfo(object memberInfo);

		/// <summary>
		/// Capitalizes the first character of each word and removes all whitespace.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		private static string FormatName(string name)
		{
			if (name == null)
				return string.Empty;

			name = StringUtils.ToTitleCase(name);
			return StringUtils.RemoveWhitespace(name);
		}

		/// <summary>
		/// Trims the string, capitalizes the first letter and adds a period.
		/// </summary>
		/// <param name="help"></param>
		/// <returns></returns>
		private string FormatHelp(string help)
		{
			if (help == null)
				return string.Empty;

			help = help.Trim();
			help = StringUtils.UppercaseFirst(help);

			if (!string.IsNullOrEmpty(help) && !help.EndsWith("."))
				help += '.';

			return help;
		}
	}
}
