using ICD.Common.Utils.Json;
using ICD.Connect.API.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ICD.Connect.API.Info
{
	public abstract class AbstractApiInfo : IApiInfo
	{
		private const string NAME_PROPERTY = "name";
		private const string HELP_PROPERTY = "help";

		/// <summary>
		/// Gets/sets the name for the API attribute.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets/sets the help for the API attribute.
		/// </summary>
		public string Help { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractApiInfo()
			: this(null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		protected AbstractApiInfo(AbstractApiAttribute attribute)
		{
			Name = attribute == null ? null : attribute.Name;
			Help = attribute == null ? null : attribute.Help;
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
				if (!string.IsNullOrEmpty(Name))
				{
					writer.WritePropertyName(NAME_PROPERTY);
					writer.WriteValue(Name);
				}

				if (!string.IsNullOrEmpty(Help))
				{
					writer.WritePropertyName(HELP_PROPERTY);
					writer.WriteValue(Help);
				}

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

		/// <summary>
		/// Updates the instance with the information from the JSON object.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="token"></param>
		protected static void Deserialize(AbstractApiInfo instance, JToken token)
		{
			instance.Name = (string)token[NAME_PROPERTY];
			instance.Help = (string)token[HELP_PROPERTY];
		}
	}
}
