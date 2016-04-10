using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MonoDebugger.SharedLib {
	public class TcpCommunication {
		private readonly DataContractSerializer _serializer;
		private readonly Socket _socket;

		public TcpCommunication(Socket socket) {
			_socket = socket;
			List<Type> contracts =
				 GetType()
					  .Assembly.GetTypes()
					  .Where(x => x.GetCustomAttributes(typeof(DataContractAttribute), true).Any())
					  .ToList();
			_serializer = new DataContractSerializer(typeof(MessageBase), contracts);
		}

		public bool IsConnected {
			get { return _socket.IsSocketConnected(); }
		}

		public void Send(Command cmd, object payload) {
			using (var ms = new MemoryStream()) {
				_serializer.WriteObject(ms, new MessageBase { Command = cmd, Payload = payload });
				byte[] buffer = ms.ToArray();
				_socket.Send(BitConverter.GetBytes(buffer.Length));
				_socket.Send(buffer);
			}
			if (payload is MessageWithFile) {
				var file = ((MessageWithFile)payload).Filename;
				_socket.Send(BitConverter.GetBytes(new FileInfo(file).Length));
				_socket.SendFile(file);
			}
		}

		public MessageBase Receive() {
			var buffer = new byte[sizeof(int)];
			int received = _socket.Receive(buffer);
			int size = BitConverter.ToInt32(buffer, 0);
			return ReceiveContent(size);
		}

		private MessageBase ReceiveContent(int size) {
			MessageBase message;
			using (var ms = new MemoryStream()) {
				int totalReceived = 0;
				while (totalReceived != size) {
					var buffer = new byte[Math.Min(1024*10, size - totalReceived)];
					int received = _socket.Receive(buffer);
					totalReceived += received;
					ms.Write(buffer, 0, received);
				}

				ms.Seek(0, SeekOrigin.Begin);
				message = _serializer.ReadObject(ms) as MessageBase;
			}
			if (message is MessageWithFile && ((MessageWithFile)message).Filename != null) {
				var tmp = Path.GetTempFileName();
				var buffer = new byte[sizeof(long)];
				int received = _socket.Receive(buffer);
				long fsize = BitConverter.ToInt64(buffer, 0);
				const int M = 4096;
				var buf = new byte[M];
				using (var f = new FileStream(tmp, FileMode.Create, FileAccess.Write)) {
					while (fsize > 0) {
						var n = _socket.Receive(buf);
						fsize -= n;
						f.Write(buf, 0, n);
					}
				}
				((MessageWithFile)message).Filename = tmp;
			}
			return message;
		}

		public Task<MessageBase> ReceiveAsync() {
			return Task.Factory.StartNew(() => Receive());
		}

		public void Disconnect() {
			if (_socket != null) {
				_socket.Close(1);
				_socket.Dispose();
			}
		}
	}
}