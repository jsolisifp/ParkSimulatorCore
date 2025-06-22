using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkSimulator
{
    public abstract class SimulationSubsystem
    {
        public abstract void Init(Config config);
        public abstract void Finish();
    }
}
