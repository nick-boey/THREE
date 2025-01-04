using System.Collections;
using System.Diagnostics;

namespace THREE;

[Serializable]
public struct MorphTarget
{
    public string Name;
    public List<Vector3> Data;
}

[Serializable]
public struct MorphColor
{
    public string Name;

    public List<Color> Colors;
}

[Serializable]
public class Geometry : ICloneable, IDisposable
{
    protected static int GeometryIdCount;

    public BufferGeometry BufferGeometry;
    public DirectGeometry DirectGeometry;

    private bool _disposed;
    private Object3D _obj = new();
    private Vector3 _offset = Vector3.Zero();

    public int Id;
    public string Name = "";
    public string Type = "Geometry";
    public Guid Uuid = Guid.NewGuid();

    public Box3? BoundingBox;
    public Sphere? BoundingSphere;

    // One-to-one vertex colors, used in Points and Line
    public List<Color> Colors = new();

    // Update flags
    public bool ColorsNeedUpdate;
    public bool ElementsNeedUpdate;
    public bool UvsNeedUpdate;
    public bool NormalsNeedUpdate;
    public bool GroupsNeedUpdate;
    public bool LineDistancesNeedUpdate;
    public bool VerticesNeedUpdate;

    // Faces
    public List<Face3> Faces = new();
    public List<List<List<Vector2>>> FaceVertexUvs = new();

    // Groups
    public List<DrawRange> Groups = new();

    // Line distances
    public List<float> LineDistances = new();

    // Morphs
    public Hashtable MorphNormals = new();
    public Hashtable MorphTargets = new();

    // Normals
    public float[] NormalArray;
    public List<Vector3> Normals = new();

    // Skin
    public List<Vector4> SkinIndices = new();
    public List<Vector4> SkinWeights = new();

    // UVs
    public List<Vector2> Uvs = new();
    public List<Vector2> Uvs2 = new();

    // Vertices
    public List<Vector3> Vertices = new();

    public Geometry()
    {
        Id = GeometryIdCount;
        GeometryIdCount += 1;
    }

    protected Geometry(Geometry source) : this()
    {
        Copy(source);
    }

    public bool IsBufferGeometry { get; set; } = false;

    public object Clone()
    {
        return new Geometry(this);
    }

    public Geometry Copy(Geometry source)
    {
        Name = source.Name;
        var vertices = source.Vertices;

        Vertices = new List<Vector3>(source.Vertices);
        Colors = new List<Color>(source.Colors);
        Faces = new List<Face3>(source.Faces);
        FaceVertexUvs = new List<List<List<Vector2>>>(source.FaceVertexUvs);
        MorphTargets = (Hashtable)source.MorphTargets.Clone();
        MorphNormals = (Hashtable)source.MorphNormals.Clone();
        SkinWeights = new List<Vector4>(source.SkinWeights);
        SkinIndices = new List<Vector4>(source.SkinIndices);
        LineDistances = new List<float>(source.LineDistances);

        if (source.BoundingBox != null)
            BoundingBox = (Box3)source.BoundingBox.Clone();

        if (source.BoundingSphere != null)
            BoundingSphere = (Sphere)source.BoundingSphere.Clone();

        // Update flags
        ElementsNeedUpdate = source.ElementsNeedUpdate;
        VerticesNeedUpdate = source.VerticesNeedUpdate;
        UvsNeedUpdate = source.UvsNeedUpdate;
        NormalsNeedUpdate = source.NormalsNeedUpdate;
        ColorsNeedUpdate = source.ColorsNeedUpdate;
        LineDistancesNeedUpdate = source.LineDistancesNeedUpdate;
        GroupsNeedUpdate = source.GroupsNeedUpdate;

        return this;
    }

    public virtual Geometry ApplyMatrix4(Matrix4 matrix)
    {
        var normalMatrix = new Matrix3().GetNormalMatrix(matrix);

        foreach (var vertex in Vertices)
        {
            vertex.ApplyMatrix4(matrix);
        }

        foreach (var face in Faces)
        {
            face.Normal.ApplyMatrix3(normalMatrix).Normalize();
            foreach (var vertexNormal in face.VertexNormals)
                vertexNormal.ApplyMatrix3(normalMatrix).Normalize();
        }

        if (BoundingBox != null) ComputeBoundingBox();
        if (BoundingSphere != null) ComputeBoundingSphere();

        VerticesNeedUpdate = true;
        NormalsNeedUpdate = true;

        return this;
    }

