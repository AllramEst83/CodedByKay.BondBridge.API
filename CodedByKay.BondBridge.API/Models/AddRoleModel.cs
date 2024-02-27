using System.ComponentModel.DataAnnotations;

namespace CodedByKay.BondBridge.API.Models
{
    public class AddRoleModel
    {
        [Required(ErrorMessage = "Role name is required.")]
        [MinLength(1, ErrorMessage = "Role name cannot be empty.")]
        [RegularExpression(@"^\S(.*\S)?$", ErrorMessage = "Role name cannot be whitespace.")]
        public string RoleName { get; set; } = string.Empty;
    }
}
