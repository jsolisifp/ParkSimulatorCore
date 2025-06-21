using System.Collections.ObjectModel;
using System.Diagnostics;

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


            if(State == SimulatedSceneState.loaded) { so.Load(); }
            else if(State == SimulatedSceneState.playing) { so.Load(); so.Start(); }
        }

        public void RemoveSimulatedObject(SimulatedObject so)
        {
            if(State == SimulatedSceneState.loaded) { so.Unload(); }
            else if(State == SimulatedSceneState.playing) { so.Stop(); so.Unload(); }

            so.DetachFromScene();
            objects.Remove(so);
        }

        public void Load()
        {
            Debug.Assert(State == SimulatedSceneState.unloaded, "Loading already loaded scene");

            for (int i = 0; i < objects.Count; i++)
            {
                objects[i].Load();
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
                objects[i].Unload();
            }

            State = SimulatedSceneState.unloaded;
        }

        public ReadOnlyCollection<SimulatedObject> GetSimulatedObjects()
        {
            return objects.AsReadOnly<SimulatedObject>();
        }

    }
}
