using OutSystems.ExternalLibraries.SDK;

namespace MailClient
{
     [OSInterface(Description = "Client to read email using IMAP or POP3.", IconResourceName = "MailClient.resources.MailClient.png")]
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
            bool IsAllOrUnsee,
            [OSParameter(DataType = OSDataType.Boolean)]
            bool includeAttachments);

         [OSAction(Description = "Get a single email from the specified server (IMAP)", IconResourceName = "MailClient.resources.MailClient.png", ReturnName = "Message")]
         public Structures.Message GetSingleMail(
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
            [OSParameter(DataType = OSDataType.Text, Description = "The email unique ID")]
            string uid);

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
            bool IsAllOrUnsee,
            [OSParameter(DataType = OSDataType.Boolean)]
            bool includeAttachments);

         [OSAction(Description = "Get a single email from the specified server (IMAP)", IconResourceName = "MailClient.resources.MailClient.png", ReturnName = "Message")]
         public Structures.Message GetSingleMailFromGmail(
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
            [OSParameter(DataType = OSDataType.Text, Description = "The email unique ID")]
            string uid);

         [OSAction(Description = "Get folders from the specified server", IconResourceName = "MailClient.resources.MailClient.png", ReturnName = "Folders")]
         public List<String> GetFoldersFromGmail(
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