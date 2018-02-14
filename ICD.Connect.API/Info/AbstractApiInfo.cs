﻿using ICD.Common.Utils.Json;
using ICD.Connect.API.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ICD.Connect.API.Info
{
	public abstract class AbstractApiInfo : IApiInfo
	{
		private const string PROPERTY_NAME = "name";
		private const string PROPERTY_HELP = "help";
		private const string PROPERTY_RESULT = "result";

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
		protected AbstractApiInfo()
			: this(null)
		{
		}

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
				// Name
				if (!string.IsNullOrEmpty(Name))
				{
					writer.WritePropertyName(PROPERTY_NAME);
					writer.WriteValue(Name);
				}

				// Help
				if (!string.IsNullOrEmpty(Help))
				{
					writer.WritePropertyName(PROPERTY_HELP);
					writer.WriteValue(Help);
				}

				// Reponse
				if (Result != null)
				{
					writer.WritePropertyName(PROPERTY_RESULT);
					Result.Serialize(writer);
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
			instance.Name = (string)token[PROPERTY_NAME];
			instance.Help = (string)token[PROPERTY_HELP];

			JToken responseToken = token[PROPERTY_RESULT];
			if (responseToken != null)
				instance.Result = ApiResult.Deserialize(responseToken);
		}
	}
}
