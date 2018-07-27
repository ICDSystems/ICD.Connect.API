#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Attributes;

namespace ICD.Connect.API.Attributes
{
	[MeansImplicitUse]
	public abstract class AbstractApiAttribute : AbstractIcdAttribute, IApiAttribute
	{
		private readonly string m_Name;
		private readonly string m_Help;

		private string m_FormattedName;
		private string m_FormattedHelp;

		/// <summary>
		/// Gets the name for the decorated item. 
		/// </summary>
		public string Name { get { return m_FormattedName = (m_FormattedName ?? FormatName(m_Name)); } }

		/// <summary>
		/// Gets the help string for the decorated item.
		/// </summary>
		public string Help { get { return m_FormattedHelp = (m_FormattedHelp ?? FormatHelp(m_Help)); } }

		/// <summary>
		/// Gets the binding flags for API discovery.
		/// </summary>
		public static BindingFlags BindingFlags
		{
			get
			{
				return BindingFlags.Instance |
				       BindingFlags.Static |
				       BindingFlags.NonPublic |
				       BindingFlags.Public |
				       BindingFlags.FlattenHierarchy;
			}
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		protected AbstractApiAttribute(string name, string help)
		{
			m_Name = name;
			m_Help = help;
		}

		/// <summary>
		/// Capitalizes the first character of each word and removes all whitespace.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		private static string FormatName(string name)
		{
			if (name == null)
				return string.Empty;

			name = StringUtils.NiceName(name);
			name = StringUtils.ToTitleCase(name);
			return StringUtils.RemoveWhitespace(name);
		}

		/// <summary>
		/// Trims the string, capitalizes the first letter and adds a period.
		/// </summary>
		/// <param name="help"></param>
		/// <returns></returns>
		private static string FormatHelp(string help)
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
