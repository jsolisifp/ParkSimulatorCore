using System.Collections.ObjectModel;

namespace ParkSimulator
{
    public class SimulatedScene
    {
        List<SimulatedObject> objects;

        public SimulatedScene()
        {
            objects = new List<SimulatedObject>();
        }

        public void AddSimulatedObject(SimulatedObject so)
        {
            objects.Add(so);
        }

        public void RemoveSimulatedObject(SimulatedObject so)
        {
            objects.Remove(so);
        }

        public void Load()
        {
            for (int i = 0; i < objects.Count; i++)
            {
                objects[i].Load();
            }
        }

        public void Start()
        {
            for (int i = 0; i < objects.Count; i++)
            {
                objects[i].Start();
            }
        }

        public void Stop()
        {
            for (int i = 0; i < objects.Count; i++)
            {
                objects[i].Stop();
            }
        }

        public void Step(float deltaTime)
        {
            for (int i = 0; i < objects.Count; i++)
            {
                objects[i].Step(deltaTime);
            }
        }

        public void Unload()
        {
            for (int i = 0; i < objects.Count; i++)
            {
                objects[i].Unload();
            }
        }

        public ReadOnlyCollection<SimulatedObject> GetSimulatedObjects()
        {
            return objects.AsReadOnly<SimulatedObject>();
        }

    }
}
