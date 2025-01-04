using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class Object3D : BasicObject, ICloneable
{
    public static Vector3 DefaultUp = new(0, 1, 0);

    public static bool DefaultMatrixAutoUpdate = true;

    protected static int Object3DIdCount;

    public bool bVisible = false;

    public bool CastShadow;

    public Material CustomDepthMaterial;

    public Material CustomDistanceMaterial;

    public Vector3 Front = new(0.0f, 0.0f, -1.0f);

    public bool FrustumCulled = true;

    public Geometry Geometry;

    public bool glActive = false;

    public bool glInit = false;

    public bool IsObject3D = true;

    public Layers Layers = new();

    public Material Material;

    public List<Material> Materials = new();

    public Matrix4 Matrix = Matrix4.Identity();

    public bool MatrixAutoUpdate = true;

    public Matrix4 MatrixWorld = Matrix4.Identity();

    public bool MatrixWorldNeedsUpdate;

    public Matrix4 ModelViewMatrix = Matrix4.Identity();

    public Hashtable MorphTargetDictionary = new();

    public List<float> MorphTargetInfluences = new() { 0, 0, 0, 0, 0, 0, 0, 0 };

    public string Name = "";

    public Matrix3 NormalMatrix = new();

    public Vector3 Position = Vector3.Zero();

    public Quaternion Quaternion = Quaternion.Identity();

    public bool ReceiveShadow;

    public int RenderDepth = -1;

    public int RenderOrder;

    public Vector3 Right = Vector3.Zero();

    public Euler Rotation = new();

    public Vector3 Scale = Vector3.One();

    public string type = "Object3D";

    public Vector3 Up = new(0.0f, 1.0f, 0.0f);

    public Dictionary<string, object> UserData = new();

    public bool Visible = true;

    public Object3D()
    {
        Up = new Vector3(0, 1, 0);
        Rotation.PropertyChanged += OnRotationChanged;
        Quaternion.PropertyChanged += OnQuaternionChanged;
    }

    public Object3D(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    protected Object3D(Object3D source, bool recursive = true) : this()
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

    public bool IsGroup
    {
        get
        {
            if (Geometry != null && Geometry.IsBufferGeometry &&
                (Geometry as BufferGeometry).Attributes.Count == 0) return true;
            return false;
        }
    }

    public override object Clone()
    {
        var hashTable = base.Clone() as Hashtable;
        var cloned = new Object3D(this);

        foreach (DictionaryEntry item in hashTable) cloned.Add(item.Key, item.Value);
        return cloned;
    }

    private void OnRotationChanged(object sender, PropertyChangedEventArgs e)
    {
        Quaternion.SetFromEuler(sender as Euler, false);
    }

    private void OnQuaternionChanged(object sender, PropertyChangedEventArgs e)
    {
        Rotation.SetFromQuaternion(sender as Quaternion, null, false);
    }


    public void ApplyMatrix4(Matrix4 matrix)
    {
        if (MatrixAutoUpdate) UpdateMatrix();

        Matrix.PreMultiply(matrix);

        Matrix.Decompose(Position, Quaternion, Scale);
    }


    public Object3D ApplyQuaternion(Quaternion q)
    {
        Quaternion.PreMultiply(q);

        return this;
    }

    public void SetRotationFromAxisAngle(Vector3 axis, float angle)
    {
        Quaternion.SetFromAxisAngle(axis, angle);
    }

    public void SetRotationFromEuler(Euler euler)
    {
        Quaternion.SetFromEuler(euler);
    }

    public void SetRotationFromMatrix(Matrix4 m)
    {
        Quaternion.SetFromRotationMatrix(m);
    }

    public void SetRotationFromQuaternion(Quaternion q)
    {
        Quaternion.Copy(q);
    }

    public Object3D RotateOnAxis(Vector3 axis, float angle)
    {
        _q1.SetFromAxisAngle(axis, angle);

        Quaternion.Multiply(_q1);

        return this;
    }

    public Object3D RotateOnWorldAxis(Vector3 axis, float angle)
    {
        _q1.SetFromAxisAngle(axis, angle);

        Quaternion.PreMultiply(_q1);

        return this;
    }

    public Object3D RotateX(float angle)
    {
        return RotateOnAxis(_xAxis, angle);
    }

    public Object3D RotateY(float angle)
    {
        return RotateOnAxis(_yAxis, angle);
    }

    public Object3D RotateZ(float angle)
    {
        return RotateOnAxis(_zAxis, angle);
    }

    public Object3D TranslateOnAxis(Vector3 axis, float distance)
    {
        _v1.Copy(axis).ApplyQuaternion(Quaternion);

        Position.Add(_v1.MultiplyScalar(distance));

        return this;
    }

    public Object3D TranslateX(float distance)
    {
        return TranslateOnAxis(_xAxis, distance);
    }

    public Object3D TranslateY(float distance)
    {
        return TranslateOnAxis(_yAxis, distance);
    }

    public Object3D TranslateZ(float distance)
    {
        return TranslateOnAxis(_zAxis, distance);
    }

    public Vector3 LocalToWorld(Vector3 vector)
    {
        return vector.ApplyMatrix4(MatrixWorld);
    }

    public Vector3 WorldToLocal(Vector3 vector)
    {
        return vector.ApplyMatrix4(_m1.GetInverse(MatrixWorld));
    }


    public virtual void LookAt(float x, float y, float z)
    {
        var target = new Vector3(x, y, z);

        LookAt(target);
    }

    public virtual void LookAt(Vector3 target)
    {
        UpdateWorldMatrix(true, false);

        _position.SetFromMatrixPosition(MatrixWorld);

        var m = Matrix4.Identity();

        if (this is Camera || this is Light)
            m = m.LookAt(_position, target, Up);
        else
            m = m.LookAt(target, _position, Up);


        Quaternion.SetFromRotationMatrix(m);

        if (Parent != null)
        {
            m.ExtractRotation(Parent.MatrixWorld);
            var q1 = new Quaternion().SetFromRotationMatrix(m);
            Quaternion.PreMultiply(q1.Invert());
        }
    }


    public Object3D Add(Object3D object3D)
    {
        if (object3D == this)
        {
            Trace.TraceError("THREE.Core.Object3D.Add:", object3D, "can't be added as a child of itself");
            return this;
        }

        // Only allow one instance of the child in this object
        if (Children.Contains(object3D)) return this;

        // The Object3D can only be added to one scene at a time
        object3D.Parent?.Remove(object3D);
        object3D.Parent = this;

        Children.Add(object3D);

        return this;
    }

    public Object3D Remove(Object3D object3D)
    {
        var index = Children.IndexOf(object3D);

        if (index != -1)
        {
            object3D.Parent = null;

            Children.RemoveAt(index);

            //var scene = this;

            //while (scene.Parent != null)
            //{
            //    scene = scene.Parent;
            //}
        }

        return this;
    }

    public virtual Object3D Attach(Object3D object3D)
    {
        UpdateWorldMatrix(true, false);

        _m1.GetInverse(MatrixWorld);

        if (object3D.Parent != null)
        {
            object3D.Parent.UpdateWorldMatrix(true, false);

            _m1.Multiply(object3D.Parent.MatrixWorld);
        }

        object3D.ApplyMatrix4(_m1);

        object3D.UpdateWorldMatrix(false, false);

        Add(object3D);

        return this;
    }


    public Object3D GetObjectById(int id)
    {
        if (Id == id) return this;

        for (var i = 0; i < Children.Count; i++)
        {
            var child = Children[i];
            var object3D = child.GetObjectById(id);
            if (object3D != null)
                return object3D;
        }

        return null;
    }

    public Object3D GetObjectByName(string name)
    {
        if (Name == name) return this;

        for (var i = 0; i < Children.Count; i++)
        {
            var child = Children[i];
            var object3D = child.GetObjectByName(name);
            if (object3D != null)
                return object3D;
        }

        return null;
    }

    private Object3D GetObjectByProperty<T>(string p, T id)
    {
        throw new NotImplementedException();
    }

    public virtual Vector3 GetWorldPosition(Vector3 target)
    {
        var result = new Vector3();

        if (target == null)
            target = result;

        UpdateWorldMatrix(true, false);

        return target.SetFromMatrixPosition(MatrixWorld);
    }

    public virtual Quaternion GetWorldQuaternion(Quaternion target)
    {
        UpdateWorldMatrix(true, false);

        MatrixWorld.Decompose(_position, target, _scale);

        return target;
    }

    public Vector3 GetWorldScale(Vector3 target = null)
    {
        if (target == null) target = new Vector3();

        UpdateWorldMatrix(true, false);

        MatrixWorld.Decompose(_position, _quaternion, target);

        return target;
    }

    public virtual Vector3 GetWorldDirection(Vector3 target)
    {
        UpdateWorldMatrix(true, false);

        var e = MatrixWorld.Elements;

        return target.Set(e[8], e[9], e[10]).Normalize();
    }

    public virtual void Raycast(Raycaster raycaster, List<Intersection> intersectionList)
    {
    }

    public void Traverse(Action<Object3D> callback)
    {
        callback(this);

        for (var i = 0; i < Children.Count; i++) Children[i].Traverse(callback);
    }

    public void TraverseVisible(Action<Object3D> callback)
    {
        if (Visible == false) return;

        callback(this);

        for (var i = 0; i < Children.Count; i++) Children[i].TraverseVisible(callback);
    }

    public void TraverseAncestors(Action<Object3D> callback)
    {
        var parent = Parent;

        if (parent != null)
        {
            callback(this);
            parent.TraverseAncestors(callback);
        }
    }

    public void UpdateMatrix()
    {
        Matrix.Compose(Position, Quaternion, Scale);

        MatrixWorldNeedsUpdate = true;
    }

    public virtual void UpdateMatrixWorld(bool force = false)
    {
        if (MatrixAutoUpdate)
            UpdateMatrix();

        if (MatrixWorldNeedsUpdate || force)
        {
            if (Parent == null)
                MatrixWorld.Copy(Matrix);
            else
                MatrixWorld.MultiplyMatrices(Parent.MatrixWorld, Matrix);

            MatrixWorldNeedsUpdate = false;
            force = true;
        }

        for (var i = 0; i < Children.Count; i++) Children[i].UpdateMatrixWorld(force);
    }

    public virtual void UpdateWorldMatrix(bool updateParents, bool updateChildren)
    {
        if (updateParents && Parent != null)
            Parent.UpdateWorldMatrix(true, false);

        if (MatrixAutoUpdate) UpdateMatrix();

        if (Parent == null)
            MatrixWorld.Copy(Matrix);
        else
            MatrixWorld.MultiplyMatrices(Parent.MatrixWorld, Matrix);

        if (updateChildren)
            for (var i = 0; i < Children.Count; i++)
                Children[i].UpdateWorldMatrix(false, true);
    }

    public override void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #region Fields

    public List<Object3D> Children = new();

    public int Id = Object3DIdCount++;

    public Guid Uuid = Guid.NewGuid();

    public Object3D? Parent;

    public bool IsCamera = false;

    public bool IsLight = false;

    public object Tag = null;

    #endregion

    #region Public Events

    public event EventHandler<EventArgs> Added;

    public event EventHandler<EventArgs> Removed;

    #endregion

    #region public Action

    public Action<IGLRenderer, Object3D, Camera, Geometry, Material, DrawRange?, GLRenderTarget> OnBeforeRender;
    public Action<IGLRenderer, Object3D, Camera> OnAfterRender;

    #endregion

    #region private field

    private Vector3 _v1 = new();
    private Quaternion _q1 = new();
    private Matrix4 _m1 = new();
    private Vector3 _target = new();

    private Vector3 _position = new();
    private Vector3 _scale = new();
    private Quaternion _quaternion = new();

    private Vector3 _xAxis = new(1, 0, 0);
    private Vector3 _yAxis = new(0, 1, 0);
    private Vector3 _zAxis = new(0, 0, 1);

    #endregion
}