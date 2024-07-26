using MailKit.Net.Imap;
using MailKit.Security;
using MimeKit;
using MailKit.Net.Pop3;
using MailKit;
using MailKit.Search;

using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;

namespace MailClient.Google
{
    public interface IGoogleClient
    {
        IEnumerable<MimeMessage> GetPop3Messages(AuthType authType, bool recent = false, bool deleteMessages = false);
        public void DeletePop3Messages(AuthType authType, params int[] ids);
        IEnumerable<MimeMessage> GetImapMessages(AuthType authType, bool recent = false, bool deleteMessages = false);
        void DeleteImapMessages(AuthType authType, params UniqueId[] uids);
        void ActivateImapListener(AuthType authType, Action<object, EventArgs, AuthType> countChangedCallback);
        Pop3Client GetPop3Client(AuthType authType, bool recent);
        ImapClient GetImapClient(AuthType authType, bool recent);
    }

    public class GoogleClient : IGoogleClient
    {
        private string MailServerUserName;
        private string MailServerPassword;
        private string ClientId;
        private string ClientSecret;
        private string Host;
        private int Port;
        private string DownloadedMessagesPath = "";
        

        public GoogleClient(string MailServerUserName, string MailServerPassword, string ClientId, string ClientSecret, string Host, int Port)
        {
            this.MailServerUserName = MailServerUserName;
            this.MailServerPassword = MailServerPassword;
            this.ClientId = ClientId;
            this.ClientSecret = ClientSecret;
            this.Host = Host;
            this.Port = Port;
        }

        public SaslMechanismOAuth2 GetOAuthCredentials(string username, string clientId, string clientSecret)
        {
            var clientSecrets = new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            };

