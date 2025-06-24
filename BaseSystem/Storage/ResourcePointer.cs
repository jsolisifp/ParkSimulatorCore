using System.Diagnostics;

namespace ParkSimulator
{
    public struct ResourcePointer
    {
        public string? ResourceId { get { return resourceId; } }
        public string? TypeId { get { return typeId; } }

        string? resourceId;
        string? typeId;

        public object? resource;

        Component? component;

        public ResourcePointer()
        {
            resourceId = null;
            typeId = null;
        }

        public ResourcePointer(string _resourceId, string _typeId)
        {
            resourceId = _resourceId;
            typeId = _typeId;
        }

        public T? GetResource<T>()
        {
            return (T?) resource;
        }

        public void AttachToComponent(Component c)
        {
            component = c;
        }

        public void DetachFromComponent()
        {
            component = null;
        }

        public Component? GetComponent()
        {
            return component;
        }

        public void SetAndLink(string _resourceId, string _typeId)
        {
            Debug.Assert(Simulation.Storage != null , "Storage is no present");

            SimulatedScene? scene = component?.GetSimulatedObject()?.GetAttachedScene();
            Debug.Assert(scene != null , "Cannot change link of a resource pointer that is not attached to a scene");
            Debug.Assert(scene.State != SimulatedSceneState.unlinked, "Cannot change link of a resource pointer that is attached to unlinked scene");

            if(_resourceId != resourceId && typeId != _typeId)
            {
                if(resourceId != null && typeId != null)
                {
                    Simulation.Storage.RemoveReference(resourceId, typeId);
                    resource = null;
                }

                if(_resourceId != null && _typeId != null)
                {
                    Simulation.Storage.AddReference(_resourceId, _typeId);
                    resource = Simulation.Storage.GetLoadedResource<object>(_resourceId);
                }
                else
                {
                    resource = null;
                }

                resourceId = _resourceId;
                typeId =_typeId;

            }

        }

        internal void LinkResource()
        {
            Debug.Assert(Simulation.Storage != null , "Storage is no present");

            if(resourceId != null && typeId != null)
            {
                Simulation.Storage.AddReference(resourceId, typeId);
                resource = Simulation.Storage.GetLoadedResource<object>(resourceId);
            }
        }

        internal void UnlinkResource()
        {
            Debug.Assert(Simulation.Storage != null , "Storage is no present");

            if(resourceId != null && typeId != null)
            {
                resource = null;
                Simulation.Storage.RemoveReference(resourceId, typeId);
            }
        }

    }
}
