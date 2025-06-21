using System.Diagnostics;

namespace ParkSimulator
{
    public enum SimulationState
    {
        uninitialized,
        stopped,
        playing
    };

    public class Simulation
    {
        public static SimulationState  State { get { return state; } }
        public static SimulatedScene? Scene { get { return scene; } }
        public static Storage? Storage { get { return storage; } }

        static SimulationState state = SimulationState.uninitialized;
        static SimulatedScene? scene;
        static Storage? storage;

        public static void Init(Storage _storage)
        {
            Debug.Assert(state == SimulationState.uninitialized, "Simulation is already initialized");

            storage = _storage;
            state = SimulationState.stopped;

            scene = new SimulatedScene();
        }

        public static void NewScene()
        {
            Debug.Assert(state == SimulationState.stopped, "Simulation is not stopped");
            if(scene != null) { scene.Unload(); }
            scene = new SimulatedScene();
            scene.Load();
        }

        public static void LoadScene(string resourceId)
        {
            Debug.Assert(state == SimulationState.stopped, "Simulation is not stopped");
            if(scene != null) { scene.Unload(); }
            scene = storage?.GetResource<SimulatedScene>(resourceId);
            if(scene != null) { scene.Load(); }
        }

        public static void SaveScene(string resourceId)
        {
            Debug.Assert(state == SimulationState.stopped, "Simulation is not stopped");
            Debug.Assert(scene != null, "You must create a new scene or load one from storage");
        }
        

        public static void Play()
        {
            Debug.Assert(scene != null, "You must create a new scene or load one from storage before playing");
            Debug.Assert(state == SimulationState.stopped, "Simulation is not stopped");
        }

        public static void Step()
        {
            Debug.Assert(state == SimulationState.playing, "Simulation is not playing");

        }

        public static void Stop()
        {
            Debug.Assert(state == SimulationState.playing, "Simulation is not playing");
            scene.Stop();

        }

        public static void Finish()
        {
            Debug.Assert(state == SimulationState.stopped, "Stop the simulation first");
            if(scene != null) { scene.Unload(); }
            storage.Finish();
            storage = null;

        }



    }
}
