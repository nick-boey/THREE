using System.Collections;
using System.Diagnostics;

namespace THREE;

[Serializable]
public struct DrawRange
{
    public float Start;
    public int MaterialIndex;
    public float Count;
}

[Serializable]
public class BufferGeometry : Geometry
{
    protected static int BufferGeometryIdCount;

    private Box3 _box = new();

    private Box3 _boxMorphTargets = new();

    private Object3D _obj = new();

    private Vector3 _offset = Vector3.Zero();

    private Vector3 _vector = Vector3.Zero();

    public Dictionary<object, object> Attributes;

    public DrawRange DrawRange = new() { Start = 0, MaterialIndex = -1, Count = float.PositiveInfinity };

    //public List<string> AttributesKeys { get; set; }

    //public IList<DrawRange> Drawcalls = new List<DrawRange>();

    //public IList<DrawRange> Offsets;

    public BufferAttribute<int>? Index;

    public Hashtable MorphAttributes;

    public bool MorphTargetsRelative;

    public Hashtable UserData = new();

    public BufferGeometry()
    {
        Id = BufferGeometryIdCount;
        BufferGeometryIdCount += 2;

        Type = "BufferGeometry";

        Attributes = new Dictionary<object, object>();

        MorphAttributes = new Hashtable();

        //this.Offsets = this.Drawcalls;

        BoundingBox = null;

        BoundingSphere = null;

        IsBufferGeometry = true;

        //AttributesKeys = new List<string>();
    }

    protected BufferGeometry(BufferGeometry source)
    {
        Copy(source);
    }

    public new BufferGeometry Clone()
    {
        return new BufferGeometry(this);
    }

    public BufferGeometry Copy(BufferGeometry source)
    {
        base.Copy(source);
        Index = null;
        Attributes = new Dictionary<object, object>();

        MorphAttributes = (Hashtable)source.MorphAttributes.Clone();

        Groups = new List<DrawRange>(source.Groups);
        BoundingBox = null;
        BoundingSphere = null;
        IsBufferGeometry = source.IsBufferGeometry;
        // used for storing cloned, shared data


        // name

        Name = source.Name;

        // index

        var index = source.Index;

        if (index != null) Index = source.Index.Clone();

        // attributes

        var attributes = source.Attributes;
        foreach (var entry in attributes)
        {
            if (entry.Value is BufferAttribute<float>)
                Attributes.Add(entry.Key, (entry.Value as BufferAttribute<float>).Clone());
            if (entry.Value is BufferAttribute<int>)
                Attributes.Add(entry.Key, (entry.Value as BufferAttribute<int>).Clone());
            if (entry.Value is BufferAttribute<byte>)
                Attributes.Add(entry.Key, (entry.Value as BufferAttribute<byte>).Clone());
        }

        MorphTargetsRelative = source.MorphTargetsRelative;

        // groups
        //const groups = source.groups;

        //for (let i = 0, l = groups.length; i < l; i++)
        //{

        //    const group = groups[i];
        //    this.addGroup(group.start, group.count, group.materialIndex);

        //}

        // bounding box

        var boundingBox = source.BoundingBox;

        if (boundingBox != null) BoundingBox = (Box3)boundingBox.Clone();

        // bounding sphere

        var boundingSphere = source.BoundingSphere;

        if (boundingSphere != null) BoundingSphere = (Sphere)boundingSphere.Clone();

        // draw range

        DrawRange.Start = source.DrawRange.Start;
        DrawRange.Count = source.DrawRange.Count;

        // user data

        UserData = (Hashtable)source.UserData.Clone();

        return this;
    }

    public BufferAttribute<int> GetIndex()
    {
        return Index;
    }

    public void SetIndex(List<int> index, int itemSize = 1)
    {
        Index = new BufferAttribute<int>(index.ToArray<int>(), itemSize);
    }

    public void SetIndex(BufferAttribute<int> index)
    {
        Index = index;
    }

    public IBufferAttribute GetAttribute<T>(string name)
    {
        return Attributes[name] as IBufferAttribute;
    }

