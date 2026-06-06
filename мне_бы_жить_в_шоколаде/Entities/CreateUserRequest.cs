using System.Text.Json.Serialization;

namespace мне_бы_жить_в_шоколаде.Entities
{
    public class CreateUserRequest
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("full_name")]
        public string FullName { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }
    }
}
