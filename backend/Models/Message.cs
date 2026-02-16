using System;

namespace backend.Models
{
    public class Message
    {
        
            public Guid Id { get; set; } 
        public string SenderId { get; set; } = string.Empty;
        public string ReceiverRole { get; set; } = string.Empty;
        public string EncryptedMessage { get; set; } = string.Empty;
        public string MessageHash { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } 
    }
}


