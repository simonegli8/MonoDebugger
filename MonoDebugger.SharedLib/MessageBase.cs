using System.Runtime.Serialization;

namespace MonoDebugger.SharedLib {
	[DataContract]
	public class MessageBase : MessageWithFile {
		[DataMember]
		public Command Command { get; set; }

		[DataMember]
		public object Payload { get; set; }

		string MessageWithFile.Filename {
			get {
				if (Payload is MessageWithFile) return ((MessageWithFile)Payload).Filename;
				return null;
			}
			set {
				if (Payload is MessageWithFile) ((MessageWithFile)Payload).Filename = value;
			}
		}
	}
}