using System;
using System.IO;
using System.Runtime.Serialization;


namespace MonoDebugger.SharedLib {

	public interface MessageWithFile {
		string Filename { get; set; }
	}

	[DataContract]
	public class StartDebuggingMessage : MessageWithFile {
		[DataMember]
		public string FileName { get; set; }

		[DataMember]
		public ApplicationType AppType { get; set; }

		[DataMember]
		public WebsiteParameters WebParameters { get; set; }

		[DataMember]
		public string DebugZip { get; set; }

		string MessageWithFile.Filename { get { return DebugZip; }	set { DebugZip = value; } }
	}
}