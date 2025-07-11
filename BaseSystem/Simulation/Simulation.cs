﻿using System;
using System.Diagnostics;
using System.Reflection;

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
        public static Render? Render { get { return render; } }
        public static Log? Log { get { return log; } }
        public static Random? Random { get { return random; } } 

        static SimulationState state = SimulationState.uninitialized;
        static SimulatedScene? scene;
        static string? sceneResourceId;
        static Storage? storage;
        static Render? render;
        static Log? log;
        static Config? config;
        static Random? random;

        static internal Object LockObject = new Object();

        static string? playingTemporarySceneResourceId;
        static SimulatedScene? playingPreviousScene;


        public static void Init(Config _config, Storage? _storage = null, Render? _rendering = null, Log? _log = null)
        {
            Debug.Assert(state == SimulationState.uninitialized, "Simulation is already initialized");

            storage = _storage;
            config = _config;
            render = _rendering;
            log = _log;
            state = SimulationState.stopped;

            storage?.Init(config);
            render?.Init(config);
            log?.Init(config);

            scene = new SimulatedScene();
            sceneResourceId = null;
            scene.LinkResources();

            playingTemporarySceneResourceId = null;

        }

        public static void GetVersion(out int major, out int minor, out int build, out int revision)
        {
            Version ?v = Assembly.GetAssembly(typeof(Simulation))?.GetName().Version;

            Debug.Assert(v != null, "Cannot retrieve assembly version info");
            major = v.Major;
            minor = v.Minor;
            build = v.Build;
            revision = v.Revision;

        }

        public static void NewScene()
        {
            lock(LockObject)
            {
                Debug.Assert(state == SimulationState.stopped, "Simulation is not stopped");
                Debug.Assert(storage != null, "You must create a storage");

                if(scene != null) { scene.UnlinkResources(); }
                if(sceneResourceId != null) { storage.RemoveReference(sceneResourceId, Storage.typeIdScene); sceneResourceId = null; }

                scene = new SimulatedScene();
                sceneResourceId = null;

                scene.LinkResources();

            }
        }

        public static void LoadScene(string resourceId)
        {
            lock(Simulation.LockObject)
            {
                Debug.Assert(state == SimulationState.stopped, "Simulation is not stopped");
                Debug.Assert(scene != null, "You must create a new scene or load one from storage");
                Debug.Assert(storage != null, "You must create a storage");

                if(scene != null) { scene.UnlinkResources(); }
                if(sceneResourceId != null) { storage.RemoveReference(sceneResourceId, Storage.typeIdScene); sceneResourceId = null; }

                sceneResourceId = resourceId;
                storage.AddReference(resourceId, Storage.typeIdScene);
                scene = storage.GetLoadedResource<SimulatedScene>(resourceId);

                Debug.Assert(scene != null, "Cannot load scene resource");

                scene.LinkResources();
            }

        }

        public static void SaveScene(ref string resourceId)
        {
            lock(LockObject)
            {
                Debug.Assert(state == SimulationState.stopped, "Simulation is not stopped");
                Debug.Assert(scene != null, "You must create a new scene or load one from storage");
                Debug.Assert(storage != null, "You must create a storage");

                scene.UnlinkResources();

                if(sceneResourceId != null) { storage.RemoveReference(sceneResourceId, Storage.typeIdScene); sceneResourceId = null; }

                storage.SaveResource(ref resourceId, Storage.typeIdScene, scene);

                storage.AddReference(resourceId, Storage.typeIdScene);
                scene = storage.GetLoadedResource<SimulatedScene>(resourceId);
                sceneResourceId = resourceId;

                Debug.Assert(scene != null, "Cannot load scene resource");

                scene.LinkResources();

            }
        }

        public static void Play()
        {
            lock(LockObject)
            {
                Debug.Assert(scene != null, "You must create a new scene or load one from storage before playing");
                Debug.Assert(state == SimulationState.stopped, "Simulation is not stopped");
                Debug.Assert(scene != null, "You must create a new scene or load one from storage");
                Debug.Assert(storage != null, "Storage not present");
                //Debug.Assert(sceneResourceId != null, "Scene must be saved before ");

                scene.UnlinkResources();

                playingPreviousScene = scene;
                playingTemporarySceneResourceId = Guid.NewGuid().ToString();
                storage.SaveResource(ref playingTemporarySceneResourceId, Storage.typeIdScene, scene);

                storage.AddReference(playingTemporarySceneResourceId, Storage.typeIdScene);
                scene = storage.GetLoadedResource<SimulatedScene>(playingTemporarySceneResourceId);

                Debug.Assert(scene != null, "Temporary scene not loaded");

                scene.LinkResources();

                random = new Random(scene.Seed);

                scene.Start();

                state = SimulationState.playing;

            }

        }

        public static void Step(float deltaTime)
        {
            lock(LockObject)
            {
                Debug.Assert(state == SimulationState.playing, "Simulation is not playing");
                Debug.Assert(scene != null, "You must create a new scene or load one from storage");

                scene.Step(deltaTime);

            }

        }

        public static void Stop()
        {
            lock(LockObject)
            {
                Debug.Assert(state == SimulationState.playing, "Simulation is not playing");
                Debug.Assert(scene != null, "You must create a new scene or load one from storage");
                Debug.Assert(playingTemporarySceneResourceId != null, "Temporary playing scene not saved");
                Debug.Assert(storage != null, "Storage not present");

                scene.Stop();

                scene.UnlinkResources();

                storage.RemoveReference(playingTemporarySceneResourceId, Storage.typeIdScene);

                scene = playingPreviousScene;

                Debug.Assert(scene != null);

                scene.LinkResources();

                state = SimulationState.stopped;

            }

        }

        public static void Finish()
        {
            lock(LockObject)
            {
                Debug.Assert(state == SimulationState.stopped, "Stop the simulation first");
                Debug.Assert(scene != null, "You must create a new scene or load one from storage");
                Debug.Assert(storage != null, "You must create a storage");

                scene.UnlinkResources();
                scene = null;

                storage?.Finish();
                storage = null;

                render?.Finish();
                render = null;

                log?.Finish();
                log = null;

                state = SimulationState.uninitialized;

            }

        }

        public static void RenderFrame()
        {
            lock(LockObject)
            {
                Debug.Assert(scene != null, "You must create a new scene or load one from storage");

                render?.RenderFrame();

            }

        }

    }
}