    public BufferGeometry SetAttribute(string name, IBufferAttribute attribute)
    {
        Attributes[name] = attribute;
        //if (!AttributesKeys.Contains(name))
        //    this.AttributesKeys.Add(name);

        return this;
    }

    public void deleteAttribute(string name)
    {
        Attributes.Remove(name);
    }


    //public void AddAttribute(string name, Renderers.Shaders.Attribute attribute)
    //{
    //    if (attribute is IBufferAttribute == false)
    //    {
    //        Trace.TraceWarning("BufferGeometry: .addAttribute() now expects ( name, attribute ).");
    //    }

    //    this.Attributes[name] = attribute;

    //    this.AttributesKeys = new List<string>();
    //    foreach (var entry in this.Attributes)
    //    {
    //        this.AttributesKeys.Add(entry.Key);
    //    }
    //}

    public virtual void AddGroup(int start, int count, int materialIndex = 0)
    {
        Groups.Add(new DrawRange { Start = start, Count = count, MaterialIndex = materialIndex });
    }

    public void ClearGroups()
    {
        Groups.Clear();
    }

    public void SetDrawRange(int start, int count)
    {
        DrawRange.Start = start;
        DrawRange.Count = count;
    }

    public BufferGeometry ApplyMatrix(Matrix4 matrix)
    {
        if (Attributes.ContainsKey("position"))
        {
            var position = (BufferAttribute<float>)Attributes["position"];

            if (position != null)
            {
                position = matrix.ApplyToBufferAttribute(position);
                position.NeedsUpdate = true;
            }
        }

        if (Attributes.ContainsKey("normal"))
        {
            var normal = (BufferAttribute<float>)Attributes["normal"];

            if (normal != null)
            {
                var normalMatrix = new Matrix3().GetNormalMatrix(matrix);

                normal = normalMatrix.ApplyToBufferAttribute(normal);
                normal.NeedsUpdate = true;
            }
        }

        if (Attributes.ContainsKey("tangent"))
        {
            var tangent = (BufferAttribute<float>)Attributes["tangent"];

            if (tangent != null)
            {
                var normalMatrix = new Matrix3().GetNormalMatrix(matrix);
                tangent = normalMatrix.ApplyToBufferAttribute(tangent);
                tangent.NeedsUpdate = true;
            }
        }

        if (BoundingBox != null) ComputeBoundingBox();

        if (BoundingSphere != null) ComputeBoundingSphere();
        return this;
    }

    public new BufferGeometry RotateX(float angle)
    {
        var m = Matrix4.Identity().MakeRotationX(angle);

        ApplyMatrix(m);

        return this;
    }

    public new BufferGeometry RotateY(float angle)
    {
        var m = Matrix4.Identity().MakeRotationY(angle);

        ApplyMatrix(m);

        return this;
    }

    public new BufferGeometry RotateZ(float angle)
    {
        var m = Matrix4.Identity().MakeRotationZ(angle);

        ApplyMatrix(m);

        return this;
    }

    public new BufferGeometry Translate(float x, float y, float z)
    {
        var m = Matrix4.CreateTranslation(x, y, z);

        ApplyMatrix(m);

        return this;
    }

    public new BufferGeometry Scale(float x, float y, float z)
    {
        var m = Matrix4.Identity().MakeScale(x, y, z);

        ApplyMatrix(m);

        return this;
    }

    public new BufferGeometry LookAt(Vector3 vector)
    {
        _obj.LookAt(vector);

        _obj.UpdateMatrix();

        ApplyMatrix(_obj.Matrix);

        return this;
    }

    public new BufferGeometry Center()
    {
        ComputeBoundingBox();

        BoundingBox.GetCenter(_offset).Negate();

        Translate(_offset.X, _offset.Y, _offset.Z);

        return this;
    }

