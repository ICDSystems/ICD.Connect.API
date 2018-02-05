using ICD.Common.Utils.Json;
using ICD.Connect.API.Attributes;
using Newtonsoft.Json;

namespace ICD.Connect.API.Info
{
	public abstract class AbstractApiInfo : IApiInfo
	{
		private const string NAME_PROPERTY = "name";
		private const string HELP_PROPERTY = "help";

		private readonly string m_Name;
		private readonly string m_Help;

		/// <summary>
		/// Gets the name for the API attribute.
		/// </summary>
		public string Name { get { return m_Name; } }

		/// <summary>
		/// Gets the help for the API attribute.
		/// </summary>
		public string Help { get { return m_Help; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		protected AbstractApiInfo(string name, string help)
		{
			m_Name = name;
			m_Help = help;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		protected AbstractApiInfo(AbstractApiAttribute attribute)
			: this(attribute.Name, attribute.Help)
		{
		}

		/// <summary>
		/// Serializes the instance to JSON.
		/// </summary>
		/// <returns></returns>
		public string Serialize()
		{
			return JsonUtils.Serialize(Serialize);
		}

		/// <summary>
		/// Serializes the instance to JSON.
		/// </summary>
		/// <param name="writer"></param>
		public void Serialize(JsonWriter writer)
		{
			writer.WriteStartObject();
			{
				writer.WritePropertyName(NAME_PROPERTY);
				writer.WriteValue(m_Name);

				writer.WritePropertyName(HELP_PROPERTY);
				writer.WriteValue(m_Help);

				WriteProperties(writer);
			}
			writer.WriteEndObject();
		}

		/// <summary>
		/// Override to serialize additional properties to the JSON.
		/// </summary>
		/// <param name="writer"></param>
		protected virtual void WriteProperties(JsonWriter writer)
		{
		}
	}
}
