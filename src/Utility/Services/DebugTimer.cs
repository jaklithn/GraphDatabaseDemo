using System.Diagnostics;
using System.Text;


namespace Utility.Services
{
    public class DebugTimer
    {
        #region Private Declarations

        private const int RowLength = 70;
        private bool _isFirstWrite = true;
        private readonly bool _accumulate;
        private readonly StringBuilder _sb;
        private readonly Stopwatch _lapwatch = new Stopwatch();
        private int _totalTime;

        #endregion


        #region Public Properties

        /// <summary>
        /// Time in milliseconds since start or since last Write.
        /// Most times it is better to use method Write. It will return exactly the same time but also: a) reset lap timer and b) in debug mode print a meaningful comment.
        /// </summary>
        public int LapTime => (int)_lapwatch.Elapsed.TotalMilliseconds;

        /// <summary>
        /// Total time in milliseconds since start or since last WriteTotal.
        /// Most times it is better to use method WriteTotal. It will return exactly the same time but also: a) reset total timer and b) in debug mode print a meaningful comment.
        /// </summary>
        public int TotalTime => _totalTime + LapTime;

        #endregion


        #region Constructor

        /// <summary>
        /// Timer to be used during debug.
        /// </summary>
        /// <param name="accumulate">Accumulate all writes until final WriteTotal call where all content is flushed together. This is useful to prevent scattered writes to be mixed with execution printouts.</param>
        /// <param name="header">Optional header to be written above time list</param>
        /// <param name="args">Possible string arguments to header format</param>
        public DebugTimer(bool accumulate = false, string header = "", params object[] args)
        {
            _accumulate = accumulate;
            if (_accumulate)
                _sb = new StringBuilder();
            WriteHeader(string.Format(header, args));
            _lapwatch.Start();
        }

        #endregion


        #region Public Write Methods

        /// <summary>
        /// Write underlined header without time.
        /// </summary>
        public void WriteHeader(string header, params object[] args)
        {
            if (IsDebug() && !string.IsNullOrEmpty(header))
            {
                if (!_isFirstWrite)
                    WriteLine(new string('─', RowLength));
                WriteLine(string.Format(header, args));
                WriteLine(new string('─', RowLength));
            }
        }

        /// <summary>
        /// Write message to debug window with time since last Write.
        /// Lap timer is reset and restarted to measure next "lap".
        /// </summary>
        /// <returns>Elapsed time in ms</returns>
        public int Write(string format, params object[] args)
        {
            var lapTime = LapTime;
            _lapwatch.Stop();
            _totalTime += lapTime;
            if (IsDebug())
            {
                var message = string.Format(format, args);
                WriteLine(FormatMessage(message, GetDuration(lapTime)));
            }
            _lapwatch.Restart();
            return lapTime;
        }

        /// <summary>
        /// Write summary to debug window with total time since start.
        /// Note: This will reset total timer which is useful when called repeatedly for several test sets.
        /// </summary>
        /// <returns>Total time in ms</returns>
        public int WriteTotal()
        {
            var lapTime = LapTime;
            _lapwatch.Stop();
            _totalTime += lapTime;
            var totalTime = _totalTime;
            if (IsDebug())
            {
                WriteLine(new string('─', RowLength));
                WriteLine(FormatMessage("Total time", GetDuration(totalTime)));
                WriteLine(new string('═', RowLength));
                if (_accumulate)
                    Debug.WriteLine(_sb.ToString());
            }
            _totalTime = 0;
            _lapwatch.Restart();
            return totalTime;
        }

        #endregion


        #region Support Methods

        private void WriteLine(string message)
        {
            if (_isFirstWrite)
            {
                _isFirstWrite = false;
                WriteLine(new string('═', RowLength));
            }
            if (_accumulate)
                _sb.AppendLine(message);
            else
                Debug.WriteLine(message);
        }

        /// <summary>
        /// Custom format for elapsed time.
        /// </summary>
        private static string GetDuration(int time)
        {
            return $"{time:N0} ms";
        }

        /// <summary>
        /// Format message to same length to set duration with right margin.
        /// </summary>
        private static string FormatMessage(string message, string duration)
        {
            int totalLength = message.Length + duration.Length;
            int padCount = (totalLength < RowLength) ? RowLength - totalLength : 2;
            return $"{message}{new string(' ', padCount)}{duration}";
        }

        public static bool IsDebug()
        {
#if DEBUG
            return true;
#endif
#pragma warning disable 162 // Compiler issues a warning because code can not be reached
            return false;
#pragma warning restore 162
        }

        #endregion
    }
}