using System;
using System.Linq;
using RemoteApi.Trace;

namespace RemoteApi
{
    public interface IConsoleTextBox
    {
        void DisplayNextMessage(string message);
        void Clear();
    }
    
    public class ConsoleTextBox : IConsoleTextBox
    {
        private readonly TextBuffer _textBuffer;
        private readonly IConsoleAbstraction _consoleAbstraction;
        private readonly int _height;
        
        public ConsoleTextBox(IConsoleAbstraction consoleAbstraction, int height)
        {
            _consoleAbstraction = consoleAbstraction;
            _textBuffer = new TextBuffer();
            _height = height;
        }

        public void DisplayNextMessage(string message)
        {
            DrawText(message);
        }

        public void Clear()
        {
            ClearInternal();
        }
        
        private void DrawText(string value)
        {
            _textBuffer.Add(value);
            
            var result = _textBuffer.Strings.TakeLast(_height + 1);

            ClearInternal();
            
            _consoleAbstraction.SetCursorPosition(0, 0);

            foreach (var s in result)
            {
                _consoleAbstraction.WriteLine(s);
            }
        }

        private void ClearInternal()
        {
            for (int i = 0; i < _height; i++)
            {
                _consoleAbstraction.SetCursorPosition(i, 0);
                _consoleAbstraction.ClearLine(0,i);
            }
        }
        
        private class TextBuffer
        {
            private string content = string.Empty;

            public string[] Strings { get; private set; } 

            public void Add(string value)
            {
                content += value + Environment.NewLine;
                Strings = content.Split(Environment.NewLine);
            }
        } 
    }
}