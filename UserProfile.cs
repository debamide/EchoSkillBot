using Microsoft.Bot.Schema;

namespace EchoBot1
{
    public class UserProfile
    {
        public string Transport { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }

        public Attachment Picture { get; set; }

        public string Ticket { get; set; }
    }
}
