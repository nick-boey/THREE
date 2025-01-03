namespace THREE;

[Serializable]
public class PointLightShadow : LightShadow
{
    private List<Vector3> _cubeDirections = new()
    {
        new Vector3(1, 0, 0),
        new Vector3(-1, 0, 0),
        new Vector3(0, 0, 1),
        new Vector3(0, 0, -1),
        new Vector3(0, 1, 0),
        new Vector3(0, -1, 0)
    };

    private List<Vector3> _cubeUps = new()
    {
        new Vector3(0, 1, 0),
        new Vector3(0, 1, 0),
        new Vector3(0, 1, 0),
        new Vector3(0, 1, 0),
        new Vector3(0, 0, 1),
        new Vector3(0, 0, -1)
    };

    public PointLightShadow()
        : base(new PerspectiveCamera(90, 1, 0.5f, 500))
    {
        _frameExtents = new Vector2(4, 2);
        _viewportCount = 6;

        _viewports = new List<Vector4>
        {
            new(2, 1, 1, 1),
            new(0, 1, 1, 1),
            new(3, 1, 1, 1),
            new(1, 1, 1, 1),
            new(3, 0, 1, 1),
            new(1, 0, 1, 1)
        };
    }

    public void UpdateMatrices(Light light, int? _viewportIndex = null)
    {
        var viewportIndex = 0;
        if (_viewportIndex == null) viewportIndex = 0;
        else viewportIndex = (int)_viewportIndex;

        var camera = Camera;
        var shadowMatrix = Matrix;
        var lightPositionWorld = _lightPositionWorld;
        var lookTarget = _lookTarget;
        var projScreenMatrix = _projScreenMatrix;

        lightPositionWorld.SetFromMatrixPosition(light.MatrixWorld);
        camera.Position.Copy(lightPositionWorld);

        lookTarget.Copy(camera.Position);
        lookTarget.Add(_cubeDirections[viewportIndex]);
        camera.Up.Copy(_cubeUps[viewportIndex]);
        camera.LookAt(lookTarget);
        camera.UpdateMatrixWorld();

        shadowMatrix.MakeTranslation(-lightPositionWorld.X, -lightPositionWorld.Y, -lightPositionWorld.Z);

        projScreenMatrix.MultiplyMatrices(camera.ProjectionMatrix, camera.MatrixWorldInverse);
        _frustum.SetFromProjectionMatrix(projScreenMatrix);
    }
}