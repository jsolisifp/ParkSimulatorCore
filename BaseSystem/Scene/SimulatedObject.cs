using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;

namespace ParkSimulator
{
    public struct ComponentInfo
    {
        public string name;
        public string? fullName;
    };

    public class SimulatedObject
    {
        public delegate void ComponentDeletedEventHandler(Component component);

        public event ComponentDeletedEventHandler? componentRemovedEvent;

        public string Name { get; set; }
        public bool Active { get; set; }

        List<Component> components;
        SimulatedScene? scene;

        public SimulatedObject()
        {
            Name = "New simulated object";
            Active = true;

            components = new List<Component>();

            scene = null;

        }

        public void AttachToScene(SimulatedScene _scene)
        {
            scene = _scene;
        }

        public void DetachFromScene()
        {
            scene = null;
        }

        public SimulatedScene? GetAttachedScene()
        {
            return scene;
        }

        public void Start()
        {
            Debug.Assert(scene != null, "Simulated Object not attached to scene when started");

            for (int i = 0; i < components.Count; i++)
            {
                components[i].Start();
            }
        }

        public void Stop()
        {
            Debug.Assert(scene != null, "Simulated Object not attached to scene when stopped");

            for (int i = 0; i < components.Count; i++)
            {
                components[i].Stop();
            }
        }

        public void Step()
        {
            Debug.Assert(scene != null, "Simulated Object not attached to scene when stepped");

            for (int i = 0; i < components.Count; i++)
            {
                components[i].Step();
            }
        }

        public void Pass(int passId)
        {
            Debug.Assert(scene != null, "Simulated Object not attached to scene when pass");

            for (int i = 0; i < components.Count; i++)
            {
                components[i].Pass(passId);
            }
        }

        public void AddComponent(Component c)
        {
            components.Add(c);
            c.AttachToSimulatedObject(this);

            if(scene != null)
            {
                if(scene.State == SimulatedSceneState.linked) { scene.LinkComponentResources(c); }
                else if(scene.State == SimulatedSceneState.playing) { scene.LinkComponentResources(c); c.Start(); }
            }

        }

        public void RemoveComponent(Component c)
        {
            if(scene != null)
            {
                if(scene.State == SimulatedSceneState.linked) { scene.UnlinkComponentResources(c); }
                else if(scene.State == SimulatedSceneState.playing) { c.Stop(); scene.UnlinkComponentResources(c); }
            }

            componentRemovedEvent?.Invoke(c);

            components.Remove(c);
            c.DetachFromSimulatedObject();
        }

        public ReadOnlyCollection<Component> GetComponents()
        {
            return components.AsReadOnly<Component>();
        }

        public T? GetComponent<T>() where T: Component
        {
            T? c = (T?)components.Find((c) => c.GetType() == typeof(T));

            return c;
        }

        public static ReadOnlyCollection<ComponentInfo> GetComponentsInfo()
        {
            List<ComponentInfo> components = new();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for(int k = 0; k < assemblies.Length; k ++)
            {
                Assembly? a = assemblies[k];

                Debug.Assert(a != null, "No se encuentra el ensamblado");

                Type[] types = a.GetTypes();

                for (int i = 0; i < types.Length; i++)
                {
                    Type t = types[i];

                    if (t.IsSubclassOf(typeof(Component)))
                    {
                        ComponentInfo c = new();
                        c.name = t.Name;
                        c.fullName = t.FullName;
                        components.Add(c);

                    }
                }

            }


            return components.AsReadOnly<ComponentInfo>();
        }

        public static Component CreateComponentByName(string name)
        {
            Component? r = null;

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for(int k = 0; k < assemblies.Length; k ++)
            {
                Assembly? a = assemblies[k];

                Debug.Assert(a != null, "No se encuentra el ensamblado");

                Type[] types = a.GetTypes();

                for (int i = 0; i < types.Length; i++)
                {
                    Type t = types[i];

                    if (t.IsSubclassOf(typeof(Component)))
                    {
                        if(t.Name == name)
                        {
                            object? o = Activator.CreateInstance(t);

                            Debug.Assert(o != null, "No se puede crear un objeto de tipo " + name);

                            r = (Component)o;
                        }
                    }
                }

            }

            Debug.Assert(r != null, "No existe ningún tipo de componente llamado " + name);

            return r;

        }
    }
}
