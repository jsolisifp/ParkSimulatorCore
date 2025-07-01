using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ParkSimulator
{
    public class OpenglParkServiceRenderer : Component
    {
        public ResourcePointer PersonModel { get; set; }
        public ResourcePointer PersonShader { get; set; }
        public ResourcePointer TextureWaiting { get; set; }
        public ResourcePointer TextureService { get; set; }

        public float VisitorSeparation { get; set; } = 1;
        public int MaxColumnSize { get; set; } = 5;


        OpenglRender? render;
        Location? location;
        ParkService? service;

        int waiting;
        int servicing;

        public OpenglParkServiceRenderer()
        {
            render = null;
            location = null;
            service = null;

            waiting = 0;
            servicing = 0;
        }

        public override void Step(float deltaTime)
        {
            service ??= simulatedObject.GetComponent<ParkService>();

            waiting = service.GetVisitorsWaitingOccupation();
            servicing = service.GetServiceVisitorOccupation();             
        }

        public override void Pass(int id, object? parameters)
        {
            Debug.Assert(Simulation.Render != null, "La simulación no está iniciada");
            Debug.Assert(simulatedObject != null, "El componente no está añadido a un objeto");

            render ??= (OpenglRender)Simulation.Render;
            location ??= simulatedObject.GetComponent<Location>();

            Debug.Assert(location != null, "Falta el componente location");  

            Matrix4x4 model = RenderMathUtils.GetModelMatrix(location.Position, location.Rotation);
            Vector3 visitorOffsetColumn = RenderMathUtils.TransformDirection(Vector3.UnitY * VisitorSeparation, model);
            Vector3 visitorOffsetRow = RenderMathUtils.TransformDirection(Vector3.UnitX * VisitorSeparation, model);

            Vector3 visitorPosition = location.Position;

            Model? personModel = (Model?)PersonModel.resource;
            Shader? shader = (Shader?)PersonShader.resource;
            Texture? textureWaiting = (Texture?)TextureWaiting.resource; 
            Texture? textureService = (Texture?)TextureService.resource; 

            if(personModel != null && shader != null && textureWaiting != null && textureService != null)
            {
                for(int i = 0; i < waiting + servicing; i++)
                {
                    Vector3 position = visitorPosition + (i % MaxColumnSize) * visitorOffsetColumn + (i / MaxColumnSize) * visitorOffsetRow;

                    if(i < servicing)
                    {
                        render.DrawModel(position, location.Rotation, Vector3.One, personModel, shader, textureService);
                    }
                    else
                    {
                        render.DrawModel(position, location.Rotation, Vector3.One, personModel, shader, textureWaiting);
                    }
                }

            }


            

            

        }

    }
}
