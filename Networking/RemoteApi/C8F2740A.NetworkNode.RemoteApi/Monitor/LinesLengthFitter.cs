using System;
using System.Collections.Generic;
using System.Linq;

namespace C8F2740A.NetworkNode.RemoteApi.Monitor
{
    public interface ILinesLengthFitter
    {
        IEnumerable<string> ProceedLines(IEnumerable<string> rawLines);
    }
    
    public class LinesLengthFitter : ILinesLengthFitter
    {
        private readonly int _lineMaxLength;
        
        public LinesLengthFitter(int lineMaxLength)
        {
            _lineMaxLength = lineMaxLength;
        }

        public IEnumerable<string> ProceedLines(IEnumerable<string> rawLines)
        {
            var preparedLines = new List<string>();

            foreach (var line in rawLines)
            {
                var stringArray = CharArrayToStringArray(line.ToArray());
                var segments = GetSegmentsFor(stringArray.Length, _lineMaxLength);
                
                foreach (var segment in segments)
                {
                    var stringSegment = new ArraySegment<string>(stringArray,segment.Item1, segment.Item2);
                    var res = string.Join("", stringSegment.ToArray());
                    preparedLines.Add(res);
                }
            }

            return preparedLines;
        }

        private static IEnumerable<(int, int)> GetSegmentsFor(int valueLengs, int segmentLengs)
        {
            var result = new List<(int, int)>();

            var arr = new List<Queue<int>>();
            var q = new Queue<int>();
            arr.Add(q);
            for (int i = 0; i < valueLengs; i++)
            {
                if (q.Count >= segmentLengs)
                {
                    q = new Queue<int>();
                    arr.Add(q);
                }
                
                q.Enqueue(i);
            }

            foreach (var queue in arr)
            {
                result.Add((queue.First(), queue.Count));
            }
            
            return result;
        }

        private string[] CharArrayToStringArray(IEnumerable<char> charArray)
        {
            var stringList = new List<string>();
            
            foreach (var c in charArray)
            {
                stringList.Add(c.ToString());
            }

            return stringList.ToArray();
        }
    }
}