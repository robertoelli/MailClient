using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using MailKit.Net.Pop3;
using MailClient.Google;
using Microsoft.Extensions.Logging;

namespace MailClient
{
    public class MailClient : IMailClient
     {
		private readonly ILogger _logger;
		private readonly int maxMem = 5000000;
		private int usedMem;

		public MailClient(ILogger logger)
        {
        	_logger = logger;
			usedMem = 0;
        }

        public List<Structures.Message> GetMails(string protocol, string mailServer, int port, bool ssl, string username, string password, string folder, bool IsAllOrUnsee, bool includeAttachments)
        {
			List<Structures.Message> messageList = new List<Structures.Message>();

			GoogleClient gClient = new(username, password, "", "", mailServer, port);

			if (protocol.Equals("POP3")) {
				Pop3Client client = gClient.GetPop3Client(AuthType.AppPassword, false);
				for (int i = 0; i < client.Count; i++) {
					var email = client.GetMessage (i);
					var message = getMessage(email, includeAttachments);
					if (client.Capabilities.HasFlag (Pop3Capabilities.UIDL))
						message.UID = client.GetMessageUid(i);
					else
						message.UID = Guid.NewGuid().ToString();
					messageList.Add(message);
				}

				client.Disconnect (true);
				
			} else {

				ImapClient client = gClient.GetImapClient(AuthType.AppPassword, false);
				IMailFolder currentFolder = getFolder(client, folder);

				// Search for unread messages
				var searchQuery = SearchQuery.NotSeen;
				if (IsAllOrUnsee) {
					searchQuery = SearchQuery.All;
				}
				currentFolder.Open(FolderAccess.ReadOnly);
				var uids = currentFolder.Search(searchQuery);
				foreach (var uid in uids)
				{
					// Retrieve the message by UID
					var email = currentFolder.GetMessage(uid);
					var message = getMessage(email, includeAttachments);
					message.UID = uid.Id.ToString();
					message.Folder = currentFolder.Name;
					messageList.Add(message);
				}
				// Disconnect from the server
				client.Disconnect(true);

			}
           
			return messageList;
        }

        public Structures.Message GetSingleMail(string protocol, string mailServer, int port, bool ssl, string username, string password, string folder, string uid)
        {
			Structures.Message message = new Structures.Message();

			GoogleClient gClient = new(username, password, "", "", mailServer, port);

			if (protocol.Equals("POP3")) {
				_logger.LogError("Single message retrieval is not supported in POP3");				
			} else {
				using (var client = gClient.GetImapClient(AuthType.AppPassword, false)) {
					IMailFolder currentFolder = getFolder(client, folder);
					currentFolder.Open(FolderAccess.ReadWrite);

					// Retrieve the message by UID
					var uniqueId = new UniqueId((uint)Int32.Parse(uid));
					var email = currentFolder.GetMessage(uniqueId);
					_logger.LogInformation("Subject: " + email.Subject);
					message = getMessage(email, true);
					message.UID = uid;
					message.Folder = currentFolder.Name;
					currentFolder.AddFlags(uniqueId, MessageFlags.Seen, true);
					// Disconnect from the server
					client.Disconnect(true);
				}
			}
           
			return message;
        }

        public List<String> GetFolders(string mailServer, int port, bool ssl, string username, string password, string folder)
        {
			List<String> folders = new List<String>();
			GoogleClient gClient = new(username, password, "", "", mailServer, port);

			using (ImapClient client = gClient.GetImapClient(AuthType.AppPassword, false))
			{
				// Get the first personal namespace and list the toplevel folders under it.
				var personal = client.GetFolder(client.PersonalNamespaces[0]);
				foreach (var currFolder in personal.GetSubfolders (false)) {
					//_logger.LogInformation("[folder] {0}", currFolder.Name);
					folders.Add(currFolder.FullName);
				}

				// Disconnect from the server
				client.Disconnect(true);			
			}
           
			return folders;
        }


		private Structures.Message getMessage(MimeMessage email, bool includeAttachments) {
			Structures.Message message = new Structures.Message
			{
				From = email.From.ToString(),
				Subject = email.Subject ?? "",
				Date = email.Date.DateTime,
				BodyHTML = email.HtmlBody ?? email.TextBody ?? ""
			};

			List<Structures.MailAttachment> mailAattachments = new List<Structures.MailAttachment>();
			foreach (MimeEntity attachment in email.Attachments) {
				//var fileName = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;
				Structures.MailAttachment mailAttachment = new Structures.MailAttachment();
				
				using (var memory = new MemoryStream ()) {
					if (attachment is MessagePart) {
						MessagePart rfc822 = (MessagePart) attachment;
						var fileName = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;
						_logger.LogInformation("Attachment[rfc822]: " + fileName);
						mailAttachment.ContentName = fileName ?? "";
						rfc822.Message.WriteTo (memory);
					} else {
						MimePart part = (MimePart) attachment;

						_logger.LogInformation("Attachment[MimePart]: " + part.FileName);
						mailAttachment.ContentName = part.FileName ?? "";
						mailAttachment.MimeType = part.ContentType.MimeType;
						part.Content.DecodeTo (memory);
					}

					usedMem += memory.ToArray().Length;

					mailAttachment.MimeType = attachment.ContentType.MimeType;
					if (includeAttachments)
					{
						_logger.LogInformation("Subject: " + email.Subject + " - Attachment size: " + memory.ToArray().Length / 1024 + "KB");
						if (usedMem <= maxMem) {
							mailAttachment.ContentBinary = memory.ToArray();
							_logger.LogInformation("Attachment added");
						} else {
							mailAttachment.ContentName = mailAttachment.ContentName + " (not downloaded - " + memory.ToArray().Length / 1024 + "KB)";
							_logger.LogInformation("Attachment NOT added");
						}					
					}
				}

				mailAattachments.Add(mailAttachment);
			}


            // Search inline PEC attachments  

            using (var memoryInline = new MemoryStream())
            {
                foreach (var bodyPart in email.BodyParts)
                {
                    var fileName = bodyPart.ContentDisposition?.FileName ?? bodyPart.ContentType.Name;
                    Structures.MailAttachment mailAattachmentInline = new Structures.MailAttachment();

                    // postacert.eml - message/rfc822

                    var rfc822 = bodyPart as MessagePart;

                    if (rfc822 != null)
                    {
                        rfc822.Message.WriteTo(memoryInline);
                        mailAattachmentInline.ContentName = rfc822.ContentType.Name;
                        mailAattachmentInline.MimeType = rfc822.ContentType.MimeType;
                        mailAattachmentInline.ContentBinary = memoryInline.ToArray();
                        mailAattachments.Add(mailAattachmentInline);
                    }

                    // daticert.xml

                    if (fileName == "daticert.xml")
                    {
                        var part = (MimePart)bodyPart;
                        if (part != null)
                        {
                            part.Content.DecodeTo(memoryInline);
                            mailAattachmentInline.ContentName = part.ContentType.Name;
                            mailAattachmentInline.MimeType = part.ContentType.MimeType;
                            mailAattachmentInline.ContentBinary = memoryInline.ToArray();
                            mailAattachments.Add(mailAattachmentInline);
                        }
                    }
                }
            }

			message.AttachmentList = mailAattachments;
			//_logger.LogInformation("Message complete. Memory status: " + usedMem / 1024 + "KB");
			return message;
		}


		private IMailFolder getFolder(ImapClient client, string folder) {
			if (folder is not null && !folder.Trim().Equals(""))
				return client.GetFolder(folder);
			else
				return client.Inbox;
		}
     }

}
