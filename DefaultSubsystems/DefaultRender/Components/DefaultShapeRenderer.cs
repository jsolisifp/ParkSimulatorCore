﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ParkSimulator
{
    public enum FileShapeType
    {
        rect,
        disc
    }

    class DefaultShapeRenderer : Component
    {
        public FileShapeType Type { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; } = new Vector2(10, 10);
        public Vector3 Color { get; set; } = new Vector3(1, 1, 1);

        DefaultRender? render;
        override public void Pass(int id, object? parameters)
        {
            render ??= (DefaultRender?)Simulation.Render;

            Debug.Assert(render != null, "No compatible render subsystem found");

            if(Type == FileShapeType.rect)
            {
                render.DrawRect(Position - Size / 2, Size, new DefaultRender.Color24(Color));
            }
            else
            {
                render.DrawEllipse(Position - Size / 2, Size, new DefaultRender.Color24(Color));
            }
        }
    }
}
