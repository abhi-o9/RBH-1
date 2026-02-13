using System;

namespace backend.Models
{
    public class Message
    {
        
            public Guid Id { get; set; }
            public string SenderId { get; set; }
            public string ReceiverRole { get; set; }
            public string EncryptedMessage { get; set; }
            public string MessageHash { get; set; }
            public DateTime Timestamp { get; set; }
    }
}


