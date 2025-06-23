using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;

namespace ParkSimulator
{
    public enum SimulatedSceneState
    {
        unlinked,
        linked,
        playing
    };

    public class SimulatedScene
    {
        public int Seed { get; set; }
        public int Steps { get{ return steps; } }

        List<SimulatedObject> objects;

        int steps;

        public SimulatedSceneState State { get; set; }

        public SimulatedScene()
        {
            objects = new List<SimulatedObject>();
            State = SimulatedSceneState.unlinked;
        }

        public void AddSimulatedObject(SimulatedObject so)
        {
            objects.Add(so);
            so.AttachToScene(this);


            if(State == SimulatedSceneState.linked) { LinkObject(so); }
            else if(State == SimulatedSceneState.playing) { LinkObject(so); so.Start(); }
        }

        public void RemoveSimulatedObject(SimulatedObject so)
        {
            if(State == SimulatedSceneState.linked) { UnlinkObject(so); }
            else if(State == SimulatedSceneState.playing) { so.Stop(); UnlinkObject(so); }

            so.DetachFromScene();
            objects.Remove(so);
        }

        public void Link()
        {
            Debug.Assert(State == SimulatedSceneState.unlinked, "Scene storage references already added");

            for (int i = 0; i < objects.Count; i++)
            {
                LinkObject(objects[i]);
            }

            State = SimulatedSceneState.linked;
        }

        public void Start()
        {
            Debug.Assert(State == SimulatedSceneState.linked, "Starting scene with storage references pending");

            steps = 0;

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

            State = SimulatedSceneState.linked;
        }

        public void Step()
        {
            Debug.Assert(State == SimulatedSceneState.playing, "Stopping not playing scene");

            for (int i = 0; i < objects.Count; i++)
            {
                objects[i].Step();
            }
        }

        public void Unlink()
        {
            Debug.Assert(State == SimulatedSceneState.linked, "Removing references not previously added or during play");

            for (int i = 0; i < objects.Count; i++)
            {
                UnlinkObject(objects[i]);
            }

            State = SimulatedSceneState.unlinked;
        }

        public ReadOnlyCollection<SimulatedObject> GetSimulatedObjects()
        {
            return objects.AsReadOnly<SimulatedObject>();
        }

        void LinkObject(SimulatedObject o)
        {
            var components = o.GetComponents();
            for (int j = 0; j < components.Count; j++)
            {
                LinkComponent(components[j]);
            }

        }

        public void LinkComponent(Component c)
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

        void UnlinkObject(SimulatedObject o)
        {
            var components = o.GetComponents();
            for (int j = 0; j < components.Count; j++)
            {
                UnlinkComponent(components[j]);
            }

        }

        public void UnlinkComponent(Component c)
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
