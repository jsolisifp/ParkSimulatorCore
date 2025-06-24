using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkSimulator
{
    public abstract class Log : SimulationSubsystem
    {
        public abstract void LogMessage(string category, string message);
        public abstract void LogWarning(string category, string message);
        public abstract void LogError(string category, string message);
    }
}
