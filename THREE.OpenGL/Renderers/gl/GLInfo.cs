﻿using System.Diagnostics;
using OpenTK.Graphics.ES30;

namespace THREE;

[Serializable]
public class GLInfo
{
    public bool AutoReset = true;
    public Memory memory = new() { Geometries = 0, Textures = 0 };

    public List<GLProgram> programs;

    public Render render = new() { Frame = 0, Calls = 0, Triangles = 0, Points = 0, Lines = 0 };

    public void Update(int count, int mode, int? instanceCount = null)
    {
        if (instanceCount == null) instanceCount = 1;

        render.Calls++;

        var type = (PrimitiveType)Enum.ToObject(typeof(PrimitiveType), mode);

        switch (type)
        {
            case PrimitiveType.Triangles:
                render.Triangles += (int)instanceCount * (count / 3);
                break;
            case PrimitiveType.Lines:
                render.Lines += (int)instanceCount * (count / 2);
                break;
            case PrimitiveType.LineStrip:
                render.Lines += (int)instanceCount * (count - 1);
                break;
            case PrimitiveType.LineLoop:
                render.Lines += (int)instanceCount * count;
                break;
            case PrimitiveType.Points:
                render.Points += (int)instanceCount * count;
                break;
            default:
                Trace.TraceError("THREE.gl.GLInfo:Unknown draw mode:", mode);
                break;
        }
    }

    public void Reset()
    {
        render.Frame++;
        render.Calls = 0;
        render.Triangles = 0;
        render.Points = 0;
        render.Lines = 0;
    }

    [Serializable]
    public struct Memory
    {
        public int Geometries;

        public int Textures;
    }

    [Serializable]
    public struct Render
    {
        public int Frame;

        public int Calls;

        public int Triangles;

        public int Points;

        public int Lines;
    }
}