    public BufferGeometry SetFromObject(Object3D object3D)
    {
        var geometry = object3D.Geometry;

        if (object3D is Points || object3D is Line)
        {
            var positions =
                new BufferAttribute<float>(); //(geometry.Vertices.ToArray().ToArray<float>(), geometry.Vertices.Count * 3, 3);
            positions.ItemSize = 3;
            positions.Type = typeof(float);

            var colors = new BufferAttribute<float>();
            colors.ItemSize = 3;
            colors.Type = typeof(float);

            //TODO change Vector3 List not Vector3 to float[]
            SetAttribute("position", positions.CopyVector3sArray(geometry.Vertices.ToArray()));
            SetAttribute("color", colors.CopyColorsArray(geometry.Colors.ToArray<Color>()));

            if (geometry.LineDistances != null && geometry.LineDistances.Count == geometry.Vertices.Count)
            {
                var lineDistances = new BufferAttribute<float>();
                lineDistances.Type = typeof(float);
                lineDistances.Array = new float[geometry.LineDistances.Count];
                lineDistances.ItemSize = 1;

                Array.Copy(geometry.LineDistances.ToArray(), lineDistances.Array, (long)geometry.LineDistances.Count);

                SetAttribute("lineDistance", lineDistances);
            }

            if (geometry.BoundingSphere != null) BoundingSphere = (Sphere)geometry.BoundingSphere.Clone();

            if (geometry.BoundingBox != null) BoundingBox = (Box3)geometry.BoundingBox.Clone();
        }
        else if (object3D is Mesh)
        {
            if (geometry != null && !geometry.IsBufferGeometry) FromGeometry(geometry);
        }

        return this;
    }

    public new BufferGeometry SetFromPoints(List<Vector3> points)
    {
        var position = new List<float>();

        for (var i = 0; i < points.Count; i++)
        {
            var point = points[i];
            position.Add(point.X, point.Y, point.Z);
        }

        SetAttribute("position", new BufferAttribute<float>(position.ToArray(), 3));

        return this;
    }

    public BufferGeometry SetFromPoints(Vector3[] points)
    {
        var position = new List<float>();

        for (var i = 0; i < points.Length; i++)
        {
            var point = points[i];

            position.Add(point.X);
            position.Add(point.Y);
            position.Add(point.Z);
        }

        SetAttribute("position", new BufferAttribute<float>(position.ToArray(), 3));

        return this;
    }

    public BufferGeometry UpdateFromObject(Object3D object3D)
    {
        var geometry = object3D.Geometry;

        if (object3D is Mesh)
        {
            var direct = geometry.DirectGeometry;

            if (geometry.ElementsNeedUpdate)
            {
                direct = null;
                geometry.ElementsNeedUpdate = false;
            }

            if (direct == null) return FromGeometry(geometry);

            direct.VerticesNeedUpdate = geometry.VerticesNeedUpdate;
            direct.NormalsNeedUpdate = geometry.NormalsNeedUpdate;
            direct.ColorsNeedUpdate = geometry.ColorsNeedUpdate;
            direct.UvsNeedUpdate = geometry.UvsNeedUpdate;
            direct.GroupsNeedUpdate = geometry.GroupsNeedUpdate;

            geometry.VerticesNeedUpdate = false;
            geometry.NormalsNeedUpdate = false;
            geometry.ColorsNeedUpdate = false;
            geometry.UvsNeedUpdate = false;
            geometry.GroupsNeedUpdate = false;

            geometry = direct;
        }

        object attribute = null;

        if (geometry.VerticesNeedUpdate)
            if (Attributes.TryGetValue("position", out attribute))
            {
                if (attribute != null)
                {
                    (attribute as BufferAttribute<float>).CopyVector3sArray(geometry.Vertices.ToArray());
                    (attribute as BufferAttribute<float>).NeedsUpdate = true;
                }

                geometry.VerticesNeedUpdate = false;
            }

        if (geometry.NormalsNeedUpdate)
            if (Attributes.TryGetValue("normal", out attribute))
            {
                if (attribute != null)
                {
                    (attribute as BufferAttribute<float>).CopyVector3sArray(geometry.Normals.ToArray());
                    (attribute as BufferAttribute<float>).NeedsUpdate = true;
                }

                geometry.NormalsNeedUpdate = false;
            }

        if (geometry.ColorsNeedUpdate)
            if (Attributes.TryGetValue("color", out attribute))
            {
                if (attribute != null)
                {
                    (attribute as BufferAttribute<float>).CopyColorsArray(geometry.Colors.ToArray());
                    (attribute as BufferAttribute<float>).NeedsUpdate = true;
                }

                geometry.ColorsNeedUpdate = false;
            }

        if (geometry.UvsNeedUpdate)
            if (Attributes.TryGetValue("uv", out attribute))
            {
                if (attribute != null)
                {
                    (attribute as BufferAttribute<float>).CopyVector2sArray(geometry.Uvs.ToArray());
                    (attribute as BufferAttribute<float>).NeedsUpdate = true;
                }

                geometry.UvsNeedUpdate = false;
            }

        if (geometry.LineDistancesNeedUpdate)
            if (Attributes.TryGetValue("lineDistance", out attribute))
            {
                if (attribute != null)
                {
                    (attribute as BufferAttribute<float>).CopyArray(geometry.LineDistances.ToArray());
                    (attribute as BufferAttribute<float>).NeedsUpdate = true;
                }

                geometry.LineDistancesNeedUpdate = false;
            }

        if (geometry.GroupsNeedUpdate)
        {
            var directGeometry = geometry as DirectGeometry;

            directGeometry.ComputeGroups(geometry);

            Groups = directGeometry.Groups;

            directGeometry.GroupsNeedUpdate = false;
        }

        return this;
    }

