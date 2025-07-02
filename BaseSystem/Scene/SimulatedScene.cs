using System.Collections.ObjectModel;
using System.Diagnostics;

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
        public float Time { get { return time; } }

        List<SimulatedObject> objects;

        float time;

        public SimulatedSceneState State { get; set; }

        internal SimulatedScene()
        {
            objects = new List<SimulatedObject>();
            State = SimulatedSceneState.unlinked;
        }

        public void AddSimulatedObject(SimulatedObject so)
        {
            lock(Simulation.LockObject)
            {
                objects.Add(so);
                so.AttachToScene(this);
                so.componentRemovedEvent += OnComponentRemoved;


                if(State == SimulatedSceneState.linked) { LinkObjectResources(so); }
                else if(State == SimulatedSceneState.playing) { LinkObjectResources(so); so.Start(); }
            }
        }

        public void RemoveSimulatedObject(SimulatedObject so)
        {
            lock(Simulation.LockObject)
            {
                if(State == SimulatedSceneState.linked) { UnlinkObjectResources(so); }
                else if(State == SimulatedSceneState.playing) { so.Stop(); UnlinkObjectResources(so); }

                var components = so.LockComponents();

                for(int i = 0; i < components.Count; i++)
                {
                    OnComponentRemoved(components[i]);
                }

                so.componentRemovedEvent -= OnComponentRemoved;
                so.DetachFromScene();
                objects.Remove(so);

                so.UnlockComponents();
            }
        }

        internal void LinkResources()
        {
            Debug.Assert(State == SimulatedSceneState.unlinked, "Scene storage references already added");
            Debug.Assert(Simulation.Storage != null, "Storage is not present");

            for (int i = 0; i < objects.Count; i++)
            {
                LinkObjectResources(objects[i]);
            }

            Simulation.Storage.ResourceDeletedEvent += OnResourceDeleted;

            State = SimulatedSceneState.linked;
        }

        internal void Start()
        {
            Debug.Assert(State == SimulatedSceneState.linked, "Starting scene with storage references pending");

            time = 0;

            for (int i = 0; i < objects.Count; i++)
            {
                objects[i].Start();
            }

            State = SimulatedSceneState.playing;
        }

        internal void Stop()
        {
            Debug.Assert(State == SimulatedSceneState.playing, "Stopping not playing scene");

            for (int i = 0; i < objects.Count; i++)
            {
                objects[i].Stop();
            }

            State = SimulatedSceneState.linked;
        }

        internal void Step(float deltaTime)
        {
            Debug.Assert(State == SimulatedSceneState.playing, "Stepping not playing scene");

            for (int i = 0; i < objects.Count; i++)
            {
                objects[i].Step(deltaTime);
            }

            time += deltaTime;
        }

        internal void UnlinkResources()
        {
            Debug.Assert(Simulation.Storage != null, "Storage not present");

            Debug.Assert(State == SimulatedSceneState.linked, "Removing references not previously added or during play");

            Simulation.Storage.ResourceDeletedEvent -= OnResourceDeleted;

            for (int i = 0; i < objects.Count; i++)
            {
                UnlinkObjectResources(objects[i]);
            }

            State = SimulatedSceneState.unlinked;
        }

        public ReadOnlyCollection<SimulatedObject>? TryLockSimulatedObjects()
        {
            if(Monitor.TryEnter(Simulation.LockObject))
            {
                return objects.AsReadOnly<SimulatedObject>();
            }
            else
            {
                return null;
            }
        }

        public ReadOnlyCollection<SimulatedObject> LockSimulatedObjects()
        {
            Monitor.Enter(Simulation.LockObject);

            return objects.AsReadOnly<SimulatedObject>();
        }

        public void UnlockSimulatedObjects()
        {
            Monitor.Exit(Simulation.LockObject);
        }

        internal void LinkObjectResources(SimulatedObject o)
        {
            var components = o.LockComponents();

            for (int j = 0; j < components.Count; j++)
            {
                LinkComponentResources(components[j]);
            }

            o.UnlockComponents();

        }

        internal void LinkComponentResources(Component c)
        {
            Debug.Assert(Simulation.Storage != null, "No storage present");

            var fields = c.GetFieldsInfo();

            for(int k = 0; k < fields.Count; k++)
            {
                ComponentFieldInfo f = fields[k];
                if(f.type == typeof(ResourcePointer).Name)
                {
                    ResourcePointer p = c.GetFieldValue<ResourcePointer>(f.name);

                    if(p.ResourceId != null && p.TypeId != null)
                    {
                        if(!Simulation.Storage.ExistsResource(p.ResourceId, p.TypeId)) { p = new ResourcePointer(); }
                    }
                        
                    p.LinkResource();

                    c.SetFieldValue<ResourcePointer>(f.name, p, false);
                }
                else if(f.isArray && f.arrayElementIsResourcePointer)
                {
                    int length = c.GetFieldArrayLength(f.name);

                    for(int m = 0; m < length; m++)
                    {
                        ResourcePointer p = c.GetFieldArrayValue<ResourcePointer>(f.name, m);

                        if (p.ResourceId != null && p.TypeId != null)
                        {
                            if (!Simulation.Storage.ExistsResource(p.ResourceId, p.TypeId)) { p = new ResourcePointer(); }
                        }

                        p.LinkResource();

                        c.SetFieldArrayValue<ResourcePointer>(f.name, m, p, false);

                    }
                }
            }
        }

        void UnlinkObjectResources(SimulatedObject o)
        {
            var components = o.LockComponents();

            for (int j = 0; j < components.Count; j++)
            {
                UnlinkComponentResources(components[j]);
            }

            o.UnlockComponents();

        }

        internal void UnlinkComponentResources(Component c)
        {
            var fields = c.GetFieldsInfo();

            for(int k = 0; k < fields.Count; k++)
            {
                ComponentFieldInfo f = fields[k];

                if(f.type == typeof(ResourcePointer).Name)
                {
                    ResourcePointer p = c.GetFieldValue<ResourcePointer>(f.name);

                    p.UnlinkResource();

                    c.SetFieldValue<ResourcePointer>(f.name, p, false);
                }
                else if(f.isArray && f.arrayElementIsResourcePointer)
                {
                    int length = c.GetFieldArrayLength(f.name);

                    for (int m = 0; m < length; m++)
                    {
                        ResourcePointer p = c.GetFieldArrayValue<ResourcePointer>(f.name, m);

                        p.UnlinkResource();

                        c.SetFieldArrayValue<ResourcePointer>(f.name, m, p, false);
                    }
                }

            }
        }

        void OnComponentRemoved(Component c)
        {
            // Remove all references to the component

            for(int i = 0; i < objects.Count; i++)
            {
                var components = objects[i].LockComponents();

                for (int j = 0; j < components.Count; j++)
                {
                    var fields = components[j].GetFieldsInfo();

                    for (int k = 0; k < fields.Count; k++)
                    {
                        if (fields[k].isComponent)
                        {
                            Component? value = components[j].GetFieldValue<Component>(fields[k].name);

                            if (c == value)
                            {
                                components[j].SetFieldValue<Component>(fields[k].name, null);
                            }
                        }
                        else if (fields[k].isArray && fields[k].arrayElementIsComponent)
                        {
                            int length = components[j].GetFieldArrayLength(fields[k].name);

                            for (int m = 0; m < length; m++)
                            {
                                Component? value = components[j].GetFieldArrayValue<Component>(fields[k].name, m);

                                if (c == value)
                                {
                                    components[j].SetFieldArrayValue<Component>(fields[k].name, m, null);
                                }
                            }
                        }

                    }
                }

                objects[i].UnlockComponents();
            }


        }

        void OnResourceDeleted(string resourceId, string typeId)
        {
            Debug.Assert(State != SimulatedSceneState.playing, "Cannot delete resources while scene is playing");

            // Remove all references to the resource

            for(int i = 0; i < objects.Count; i++)
            {
                var components = objects[i].LockComponents();

                for(int j = 0; j < components.Count; j++)
                {
                    var fields = components[j].GetFieldsInfo();

                    for(int k = 0; k < fields.Count; k++)
                    {
                        if(fields[k].isResourcePointer)
                        {
                            ResourcePointer? p = components[j].GetFieldValue<ResourcePointer>(fields[k].name);

                            if(p.HasValue)
                            {
                                if(p.Value.ResourceId == resourceId && p.Value.TypeId == typeId)
                                {
                                    if(State == SimulatedSceneState.linked)
                                    {
                                        p.Value.UnlinkResource();
                                        // Simulation.Storage.RemoveReference(resourceId, typeId);
                                    }

                                    ResourcePointer nullPointer = new ResourcePointer();
                                    components[j].SetFieldValue<ResourcePointer>(fields[k].name, nullPointer, false);

                                }
                            }
                        }
                        else if(fields[k].isArray && fields[k].arrayElementIsResourcePointer)
                        {
                            int length = components[j].GetFieldArrayLength(fields[k].name);

                            for(int m = 0; m < length; m++)
                            {
                                ResourcePointer? p = components[j].GetFieldArrayValue<ResourcePointer>(fields[k].name, m);

                                if (p.HasValue)
                                {
                                    if (p.Value.ResourceId == resourceId && p.Value.TypeId == typeId)
                                    {
                                        if (State == SimulatedSceneState.linked)
                                        {
                                            p.Value.UnlinkResource();
                                            // Simulation.Storage.RemoveReference(resourceId, typeId);
                                        }

                                        ResourcePointer nullPointer = new ResourcePointer();
                                        components[j].SetFieldArrayValue<ResourcePointer>(fields[k].name, m, nullPointer, false);

                                    }
                                }

                            }


                        }
                    }

                }

                objects[i].UnlockComponents();
            }
        }

    }
}
