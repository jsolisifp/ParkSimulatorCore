using System.Numerics;

namespace ParkSimulatorCore.Scene.Components
{
    public class Location : Component
    {
        public Vector3 coordinates;

        public Location()
        {
            coordinates = new Vector3(0, 0, 0);
        }

        public override void Step(float deltaTime)
        {
        }

    }
}
