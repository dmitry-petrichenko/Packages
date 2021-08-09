using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using C8F2740A.Common.Records;
using C8F2740A.NetworkNode.SessionTCP;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace C8F2740A.NetworkNode.SessionTCPTests
{
    public class SessionHolderTests
    {
        private ISessionHolder _sut;
        private IRecorder _recorder;

        public SessionHolderTests()
        {
            _recorder = Mock.Create<IRecorder>();
            _sut = new SessionHolder(_recorder);
        }
        
        #region Set
        [Fact]
        public void Set_WhenCalled_ShouldSubscribeOnSession()
        {
            var session = Mock.Create<ISession>();
            session.ArrangeSet(x => x.Received += null).IgnoreArguments().Occurs(1);
            session.ArrangeSet(x => x.Responded += null).IgnoreArguments().Occurs(1);
            
            _sut.Set(session);
            
            session.AssertAll();
        }
        
        [Fact]
        public void Set_WhenCalled_ShouldCallClear()
        {
            var session1 = Mock.Create<ISession>();
            var session2 = Mock.Create<ISession>();
            _sut.Set(session1);

            _sut.Set(session2);
            
            Mock.Assert(() => session1.Dispose(), Occurs.Exactly(1));
        }
        
        [Fact]
        public void Set_WhenCalledWithNull_ShouldThrow()
        {
            Assert.Throws<Exception>(() => _sut.Set(null));
        }
        
        [Fact]
        public void HasActiveSession_WhenSet_ShouldBeTrue()
        {
            var session = Mock.Create<ISession>();
            var before = _sut.HasActiveSession;
            
            _sut.Set(session);
            
            Assert.False(before);
            Assert.True(_sut.HasActiveSession);
        }
        #endregion 
        
        #region SendInstruction
        [Fact]
        public void SendInstruction_WhenCalled_ShouldSend()
        {
            var session = Mock.Create<ISession>();
            _sut.Set(session);
            _sut.SendInstruction(Enumerable.Empty<byte>());
            
            Mock.Assert(() => session.Send(Arg.IsAny<IEnumerable<byte>>()), Occurs.Exactly(1));
        }
        
        [Fact]
        public void SendInstruction_WhenCalledWithoutSession_ShouldThrow()
        {
            var task = _sut.SendInstruction(Enumerable.Empty<byte>());
            
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            Mock.Assert(() => _recorder.DefaultException(
                Arg.IsAny<Object>(), 
                Arg.IsAny<Exception>()), Occurs.Exactly(1));
        }
        
        [Fact]
        public void SendInstruction_WhenCalledTwice_ShouldThrow()
        {
            var session = Mock.Create<ISession>();
            _sut.Set(session);
            _sut.SendInstruction(Enumerable.Empty<byte>());
            var task = _sut.SendInstruction(Enumerable.Empty<byte>());
            
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            Mock.Assert(() => _recorder.DefaultException(
                Arg.IsAny<Object>(), 
                Arg.IsAny<Exception>()), Occurs.Exactly(1));
        }
        
        /*
        [Fact]
        public async void SendInstruction_WhenCalledAndSetCalled_ShouldThrow()
        {
            var session1 = new SessionMock();
            var session2 = new SessionMock();
            _sut.Set(session1);
            var task = _sut.SendInstruction(Enumerable.Empty<byte>());
            _sut.Set(session2);
            var r = await task;
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
            Mock.Assert(() => _recorder.DefaultException(
                Arg.IsAny<Object>(), 
                Arg.IsAny<Exception>()), Occurs.Exactly(1));
        }*/
        
        [Theory]
        [InlineData(new byte[] { 0b1000_1111 })]
        [InlineData(new byte[] { 0b0100_1111 })]
        [InlineData(new byte[] { 0b0010_1111 })]
        [InlineData(new byte[] { 0b1001_1111 })]
        public void SendInstruction_WhenCalled_ShouldResponseWithBytes(IEnumerable<byte> dataToReceive)
        {
            var taskStatus = new TaskStatus();
            var session = new SessionMock();
            _sut.Set(session);
            var resultTask = _sut.SendInstruction(Enumerable.Empty<byte>());
            taskStatus = resultTask.Status;
            
            session.TriggerReceiveResponse(dataToReceive);
            
            Assert.Equal(TaskStatus.WaitingForActivation, taskStatus);
            Assert.Equal(TaskStatus.RanToCompletion, resultTask.Status);
            Assert.True(resultTask.Result.Item1);
            Assert.Equal(resultTask.Result.Item2, dataToReceive);
        }
        
        [Fact]
        public void SendInstruction_WhenSessionThrows_ShouldCatch()
        {
            var session = Mock.Create<ISession>();
            Mock.Arrange(() => session.Send(Arg.IsAny<IEnumerable<byte>>())).Throws<Exception>();
            _sut.Set(session);

            var task = _sut.SendInstruction(Enumerable.Empty<byte>());
            
            Mock.Assert(() => _recorder.DefaultException(
                Arg.IsAny<Object>(), 
                Arg.IsAny<Exception>()), Occurs.Exactly(1));
        }
        #endregion 
        
        #region Received
        [Theory]
        [InlineData( 0b0100_1111, 0b0100_1111 )]
        [InlineData( 0b0100_1001, 0b0100_1011 )]
        [InlineData( 0b0100_1101, 0b0100_1110 )]
        public void Received_WhenCalled_ShouldRaiseAndSendResponse(byte dataToResponse, byte dataToReceive)
        {
            var dataToResponseEnum = dataToResponse.ToEnumerable();
            var dataToReceiveEnum = dataToReceive.ToEnumerable();
            var responseActualData = default(IEnumerable<byte>);
            var receivedActualData = default(IEnumerable<byte>);
            var session = new SessionMock();
            session.ResponseCalledWithData += data => responseActualData = data;
            _sut.InstructionReceived += received =>
            {
                receivedActualData = received;
                return dataToResponseEnum;
            };
            _sut.Set(session);
            
            session.TriggerReceiveReceived(dataToReceiveEnum);
            
            Assert.Equal(responseActualData, dataToResponseEnum);
            Assert.Equal(receivedActualData, dataToReceiveEnum);
        }
        
        [Fact]
        public void SessionResponse_WhenThrowException_ShouldRaiseAndSendResponse()
        {
            var dataToResponseEnum = ((byte)0b0101_0000).ToEnumerable();
            var dataToReceiveEnum = ((byte)0b0101_1111).ToEnumerable();
            var responseActualData = default(IEnumerable<byte>);
            var receivedActualData = default(IEnumerable<byte>);
            var session = new SessionMock(true);
            _sut.InstructionReceived += received =>
            {
                receivedActualData = received;
                return dataToResponseEnum;
            };
            _sut.Set(session);
            try
            {
                session.TriggerReceiveReceived(dataToReceiveEnum);
            }
            catch (Exception e)
            {
                var t = 10;
            }
            
            Assert.NotEqual(responseActualData, dataToResponseEnum);
            Assert.Equal(receivedActualData, dataToReceiveEnum);
            Mock.Assert(() => _recorder.DefaultException(
                Arg.IsAny<Object>(), 
                Arg.IsAny<Exception>()), Occurs.Exactly(1));
        }
        #endregion 
        
        #region Close
        [Fact]
        public void Disconnected_WhenRaised_ShouldRaiseDisconnected()
        {
            bool disconnectedCalled = false;
            var session = Mock.Create<ISession>();

            _sut.Disconnected += () => disconnectedCalled = true;
            _sut.Set(session);
            
            Mock.Raise(() => session.Disconnected += null); 
            
            Assert.True(disconnectedCalled);
        }
        #endregion
    }
    
    internal class SessionMock : ISession
    {
        private IEnumerable<byte> _dataSent;
        
        private readonly bool _throwsOnResponse;

        public SessionMock(bool throwsOnResponse = false)
        {
            _dataSent = Enumerable.Empty<byte>();
            _throwsOnResponse = throwsOnResponse;
        }

        public void Dispose()
        {
        }

        public void Listen()
        {
        }

        public void Response(IEnumerable<byte> data)
        {
            if (_throwsOnResponse)
            {
                throw new Exception("Exception");
            }
            
            ResponseCalledWithData?.Invoke(data);
        }

        public void Send(IEnumerable<byte> data)
        {
            _dataSent = data;
        }

        public void Close()
        {
            Closed?.Invoke();
        }

        public void TriggerReceiveResponse(IEnumerable<byte> data)
        {
            Responded?.Invoke(data);
        }
        
        public void TriggerReceiveReceived(IEnumerable<byte> data)
        {
            Received?.Invoke(data);
        }

        public event Action<IEnumerable<byte>> Received;
        public event Action<IEnumerable<byte>> Responded;
        public event Action Disconnected;
        public event Action Closed;
        
        public event Action<IEnumerable<byte>> ResponseCalledWithData;
    }
}