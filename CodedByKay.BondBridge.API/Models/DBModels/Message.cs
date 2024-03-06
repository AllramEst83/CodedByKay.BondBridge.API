using System.ComponentModel.DataAnnotations;

namespace CodedByKay.BondBridge.API.Models.DBModels
{
    public class Message
    {
        [Key]
        [Required(ErrorMessage = "MessageID is required.")]
        public Guid MessageID { get; set; }
        [Required(ErrorMessage = "GroupID is required.")]
        public Guid GroupID { get; set; }
        [Required(ErrorMessage = "UserID is required.")]
        public Guid UserID { get; set; }

        public string MessageText { get; set; }
        public DateTime Timestamp { get; set; }
        public ConversationUser User { get; set; }
        public Group Group { get; set; }
    }
}
