using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;

namespace ParkSimulator
{
    public enum SimulatedSceneState
    {
        unloaded,
        loaded,
        playing
    };

    public class SimulatedScene
    {
        List<SimulatedObject> objects;

        public SimulatedSceneState State { get; set; }

        public SimulatedScene()
        {
            objects = new List<SimulatedObject>();
            State = SimulatedSceneState.unloaded;
        }

        public void AddSimulatedObject(SimulatedObject so)
        {
            objects.Add(so);
            so.AttachToScene(this);


            if(State == SimulatedSceneState.loaded) { LoadSimulatedObject(so); }
            else if(State == SimulatedSceneState.playing) { LoadSimulatedObject(so); so.Start(); }
        }

        public void RemoveSimulatedObject(SimulatedObject so)
        {
            if(State == SimulatedSceneState.loaded) { UnloadSimulatedObject(so); }
            else if(State == SimulatedSceneState.playing) { so.Stop(); UnloadSimulatedObject(so); }

            so.DetachFromScene();
            objects.Remove(so);
        }

        public void Load()
        {
            Debug.Assert(State == SimulatedSceneState.unloaded, "Loading already loaded scene");

            for (int i = 0; i < objects.Count; i++)
            {
                LoadSimulatedObject(objects[i]);
            }

            State = SimulatedSceneState.loaded;
        }

        public void Start()
        {
            Debug.Assert(State == SimulatedSceneState.loaded, "Starting not loaded scene");

            for (int i = 0; i < objects.Count; i++)
            {
                objects[i].Start();
            }

            State = SimulatedSceneState.playing;
        }

        public void Stop()
        {
            Debug.Assert(State == SimulatedSceneState.playing, "Stopping not playing scene");

            for (int i = 0; i < objects.Count; i++)
            {
                objects[i].Stop();
            }

            State = SimulatedSceneState.loaded;
        }

        public void Step()
        {
            Debug.Assert(State == SimulatedSceneState.playing, "Stopping not playing scene");

            for (int i = 0; i < objects.Count; i++)
            {
                objects[i].Step();
            }
        }

        public void Unload()
        {
            Debug.Assert(State == SimulatedSceneState.loaded, "Unloading not loaded (or playing) scene");

            for (int i = 0; i < objects.Count; i++)
            {
                UnloadSimulatedObject(objects[i]);
            }

            State = SimulatedSceneState.unloaded;
        }

        public ReadOnlyCollection<SimulatedObject> GetSimulatedObjects()
        {
            return objects.AsReadOnly<SimulatedObject>();
        }

        public void LoadSimulatedObject(SimulatedObject o)
        {
            var components = o.GetComponents();
            for (int j = 0; j < components.Count; j++)
            {
                LoadComponent(components[j]);
            }

        }

        public void LoadComponent(Component c)
        {
            var fields = c.GetFieldsInfo();

            for(int k = 0; k < fields.Count; k++)
            {
                ComponentFieldInfo f = fields[k];
                if(f.type == typeof(ResourcePointer).Name)
                {
                    ResourcePointer p = c.GetFieldValue<ResourcePointer>(f.name);

                    if(p.resourceId != null && p.typeId != null && p.resource == null)
                    {
                        Debug.Assert(Simulation.Storage != null);
                        Simulation.Storage.AddReference(p.resourceId, p.typeId);
                        p.resource = Simulation.Storage.GetLoadedResource<object>(p.resourceId);
                    }

                    c.SetFieldValue<ResourcePointer>(f.name, p);
                }
            }
        }

        public void UnloadSimulatedObject(SimulatedObject o)
        {
            var components = o.GetComponents();
            for (int j = 0; j < components.Count; j++)
            {
                UnloadComponent(components[j]);
            }

        }

        public void UnloadComponent(Component c)
        {
            var fields = c.GetFieldsInfo();

            for(int k = 0; k < fields.Count; k++)
            {
                ComponentFieldInfo f = fields[k];
                if(f.type == typeof(ResourcePointer).Name)
                {
                    ResourcePointer p = c.GetFieldValue<ResourcePointer>(f.name);

                    if(p.resourceId != null && p.typeId != null && p.resource != null)
                    {
                        p.resource = null;

                        Debug.Assert(Simulation.Storage != null);
                        Simulation.Storage.RemoveReference(p.resourceId, p.typeId);
                    }

                    c.SetFieldValue<ResourcePointer>(f.name, p);
                }
            }
        }

    }
}
