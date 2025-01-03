namespace THREE;

[Serializable]
public class PerspectiveCamera : Camera
{
    public int FilmGauge = 35;

    public int FilmOffset;

    public float focus = 10.0f;


    public PerspectiveCamera(float fov = 50, float aspect = 1, float near = 0.1f, float far = 2000)
    {
        Fov = fov;
        Aspect = aspect;
        Near = near;
        Far = far;

        UpdateProjectionMatrix();

        //View.Enabled = true;
        //View.FullWidth = 1;
        //View.FullHeight = 1;
        //View.OffsetX = 0;
        //View.OffsetY = 0;
        //View.Width = 1;
        //View.Height = 1;
    }

    protected PerspectiveCamera(PerspectiveCamera other) : base(other)
    {
        Fov = other.Fov;
        Aspect = other.Aspect;
        Near = other.Near;
        Far = other.Far;
        focus = other.focus;
        Zoom = other.Zoom;
        FilmGauge = other.FilmGauge;
        FilmOffset = other.FilmOffset;
        View = other.View;
        //this.UpdateProjectionMatrix();
    }

    public override void UpdateProjectionMatrix()
    {
        //base.UpdateProjectionMatrix();

        float near = Near,
            top = near * (float)Math.Tan(MathUtils.DEG2RAD * 0.5 * Fov) / Zoom,
            height = 2 * top,
            width = Aspect * height,
            left = -0.5f * width;

        if (View.Enabled)

        {
            left += View.OffsetX * width / View.FullWidth;
            top -= View.OffsetY * height / View.FullHeight;
            width *= View.Width / View.FullWidth;
            height *= View.Height / View.FullHeight;
        }

        var skew = FilmOffset;
        if (skew != 0) left += near * skew / GetFilmWidth();

        ProjectionMatrix = ProjectionMatrix.MakePerspective(left, left + width, top, top - height, near, Far);

        ProjectionMatrixInverse.GetInverse(ProjectionMatrix);
    }

    public void SetViewOffset(float fullWidth, float fullHeight, float x, float y, float width, float height)
    {
        Aspect = fullWidth / (1.0f * fullHeight);
        View.Enabled = true;
        View.FullWidth = fullWidth;
        View.FullHeight = fullHeight;
        View.OffsetX = x;
        View.OffsetY = y;
        View.Width = width;
        View.Height = height;

        UpdateProjectionMatrix();
    }

    public override object Clone()
    {
        return new PerspectiveCamera(this);
    }

    public void SetFocalLength(float focalLength)
    {
        var vExtentSlope = 0.5f * GetFilmHeight() / focalLength;
        Fov = MathUtils.RAD2DEG * 2 * (float)Math.Atan(vExtentSlope);

        UpdateProjectionMatrix();
    }

    public float GetFocalLength()
    {
        var vExtentSlope = (float)Math.Tan(MathUtils.DEG2RAD * 0.5f * Fov);

        return 0.5f * GetFilmHeight() / vExtentSlope;
    }

    public float GetEffectiveFOV()
    {
        return MathUtils.RAD2DEG * 2 * (float)Math.Atan(Math.Tan(MathUtils.DEG2RAD * 0.5 * Fov) / Zoom);
    }

    public float GetFilmWidth()
    {
        return FilmGauge * Math.Min(Aspect, 1);
    }

    public float GetFilmHeight()
    {
        return FilmGauge / Math.Max(Aspect, 1);
    }
}