using OutSystems.ExternalLibraries.SDK;

namespace MailClient.Structures
{
    /// <summary>
    /// The Iban struct represents an International Bank Account Number (IBAN) and
    /// its components. It's exposed as a structure to your ODC apps and libraries.
    /// </summary>
    [OSStructure(Description = "Represents a message")]
    public struct Message {
        [OSStructureField(DataType = OSDataType.Text, Description = "The from address.", IsMandatory = true)]
        public string From;

        [OSStructureField(DataType = OSDataType.Text, Description = "The email subjetc.", IsMandatory = true)]
        public string Subject;

        [OSStructureField(DataType = OSDataType.Text, Description = "The email body.", IsMandatory = true)]
        public string BodyHTML;

        [OSStructureField(DataType = OSDataType.DateTime, Description = "The date.", IsMandatory = true)]
        public DateTime Date;

        [OSStructureField(Description = "The list of attachments.", IsMandatory = true)]
        public IEnumerable<Structures.MailAttachment> AttachmentList;

        /// <summary>
        /// Constructs an Iban struct from the IbanNet.Iban object.
        /// </summary>
        public Message(string From, string Subject, string BodyHTML, IEnumerable<Structures.MailAttachment> AttachmentList) : this() {
            this.From = From;
            this.Subject = Subject;
            this.BodyHTML = BodyHTML;
            this.AttachmentList = AttachmentList;
        }
    }

}