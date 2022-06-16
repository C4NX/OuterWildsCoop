using OuterWildsServerLib.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsServerLib.Utils
{
    public class ServerTimeManager : IOwTime
    {
        private DateTime _loopStart = DateTime.Now;
        private int LoopSecTime { get; set; } = 1320;

        public float GetTime()
        {
            if((DateTime.Now - _loopStart).TotalSeconds > LoopSecTime)
            {
                _loopStart = DateTime.Now;
            }
            return (float)(DateTime.Now - _loopStart).TotalSeconds;
        }
    }
}
