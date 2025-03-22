using Newtonsoft.Json;
using Raven.JsonSettings;

namespace Raven.Models
{
    public class GetPostsCacheModel
    {
        public DateTime cursor {  get; set; }
        public List<PostMessage> postMessageList = new();

        public string Serialize()
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ByteStringJsonConverter());
            return JsonConvert.SerializeObject(this, settings);
        }
    }
}
