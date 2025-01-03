namespace THREE;

[Serializable]
public class OrthographicCamera : Camera, ICloneable
{
    #region Constructors and Destructors

    /// <summary>
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <param name="top"></param>
    /// <param name="bottom"></param>
    /// <param name="near"></param>
    /// <param name="far"></param>
    public OrthographicCamera(float? left = null, float? right = null, float? top = null, float? bottom = null,
        float? near = null, float? far = null)
    {
        type = "OrthographicCamera";

        Zoom = 1;

        Left = left != null ? (float)left : -1;
        CameraRight = right != null ? (float)right : 1;
        Top = top != null ? (float)top : 1;
        Bottom = bottom != null ? (float)bottom : -1;

        Near = near != null ? (float)near : 0.1f;
        Far = far != null ? (float)far : 2000;

        UpdateProjectionMatrix();
    }

    /// <summary>
    /// </summary>
    /// <param name="other"></param>
    protected OrthographicCamera(OrthographicCamera other)
        : base(other)
    {
        Zoom = other.Zoom;

        Left = other.Left;
        CameraRight = other.CameraRight;
        Top = other.Top;
        Bottom = other.Bottom;

        Near = other.Near;
        Far = other.Far;
    }

    #endregion

    #region Public Methods and Operators

    /// <summary>
    /// </summary>
    public override void UpdateProjectionMatrix()
    {
        var dx = (CameraRight - Left) / (2 * Zoom);
        var dy = (Top - Bottom) / (2 * Zoom);
        var cx = (CameraRight + Left) / 2;
        var cy = (Top + Bottom) / 2;

        var left = cx - dx;
        var right = cx + dx;
        var top = cy + dy;
        var bottom = cy - dy;

        if (View.Enabled)
        {
            //var zoomW = this.Zoom / ( this.View.Width / this.View.FullWidth );
            //var zoomH = this.Zoom / ( this.View.Height / this.View.FullHeight );

            var scaleW = (CameraRight - Left) / View.FullWidth / Zoom;
            var scaleH = (Top - Bottom) / View.FullHeight / Zoom;

            left += scaleW * View.OffsetX;
            right = left + scaleW * View.Width;
            top -= scaleH * View.OffsetY;
            bottom = top - scaleH * View.Height;
        }

        ProjectionMatrix = Matrix4.Identity().MakeOrthographic(left, right, top, bottom, Near, Far);

        ProjectionMatrixInverse.GetInverse(ProjectionMatrix);
    }

    public override object Clone()
    {
        return new OrthographicCamera(this);
    }

    #endregion
}