namespace CodedByKay.BondBridge.API.Model
{
    public class Message
    {

        public Guid MessageId { get; set; }
        public string MessageContent { get; set; }
        public Guid UserId { get; set; }
    }
}
