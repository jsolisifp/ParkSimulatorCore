namespace ParkSimulator
{
    public abstract class SimulationSubsystem
    {
        public abstract void Init(Config config);
        public abstract void Finish();
    }
}
