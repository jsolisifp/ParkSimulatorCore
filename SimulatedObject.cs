namespace ParkSimulatorCore
{
    internal class SimulatedObject
    {
        public string name;
        public bool active;

        List<Component> components;

        public SimulatedObject()
        {
            name = "";
            active = false;

            components = new List<Component>();

        }

        public void Start()
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].Start();
            }
        }

        public void Stop()
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].Stop();
            }
        }

        public void Step(float deltaTime)
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].Step(deltaTime);
            }
        }

        public void Pass(int passId)
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].Pass(passId);
            }
        }

        public void AddComponent(Component c)
        {
            if(c.GetType().Equals(typeof(Location)))
            {
                location = (Location)c;
            }

            components.Add(c);
            c.SetSimulatedObject(this);
        }

        public void RemoveComponent(Component c)
        {
            if (c.GetType().Equals(typeof(Location)))
            {
                location = null;
            }

            components.Remove(c);
            c.SetSimulatedObject(null);
        }

        public List<Component> GetComponents()
        {
            return components;
        }

        public T? GetComponent<T>() where T: Component
        {
            T? c = (T?)components.Find((Component c) => c.GetType() == typeof(T));

            return c;
        }
    }
}
