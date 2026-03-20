using OutSystems.ExternalLibraries.SDK;

namespace MailClient.Structures
{
    /// <summary>
    /// The Iban struct represents an International Bank Account Number (IBAN) and
    /// its components. It's exposed as a structure to your ODC apps and libraries.
    /// </summary>
    [OSStructure(Description = "Represents a message")]
    public struct Message {
        [OSStructureField(DataType = OSDataType.Text, Description = "The unique ID.", IsMandatory = false)]
        public string UID;

        [OSStructureField(DataType = OSDataType.Text, Description = "The from address.", IsMandatory = true)]
        public string From;

        [OSStructureField(DataType = OSDataType.Text, Description = "The email subject.", IsMandatory = true)]
        public string Subject;

        [OSStructureField(DataType = OSDataType.Text, Description = "The email body.", IsMandatory = true)]
        public string BodyHTML;

        [OSStructureField(DataType = OSDataType.DateTime, Description = "The date.", IsMandatory = true)]
        public DateTime Date;

        [OSStructureField(DataType = OSDataType.Text, Description = "The email folder.", IsMandatory = true)]
        public string Folder;

        [OSStructureField(Description = "The list of attachments.", IsMandatory = true)]
        public IEnumerable<Structures.MailAttachment> AttachmentList;

        /// <summary>
        /// Constructs an Iban struct from the IbanNet.Iban object.
        /// </summary>
        public Message(string UID, string From, string Subject, string BodyHTML, string Folder, IEnumerable<Structures.MailAttachment> AttachmentList) : this() {
            this.UID = UID;
            this.From = From;
            this.Subject = Subject;
            this.BodyHTML = BodyHTML;
            this.Folder = Folder;
            this.AttachmentList = AttachmentList;
        }
    }

}