using System.Diagnostics;
using System.Numerics;

namespace ParkSimulator
{
    public class PictureRenderer : Component
    {
        public Vector3 Color { get; set; }
        public float Size { get; set; } = 5;
        public float PointSize { get; set; } = 2;
        public Vector3 PointColor { get; set; } = new Vector3(0.0f, 0.0f, 0.0f);


        PictureRender? render;
        Location? location;

        public PictureRenderer()
        {
            render = null;
        }

        public override void Start()
        {
        }

        public override void Pass(int passId)
        {
            Debug.Assert(Simulation.Render != null, "La simulación no está iniciada");
            Debug.Assert(simulatedObject != null, "El componente no está añadido a un objeto");

            render ??= (PictureRender)Simulation.Render;
            location ??= simulatedObject.GetComponent<Location>();

            Debug.Assert(location != null, "Falta el componente location");

            float size = Size * location.Capacity;
            Vector2 p = new Vector2(location.Coordinates.X, location.Coordinates.Z) - Vector2.One * size / 2;
            render.DrawRect(p, Vector2.One * size, new PictureRender.Color24() { r = 127, g = 127, b = 127 });

            size = Size * location.Occupation;
            p = new Vector2(location.Coordinates.X, location.Coordinates.Z) - Vector2.One * size / 2;
            render.DrawRect(p, Vector2.One * size, new PictureRender.Color24(Color));

            Location? neighbourLocation = location?.Neighbour?.GetSimulatedObject()?.GetComponent<Location>();

            if(neighbourLocation != null)
            {
                Debug.Assert(location != null, "Falta el componente location");

                Vector2 p1 = new Vector2(location.Coordinates.X, location.Coordinates.Z);
                Vector2 p2 = new Vector2(neighbourLocation.Coordinates.X, neighbourLocation.Coordinates.Z);
                PictureRender.Color24 color = new (Color);
                render.DrawLine(p1, p2, color);
            }

            render.DrawDisc(new Vector2(location.Coordinates.X, location.Coordinates.Z), PointSize, new PictureRender.Color24(PointColor));
        }

    }
}
