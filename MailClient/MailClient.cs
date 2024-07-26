using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using MailKit.Net.Pop3;
using MailClient.Google;


namespace MailClient
{
    public class MailClient : IMailClient
     {
        public List<Structures.Message> GetMails(string protocol, string mailServer, int port, bool ssl, string username, string password, string folder, bool IsAllOrUnsee)
        {
			List<Structures.Message> messageList = new List<Structures.Message>();

			if (protocol.Equals("POP3")) {
				using (var client = new Pop3Client ()) {
					client.Connect (mailServer, port, ssl);
					client.Authenticate (username, password);

					for (int i = 0; i < client.Count; i++) {
						var email = client.GetMessage (i);
						messageList.Add(getMessage(email));
					}

					client.Disconnect (true);
				}
			} else {
				using (var client = new ImapClient()) {
					// Connect to the IMAP server
					client.Connect(mailServer, port, ssl);
					// Authenticate with the server
					client.Authenticate(username, password);
					// Select the INBOX folder or any special folder
					client.Inbox.Open(FolderAccess.ReadOnly);
					// Search for unread messages
					var searchQuery = SearchQuery.NotSeen;
					if (IsAllOrUnsee) {
						searchQuery = SearchQuery.All;
					}
					var uids = client.Inbox.Search(searchQuery);
					foreach (var uid in uids)
					{
						// Retrieve the message by UID
						var email = client.Inbox.GetMessage(uid);
						//GenericExtendedActions.LogMessage(AppInfo.GetAppInfo().OsContext, "Subject: " + email.Subject, AppInfo.GetAppInfo().eSpaceName);
						messageList.Add(getMessage(email));
					}
					// Disconnect from the server
					client.Disconnect(true);
				}
			}

			return messageList;
        } // GetAllMailRepository


        public List<Structures.Message> GetMailsFromGmail(string protocol, string mailServer, int port, bool ssl, string username, string password, string folder, bool IsAllOrUnsee)
        {
			List<Structures.Message> messageList = new List<Structures.Message>();

			GoogleClient gClient = new GoogleClient(username, password, "", "", mailServer, port);

			if (protocol.Equals("POP3")) {
				Pop3Client client = gClient.GetPop3Client(AuthType.AppPassword, false);
				for (int i = 0; i < client.Count; i++) {
					var email = client.GetMessage (i);
					messageList.Add(getMessage(email));
				}

				client.Disconnect (true);
				
			} else {

				ImapClient client = gClient.GetImapClient(AuthType.AppPassword, false);
				client.Inbox.Open(FolderAccess.ReadOnly);
				// Search for unread messages
				var searchQuery = SearchQuery.NotSeen;
				if (IsAllOrUnsee) {
					searchQuery = SearchQuery.All;
				}
				var uids = client.Inbox.Search(searchQuery);
				foreach (var uid in uids)
				{
					// Retrieve the message by UID
					var email = client.Inbox.GetMessage(uid);
					//GenericExtendedActions.LogMessage(AppInfo.GetAppInfo().OsContext, "Subject: " + email.Subject, AppInfo.GetAppInfo().eSpaceName);
					messageList.Add(getMessage(email));
				}
				// Disconnect from the server
				client.Disconnect(true);

			}
           
			return messageList;
        } // GetAllMailRepository


		private Structures.Message getMessage(MimeMessage email) {
			Structures.Message message = new Structures.Message
			{
				From = email.From.ToString(),
				Subject = email.Subject,
				Date = email.Date.DateTime,
				BodyHTML = email.TextBody
			};

			List<Structures.MailAttachment> mailAattachments = new List<Structures.MailAttachment>();
			foreach (MimeEntity attachment in email.Attachments) {
				//var fileName = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;
				Structures.MailAttachment mailAattachment = new Structures.MailAttachment();

				using (var memory = new MemoryStream ()) {
					if (attachment is MessagePart) {
						MessagePart rfc822 = (MessagePart) attachment;
						var fileName = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;

						mailAattachment.ContentName = fileName;
						rfc822.Message.WriteTo (memory);
					} else {
						MimePart part = (MimePart) attachment;

						mailAattachment.ContentName = part.FileName;
						mailAattachment.MimeType = part.ContentType.MimeType;
						part.Content.DecodeTo (memory);
					}

					mailAattachment.MimeType = attachment.ContentType.MimeType;
					mailAattachment.ContentBinary = memory.ToArray();
				}

				mailAattachments.Add(mailAattachment);
			}

			message.AttachmentList = mailAattachments;
			return message;
		}


     }

}
