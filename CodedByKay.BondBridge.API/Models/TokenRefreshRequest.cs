namespace CodedByKay.BondBridge.API.Models
{
    public class TokenRefreshRequest
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
