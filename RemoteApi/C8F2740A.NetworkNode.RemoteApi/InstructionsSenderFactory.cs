using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;
using C8F2740A.NetworkNode.SessionProtocol;

namespace RemoteApi
{
    public interface IInstructionsSenderFactory
    {
        IInstructionsSender Create(string address);
    }
    
    public class InstructionsSenderFactory : IInstructionsSenderFactory
    {
        public IInstructionsSender Create(string address)
        {
            var recorder = new DefaultRecorder() {ShowErrors = true, ShowInfo = true};
            var isender = new InstructionsSender(new NetworkAddress(address), new NodeVisitorFactory(recorder), new DefaultRecorder() {ShowErrors = true, ShowInfo = true});

            return isender;
        }
    }
}