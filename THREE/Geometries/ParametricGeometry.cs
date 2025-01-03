﻿using System.Collections;

namespace THREE;

[Serializable]
public delegate Vector3 ParameterFunc(float u, float v, Vector3 optionalTarget);

[Serializable]
public class ParametricGeometry : Geometry
{
    public Hashtable parameters;

    public ParametricGeometry(ParameterFunc func, int slices, float stacks)
    {
        parameters = new Hashtable
        {
            { "func", func },
            { "slices", slices },
            { "stacks", stacks }
        };

        FromBufferGeometry(new ParametricBufferGeometry(func, slices, stacks));
        MergeVertices();
    }
}

[Serializable]
public class ParametricBufferGeometry : BufferGeometry
{
    public Hashtable parameters;

    public ParametricBufferGeometry(ParameterFunc func, int slices, float stacks)
    {
        parameters = new Hashtable
        {
            { "func", func },
            { "slices", slices },
            { "stacks", stacks }
        };

        var indices = new List<int>();
        var vertices = new List<float>();
        var normals = new List<float>();
        var uvs = new List<float>();

        var EPS = 0.00001f;

        var normal = new Vector3();

        var p0 = new Vector3();
        var p1 = new Vector3();
        var pu = new Vector3();
        var pv = new Vector3();

        int i, j;

        // generate vertices, normals and uvs

        var sliceCount = slices + 1;

        for (i = 0; i <= stacks; i++)
        {
            var v = i / stacks;

            for (j = 0; j <= slices; j++)
            {
                var u = j / (float)slices;

                // vertex

                func(u, v, p0);
                vertices.Add(p0.X);
                vertices.Add(p0.Y);
                vertices.Add(p0.Z);

                // normal

                // approximate tangent vectors via finite differences

                if (u - EPS >= 0)
                {
                    func(u - EPS, v, p1);
                    pu.SubVectors(p0, p1);
                }
                else
                {
                    func(u + EPS, v, p1);
                    pu.SubVectors(p1, p0);
                }

                if (v - EPS >= 0)
                {
                    func(u, v - EPS, p1);
                    pv.SubVectors(p0, p1);
                }
                else
                {
                    func(u, v + EPS, p1);
                    pv.SubVectors(p1, p0);
                }

                // cross product of tangent vectors returns surface normal

                normal.CrossVectors(pu, pv).Normalize();
                normals.Add(normal.X);
                normals.Add(normal.Y);
                normals.Add(normal.Z);

                // uv

                uvs.Add(u);
                uvs.Add(v);
            }
        }

        // generate indices

        for (i = 0; i < stacks; i++)
        for (j = 0; j < slices; j++)
        {
            var a = i * sliceCount + j;
            var b = i * sliceCount + j + 1;
            var c = (i + 1) * sliceCount + j + 1;
            var d = (i + 1) * sliceCount + j;

            // faces one and two
            indices.Add(a);
            indices.Add(b);
            indices.Add(d);
            indices.Add(b);
            indices.Add(c);
            indices.Add(d);
        }

        // build geometry

        SetIndex(indices);
        SetAttribute("position", new BufferAttribute<float>(vertices.ToArray(), 3));
        SetAttribute("normal", new BufferAttribute<float>(normals.ToArray(), 3));
        SetAttribute("uv", new BufferAttribute<float>(uvs.ToArray(), 2));
    }
}