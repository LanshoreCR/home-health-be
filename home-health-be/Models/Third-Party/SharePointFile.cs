using System.Text.Json.Serialization;

namespace home_health_be.Models.Third_Party
{
    public class SharePointFile
    {
        [JsonPropertyName("fileBase64")]
        public string FileBase64 { get; set; } = "";

        [JsonPropertyName("fileName")]
        public string FileName { get; set; } = "";

        [JsonPropertyName("fileType")]
        public string FileType { get; set; } = "";
    }
}
