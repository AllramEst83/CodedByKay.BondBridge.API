namespace CodedByKay.BondBridge.API.Models
{
    public class ApplicationSettings
    {
        public string JwtAudience { get; set; }
        public string JwtIssuer { get; set; }
        public string JwtSigningKey { get; set; }
        public string CustomHeader { get; set; }
    }

}
