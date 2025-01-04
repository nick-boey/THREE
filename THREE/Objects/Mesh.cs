using System.Collections;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class Mesh : Object3D
{
    private Vector3 _intersectionPoint = new();
    private Vector3 _intersectionPointWorld = new();
    private Matrix4 _inverseMatrix = new();
    private Vector3 _morphA = new();
    private Vector3 _morphB = new();
    private Vector3 _morphC = new();
    private Ray _ray = new();
    private Sphere _sphere = new();
    private Vector3 _tempA = new();
    private Vector3 _tempB = new();
    private Vector3 _tempC = new();

    private Vector2 _uvA = new();
    private Vector2 _uvB = new();
    private Vector2 _uvC = new();
    private Vector3 _vA = new();
    private Vector3 _vB = new();
    private Vector3 _vC = new();

    //public List<int> MorphTargetInfluences = new List<int>();

    //public Dictionary<string, int> MorphTargetDictionary = new Dictionary<string, int>();
    public Mesh()
    {
        InitGeometry(null, null);
    }

    public Mesh(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public Mesh(Geometry geometry = null, List<Material> materials = null)
    {
        InitGeometries(geometry, materials);
    }

    public Mesh(Geometry geometry = null, Material material = null)
    {
        InitGeometry(geometry, material);
    }

    protected Mesh(Mesh source, bool recursive = true) : this()
    {
        Name = source.Name;

        Up.Copy(source.Up);

        Position.Copy(source.Position);
        Quaternion.Copy(source.Quaternion);
        Scale.Copy(source.Scale);

        Matrix.Copy(source.Matrix);
        MatrixWorld.Copy(source.MatrixWorld);

        MatrixAutoUpdate = source.MatrixAutoUpdate;
        MatrixWorldNeedsUpdate = source.MatrixWorldNeedsUpdate;

        Layers.Mask = source.Layers.Mask;
        Visible = source.Visible;

        CastShadow = source.CastShadow;
        ReceiveShadow = source.ReceiveShadow;

        FrustumCulled = source.FrustumCulled;
        RenderOrder = source.RenderOrder;

        UserData = source.UserData;

        /*
         * if you deal with this cloned object to indivisual, you need to adopt real deep copy of source's Geometry, Material, Materials, and it's base class , Hashtable
         * this will be accomplished by declaring Serialize all three class and  writing all class member to Memory stream , and deserializing...
         * please refer to Deep copy of C# Class
         * */
        if (source.Geometry != null)
        {
            if (source.Geometry is BufferGeometry)
                Geometry = source.Geometry as BufferGeometry;
            else
                Geometry = source.Geometry;
        }

        if (source.Material != null) Material = source.Material;
        if (source.Materials.Count > 0) Materials = source.Materials;

        if (recursive)
            for (var i = 0; i < source.Children.Count; i++)
            {
                var child = source.Children[i];
                Add((Object3D)child.Clone());
            }
    }

    public Mesh Copy(Mesh source)
    {
        return source.DeepCopy();
    }

    public override object Clone()
    {
        //Hashtable hastTable = base.Clone() as Hashtable;
        //Mesh cloned = new Mesh(this);
        //foreach (DictionaryEntry entry in hastTable)
        //{
        //    cloned.Add(entry.Key, entry.Value);
        //}

        //return cloned;
        return this.DeepCopy();
    }

    public virtual void InitGeometries(Geometry geometry, List<Material> materials)
    {
        type = "Mesh";

        if (geometry == null)
            Geometry = new BufferGeometry();
        else
            Geometry = geometry;

        if (materials == null)
        {
            Material = new MeshBasicMaterial { Color = new Color().SetHex(0xffffff) };
            Materials.Add(Material);
        }
        else
        {
            Materials = materials;
            if (Materials.Count > 0)
                Material = Materials[0];
        }

        UpdateMorphTargets();
    }

    public virtual void InitGeometry(Geometry geometry, Material material)
    {
        type = "Mesh";

        if (geometry == null)
            Geometry = new BufferGeometry();
        else
            Geometry = geometry;

        if (material == null)
        {
            Material = new MeshBasicMaterial { Color = new Color().SetHex(0xffffff) };
        }
        else
        {
            Materials.Clear();
            Material = material;
        }

        Materials.Add(Material);

        UpdateMorphTargets();
    }

    public void UpdateMorphTargets()
    {
        var geometry = Geometry as BufferGeometry;
        if (geometry != null && geometry is BufferGeometry)
        {
            var morphAttributes = geometry.MorphAttributes;
            var keys = morphAttributes.Keys;

            //if (keys.Count > 0)
            if (morphAttributes != null && morphAttributes.Count > 0)
                foreach (DictionaryEntry entry in morphAttributes)
                {
                    var morphAttribute = morphAttributes[entry.Key] as List<IBufferAttribute>;

                    if (morphAttribute != null)
                    {
                        MorphTargetInfluences = new List<float>();
                        MorphTargetDictionary = new Hashtable();

                        for (var m = 0; m < morphAttribute.Count; m++)
                        {
                            var name = morphAttribute[m] != null
                                ? (morphAttribute[m] as BufferAttribute<float>).Name
                                : m.ToString();

                            MorphTargetInfluences.Add(0);
                            if (MorphTargetDictionary.ContainsKey(name))
                                MorphTargetDictionary[name] = m;
                            else
                                MorphTargetDictionary.Add(name, m);
                        }
                    }
                }
        }
        else
        {
            if (geometry != null && geometry.MorphTargets != null && geometry.MorphTargets.Count > 0)
                Trace.TraceError(
                    "THREE.Mesh.updateMorphTargets() no longer supports THREE.Geometry. Use THREE.BufferGeometry instead.");
        }
    }

    public override void Raycast(Raycaster raycaster, List<Intersection> intersects)
    {
        if (Material == null) return;

        // Checking boundingSphere distance to ray

        if (Geometry.BoundingSphere == null) Geometry.ComputeBoundingSphere();

        _sphere.Copy(Geometry.BoundingSphere);
        _sphere.ApplyMatrix4(MatrixWorld);

        if (raycaster.ray.IntersectsSphere(_sphere) == false) return;

        //

        _inverseMatrix.GetInverse(MatrixWorld);
        _ray.copy(raycaster.ray).ApplyMatrix4(_inverseMatrix);

        // Check boundingBox before continuing

        if (Geometry.BoundingBox != null)
            if (_ray.IntersectsBox(Geometry.BoundingBox) == false)
                return;

        Intersection intersection;

        if (Geometry is BufferGeometry)
        {
            var bufferGeometry = Geometry as BufferGeometry;
            //const index = geometry.index;
            var position = bufferGeometry.Attributes.ContainsKey("position")
                ? bufferGeometry.Attributes["position"] as BufferAttribute<float>
                : null;
            List<IBufferAttribute> morphPosition = bufferGeometry.MorphAttributes.ContainsKey("position")
                ? bufferGeometry.MorphAttributes["position"] as List<IBufferAttribute>
                : null;
            var morphTargetsRelative = bufferGeometry.MorphTargetsRelative;
            var uv = bufferGeometry.Attributes.ContainsKey("uv")
                ? bufferGeometry.Attributes["uv"] as BufferAttribute<float>
                : null;
            ;
            var uv2 = bufferGeometry.Attributes.ContainsKey("uv2")
                ? bufferGeometry.Attributes["uv2"] as BufferAttribute<float>
                : null;
            ;
            //const groups = geometry.groups;
            //const drawRange = geometry.drawRange;

            if (bufferGeometry.Index != null)
            {
                // indexed buffer geometry

                if (Materials.Count > 1)
                {
                    for (var i = 0; i < bufferGeometry.Groups.Count; i++)
                    {
                        var group = bufferGeometry.Groups[i];
                        var groupMaterial = Materials[group.MaterialIndex];

                        var start = (int)Math.Max(group.Start, bufferGeometry.DrawRange.Start);
                        var end = (int)Math.Min(group.Start + group.Count,
                            bufferGeometry.DrawRange.Start + bufferGeometry.DrawRange.Count);

                        for (var j = start; j < end; j += 3)
                        {
                            var a = bufferGeometry.Index.GetX(j);
                            var b = bufferGeometry.Index.GetX(j + 1);
                            var c = bufferGeometry.Index.GetX(j + 2);

                            intersection = checkBufferGeometryIntersection(this, groupMaterial, raycaster, _ray,
                                position, morphPosition, morphTargetsRelative, uv, uv2, a, b, c);

                            if (intersection != null)
                            {
                                intersection.FaceIndex =
                                    (int)Math.Floor((decimal)j / 3); // triangle number in indexed buffer semantics
                                intersection.Face.MaterialIndex = group.MaterialIndex;
                                intersects.Add(intersection);
                            }
                        }
                    }
                }
                else
                {
                    var start = Math.Max(0, bufferGeometry.DrawRange.Start);
                    var end = Math.Min(bufferGeometry.Index.count,
                        bufferGeometry.DrawRange.Start + bufferGeometry.DrawRange.Count);

                    for (var i = (int)start; i < (int)end; i += 3)
                    {
                        var a = bufferGeometry.Index.GetX(i);
                        var b = bufferGeometry.Index.GetX(i + 1);
                        var c = bufferGeometry.Index.GetX(i + 2);

                        intersection = checkBufferGeometryIntersection(this, Material, raycaster, _ray, position,
                            morphPosition, morphTargetsRelative, uv, uv2, a, b, c);

                        if (intersection != null)
                        {
                            intersection.FaceIndex =
                                (int)Math.Floor((decimal)i / 3); // triangle number in indexed buffer semantics
                            intersects.Add(intersection);
                        }
                    }
                }
            }
            else if (bufferGeometry.Attributes["position"] != null)
            {
                // non-indexed buffer geometry

                if (Materials.Count > 1)
                {
                    for (var i = 0; i < bufferGeometry.Groups.Count; i++)
                    {
                        var group = bufferGeometry.Groups[i];
                        var groupMaterial = Materials[group.MaterialIndex];

                        var start = (int)Math.Max(group.Start, bufferGeometry.DrawRange.Start);
                        var end = (int)Math.Min(group.Start + group.Count,
                            bufferGeometry.DrawRange.Start + bufferGeometry.DrawRange.Count);

                        for (var j = start; j < end; j += 3)
                        {
                            var a = j;
                            var b = j + 1;
                            var c = j + 2;

                            intersection = checkBufferGeometryIntersection(this, groupMaterial, raycaster, _ray,
                                position, morphPosition, morphTargetsRelative, uv, uv2, a, b, c);

                            if (intersection != null)
                            {
                                intersection.FaceIndex =
                                    (int)Math.Floor((decimal)j / 3); // triangle number in non-indexed buffer semantics
                                intersection.Face.MaterialIndex = group.MaterialIndex;
                                intersects.Add(intersection);
                            }
                        }
                    }
                }
                else
                {
                    var start = (int)Math.Max(0, bufferGeometry.DrawRange.Start);
                    var end = (int)Math.Min(position.count,
                        bufferGeometry.DrawRange.Start + bufferGeometry.DrawRange.Count);

                    for (var i = start; i < end; i += 3)
                    {
                        var a = i;
                        var b = i + 1;
                        var c = i + 2;

                        intersection = checkBufferGeometryIntersection(this, Material, raycaster, _ray, position,
                            morphPosition, morphTargetsRelative, uv, uv2, a, b, c);

                        if (intersection != null)
                        {
                            intersection.FaceIndex =
                                (int)Math.Floor((decimal)i / 3); // triangle number in non-indexed buffer semantics
                            intersects.Add(intersection);
                        }
                    }
                }
            }
        }
        else if (Geometry is Geometry)
        {
            var isMultiMaterial = Materials.Count > 1;

            var vertices = Geometry.Vertices;
            var faces = Geometry.Faces;
            List<List<Vector2>> uvs = null;

            if (Geometry.FaceVertexUvs.Count > 0) uvs = Geometry.FaceVertexUvs[0];

            for (var f = 0; f < faces.Count; f++)
            {
                var face = faces[f];
                var faceMaterial = isMultiMaterial ? Materials[face.MaterialIndex] : Material;

                if (faceMaterial == null) continue;

                var fvA = vertices[face.a];
                var fvB = vertices[face.b];
                var fvC = vertices[face.c];

                intersection =
                    checkIntersection(this, faceMaterial, raycaster, _ray, fvA, fvB, fvC, _intersectionPoint);

                if (intersection != null)
                {
                    if (uvs != null && uvs.Count > 0 && uvs[f].Count > 0)
                    {
                        var uvs_f = uvs[f];
                        _uvA.Copy(uvs_f[0]);
                        _uvB.Copy(uvs_f[1]);
                        _uvC.Copy(uvs_f[2]);

                        intersection.Uv = Triangle.GetUV(_intersectionPoint, fvA, fvB, fvC, _uvA, _uvB, _uvC,
                            new Vector2());
                    }

                    intersection.Face = face;
                    intersection.FaceIndex = f;
                    intersects.Add(intersection);
                }
            }
        }
    }

    private Intersection checkIntersection(Object3D? object3D, Material material, Raycaster raycaster, Ray ray,
        Vector3 pA, Vector3 pB, Vector3 pC, Vector3 point)
    {
        Vector3 intersect;

        if (material.Side == Constants.BackSide)
            intersect = ray.IntersectTriangle(pC, pB, pA, true, point);
        else
            intersect = ray.IntersectTriangle(pA, pB, pC, material.Side != Constants.DoubleSide, point);


        if (intersect == null) return null;

        _intersectionPointWorld.Copy(point);
        _intersectionPointWorld.ApplyMatrix4(object3D.MatrixWorld);

        var distance = raycaster.ray.origin.DistanceTo(_intersectionPointWorld);

        if (distance < raycaster.near || distance > raycaster.far) return null;

        var result = new Intersection();
        result.Distance = distance;
        result.Point = _intersectionPointWorld.Clone();
        result.Object3D = object3D;

        return result;
    }

    private Intersection checkBufferGeometryIntersection(Object3D? object3D, Material material, Raycaster raycaster,
        Ray ray, IBufferAttribute position, List<IBufferAttribute> morphPosition, bool morphTargetsRelative,
        BufferAttribute<float> uv, BufferAttribute<float> uv2, int a, int b, int c)
    {
        _vA.FromBufferAttribute(position, a);
        _vB.FromBufferAttribute(position, b);
        _vC.FromBufferAttribute(position, c);

        var morphInfluences = object3D.MorphTargetInfluences;

        if (material.MorphTargets && morphPosition != null && morphPosition.Count > 0 && morphInfluences != null &&
            morphInfluences.Count > 0)
        {
            _morphA.Set(0, 0, 0);
            _morphB.Set(0, 0, 0);
            _morphC.Set(0, 0, 0);

            for (var i = 0; i < morphPosition.Count; i++)
            {
                var influence = morphInfluences[i];
                var morphAttribute = morphPosition[i];

                if (influence == 0) continue;

                _tempA.FromBufferAttribute(morphAttribute, a);
                _tempB.FromBufferAttribute(morphAttribute, b);
                _tempC.FromBufferAttribute(morphAttribute, c);

                if (morphTargetsRelative)
                {
                    _morphA.AddScaledVector(_tempA, influence);
                    _morphB.AddScaledVector(_tempB, influence);
                    _morphC.AddScaledVector(_tempC, influence);
                }
                else
                {
                    _morphA.AddScaledVector(_tempA.Sub(_vA), influence);
                    _morphB.AddScaledVector(_tempB.Sub(_vB), influence);
                    _morphC.AddScaledVector(_tempC.Sub(_vC), influence);
                }
            }

            _vA.Add(_morphA);
            _vB.Add(_morphB);
            _vC.Add(_morphC);
        }

        if (object3D is SkinnedMesh)
        {
            var skin = object3D as SkinnedMesh;

            skin.BoneTransform(a, _vA.ToVector4());
            skin.BoneTransform(b, _vB.ToVector4());
            skin.BoneTransform(c, _vC.ToVector4());
        }

        var intersection = checkIntersection(object3D, material, raycaster, ray, _vA, _vB, _vC, _intersectionPoint);

        if (intersection != null)
        {
            if (uv != null)
            {
                _uvA.FromBufferAttribute(uv, a);
                _uvB.FromBufferAttribute(uv, b);
                _uvC.FromBufferAttribute(uv, c);

                intersection.Uv = Triangle.GetUV(_intersectionPoint, _vA, _vB, _vC, _uvA, _uvB, _uvC, new Vector2());
            }

            if (uv2 != null)
            {
                _uvA.FromBufferAttribute(uv2, a);
                _uvB.FromBufferAttribute(uv2, b);
                _uvC.FromBufferAttribute(uv2, c);

                intersection.Uv2 = Triangle.GetUV(_intersectionPoint, _vA, _vB, _vC, _uvA, _uvB, _uvC, new Vector2());
            }

            var face = new Face3(a, b, c);
            Triangle.GetNormal(_vA, _vB, _vC, face.Normal);

            intersection.Face = face;
        }

        return intersection;
    }
}