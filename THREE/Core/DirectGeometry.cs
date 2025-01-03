using System.Diagnostics;

namespace THREE;

[Serializable]
public class DirectGeometry : Geometry
{
    public List<int> Indices = new();

    public void ComputeGroups(Geometry geometry)
    {
        var faces = geometry.Faces;
        var group = new DrawRange { Start = -1 };
        var groups = new List<DrawRange>();

        var materialIndex = -1;
        int i;
        for (i = 0; i < faces.Count; i++)
        {
            var face = faces[i];

            if (face.MaterialIndex != materialIndex)
            {
                materialIndex = face.MaterialIndex;

                if (group.Start != -1)
                {
                    group.Count = i * 3 - group.Start;
                    groups.Add(group);
                }

                group = new DrawRange { Start = i * 3, MaterialIndex = materialIndex };
            }
        }

        if (group.Start != -1)
        {
            group.Count = i * 3 - group.Start;
            groups.Add(group);
        }

        Groups = groups;
    }

    public DirectGeometry FromGeometry(Geometry geometry)
    {
        var faces = geometry.Faces;
        var vertices = geometry.Vertices;
        var faceVertexUvs = geometry.FaceVertexUvs;

        var hasFaceVertexUv = faceVertexUvs.Count > 0 && faceVertexUvs[0] != null && faceVertexUvs[0].Count > 0;
        var hasFaceVertexUv2 = faceVertexUvs.Count > 1 && faceVertexUvs[1] != null && faceVertexUvs[1].Count > 0;


        var morphTargets = geometry.MorphTargets;

        var morphTargetsLength = geometry.MorphTargets.Count;

        var morphTargetsPosition = new List<MorphTarget>();
        List<string> morphTargetKeys = new();
        if (morphTargetsLength > 0)
        {
            foreach (string key in morphTargets.Keys)
            {
                morphTargetsPosition.Add(new MorphTarget { Name = key, Data = new List<Vector3>() });
                morphTargetKeys.Add(key);
            }

            MorphTargets.Add("position", morphTargetsPosition);
        }


        var morphNormals = geometry.MorphNormals;
        var morphNormalsLength = morphNormals.Count;

        var morphTargetsNormal = new List<MorphTarget>();
        List<string> morphTargetsNormalKeys = new();
        if (morphNormalsLength > 0)
        {
            foreach (string key in morphNormals.Keys)
            {
                morphTargetsNormal.Add(new MorphTarget { Name = key, Data = new List<Vector3>() });
                morphTargetsNormalKeys.Add(key);
            }

            MorphTargets.Add("normal", morphTargetsNormal);
        }

        var skinIndices = geometry.SkinIndices;
        var skinWeights = geometry.SkinWeights;

        var hasSkinIndices = skinIndices.Count == vertices.Count;
        var hasSkinWeights = skinWeights.Count == vertices.Count;

        if (vertices.Count > 0 && faces.Count == 0)
            Trace.TraceError("THREE.Core.DirectGeometry:Faceless geometries are not supported.");

        var vLen = vertices.Count - 1;
        for (var i = 0; i < faces.Count; i++)
        {
            var face = faces[i];
            if (face.a > vLen) continue;
            Vertices.Add(vertices[face.a]);
            Vertices.Add(vertices[face.b]);
            Vertices.Add(vertices[face.c]);

            var vertexNormals = face.VertexNormals;

            if (vertexNormals.Count == 3)
            {
                Normals.Add(vertexNormals[0]);
                Normals.Add(vertexNormals[1]);
                Normals.Add(vertexNormals[2]);
            }
            else
            {
                var normal = face.Normal;

                Normals.Add(normal);
                Normals.Add(normal);
                Normals.Add(normal);
            }

            var vertexColors = face.VertexColors;

            if (vertexColors.Count == 3)
            {
                Colors.Add(vertexColors[0]);
                Colors.Add(vertexColors[1]);
                Colors.Add(vertexColors[2]);
            }
            else
            {
                var color = face.Color;

                Colors.Add(color);
                Colors.Add(color);
                Colors.Add(color);
            }

            if (hasFaceVertexUv)
            {
                var vertexUvs = faceVertexUvs[0][i];

                if (vertexUvs.Count > 0)
                {
                    Uvs.Add(vertexUvs[0]);
                    Uvs.Add(vertexUvs[1]);
                    Uvs.Add(vertexUvs[2]);
                }
                else
                {
                    Trace.TraceError("THREE.Core.DirectGeometry.FromGeometry():undefined vertexUV");

                    Uvs.Add(Vector2.Zero());
                    Uvs.Add(Vector2.Zero());
                    Uvs.Add(Vector2.Zero());
                }
            }

            if (hasFaceVertexUv2)
            {
                var vertexUvs = faceVertexUvs[1][i];

                if (vertexUvs.Count > 0)
                {
                    Uvs2.Add(vertexUvs[0]);
                    Uvs2.Add(vertexUvs[1]);
                    Uvs2.Add(vertexUvs[2]);
                }
                else
                {
                    Trace.TraceError("THREE.Core.DirectGeometry.FromGeometry():undefined vertexUV2");

                    Uvs2.Add(Vector2.Zero());
                    Uvs2.Add(Vector2.Zero());
                    Uvs2.Add(Vector2.Zero());
                }
            }

            // morphs
            for (var j = 0; j < morphTargetKeys.Count; j++)
            {
                var key = morphTargetKeys[j];
                var morphTarget = geometry.MorphTargets[key] as List<Vector3>;

                morphTargetsPosition[j].Data.Add(morphTarget[face.a]);
                morphTargetsPosition[j].Data.Add(morphTarget[face.b]);
                morphTargetsPosition[j].Data.Add(morphTarget[face.c]);
            }

            for (var j = 0; j < morphTargetsNormalKeys.Count; j++)
            {
                var key = morphTargetsNormalKeys[j];

                var morphNormal = ((MorphTarget)geometry.MorphNormals[key]).Data;

                ((MorphTarget)morphNormals[key]).Data.Add(morphNormal[face.a]);
                ((MorphTarget)morphNormals[key]).Data.Add(morphNormal[face.b]);
                ((MorphTarget)morphNormals[key]).Data.Add(morphNormal[face.c]);
            }

            //skins
            if (hasSkinIndices)
            {
                SkinIndices.Add(skinIndices[face.a]);
                SkinIndices.Add(skinIndices[face.b]);
                SkinIndices.Add(skinIndices[face.c]);
            }

            if (hasSkinWeights)
            {
                SkinWeights.Add(skinWeights[face.a]);
                SkinWeights.Add(skinWeights[face.b]);
                SkinWeights.Add(skinWeights[face.c]);
            }
        }

        ComputeGroups(geometry);

        VerticesNeedUpdate = geometry.VerticesNeedUpdate;
        NormalsNeedUpdate = geometry.NormalsNeedUpdate;
        ColorsNeedUpdate = geometry.ColorsNeedUpdate;
        UvsNeedUpdate = geometry.UvsNeedUpdate;

        if (geometry.BoundingSphere != null) BoundingSphere = (Sphere)geometry.BoundingSphere.Clone();

        if (geometry.BoundingBox != null) BoundingBox = (Box3)geometry.BoundingBox.Clone();

        return this;
    }


    #region IDisposable Members

    /// <summary>
    ///     Implement the IDisposable interface
    /// </summary>
    public override void Dispose()
    {
        Dispose(true);
        // This object will be cleaned up by the Dispose method.
        // Therefore, you should call GC.SupressFinalize to
        // take this object off the finalization queue 
        // and prevent finalization code for this object
        // from executing a second time.
        GC.SuppressFinalize(this);
    }

    #endregion
}