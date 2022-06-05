using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsServerLib.Utils.Logger
{
    public class FileLogWriter : ILogWriter
    {
        private string filename;

        private readonly object _lockfile = new object();

        public FileLogWriter(string filename)
        {
            this.filename = filename;
        }

        public void Write(string formattedMessage, LogLevel logLevel)
        {
            lock (_lockfile)
            {
                using (FileStream fileStream = File.OpenWrite(filename))
                {
                    using (StreamWriter streamWriter = new StreamWriter(fileStream))
                    {
                        streamWriter.WriteLine(formattedMessage);
                    }
                }
            }
        }
    }
}