    public BufferGeometry FromGeometry(Geometry geometry)
    {
        geometry.DirectGeometry = new DirectGeometry().FromGeometry(geometry);

        return FromDirectGeometry(geometry.DirectGeometry);
    }

    public BufferGeometry FromDirectGeometry(DirectGeometry geometry)
    {
        var positions = new float[geometry.Vertices.Count * 3];

        SetAttribute("position",
            new BufferAttribute<float>(positions, 3).CopyVector3sArray(geometry.Vertices.ToArray()));

        if (geometry.Normals.Count > 0)
        {
            var normals = new float[geometry.Normals.Count * 3];
            SetAttribute("normal",
                new BufferAttribute<float>(normals, 3).CopyVector3sArray(geometry.Normals.ToArray()));
        }

        if (geometry.Colors.Count > 0)
        {
            var colors = new float[geometry.Colors.Count * 3];
            SetAttribute("color", new BufferAttribute<float>(colors, 3).CopyColorsArray(geometry.Colors.ToArray()));
        }

        if (geometry.Uvs.Count > 0)
        {
            var uvs = new float[geometry.Uvs.Count * 2];
            SetAttribute("uv", new BufferAttribute<float>(uvs, 2).CopyVector2sArray(geometry.Uvs.ToArray()));
        }

        if (geometry.Uvs2.Count > 0)
        {
            var uvs2 = new float[geometry.Uvs2.Count * 2];
            SetAttribute("uv2", new BufferAttribute<float>(uvs2, 2).CopyVector2sArray(geometry.Uvs2.ToArray()));
        }

        // groups
        Groups = geometry.Groups;

        // morphs

        foreach (string name in geometry.MorphTargets.Keys)
        {
            var array = new List<IBufferAttribute>();

            var morphTargets = (List<MorphTarget>)geometry.MorphTargets[name];

            for (var i = 0; i < morphTargets.Count; i++)
            {
                var morphTarget = morphTargets[i];
                var values = new float[morphTarget.Data.Count * 3];
                var attribute = new BufferAttribute<float>(values, 3);
                attribute.Name = morphTarget.Name;

                array.Add(attribute.CopyVector3sArray(morphTarget.Data.ToArray()));
            }

            MorphAttributes[name] = array;
        }

        //skinning

        if (geometry.SkinIndices.Count > 0)
        {
            var skinBuffer = new float[geometry.SkinIndices.Count * 4];
            var skinIndices = new BufferAttribute<float>(skinBuffer, 4);
            SetAttribute("skinIndex", skinIndices.CopyVector4sArray(geometry.SkinIndices.ToArray()));
        }

        if (geometry.SkinWeights.Count > 0)
        {
            var skinBuffer = new float[geometry.SkinWeights.Count * 4];
            var skinWeights = new BufferAttribute<float>(skinBuffer, 4);
            SetAttribute("skinWeight", skinWeights.CopyVector4sArray(geometry.SkinWeights.ToArray()));
        }

        if (geometry.BoundingSphere != null) BoundingSphere = (Sphere)geometry.BoundingSphere.Clone();

        if (geometry.BoundingBox != null) BoundingBox = (Box3)geometry.BoundingBox.Clone();
        return this;
    }

