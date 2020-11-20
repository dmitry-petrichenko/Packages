using System.Threading.Tasks;
using C8F2740A.Common.Records;

namespace C8F2740A.NetworkNode.SessionProtocol
{
    public interface INodeVisitorFactory
    {
        Task<INodeVisitor> Create(string remoteAddress);
    }
    
    public class NodeVisitorFactory : INodeVisitorFactory
    {
        private readonly IRecorder _recorder;

        public NodeVisitorFactory(IRecorder recorder)
        {
            _recorder = recorder;
        }

        public async Task<INodeVisitor> Create(string remoteAddress)
        {
            var sessionFactory = new TransmitSessionFactory(_recorder);
            var nodeVisitor = new NodeVisitor(remoteAddress, sessionFactory, _recorder);

            return nodeVisitor;
        }
    }
}