using System;
using System.Net.Sockets;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP.Network;
using C8F2740A.NetworkNode.SessionTCP.Factories;

namespace RemoteApi.Integration.Helpers
{
    public class TestInstructionSenderFactory : BaseInstructionSenderFactory
    {
        private readonly Func<AddressFamily, SocketType, ProtocolType, ISocket> _socketFactory;
        
        public TestInstructionSenderFactory(Func<AddressFamily, SocketType, ProtocolType, ISocket> socketFactory, IRecorder recorder) : base(recorder)
        {
            _socketFactory = socketFactory;
        }

        protected override ISocket SocketFactory(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            return _socketFactory.Invoke(addressFamily, socketType, protocolType);
        }
    }
}