﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RemoteApi;
using RemoteApi.Trace;

namespace C8F2740A.NetworkNode.RemoteApi.Nuget.Trace
{
    public interface IExternalConsolePoint
    {
        void SetLocalStreaming(bool value);
        void SetRemoteStreaming(bool value);
    }
    
    public class ExternalConsolePoint : IExternalConsolePoint
    {
        private IMessageStreamer _messageStreamer;
        private IRemoteApiMap _remoteApiMap;

        private bool IsActiveMessageStream = false;

        public ExternalConsolePoint(
            IRemoteApiMap remoteApiMap,
            IMessageStreamer messageStreamer)
        {
            _remoteApiMap = remoteApiMap;
            _messageStreamer = messageStreamer;
            
            _messageStreamer.SendInstruction += MessageStreamerOnSendInstruction;
            
            _remoteApiMap.RegisterCommand(RemoteApiCommands.TRACE, TraceHandler);
        }

        private Task<(bool, IEnumerable<byte>)> MessageStreamerOnSendInstruction(IEnumerable<byte> instruction)
        {
            if (IsActiveMessageStream)
            {
                return _remoteApiMap.TrySendInstruction(instruction);
            }

            return Task.FromResult((true, Enumerable.Empty<byte>()));
        }

        private IEnumerable<byte> TraceHandler()
        {
            IsActiveMessageStream = true;
            return Convert(_messageStreamer.GetCache());
        }

        private IEnumerable<byte> Convert(IEnumerable<string> values)
        {
            var result = new List<byte>();
            var oneText = string.Empty;
            
            foreach (var value in values)
            {
                oneText += value + Environment.NewLine;
            }

            return oneText.ToEnumerableByte();
        }

        public void SetLocalStreaming(bool value)
        {
            _messageStreamer.SetLocalStreaming(value);
        }

        public void SetRemoteStreaming(bool value)
        {
            _messageStreamer.SetRemoteStreaming(value);
        }
    }
}