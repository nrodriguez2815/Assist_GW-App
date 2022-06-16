using System.Collections.Generic;

namespace Assist_GW.DTO
{
    public class ApiClient
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string UrlBase { get; set; }
        public List<ApiClientActions> Actions { get; set; }
    }
}
