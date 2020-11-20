using System;
using System.Timers;
using System.Threading.Tasks;
using C8F2740A.Common.ExecutionStrategies;
using C8F2740A.Common.Records;
using C8F2740A.Networking.ConnectionTCP;

namespace C8F2740A.NetworkNode.SessionProtocol
{
    public interface IInstructionsSender : IDisposable
    {
        Task<(bool, byte[])> TrySendInstruction(byte[] instruction, string password = "aninel");
    }
    
    public class InstructionsSender : IInstructionsSender
    {
        private readonly INodeVisitorFactory _nodeVisitorFactory;
        private readonly INetworkAddress _remoteAddress;
        private readonly Timer _timer;
        private readonly IRecorder _recorder;

        private INodeVisitor _activeNodeVisitor;
        private TaskCompletionSource<byte[]> _sendCommandTask;
        
        public InstructionsSender(
            INetworkAddress remoteAddress, 
            INodeVisitorFactory nodeVisitorFactory,
            IRecorder recorder)
        {
            _recorder = recorder;
            _remoteAddress = remoteAddress;
            _nodeVisitorFactory = nodeVisitorFactory;
            _timer = new Timer(100000) { AutoReset = false, Enabled = true };
            _timer.Elapsed += TimerEventHandler;
        }

        private void TimerEventHandler(object sender, ElapsedEventArgs _)
        {
            SafeExecution.TryCatchAsync(Task.Run(() => TimerEventHandlerInternal(sender, _)), ExceptionHandler);
        }
        
        private void TimerEventHandlerInternal(object sender, ElapsedEventArgs _)
        {
            if (_activeNodeVisitor != default)
            {
                if (!_sendCommandTask.Task.IsCompleted)
                    _sendCommandTask.SetResult(Array.Empty<byte>());
                _activeNodeVisitor.Dispose();
                _activeNodeVisitor = default;
            }
        }

        private void ExceptionHandler(Exception exception)
        {
            _recorder.RecordError(nameof(InstructionsSender), "|TimerEvent| Exception");
        }

        public void Dispose()
        {
            if (!_sendCommandTask.Task.IsCompleted)
                _sendCommandTask.SetResult(Array.Empty<byte>());
        }

        public Task<(bool, byte[])> TrySendInstruction(byte[] command, string password)
        {
            return SafeExecution.TryCatchWithResultAsync(TrySendInstructionInternal(command, password), ExceptionHandler);
        }
        
        private async Task<(bool, byte[])> TrySendInstructionInternal(byte[] command, string password)
        {
            _sendCommandTask = new TaskCompletionSource<byte[]>();
            ResetTimer();
            
            if (_activeNodeVisitor == default)
            {
                _activeNodeVisitor = await CreateNodeVisitor();
            }
            
            return await _activeNodeVisitor.TrySendCommand(command, password);
        }

        private async Task<INodeVisitor> CreateNodeVisitor()
        {
            var nodeVisitor = await _nodeVisitorFactory.Create($"{_remoteAddress.IP}:{_remoteAddress.Port}");
            return nodeVisitor;
        }

        private void ResetTimer()
        {
            _timer.Stop();
            _timer.Start();
        }
    }
}