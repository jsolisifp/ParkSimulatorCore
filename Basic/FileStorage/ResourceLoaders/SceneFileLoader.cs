using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkSimulator
{
    public class SceneFileLoader : ResourceLoader
    {
        public override object? Load(string id)
        {
            return default;
        }

        public override void Save(ref string id, object resource)
        {
            Debug.Assert(typeof(SimulatedScene) == resource.GetType());
        }

        public override void Unload(string id)
        {
            // Nothing to do
        }
    }
}