    /// <summary>
    /// Rotate the geometry about the x-axis
    /// </summary>
    /// <param name="angle">Angle in degrees</param>
    /// <returns>Rotated geometry</returns>
    public virtual Geometry RotateX(float angle)
    {
        return ApplyMatrix4(Matrix4.Identity().MakeRotationX(angle));
    }

    /// <summary>
    /// Rotate the geometry about the y-axis
    /// </summary>
    /// <param name="angle">Angle in degrees</param>
    /// <returns>Rotated geometry</returns>
    public virtual Geometry RotateY(float angle)
    {
        return ApplyMatrix4(Matrix4.Identity().MakeRotationY(angle));
    }

    /// <summary>
    /// Rotate the geometry about the z-axis
    /// </summary>
    /// <param name="angle">Angle in degrees</param>
    /// <returns>Rotated geometry</returns>
    public virtual Geometry RotateZ(float angle)
    {
        return ApplyMatrix4(Matrix4.Identity().MakeRotationZ(angle));
    }

    public virtual Geometry Translate(float x, float y, float z)
    {
        return ApplyMatrix4(Matrix4.Identity().MakeTranslation(x, y, z));
    }

    public virtual Geometry Scale(float x, float y, float z)
    {
        var m = Matrix4.Identity().MakeScale(x, y, z);

        ApplyMatrix4(m);

        return this;
    }

    public virtual Geometry LookAt(Vector3 vector)
    {
        _obj.LookAt(vector);
        _obj.UpdateMatrix();

        return ApplyMatrix4(_obj.Matrix);
    }

    public Geometry FromBufferGeometry(BufferGeometry geometry)
    {
        var indices = geometry.Index?.Array;
        var attributes = geometry.Attributes;

        if (!attributes.TryGetValue("position", out var attribute))
        {
            Trace.TraceError("THREE.Core.Geometry.FromBufferGeometry():Position attribute required for conversion.");
            return this;
        }

        var positions = ((BufferAttribute<float>)attribute).Array;

        float[]? normals = null;
        float[]? colors = null;
        float[]? uvs = null;
        float[]? uvs2 = null;

        if (attributes.TryGetValue("normal", out attribute))
        {
            normals = ((BufferAttribute<float>)attribute).Array;
        }

        if (attributes.TryGetValue("color", out attribute))
        {
            colors = ((BufferAttribute<float>)attribute).Array;
        }

        if (attributes.TryGetValue("uv", out attribute))
        {
            uvs = ((BufferAttribute<float>)attribute).Array;
        }

        if (attributes.TryGetValue("uv2", out attribute))
        {
            uvs2 = ((BufferAttribute<float>)attribute).Array;
        }

        if (uvs2 is { Length: > 0 })
        {
            List<Vector2> uvsList2 = new();
            List<List<Vector2>> uvsList1 = new() { uvsList2 };
            FaceVertexUvs.Add(uvsList1);
        }

        for (var i = 0; i < positions.Length; i += 3)
        {
            Vertices.Add(new Vector3().FromArray(positions, i));
            if (colors is { Length: > 0 })
                Colors.Add(Color.ColorName(ColorKeywords.white).FromArray(colors, i));
        }

        var groups = geometry.Groups;

        if (groups.Count > 0)
        {
            foreach (var group in groups)
            {
                var start = (int)group.Start;
                var count = (int)group.Count;

                for (int j = start, j1 = start + count; j < j1; j += 3)
                    if (indices is { Length: > 0 })
                        AddFace(indices[j], indices[j + 1], indices[j + 2], group.MaterialIndex, normals, uvs, uvs2);
                    else
                        AddFace(j, j + 1, j + 2, group.MaterialIndex, normals, uvs, uvs2);
            }
        }
        else
        {
            if (indices is { Length: > 0 })
                for (var i = 0; i < indices.Length; i += 3)
                    AddFace(indices[i], indices[i + 1], indices[i + 2], 0, normals, uvs, uvs2);
            else
                for (var i = 0; i < positions.Length / 3; i += 3)
                    AddFace(i, i + 1, i + 2, 0, normals, uvs, uvs2);
        }

        ComputeFaceNormals();

        if (geometry.BoundingBox != null) BoundingBox = (Box3)geometry.BoundingBox.Clone();

        if (geometry.BoundingSphere != null) BoundingSphere = (Sphere)geometry.BoundingSphere.Clone();

        return this;
    }

