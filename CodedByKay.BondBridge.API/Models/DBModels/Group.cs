using System.ComponentModel.DataAnnotations;

namespace CodedByKay.BondBridge.API.Models.DBModels
{
    public class Group
    {
        [Key]
        [Required(ErrorMessage = "GroupID is required.")]
        public Guid GroupID { get; set; }
        [Required(ErrorMessage = "GroupName is required.")]
        public string GroupName { get; set; }

        public ICollection<UserGroup> UserGroups { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}
