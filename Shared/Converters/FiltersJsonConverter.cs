using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FilterDataGrid.Converters
{
    internal class FiltersJsonConverter : JsonConverter<FilterCommon>
    {
        private enum DataType
        {
            String = 0,
            Integer = 1,
            Boolean = 3,
            DateTime = 4
        }
        public override bool CanConvert(Type typeToConvert) => typeof(FilterCommon).IsAssignableFrom(typeToConvert);

        public override FilterCommon Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            FilterCommon filterCommon = new FilterCommon();
            string propertyName;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return filterCommon;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    propertyName = reader.GetString();
                    reader.Read();

                    if (propertyName == "FieldType")
                    {
                        string typeAsString = reader.GetString();
                        switch (typeAsString)
                        {
                            case "System.String":
                                filterCommon.FieldType = typeof(string);
                                break;
                            case "System.Int32":
                                filterCommon.FieldType = typeof(int);
                                break;
                            case "System.Boolean":
                                filterCommon.FieldType = typeof(bool);
                                break;
                            case "System.DateTime":
                                filterCommon.FieldType = typeof(DateTime);
                                break;
                        }
                    }
                    else if (propertyName == "FieldName")
                    {
                        reader.GetString(); // Type as string
                        filterCommon.FieldName = reader.GetString();
                    }

                    else if (propertyName == "PreviouslyFilteredItem")
                    {
                        if (reader.TokenType == JsonTokenType.StartArray)
                        {
                            while (reader.TokenType != JsonTokenType.EndArray)
                            {
                                reader.Read();
                                if (reader.TokenType == JsonTokenType.String && filterCommon.FieldType == typeof(DateTime))
                                {
                                    string dateAsString = reader.GetString();
                                    bool validated = DateTime.TryParse(dateAsString, out DateTime dt);
                                    if (!validated) throw new JsonException();
                                    filterCommon.PreviouslyFilteredItems.Add(dt);
                                }
                                else if (reader.TokenType == JsonTokenType.String) filterCommon.PreviouslyFilteredItems.Add(reader.GetString());
                                else if (reader.TokenType == JsonTokenType.Number) filterCommon.PreviouslyFilteredItems.Add(reader.GetInt32());
                                else if (reader.TokenType == JsonTokenType.True) filterCommon.PreviouslyFilteredItems.Add(true);
                                else if (reader.TokenType == JsonTokenType.False) filterCommon.PreviouslyFilteredItems.Add(false);
                            }
                        }
                    }
                    else if (propertyName == "Translate")
                    {
                        filterCommon.Translate = new Loc { Language = (Local)reader.GetInt32() };
                    }
                }
            }
            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, FilterCommon filterCommon, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("FieldName", filterCommon.FieldName);                // To itself (string)
            writer.WriteString("FieldType", filterCommon.FieldType.ToString());     // To string
            if (filterCommon.PreviouslyFilteredItems.Count > 0)                     // To an array of its type
            {
                writer.WritePropertyName("PreviouslyFilteredItem");
                writer.WriteStartArray();
                Type type = filterCommon.PreviouslyFilteredItems.First().GetType();
                // Prevent datatype lookup in itteration
                DataType dataType = DataType.String;
                if (type == typeof(int)) dataType = DataType.Integer;
                if (type == typeof(bool)) dataType = DataType.Boolean;
                if (type == typeof(DateTime)) dataType = DataType.DateTime;

                foreach (object o in filterCommon.PreviouslyFilteredItems)
                {
                    if (dataType == DataType.String) writer.WriteStringValue(o.ToString());
                    if (dataType == DataType.Integer) writer.WriteNumberValue(Convert.ToInt32(o));
                    if (dataType == DataType.Boolean) writer.WriteBooleanValue((bool)o);
                    if (dataType == DataType.DateTime)
                    {
                        DateTime dt = (DateTime)o;
                        writer.WriteStringValue(dt.ToString());
                    }
                }
                writer.WriteEndArray();
            }
            writer.WriteNumber("Translate", (int)filterCommon.Translate.Language);  // To enum (integer)
            writer.WriteEndObject();
        }
    }
}
