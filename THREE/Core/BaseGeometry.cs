namespace THREE;

[Serializable]
public abstract class BaseGeometry : IDisposable
{
    private bool _disposed;

    public Box3 BoundingBox = null;

    public Sphere BoundingSphere = null;

    public int Id;

    public string Name;

    public string type;
    public Guid Uuid = Guid.NewGuid();

    public abstract void ComputeBoundingSphere();

    public abstract void ComputeBoundingBox();

    public abstract void ComputeVertexNormals(bool areaWeighted = false);

    public event EventHandler<EventArgs> Disposed;

    protected virtual void RaiseDisposed()
    {
        var handler = Disposed;
        if (handler != null) handler(this, new EventArgs());
    }

    #region dynamic attribute

    public bool glInit = false;

    public int glLineDistanceBuffer = 0;

    public int glVertexBuffer = 0;

    public int glNormalBuffer = 0;

    public int glTangentBuffer = 0;

    public int glColorBuffer = 0;

    public int glUVBuffer = 0;

    public int glUV2Buffer = 0;

    public int glSkinIndicesBuffer = 0;

    public int glSkinWeightsBuffer = 0;

    public int glFaceBuffer = 0;

    public int glLineBuffer = 0;

    public List<int> glMorphTargetsBuffers;

    public List<int> glMorphNormalsBuffers;

    public object sortArray;

    public float[] vertexArray;

    public float[] normalArray;

    public float[] tangentArray;

    public float[] colorArray;

    public float[] uvArray;

    public float[] uv2Array;

    public float[] skinIndexArray;

    public float[] skinWeightArray;

    public Type typeArray;

    public ushort[] faceArray;

    public ushort[] lineArray;

    public List<float[]> morphTargetsArrays;

    public List<float[]> morphNormalsArrays;

    public int glFaceCount = -1;

    public int glLineCount = -1;

    public int glParticleCount = -1;

    public List<IGLAttribute> glCustomAttributesList;

    public bool initttedArrays;

    public float[] lineDistanceArray;

    #endregion

    #region IDisposable Members

    /// <summary>
    ///     Implement the IDisposable interface
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
        if (!_disposed)
        {
            _disposed = true;

            RaiseDisposed();
        }
    }

    #endregion
}