    public virtual Geometry Center()
    {
        ComputeBoundingBox();

        var translationOffset = BoundingBox!.GetCenter(_offset).Negate();
        Translate(translationOffset.X, translationOffset.Y, translationOffset.Z);

        return this;
    }

    public virtual Geometry Normalize()
    {
        ComputeBoundingSphere();

        var center = BoundingSphere!.Center;
        var radius = BoundingSphere!.Radius;

        var s = radius == 0 ? 1 : 1.0f / radius;
        var matrix = new Matrix4(
            s, 0, 0, -s * center.X,
            0, s, 0, -s * center.Y,
            0, 0, s, -s * center.Z,
            0, 0, 0, 1);

        return ApplyMatrix4(matrix);
    }

    public virtual void ComputeFaceNormals()
    {
        for (int f = 0, f1 = Faces.Count; f < f1; f++)
        {
            var face = Faces[f];

            var vA = Vertices[face.a];
            var vB = Vertices[face.b];
            var vC = Vertices[face.c];

            var cb = vC - vB;
            var ab = vA - vB;
            cb.Cross(ab);

            cb.Normalize();

            face.Normal.Copy(cb);
        }
    }

    public virtual void ComputeVertexNormals(bool? areaWeighted = true)
    {
        areaWeighted ??= true;

        var vertices = new Vector3[Vertices.Count];

        for (var v = 0; v < Vertices.Count; v++) vertices[v] = new Vector3();

        if (areaWeighted.Value)
        {
            // Vertex normals weighted by triangle areas
            // http://www.iquilezles.org/www/articles/normals/normals.htm

            Vector3 vA, vB, vC;
            var cb = new Vector3();
            var ab = new Vector3();

            foreach (var face in Faces)
            {
                vA = Vertices[face.a];
                vB = Vertices[face.b];
                vC = Vertices[face.c];

                cb.SubVectors(vC, vB);
                ab.SubVectors(vA, vB);
                cb.Cross(ab);

                vertices[face.a].Add(cb);
                vertices[face.b].Add(cb);
                vertices[face.c].Add(cb);
            }
        }
        else
        {
            ComputeFaceNormals();

            foreach (var face in Faces)
            {
                vertices[face.a].Add(face.Normal);
                vertices[face.b].Add(face.Normal);
                vertices[face.c].Add(face.Normal);
            }
        }

        foreach (var vertex in vertices) vertex.Normalize();

        foreach (var face in Faces)
        {
            var vertexNormals = face.VertexNormals;

            if (vertexNormals.Count == 3)
            {
                vertexNormals[0].Copy(vertices[face.a]);
                vertexNormals[1].Copy(vertices[face.b]);
                vertexNormals[2].Copy(vertices[face.c]);
            }
            else
            {
                vertexNormals.Add((Vector3)vertices[face.a].Clone());
                vertexNormals.Add((Vector3)vertices[face.b].Clone());
                vertexNormals.Add((Vector3)vertices[face.c].Clone());
            }
        }

        if (Faces.Count > 0) NormalsNeedUpdate = true;
    }


