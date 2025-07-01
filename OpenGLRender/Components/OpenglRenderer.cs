using System.Numerics;

namespace ParkSimulator
{
    public class OpenglRenderer : Component
    {
        public ResourcePointer Model { get; set; }
        public ResourcePointer Shader { get; set; }
        public ResourcePointer Texture { get; set; }

        Location? location;
        OpenglRender? render;

        public override void Pass(int id, object? parameters)
        {
            Model? m = (Model)Model.resource;
            Texture? t = (Texture)Texture.resource;
            Shader? s = (Shader)Shader.resource;

            location ??= GetSimulatedObject()?.GetComponent<Location>();
            render ??= (OpenglRender?)Simulation.Render;

            if(m != null && t != null && s != null && render != null)
            {
                render.DrawModel(location.Position, location.Rotation, Vector3.One, m, s, t);
            }
        }
    }
}
