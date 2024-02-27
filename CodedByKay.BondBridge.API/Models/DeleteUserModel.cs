using CodedByKay.BondBridge.API.Attributes;

namespace CodedByKay.BondBridge.API.Models
{
    public class DeleteUserModel
    {
        [NotDefaultGuid(ErrorMessage = "UserId must be a non-default GUID.")]
        public Guid UserId { get; set; }
    }

}
