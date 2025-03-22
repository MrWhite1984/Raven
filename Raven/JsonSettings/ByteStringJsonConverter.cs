using Google.Protobuf;
using Newtonsoft.Json;

namespace Raven.JsonSettings
{
    public class ByteStringJsonConverter : JsonConverter<ByteString>
    {
        public override ByteString ReadJson(JsonReader reader, Type objectType, ByteString existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var base64String = reader.Value as string;
            return base64String != null ? ByteString.FromBase64(base64String) : null;
        }

        public override void WriteJson(JsonWriter writer, ByteString value, JsonSerializer serializer)
        {
            writer.WriteValue(value?.ToBase64());
        }
    }
}