    public override void ComputeBoundingBox()
    {
        if (BoundingBox == null) BoundingBox = new Box3();

        //var position = this.Attributes["position"];
        //var morphAttributesPosition = this.MorphAttributes["position"] as List<BufferAttribute<float>>;

        BufferAttribute<float> position = null;

        if (Attributes.ContainsKey("position"))
            position = (BufferAttribute<float>)Attributes["position"];

        List<IBufferAttribute> morphAttributesPosition = null;
        if (MorphAttributes.ContainsKey("position"))
            morphAttributesPosition = MorphAttributes["position"] as List<IBufferAttribute>;

        if (position != null)
        {
            BoundingBox.SetFromBufferAttribute(position);

            // process morph attributes if present

            if (morphAttributesPosition != null)
                for (var i = 0; i < morphAttributesPosition.Count; i++)
                {
                    var morphAttribute = morphAttributesPosition[i];
                    _box.SetFromBufferAttribute(morphAttribute);

                    if (MorphTargetsRelative)
                    {
                        _vector.AddVectors(BoundingBox.Min, _box.Min);
                        BoundingBox.ExpandByPoint(_vector);

                        _vector.AddVectors(BoundingBox.Max, _box.Max);
                        BoundingBox.ExpandByPoint(_vector);
                    }
                    else
                    {
                        BoundingBox.ExpandByPoint(_box.Min);
                        BoundingBox.ExpandByPoint(_box.Max);
                    }
                }
        }
        else
        {
            BoundingBox.MakeEmpty();
        }

        if (float.IsNaN(BoundingBox.Min.X) || float.IsNaN(BoundingBox.Min.Y) || float.IsNaN(BoundingBox.Min.Z))
            Trace.TraceError(
                "THREE.Core.BufferGeometry.ComputeBoundingBox : Compute min/max have Nan values. The \"Position\" attribute is likely to have NaN values.");
    }

    public override void ComputeBoundingSphere()
    {
        if (BoundingSphere == null) BoundingSphere = new Sphere();

        BufferAttribute<float> position = null;

        if (Attributes.ContainsKey("position") && Attributes["position"] is GLBufferAttribute)
        {
            BoundingSphere.Set(new Vector3(), float.PositiveInfinity);
            return;
        }

        if (Attributes.ContainsKey("position"))
            position = (BufferAttribute<float>)Attributes["position"];

        List<IBufferAttribute> morphAttributesPosition = null;
        if (MorphAttributes.ContainsKey("position"))
            morphAttributesPosition = MorphAttributes["position"] as List<IBufferAttribute>;

        if (position != null)
        {
            var center = BoundingSphere.Center;

            if (position is InterleavedBufferAttribute<float>)
                _box.SetFromBufferAttribute(position as InterleavedBufferAttribute<float>);
            else
                _box.SetFromBufferAttribute(position);

            if (morphAttributesPosition != null)
                for (var i = 0; i < morphAttributesPosition.Count; i++)
                {
                    var morphAttribute = morphAttributesPosition[i];

                    _boxMorphTargets.SetFromBufferAttribute(morphAttribute);

                    if (MorphTargetsRelative)
                    {
                        _vector.AddVectors(_box.Min, _boxMorphTargets.Min);
                        _box.ExpandByPoint(_vector);

                        _vector.AddVectors(_box.Max, _boxMorphTargets.Max);
                        _box.ExpandByPoint(_vector);
                    }
                    else
                    {
                        _box.ExpandByPoint(_boxMorphTargets.Min);
                        _box.ExpandByPoint(_boxMorphTargets.Max);
                    }
                }

            center = _box.GetCenter(center);
            BoundingSphere.Center = center;

            // second, try to find a boundingSphere with a radius smaller than the
            // boundingSphere of the boundingBox: sqrt(3) smaller in the best case

            float maxRadiusSq = 0;

            for (var i = 0;
                 i < (position is InterleavedBufferAttribute<float>
                     ? (position as InterleavedBufferAttribute<float>).count
                     : position.count);
                 i++)
            {
                if (position is InterleavedBufferAttribute<float>)
                    _vector = _vector.FromBufferAttribute(position as InterleavedBufferAttribute<float>, i);
                else
                    _vector = _vector.FromBufferAttribute(position, i);
                maxRadiusSq = Math.Max(maxRadiusSq, center.DistanceToSquared(_vector));
            }

            // process morph attributes if present
            if (morphAttributesPosition != null)
                for (var i = 0; i < morphAttributesPosition.Count; i++)
                {
                    var morphAttribute = morphAttributesPosition[i];

                    for (var j = 0; j < morphAttribute.count; j++)
                    {
                        _vector = _vector.FromBufferAttribute(morphAttribute, j);
                        maxRadiusSq = Math.Max(maxRadiusSq, center.DistanceToSquared(_vector));
                    }
                }

            BoundingSphere.Radius = (float)Math.Sqrt(maxRadiusSq);

            if (float.IsNaN(BoundingSphere.Radius))
                Trace.TraceError(
                    "THREE.Core.BufferGeometry.ComputeBoundingSphere():Computed radius is Nan. The 'Position' attribute is likely to hava Nan values.");
        }
    }

