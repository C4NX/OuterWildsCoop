using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsServerLib.Utils.Logger
{
    public interface ILogWriter
    {
        void Write(string formattedMessage, LogLevel logLevel);
    }
}
