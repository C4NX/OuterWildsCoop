/*
MIT License
Copyright (c) 2016 Heiswayi Nrird
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsServerLib.Utils.Logger
{
    public class ServerLogger
    {
        private IList<ILogWriter> _writers;

        public static ServerLogger Logger { get; set; } = new ServerLogger(new FileLogWriter("latest.log"), new ConsoleLogWriter());

        public ServerLogger(params ILogWriter[] logWriters)
        {
            _writers = logWriters.ToList();
        }

        public ServerLogger AddWriter(ILogWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            _writers.Add(writer);
            return this;
        }

        public void Log(LogLevel logLevel, string message)
        {
            string formattedMessage = $"({DateTime.Now}) [{logLevel}] - {message}";

            for (int i = 0; i < _writers.Count; i++)
            {
                _writers[i].Write(formattedMessage, logLevel);
            }
        }

        public void Info(string message) => Log(LogLevel.INFO, message);
        public void Debug(string message) => Log(LogLevel.DEBUG, message);
        public void Trace(string message) => Log(LogLevel.TRACE, message);
        public void Error(string message) => Log(LogLevel.ERROR, message);
        public void Warn(string message) => Log(LogLevel.WARN, message);
        public void Fatal(string message) => Log(LogLevel.FATAL, message);
    }
}
