﻿using System.Diagnostics;
using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class LineSegments : Line
{
    private Vector3 end = Vector3.Zero();
    private Vector3 start = Vector3.Zero();

    public LineSegments()
    {
    }

    public LineSegments(Geometry geometry, Material material) : base(geometry, material)
    {
    }

    public LineSegments(Geometry geometry, List<Material> materials) : base(geometry, materials)
    {
    }

    public LineSegments(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public override Line ComputeLineDistances()
    {
        var geometry = Geometry;

        if (geometry.Type.Equals("BufferGeometry"))
        {
            if ((geometry as BufferGeometry).Index == null)
            {
                var positionAttribute = (BufferAttribute<float>)(geometry as BufferGeometry).Attributes["position"];
                var lineDistances = new List<float>();

                for (var i = 0; i < positionAttribute.count; i += 2)
                {
                    start = start.FromBufferAttribute(positionAttribute, i);
                    end = end.FromBufferAttribute(positionAttribute, i + 1);

                    lineDistances.Add(i == 0 ? 0 : lineDistances[i - 1]);
                    lineDistances.Add(lineDistances[i] + start.DistanceTo(end));
                }

                (geometry as BufferGeometry).SetAttribute("lineDistance",
                    new BufferAttribute<float>(lineDistances.ToArray(), 1));
            }
            else
            {
                Trace.TraceWarning(
                    "THREE.Objects.LineSegments.ComputeLineDistance(): Computation only possible with non-indexed BufferGeometry");
            }
        }
        else if (geometry.Type.Equals("Geometry"))
        {
            var vertices = geometry.Vertices;
            var lineDistances = new List<float>();

            for (var i = 0; i < vertices.Count; i += 2)
            {
                start = vertices[i];
                end = vertices[i + 1];

                lineDistances.Add(i == 0 ? 0 : lineDistances[i - 1]);
                lineDistances.Add(lineDistances[i] + start.DistanceTo(end));
            }

            geometry.LineDistances = lineDistances;
        }

        return this;
    }
}