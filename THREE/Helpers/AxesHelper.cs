﻿using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class AxesHelper : LineSegments
{
    public AxesHelper(int size)
    {
        float[] vertices =
        {
            0, 0, 0, size, 0, 0,
            0, 0, 0, 0, size, 0,
            0, 0, 0, 0, 0, size
        };

        float[] colors =
        {
            1, 0, 0, 1, 0.6f, 0,
            0, 1, 0, 0.6f, 1, 0,
            0, 0, 0, 0, 0.6f, 1
        };

        var geometry = new BufferGeometry();
        geometry.SetAttribute("position", new BufferAttribute<float>(vertices, 3));
        geometry.SetAttribute("color", new BufferAttribute<float>(colors, 3));

        var material = new LineBasicMaterial { ToneMapped = false };

        material.VertexColors = Constants.VertexColors > 0 ? true : false;

        Geometry = geometry;
        Material = material;
    }

    public AxesHelper(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}