    public new void ComputeVertexNormals(bool? areaWeighted = null)
    {
        var index = Index;
        var attributes = Attributes;

        var positionsAttribute = (BufferAttribute<float>)Attributes["position"];
        if (positionsAttribute != null)
        {
            BufferAttribute<float> normalAttribute = null;
            if (Attributes.ContainsKey("normal"))
                normalAttribute = (BufferAttribute<float>)Attributes["normal"];


            if (normalAttribute == null)
            {
                normalAttribute = new BufferAttribute<float>(new float[positionsAttribute.count * 3], 3);
                SetAttribute("normal", normalAttribute);
            }
            else
            {
                for (var i = 0; i < normalAttribute.count; i++) normalAttribute.SetXYZ(i, 0, 0, 0);
            }

            var normals = ((BufferAttribute<float>)attributes["normal"]).Array;

            var pA = new Vector3();
            var pB = new Vector3();
            var pC = new Vector3();
            var cb = new Vector3();
            var ab = new Vector3();
            var nA = new Vector3();
            var nB = new Vector3();
            var nC = new Vector3();


            if (index != null)
            {
                var indices = index.Array;

                for (var i = 0; i < index.count; i += 3)
                {
                    var vA = index.GetX(i + 0);
                    var vB = index.GetX(i + 1);
                    var vC = index.GetX(i + 2);

                    pA.FromBufferAttribute(positionsAttribute, vA);
                    pB.FromBufferAttribute(positionsAttribute, vB);
                    pC.FromBufferAttribute(positionsAttribute, vC);

                    cb.SubVectors(pC, pB);
                    ab.SubVectors(pA, pB);

                    cb.Cross(ab);

                    nA.FromBufferAttribute(normalAttribute, vA);
                    nB.FromBufferAttribute(normalAttribute, vB);
                    nC.FromBufferAttribute(normalAttribute, vC);

                    nA.Add(cb);
                    nB.Add(cb);
                    nC.Add(cb);

                    normalAttribute.SetXYZ(vA, nA.X, nA.Y, nA.Z);
                    normalAttribute.SetXYZ(vB, nB.X, nB.Y, nB.Z);
                    normalAttribute.SetXYZ(vC, nC.X, nC.Y, nC.Z);
                }
            }
            else
            {
                for (var i = 0; i < positionsAttribute.count; i += 3)
                {
                    pA.FromBufferAttribute(positionsAttribute, i + 0);
                    pB.FromBufferAttribute(positionsAttribute, i + 1);
                    pC.FromBufferAttribute(positionsAttribute, i + 2);

                    cb.SubVectors(pC, pB);
                    ab.SubVectors(pA, pB);
                    cb.Cross(ab);

                    normalAttribute.SetXYZ(i + 0, cb.X, cb.Y, cb.Z);
                    normalAttribute.SetXYZ(i + 1, cb.X, cb.Y, cb.Z);
                    normalAttribute.SetXYZ(i + 2, cb.X, cb.Y, cb.Z);
                }
            }

            NormalizeNormals();
            normalAttribute.NeedsUpdate = true;
        }
    }

