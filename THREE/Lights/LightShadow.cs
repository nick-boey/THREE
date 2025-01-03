namespace THREE;

[Serializable]
public class LightShadow : ICloneable
{
    public Vector2 _frameExtents = new(1, 1);

    public Frustum _frustum = new();

    public Vector3 _lightPositionWorld = Vector3.Zero();

    public Vector3 _lookTarget = Vector3.Zero();

    public Matrix4 _projScreenMatrix = Matrix4.Identity();

    public int _viewportCount = 1;

    public List<Vector4> _viewports = new();

    public bool AutoUpdate = true;

    public float Bias;
    public Camera Camera;

    public float Focus = 1;

    public GLRenderTarget Map;

    public GLRenderTarget MapPass;

    public Vector2 MapSize;

    public Matrix4 Matrix = Matrix4.Identity();

    public bool NeedsUpdate = false;

    public float NormalBias;

    public float Radius;

    public LightShadow(Camera camera)
    {
        Camera = camera;

        Bias = 0;

        NormalBias = 0;

        Radius = 1;

        MapSize = new Vector2(512, 512);

        Map = null;

        MapPass = null;

        _viewports.Add(new Vector4(0, 0, 1, 1));
    }

    protected LightShadow(LightShadow other)
    {
        Camera = (Camera)other.Camera.Clone();

        Bias = other.Bias;

        Radius = other.Radius;

        MapSize = other.MapSize;
    }

    public object Clone()
    {
        return new LightShadow(this);
    }

    public int GetViewportCount()
    {
        return _viewportCount;
    }

    public Frustum GetFrustum()
    {
        return _frustum;
    }

    public virtual void UpdateMatrices(Light light)
    {
        var shadowCamera = Camera;
        var shadowMatrix = Matrix;
        var projScreenMatrix = _projScreenMatrix;
        var lookTarget = _lookTarget;
        var lightPositionWorld = _lightPositionWorld;

        lightPositionWorld.SetFromMatrixPosition(light.MatrixWorld);
        shadowCamera.Position.Copy(lightPositionWorld);

        lookTarget.SetFromMatrixPosition(light.Target.MatrixWorld);
        shadowCamera.LookAt(lookTarget);
        shadowCamera.UpdateMatrixWorld();

        projScreenMatrix.MultiplyMatrices(shadowCamera.ProjectionMatrix, shadowCamera.MatrixWorldInverse);
        _frustum.SetFromProjectionMatrix(projScreenMatrix);

        shadowMatrix.Set(
            0.5f, 0.0f, 0.0f, 0.5f,
            0.0f, 0.5f, 0.0f, 0.5f,
            0.0f, 0.0f, 0.5f, 0.5f,
            0.0f, 0.0f, 0.0f, 1.0f
        );

        shadowMatrix.Multiply(shadowCamera.ProjectionMatrix);
        shadowMatrix.Multiply(shadowCamera.MatrixWorldInverse);
    }

    public Vector4 GetViewport(int viewportIndex)
    {
        return _viewports[viewportIndex];
    }

    public Vector2 GetFrameExtents()
    {
        return _frameExtents;
    }
}