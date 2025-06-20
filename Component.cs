namespace ParkSimulatorCore
{
    public abstract class Component
    {
        public bool active = false;

        protected SimulatedObject? simulatedObject;

        public virtual void Start() { }
        public virtual void Step(float deltaTime) { }
        public virtual void Pass(int passId) { }
        public virtual void Stop() { }


        public void SetSimulatedObject(SimulatedObject? so)
        {
            simulatedObject = so;
        }

        public SimulatedObject? GetSimulatedObject()
        {
            return simulatedObject;
        }

    }
}