    public BufferGeometry Merge(BufferGeometry geometry, int offset)
    {
        if (offset == 0)
            Trace.TraceWarning(
                "THREE.Core.BufferGeometry.Merge():Overwriting original geometry, starting at offset=0. Use BufferGeomeryUtils.mergeBufferGeometries() for lossless merge.");

        var attributes = Attributes;

        foreach (string key in attributes.Keys)
        {
            if (geometry.Attributes[key] == null) continue;

            var attribute1 = (BufferAttribute<float>)attributes[key];
            var attributeArray1 = attribute1.Array;

            var attribute2 = (BufferAttribute<float>)geometry.Attributes[key];
            var attributeArray2 = attribute2.Array;

            var attributeOffset = attribute2.ItemSize * offset;
            var length = Math.Min(attributeArray2.Length, attributeArray1.Length - attributeOffset);

            for (int i = 0, j = attributeOffset; i < length; i++, j++) attributeArray1[j] = attributeArray2[i];
        }

        return this;
    }

    public void NormalizeNormals()
    {
        var normals = (BufferAttribute<float>)Attributes["normal"];

        for (var i = 0; i < normals.count; i++)
        {
            _vector.FromBufferAttribute(normals, i);

            _vector.Normalize();

            normals.SetXYZ(i, _vector.X, _vector.Y, _vector.Z);
        }
    }

    public new void ComputeFaceNormals()
    {
        //backwards compatibility
    }

    public BufferAttribute<float> ConvertBufferAttribute(IBufferAttribute attribute, int[] indices)
    {
        var attr = attribute as BufferAttribute<float>;
        var array = attr.Array;
        var itemSize = attribute.ItemSize;

        var array2 = new float[indices.Length * itemSize];

        var index = 0;
        var index2 = 0;

        for (var i = 0; i < indices.Length; i++)
        {
            index = indices[i] * itemSize;
            for (var j = 0; j < itemSize; j++) array2[index2++] = array[index++];
        }

        return new BufferAttribute<float>(array2, itemSize);
    }

    public BufferGeometry ToNonIndexed()
    {
        if (Index == null)
        {
            Trace.TraceError("THREE.Core.BufferGeometry.ToNonIndexed:Geometry is already non-indexed.");
            return this;
        }

        var geometry2 = new BufferGeometry();

        var indices = Index.Array;
        var attributes = Attributes;

        foreach (string name in attributes.Keys)
        {
            var attribute = (BufferAttribute<float>)attributes[name];

            var newAttribute = ConvertBufferAttribute(attribute, indices);

            geometry2.SetAttribute(name, newAttribute);
        }

        var morphAttributes = MorphAttributes;

        foreach (string name in MorphAttributes.Keys)
        {
            List<IBufferAttribute> morphArray = new();
            List<IBufferAttribute> morphAttribute = morphAttributes[name] as List<IBufferAttribute>;

            for (var i = 0; i < morphAttribute.Count; i++)
            {
                var attribute = morphAttribute[i];
                var newAttribute = ConvertBufferAttribute(attribute, indices);
                morphArray.Add(newAttribute);
            }

            geometry2.MorphAttributes.Add(name, morphArray);
        }

        geometry2.MorphTargetsRelative = MorphTargetsRelative;

        var groups = Groups;

        for (var i = 0; i < groups.Count; i++)
        {
            var group = groups[i];
            geometry2.AddGroup((int)group.Start, (int)group.Count, group.MaterialIndex);
        }

        return geometry2;
    }
    //TODO
    // aplyMatrix,
}