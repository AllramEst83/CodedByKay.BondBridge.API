using System.ComponentModel.DataAnnotations;

namespace CodedByKay.BondBridge.API.Models
{
    public class TokenRefreshRequest
    {
        [Required(ErrorMessage = "AccessToken is required.")]
        [RegularExpression(@"^\S(.*\S)?$", ErrorMessage = "AccessTokencannot be whitespace.")]
        public string AccessToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "RefreshToken is required.")]
        [RegularExpression(@"^\S(.*\S)?$", ErrorMessage = "RefreshToken cannot be whitespace.")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
