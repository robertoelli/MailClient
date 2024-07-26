using OutSystems.ExternalLibraries.SDK;

namespace MailClient
{
     [OSInterface(Description = "Client to read email using IMAP.", IconResourceName = "MailClient.resources.MailClient.png")]
     public interface IMailClient
     {
         [OSAction(Description = "Get emails from the specified server", IconResourceName = "MailClient.resources.MailClient.png", ReturnName = "MessageList")]
         public List<Structures.Message> GetMails(
            [OSParameter(DataType = OSDataType.Text, Description = "Protocol to use (IMAP, POP3)")]
            string protocol, 
            [OSParameter(DataType = OSDataType.Text, Description = "The email server")]
            string mailServer, 
            [OSParameter(DataType = OSDataType.Integer, Description = "The email server's port")]
            int port, 
            [OSParameter(DataType = OSDataType.Boolean)]
            bool ssl, 
            [OSParameter(DataType = OSDataType.Text, Description = "The email username")]
            string username, 
            [OSParameter(DataType = OSDataType.Text, Description = "The email password")]
            string password, 
            [OSParameter(DataType = OSDataType.Text, Description = "The email folder on the server")]
            string folder, 
            [OSParameter(DataType = OSDataType.Boolean)]
            bool IsAllOrUnsee);

         [OSAction(Description = "Get emails from the specified server", IconResourceName = "MailClient.resources.MailClient.png", ReturnName = "MessageList")]
         public List<Structures.Message> GetMailsFromGmail(
            [OSParameter(DataType = OSDataType.Text, Description = "Protocol to use (IMAP, POP3)")]
            string protocol, 
            [OSParameter(DataType = OSDataType.Text, Description = "The email server")]
            string mailServer, 
            [OSParameter(DataType = OSDataType.Integer, Description = "The email server's port")]
            int port, 
            [OSParameter(DataType = OSDataType.Boolean)]
            bool ssl, 
            [OSParameter(DataType = OSDataType.Text, Description = "The email username")]
            string username, 
            [OSParameter(DataType = OSDataType.Text, Description = "The email password")]
            string password, 
            [OSParameter(DataType = OSDataType.Text, Description = "The email folder on the server")]
            string folder, 
            [OSParameter(DataType = OSDataType.Boolean)]
            bool IsAllOrUnsee);

     }
}