            // we need access to gmail so set the scope for it
            var codeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                DataStore = new FileDataStore("/tmp/CredentialCacheFolder", true),
                Scopes = new[] { "https://mail.google.com/" },
                ClientSecrets = clientSecrets
            });

            var codeReceiver = new LocalServerCodeReceiver();
            var authCode = new AuthorizationCodeInstalledApp(codeFlow, codeReceiver);

            // authorize with the username and client secrets
            var credential = authCode.AuthorizeAsync(username, CancellationToken.None).Result;

            // if it's stale, go ahead and refresh it
            if (credential.Token.IsStale)
            {
                credential.RefreshTokenAsync(CancellationToken.None).ConfigureAwait(false);
            }

            // create the credentials
            var credentials = new SaslMechanismOAuth2(credential.UserId, credential.Token.AccessToken);

            return credentials;
        }
 

        // determine whether or not we're using the most recent messages or all
        private string GetUsername(bool recent, string username)
        {
            return recent ? $"recent:{username}" : username;
        }

        public ImapClient GetImapClient(AuthType authType, bool recent)
        {
            SaslMechanismOAuth2? oAuthCredentials = null;

            // if it's oauth get the credentials
            if (authType == AuthType.OAuth)
            {
                oAuthCredentials = GetOAuthCredentials(MailServerUserName, ClientId, ClientSecret);
            }

            var client = new ImapClient();

            // connect using the imap host and port and ssl = true
            client.Connect(Host, Port, true);

            // if we're not using oauth then use app passwords
            if (oAuthCredentials == null)
            {
                var username = GetUsername(recent, MailServerUserName);

                // make sure the mail server password you use is an app password not your actual gmail password
                client.Authenticate(username, MailServerPassword);
            }
            else
            {
                // if we're using oauth then apply recent to the username if necessary and authenticate
                oAuthCredentials.Credentials.UserName = GetUsername(recent, oAuthCredentials.Credentials.UserName);
                client.Authenticate(oAuthCredentials);
            }

            return client;
        }

        public Pop3Client GetPop3Client(AuthType authType, bool recent)
        {
            SaslMechanismOAuth2? oAuthCredentials = null;

            // get oauth credentials if we're using oauth
            if (authType == AuthType.OAuth)
            {
                oAuthCredentials = GetOAuthCredentials(MailServerUserName, ClientId, ClientSecret);
            }

            var client = new Pop3Client();

            // connect using the pop3 host and port with ssl = true
            client.Connect(Host, Port, true);

            // use app passwords if we're not using oauth
            if (oAuthCredentials == null)
            {
                var username = GetUsername(recent, MailServerUserName);
                client.Authenticate(username, MailServerPassword);
            }
            else
            {
                // otherwise set the username with recent if necessary and authenticate
                oAuthCredentials.Credentials.UserName = GetUsername(recent, oAuthCredentials.Credentials.UserName);
                client.Authenticate(oAuthCredentials);
            }

            return client;
        }

        public IEnumerable<MimeMessage> GetPop3Messages(AuthType authType, bool recent = false, bool deleteMessages = false)
        {
            var result = new List<MimeMessage>();

            // get a pop3 client
            using (var client = GetPop3Client(authType, recent))
            {
                // start from the top and work your way down
                for (var i = client.Count - 1; i >= 0; i--)
                {
                    // get the message by index
                    var message = client.GetMessage(i);
                    result.Add(message);

                    // optionally you can delete the message when you get it, and if you do it will save a copy locally using the pop3 downloaded messages path specified
                    if (deleteMessages)
                    {
                        // remove bad path characters
                        var name = string.Join("_", message.Subject.Split(Path.GetInvalidFileNameChars()));

                        // save it locally
                        message.WriteTo(Path.Combine(DownloadedMessagesPath, $"{i}-{name}.txt"));

                        // tell the server we want to delete it
                        client.DeleteMessage(i);
                    }
                }

                // when disconnecting, optionally delete the messages
                client.Disconnect(deleteMessages);
            }

            return result;
        }

        // Use GetPop3Messages to get the ids, and select which ones to delete and pass to this method
        public void DeletePop3Messages(AuthType authType, params int[] indexes)
        {
            // make sure you have specified a path
            if (!string.IsNullOrWhiteSpace(DownloadedMessagesPath))
            {
                throw new InvalidOperationException($"Invalid {nameof(DownloadedMessagesPath)}");
            }

            // get a pop3 client
            using (var client = GetPop3Client(authType, false))
            {
                foreach (var index in indexes)
                {
                    var message = client.GetMessage(index);

                    // save a copy 
                    var name = string.Join("_", message.Subject.Split(Path.GetInvalidFileNameChars()));
                    
                    // if for whatever reason it fails to write the email, don't delete it and quit early.
                    try
                    {
                        message.WriteTo(Path.Combine(DownloadedMessagesPath, $"{index}-{name}.txt"));
                    }
                    catch
                    {
                        throw;
                    }

                    client.DeleteMessage(index);
                }

                // call disconnect with quit = true to delete the messages
                client.Disconnect(true);
            }
        }

        public IEnumerable<MimeMessage> GetImapMessages(AuthType authType, bool recent = false, bool deleteMessages = false)
        {
            var result = new List<MimeMessage>();

            // get an imap client
            using (var client = GetImapClient(authType, recent))
            {
                // open the primary inbox
                client.Inbox.Open(FolderAccess.ReadOnly);

                // search over all of the messages except for ones that are marked for deletion
                foreach (var uid in client.Inbox.Search(SearchQuery.And(SearchQuery.All, SearchQuery.NotDeleted)))
                {
                    try
                    {
                        // get the message by uid
                        var message = client.Inbox.GetMessage(uid);
                        result.Add(message);

                        // if we're deleting the message, save a local copy, and mark it with the deleted flag silently
                        if (deleteMessages)
                        {
                            var name = string.Join("_", message.Subject.Split(Path.GetInvalidFileNameChars()));
                            message.WriteTo(Path.Combine(DownloadedMessagesPath, $"{uid}-{name}.txt"));
                            client.Inbox.AddFlags(uid, MessageFlags.Deleted, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex); // keep trying each message
                    }
                }

                // if we're deleting messages, expunge the inbox to delete messages marked for deletion
                if (deleteMessages)
                {
                    client.Inbox.Expunge();
                }
            }

            return result;
        }

        public void DeleteImapMessages(AuthType authType, params UniqueId[] uids)
        {
            // nothing to do
            if (uids.Length == 0)
            {
                return;
            }

            using (var client = GetImapClient(authType, false))
            {
                var folder = client.Inbox;

                // open the primary inbox with read/write permissions
                folder.Open(FolderAccess.ReadWrite);

                // for each uid, add the deleted flag to mark for deletion
                foreach (var uid in uids)
                {
                    var message = client.Inbox.GetMessage(uid);

                    // save a copy 
                    var name = string.Join("_", message.Subject.Split(Path.GetInvalidFileNameChars()));
                    
                    // if for whatever reason it fails to write the email, don't delete it and quit early.
                    try
                    {
                        message.WriteTo(Path.Combine(DownloadedMessagesPath, $"{uid}-{name}.txt"));
                    }
                    catch
                    {
                        throw;
                    }

                    client.Inbox.AddFlags(uid, MessageFlags.Deleted, true); // this will be deleted after
                }

                // delete all messages marked for deletion
                client.Inbox.Expunge();
            }
        }

        public void ActivateImapListener(AuthType authType, Action<object, EventArgs, AuthType> countChangedCallback)
        {
            using (var client = GetImapClient(authType, false)) // this is the main client that runs
            {
                client.Inbox.Open(FolderAccess.ReadOnly);

                client.Inbox.CountChanged += (sender, e) => 
                {
                    countChangedCallback(sender, e, authType);
                };

                using (var done = new CancellationTokenSource()) // keep listening indefinitely
                {
                    var task = client.IdleAsync(done.Token);
                    Task.Delay(new TimeSpan(0, 15, 0)).Wait(); // After 15 minutes, shut down the listener and let it start up again
                    done.Cancel();
                    task.Wait();
                }

                client.Disconnect(true);
            }
        }
    }
}