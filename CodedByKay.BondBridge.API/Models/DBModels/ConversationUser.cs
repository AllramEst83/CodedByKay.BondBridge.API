using System.ComponentModel.DataAnnotations;

namespace CodedByKay.BondBridge.API.Models.DBModels
{
    public class ConversationUser
    {
        [Key]
        [Required(ErrorMessage = "UserID is required.")]
        public Guid UserID { get; set; }
        [Required(ErrorMessage = "UserName is required.")]
        public string UserName { get; set; }

        public string ImagePath { get; set; }
        public ICollection<UserGroup> UserGroups { get; set; }
    }
}
