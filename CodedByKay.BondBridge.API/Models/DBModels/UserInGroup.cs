using System.ComponentModel.DataAnnotations;

namespace CodedByKay.BondBridge.API.Models.DBModels
{
    public class UserGroup
    {
        [Key]
        [Required(ErrorMessage = "UserGroupID is required.")]
        public Guid UserGroupID { get; set; }
        [Required(ErrorMessage = "UserID is required.")]
        public Guid UserID { get; set; }
        [Required(ErrorMessage = "UserGroupID is required.")]
        public Guid GroupID { get; set; }

        public ConversationUser ConversationUser { get; set; }

        public Group Group { get; set; }
    }
}
