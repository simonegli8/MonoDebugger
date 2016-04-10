using System.Runtime.Serialization;

namespace MonoDebugger.SharedLib {

	[DataContract]
	public enum Runtimes {
		[EnumMember] Net4,
		[EnumMember] Net2
	}

	[DataContract]
	public class WebsiteParameters {
		[DataMember]
		public string Url { get; set; }
		[DataMember]
		public Runtimes Runtime { get; set; }
	}
}
