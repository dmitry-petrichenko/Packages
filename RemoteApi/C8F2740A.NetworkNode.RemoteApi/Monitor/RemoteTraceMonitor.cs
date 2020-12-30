using System;
using System.Threading;
using System.Threading.Tasks;
using C8F2740A.Common.ExecutionStrategies;

namespace RemoteApi.Monitor
{
    public interface IRemoteTraceMonitor
    {
        void Start();
        void Stop();
        void DisplayNextMessage(string message);
        void DisplayDebugMessage(string message);
        void ClearTextBox();

        public void SetPrompt(string value);

        event Action<string> TextEntered;
    }
    
    public class RemoteTraceMonitor : IRemoteTraceMonitor
    {
        private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);
        
        private bool _isStarted;
        
        public event Action<string> TextEntered;

        private IConsoleTextBox _remoteTextBox;
        private IConsoleTextBox _debugTextBox;
        private IConsoleAbstraction _consoleAbstraction;
        private ILineReaderWithPrompt _lineReaderWithPrompt;
        private int _numberOfLines;

        public RemoteTraceMonitor(IConsoleAbstraction consoleAbstraction, int numberOfLines)
        {
            _numberOfLines = numberOfLines;
            _consoleAbstraction = consoleAbstraction;
            _lineReaderWithPrompt = new LineReaderWithPrompt(new AsyncLineReader(_consoleAbstraction), _consoleAbstraction, _numberOfLines + 1);
            _remoteTextBox = new ConsoleTextBox(_consoleAbstraction, 0, _numberOfLines);
            _debugTextBox = new ConsoleTextBox(_consoleAbstraction, _numberOfLines + 3, 7);
        }

        public void Start()
        {
            SafeExecution.TryCatchAsync(() => StartInternal(), exception => 
                Console.WriteLine(exception.Message));
        }

        private async Task StartInternal()
        {
            _isStarted = true;
            _consoleAbstraction.Clear();
            _lineReaderWithPrompt.Start();
            _consoleAbstraction.DrawSeparatorOnLine(_numberOfLines, ".");
            
            while (_isStarted)
            {
                var text = await _lineReaderWithPrompt.ReadLineAsync();
                TextEntered?.Invoke(text);
            }
        }

        public void Stop()
        {
            _isStarted = false;
            _consoleAbstraction.Clear();
            _consoleAbstraction.SetCursorPosition(0, 0);
        }

        public async void ClearTextBox()
        {
            await semaphoreSlim.WaitAsync();
            
            _remoteTextBox.Clear();
            _lineReaderWithPrompt.SetCursorAfterPrompt();

            semaphoreSlim.Release();
        }

        public async void SetPrompt(string value)
        {
            await semaphoreSlim.WaitAsync();
            
            _lineReaderWithPrompt.SetPrompt(value); 
            _lineReaderWithPrompt.SetCursorAfterPrompt();
            
            semaphoreSlim.Release();
        }

        public async void DisplayNextMessage(string message)
        {
            await semaphoreSlim.WaitAsync();
            
            _remoteTextBox.DisplayNextMessage(message);
            _lineReaderWithPrompt.SetCursorAfterPrompt();
            
            semaphoreSlim.Release();
        }
        
        public async void DisplayDebugMessage(string message)
        {
            await semaphoreSlim.WaitAsync();
            
            _debugTextBox.DisplayNextMessage(message); 
            _lineReaderWithPrompt.SetCursorAfterPrompt();
            
            semaphoreSlim.Release();
        }
    }
}