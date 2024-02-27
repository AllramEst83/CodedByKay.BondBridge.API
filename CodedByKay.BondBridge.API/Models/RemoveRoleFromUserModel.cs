using CodedByKay.BondBridge.API.Attributes;
using System.ComponentModel.DataAnnotations;

namespace CodedByKay.BondBridge.API.Models
{
    public class RemoveRoleFromUserModel
    {
        [NotDefaultGuid(ErrorMessage = "UserId must be a non-default GUID.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Role name is required.")]
        [MinLength(1, ErrorMessage = "Role name cannot be empty.")]
        [RegularExpression(@"^\S(.*\S)?$", ErrorMessage = "Role name cannot be whitespace.")]
        public string Role { get; set; } = string.Empty;
    }
}