    public virtual void ComputeFlatVertexNormals()
    {
        ComputeFaceNormals();

        foreach (var face in Faces)
        {
            var vertexNormals = face.VertexNormals;

            switch (vertexNormals.Count)
            {
                case 3:
                    vertexNormals[0].Copy(face.Normal);
                    vertexNormals[1].Copy(face.Normal);
                    vertexNormals[2].Copy(face.Normal);
                    break;
                case 0:
                    vertexNormals.Add(face.Normal.Clone());
                    vertexNormals.Add(face.Normal.Clone());
                    vertexNormals.Add(face.Normal.Clone());
                    break;
                default:
                    vertexNormals[0] = (Vector3)face.Normal.Clone();
                    vertexNormals[1] = (Vector3)face.Normal.Clone();
                    vertexNormals[2] = (Vector3)face.Normal.Clone();
                    break;
            }
        }

        if (Faces.Count > 0) NormalsNeedUpdate = true;
    }

    public virtual void ComputeMorphNormals()
    {
        throw new NotImplementedException();
    }


    public virtual void ComputeBoundingBox()
    {
        BoundingBox ??= new Box3();
        BoundingBox.SetFromPoints(Vertices);
    }

    public virtual void ComputeBoundingSphere()
    {
        BoundingSphere ??= new Sphere();
        BoundingSphere.SetFromPoints(Vertices);
    }

    private void AddFace(int a, int b, int c, int materialIndex, float[]? normals, float[]? uvs, float[]? uvs2)
    {
        var vertexColors = new List<Color>();
        if (Colors.Count > 0)
        {
            vertexColors.Add(Colors[a]);
            vertexColors.Add(Colors[b]);
            vertexColors.Add(Colors[c]);
        }

        var vertexNormals = new List<Vector3>();

        if (normals is { Length: > 0 })
        {
            vertexNormals.Add(new Vector3().FromArray(normals, a * 3));
            vertexNormals.Add(new Vector3().FromArray(normals, b * 3));
            vertexNormals.Add(new Vector3().FromArray(normals, c * 3));
        }

        var face = new Face3(a, b, c, vertexNormals, vertexColors, materialIndex);

        Faces.Add(face);

        if (uvs is { Length: > 0 })
        {
            List<Vector2> list2 = new()
            {
                new Vector2().FromArray(uvs, a * 2),
                new Vector2().FromArray(uvs, b * 2),
                new Vector2().FromArray(uvs, c * 2)
            };
            List<List<Vector2>> list1 = new() { list2 };
            if (FaceVertexUvs.Count == 0)
                FaceVertexUvs.Add(list1);
            else
                FaceVertexUvs[0].Add(list2);
        }

        if (uvs2 is { Length: > 0 })
        {
            List<Vector2> list2 = new()
            {
                new Vector2().FromArray(uvs2, a * 2),
                new Vector2().FromArray(uvs2, b * 2),
                new Vector2().FromArray(uvs2, c * 2)
            };
            List<List<Vector2>> list1 = new() { list2 };
            if (FaceVertexUvs.Count == 1)
                FaceVertexUvs.Add(list1);
            else
                FaceVertexUvs[1].Add(list2);
        }
    }

    public virtual void Merge(Geometry geometry, Matrix4 matrix, int materialIndexOffset = 0)
    {
        var normalMatrix = new Matrix3();
        var vertexOffset = Vertices.Count;
        var vertices1 = Vertices;
        var vertices2 = geometry.Vertices;
        var faces1 = Faces;
        var faces2 = geometry.Faces;
        var color1 = Colors;
        var color2 = geometry.Colors;

        normalMatrix.GetNormalMatrix(matrix);

        // Vertices
        foreach (var vertex in vertices2)
        {
            var vertexCopy = vertex;

            vertexCopy = vertexCopy.ApplyMatrix4(matrix);

            vertices1.Add(vertexCopy);
        }

        // Colors

        foreach (var t in color2)
            color1.Add(t);

        // Faces

        foreach (var face in faces2)
        {
            var faceVertexNormals = face.VertexNormals;
            var faceVertexColors = face.VertexColors;

            var faceCopy = new Face3(face.a + vertexOffset, face.b + vertexOffset, face.c + vertexOffset);
            faceCopy.Normal.Copy(face.Normal);

            faceCopy.Normal.ApplyMatrix3(normalMatrix).Normalize();

            foreach (var t in faceVertexNormals)
            {
                var normal = (Vector3)t.Clone();

                normal.ApplyMatrix3(normalMatrix).Normalize();

                faceCopy.VertexNormals.Add(normal);
            }

            faceCopy.Color = face.Color;

            foreach (var color in faceVertexColors)
            {
                faceCopy.VertexColors.Add(color);
            }

            faceCopy.MaterialIndex = face.MaterialIndex + materialIndexOffset;

            faces1.Add(faceCopy);
        }

        // UVs

        for (var i = 0; i < geometry.FaceVertexUvs.Count; i++)
        {
            var faceVertexUvs2 = geometry.FaceVertexUvs[i];

            foreach (var uvs2 in faceVertexUvs2)
            {
                List<Vector2> uvsCopy = new();
                foreach (var t in uvs2)
                    uvsCopy.Add((Vector2)t.Clone());

                if (FaceVertexUvs.Count - 1 < i)
                    FaceVertexUvs.Add(new List<List<Vector2>>());
                FaceVertexUvs[i].Add(uvsCopy);
            }
        }
    }

