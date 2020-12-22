using System;
using System.Threading.Tasks;
using C8F2740A.Common.ExecutionStrategies;
using RemoteApi.Trace;

namespace RemoteApi
{
    public interface IRemoteTraceMonitor
    {
        void Start();
        void Stop();
        void DisplayNextMessage(string message);
        void ClearTextBox();

        public void SetPrompt(string value);

        event Action<string> TextEntered;
    }
    
    public class RemoteTraceMonitor : IRemoteTraceMonitor
    {
        private bool _isStarted;
        
        public event Action<string> TextEntered;

        private IConsoleTextBox _consoleTextBox;
        private IConsoleAbstraction _consoleAbstraction;
        private ILineReaderWithPrompt _lineReaderWithPrompt;
        private int _numberOfLines;

        public RemoteTraceMonitor(int numberOfLines)
        {
            _numberOfLines = numberOfLines;
            _consoleAbstraction = new ConsoleAbstraction();
            _lineReaderWithPrompt = new LineReaderWithPrompt(new AsyncLineReader(_consoleAbstraction), _consoleAbstraction, _numberOfLines + 1);
            _consoleTextBox = new ConsoleTextBox(_consoleAbstraction, _numberOfLines);
        }

        public void Start()
        {
            SafeExecution.TryCatchAsync(StartInternal(), exception => 
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

        public void ClearTextBox()
        {
            _consoleTextBox.Clear();
            _lineReaderWithPrompt.SetCursorAfterPrompt();
        }

        public void SetPrompt(string value)
        {
            _lineReaderWithPrompt.SetPrompt(value);
            _lineReaderWithPrompt.SetCursorAfterPrompt();
        }

        public void DisplayNextMessage(string message)
        {
            _consoleTextBox.DisplayNextMessage(message);
            _lineReaderWithPrompt.SetCursorAfterPrompt();
        }
    }

}