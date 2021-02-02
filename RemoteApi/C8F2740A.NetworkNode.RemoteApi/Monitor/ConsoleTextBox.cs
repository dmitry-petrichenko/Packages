using System;
using System.Collections.Generic;
using System.Linq;

namespace C8F2740A.NetworkNode.RemoteApi.Monitor
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
        private readonly int _top;
        
        public ConsoleTextBox(IConsoleAbstraction consoleAbstraction, int top, int height)
        {
            _consoleAbstraction = consoleAbstraction;
            _textBuffer = new TextBuffer(height + 5);
            _height = height;
            _top = top;
        }

        public void DisplayNextMessage(string message)
        {
            DrawText(message);
        }

        public void Clear()
        {
            ClearBox();
            _textBuffer.Clear();
        }
        
        private void DrawText(string value)
        {
            _textBuffer.Add(value);
            
            var result = _textBuffer.Strings.TakeLast(_height);

            ClearBox();
            
            _consoleAbstraction.SetCursorPosition(0, _top);

            foreach (var s in result)
            {
                _consoleAbstraction.WriteLine(s);
            }
        }

        private void ClearBox()
        {
            for (int i = _top; i < _top + _height; i++)
            {
                _consoleAbstraction.ClearLine(0,i);
            }
        }
        
        private class TextBuffer
        {
            public IEnumerable<string> Strings => _allStrings;

            private Queue<string> _allStrings;
            private int _size;
            
            public TextBuffer(int size)
            {
                _size = size;
                _allStrings = new Queue<string>();
            }
            
            public void Add(string value)
            {
                var splitByNewLine = value.Split(Environment.NewLine);
                var valuableStrings = splitByNewLine.Where(s => !s.Equals(string.Empty)).ToArray();
                
                foreach (var line in valuableStrings)
                {
                    _allStrings.Enqueue(line);
                }

                while (_allStrings.Count > _size)
                {
                    _allStrings.Dequeue();
                }
            }

            public void Clear()
            {
                _allStrings.Clear();
            }
        }
    }
}