    public virtual void MergeMesh(Mesh mesh)
    {
        if (mesh.MatrixAutoUpdate) mesh.UpdateMatrix();
        Merge(mesh.Geometry, mesh.Matrix);
    }

    /*
     * Checks for duplicate vertices with hashmap.
     * Duplicated vertices are removed
     * and faces' vertices are updated.
     */
    /// <summary>
    /// Checks for duplicate vertices with hashmap. Duplicate vertices are removed and face vertices are updated.
    /// </summary>
    /// <returns>Number of removed duplicate vertices.</returns>
    public virtual int MergeVertices()
    {
        var verticesMap = new Dictionary<string, int>();
        List<Vector3> unique = new();
        var changes = new List<int>();

        const int precisionPoints = 4;
        var precision = (float)Math.Pow(10, precisionPoints);

        for (var i = 0; i < Vertices.Count; i++)
        {
            var v = Vertices[i];
            var key = Math.Round(v.X * precision) + "_" + Math.Round(v.Y * precision) + "_" +
                      Math.Round(v.Z * precision);

            if (!verticesMap.TryGetValue(key, out _))
            {
                verticesMap[key] = i;
                unique.Add(v);
                changes.Add(unique.Count - 1);
            }
            else
            {
                var idx = verticesMap[key];
                changes.Add(changes[idx]);
            }
        }


        // If faces are completely degenerate after merging vertices, we
        // have to remove them from the geometry.
        var faceIndicesToRemove = new List<int>();

        for (var i = 0; i < Faces.Count; i++)
        {
            var face = Faces[i];


            face.a = changes[face.a];
            face.b = changes[face.b];
            face.c = changes[face.c];

            var indices = new[] { face.a, face.b, face.c };

            for (var n = 0; n < 3; n++)
            {
                if (indices[n] != indices[(n + 1) % 3]) continue;

                faceIndicesToRemove.Add(i);
                break;
            }
        }

        for (var i = faceIndicesToRemove.Count - 1; i >= 0; i--)
        {
            var idx = faceIndicesToRemove[i];

            Faces.RemoveAt(idx);

            foreach (var t in FaceVertexUvs)
                t.RemoveAt(idx);
        }

        // Calculate the number of removed vertices
        var diff = Vertices.Count - unique.Count;

        Vertices = unique;

        return diff;
    }

    public Geometry SetFromPoints(List<Vector3> points)
    {
        Vertices.Clear();
        foreach (var point in points)
        {
            Vertices.Add(new Vector3(point.X, point.Y, point.Z));
        }

        return this;
    }

    #region IDisposable Members

    public event EventHandler<EventArgs>? Disposed;

    protected virtual void RaiseDisposed()
    {
        var handler = Disposed;
        handler?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Implement the IDisposable interface
    /// </summary>
    public virtual void Dispose()
    {
        Dispose(true);
        // This object will be cleaned up by the Dispose method.
        // Therefore, you should call GC.SupressFinalize to
        // take this object off the finalization queue 
        // and prevent finalization code for this object
        // from executing a second time.
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        // Check to see if Dispose has already been called.
        if (_disposed) return;

        _disposed = true;
        RaiseDisposed();
    }

    #endregion
}