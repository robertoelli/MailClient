using OutSystems.ExternalLibraries.SDK;

namespace MailClient.Structures {
    /// <summary>
    /// The Iban struct represents an International Bank Account Number (IBAN) and
    /// its components. It's exposed as a structure to your ODC apps and libraries.
    /// </summary>
    [OSStructure(Description = "Represents an attachment")]
    public struct MailAttachment {
        [OSStructureField(DataType = OSDataType.Text, Description = "Content name.", IsMandatory = true)]
        public string ContentName;

        [OSStructureField(DataType = OSDataType.Text, Description = "MIME type.", IsMandatory = true)]
        public string MimeType;

        [OSStructureField(DataType = OSDataType.BinaryData, Description = "Content.", IsMandatory = true)]
        public byte[] ContentBinary;

        /// <summary>
        /// Constructs an Iban struct from the IbanNet.Iban object.
        /// </summary>
        public MailAttachment(string ContentName, string MimeType, byte[] ContentBinary) : this() {
            this.ContentName = ContentName;
            this.MimeType = MimeType;
            this.ContentBinary = ContentBinary;
        }
    }

}