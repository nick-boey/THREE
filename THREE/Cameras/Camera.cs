using System.Collections;

namespace THREE;

[Serializable]
public struct View
{
    public bool Enabled;

    public float FullWidth;

    public float FullHeight;

    public float OffsetX;

    public float OffsetY;

    public float Width;

    public float Height;
}

[Serializable]
public class Camera : Object3D
{
    public float Aspect = 1.0f;

    public float Bottom;

    public float CameraRight;
    public float Far = 2000.0f;

    public float Fov;

    public float FullHeight = -1;

    public float FullWidth = -1;

    public float Height = -1;

    public float Left;

    public Matrix4 MatrixWorldInverse = Matrix4.Identity();
    public float Near = 0.1f;

    public bool NeedsUpdate = false;

    public Matrix4 ProjectionMatrix = Matrix4.Identity();

    public Matrix4 ProjectionMatrixInverse = Matrix4.Identity();

    public float Top;
    public View View;

    public Vector4 Viewport;

    public float Width = -1;

    public float X = -1;

    public float Y = -1;

    public float Zoom = 1;

    public Camera()
    {
        IsCamera = true;
        type = "Camera";

        View = new View
        {
            Enabled = false,
            FullWidth = 1,
            FullHeight = 1,
            OffsetX = 0,
            OffsetY = 0,
            Width = 1,
            Height = 1
        };
    }

    protected Camera(Camera source, bool recursive = true) : base(source, recursive)
    {
        IsCamera = true;
        type = "Camera";

        MatrixWorldInverse.Copy(source.MatrixWorldInverse);

        ProjectionMatrix.Copy(source.ProjectionMatrix);

        ProjectionMatrixInverse.Copy(source.ProjectionMatrixInverse);

        View = source.View;
    }

    public override Vector3 GetWorldDirection(Vector3 target)
    {
        UpdateMatrixWorld(true);

        var e = MatrixWorld.Elements;

        return target.Set(-e[8], -e[9], -e[10]).Normalize();
    }

    public override void UpdateMatrixWorld(bool force = false)
    {
        base.UpdateMatrixWorld(force);

        MatrixWorldInverse.GetInverse(MatrixWorld);
    }

    public override void UpdateWorldMatrix(bool updateParents, bool updateChildren)
    {
        base.UpdateWorldMatrix(updateParents, updateChildren);

        MatrixWorldInverse.GetInverse(MatrixWorld);
    }

    public void SetViewOffset(int fullWidth, int fullHeight, int x, int y, int width, int height)
    {
        View.Enabled = true;
        View.FullWidth = fullWidth;
        View.FullHeight = fullHeight;
        View.OffsetX = x;
        View.OffsetY = y;
        View.Width = width;
        View.Height = height;

        UpdateProjectionMatrix();
    }

    public void ClearViewOffset()
    {
        View.Enabled = false;

        UpdateProjectionMatrix();
    }

    public virtual void UpdateProjectionMatrix()
    {
        //this.MatrixWorldInverse.GetInverse(this.MatrixWorld);
        UpdateWorldMatrix(false, true);
    }

    public override object Clone()
    {
        var object3D = base.Clone() as Object3D;

        var cloned = new Camera(this);

        foreach (DictionaryEntry item in object3D) cloned.Add(item.Key, item.Value);

        return cloned;
    }
}