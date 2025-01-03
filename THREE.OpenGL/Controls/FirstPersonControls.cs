using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace THREE;

[Serializable]
public class FirstPersonControls
{
    public bool ActiveLook = true;

    public bool AutoForward = false;

    public float AutoSpeedFactor;
    private Camera camera;

    public bool ConstrainVertical = false;

    public bool Enabled = true;
    private IControlsContainer glControl;

    public float HeightCoef = 1.0f;
    public float HeightMax = 1.0f;
    public float HeightMin = 0.0f;

    public bool HeightSpeed = false;

    public float lat;
    public float lon;

    public float LookSpeed = 0.005f;

    public bool LookVertical = true;
    private bool mouseDragOn;

    private int mouseX;
    private int mouseY;
    private bool moveBackward;
    private bool moveDown;

    private bool moveForward;
    private bool moveLeft;

    public float MovementSpeed = 1.0f;
    private bool moveRight;
    private bool moveUp;
    private float phi;

    private Rectangle screen;

    private Vector3 target = Vector3.Zero();
    private float theta;
    public float VerticalMax = (float)Math.PI;
    public float VerticalMin = 0;

    private int viewHalfX;
    private int viewHalfY;

    public FirstPersonControls(IControlsContainer control, Camera camera)
    {
        this.camera = camera;
        glControl = control;
        screen = control.ClientRectangle;

        control.MouseDown += Control_MouseDown;

        control.MouseMove += Control_MouseMove;

        control.MouseUp += Control_MouseUp;

        //control.MouseWheel += Control_MouseWheel;

        control.SizeChanged += Control_SizeChanged;

        control.KeyDown += Control_KeyDown;

        control.KeyUp += Control_KeyUp;
    }

    private void Control_KeyDown(object sender, KeyboardKeyEventArgs e)
    {
        switch (e.Key)
        {
            case Keys.Up:
            case Keys.W:
                moveForward = true;
                break;
            case Keys.Left:
            case Keys.A:
                moveLeft = true;
                break;
            case Keys.Down:
            case Keys.S:
                moveBackward = true;
                break;
            case Keys.Right:
            case Keys.D:
                moveRight = true;
                break;
            case Keys.R:
                moveUp = true;
                break;
            case Keys.F:
                moveDown = true;
                break;
        }
    }

    private void Control_KeyUp(object sender, KeyboardKeyEventArgs e)
    {
        switch (e.Key)
        {
            case Keys.Up:
            case Keys.W:
                moveForward = false;
                break;
            case Keys.Left:
            case Keys.A:
                moveLeft = false;
                break;
            case Keys.Down:
            case Keys.S:
                moveBackward = false;
                break;
            case Keys.Right:
            case Keys.D:
                moveRight = false;
                break;
            case Keys.R:
                moveUp = false;
                break;
            case Keys.F:
                moveDown = false;
                break;
        }
    }

    private void Control_MouseMove(object sender, MouseEventArgs e)
    {
        mouseX = e.X - viewHalfX;
        mouseY = e.Y - viewHalfY;
    }

    private void Control_MouseUp(object sender, MouseEventArgs e)
    {
        if (ActiveLook)
            switch (e.Button)
            {
                case MouseButton.Left:
                    moveForward = true;
                    break;
                case MouseButton.Right:
                    moveBackward = true;
                    break;
            }

        mouseDragOn = false;
    }

    private void Control_MouseDown(object sender, MouseEventArgs e)
    {
        if (ActiveLook)
            switch (e.Button)
            {
                case MouseButton.Left:
                    moveForward = true;
                    break;
                case MouseButton.Right:
                    moveBackward = true;
                    break;
            }

        mouseDragOn = true;
    }

    private void Control_SizeChanged(object sender, ResizeEventArgs e)
    {
        HandleResize();
    }

    public void HandleResize()
    {
        screen = glControl.ClientRectangle;
        viewHalfX = screen.Width / 2;
        viewHalfY = screen.Height / 2;
    }

    public void Update(float delta)
    {
        if (Enabled == false) return;

        if (HeightSpeed)
        {
            var y = camera.Position.Y.Clamp(HeightMin, HeightMax);
            var heightDelta = y - HeightMin;
            AutoSpeedFactor = delta * (heightDelta * HeightCoef);
        }
        else
        {
            AutoSpeedFactor = 0.0f;
        }

        var actualMoveSpeed = delta * MovementSpeed;

        if (moveForward || (AutoForward && !moveBackward))
            camera.Position.Z = camera.Position.Z - (actualMoveSpeed + AutoSpeedFactor);

        if (moveBackward)
            camera.Position.Z = camera.Position.Z + actualMoveSpeed;

        if (moveLeft)
            camera.Position.X = camera.Position.X - actualMoveSpeed;

        if (moveRight)
            camera.Position.X = camera.Position.X + actualMoveSpeed;

        if (moveUp)
            camera.Position.Y = camera.Position.Y + actualMoveSpeed;

        if (moveDown)
            camera.Position.Y = camera.Position.Y - actualMoveSpeed;


        var actualLookSpeed = delta * LookSpeed;

        if (!ActiveLook) actualMoveSpeed = 0;

        float verticalLookRatio = 1;

        if (ConstrainVertical) verticalLookRatio = (float)Math.PI / (VerticalMax - VerticalMin);

        lon += mouseX * actualLookSpeed;
        if (LookVertical)
            lat -= mouseY * actualLookSpeed * verticalLookRatio;

        lat = Math.Max(-85, Math.Min(85, lat));
        phi = MathUtils.DegToRad(90 - lat);
        theta = MathUtils.DegToRad(lon);

        if (ConstrainVertical) phi = MathUtils.mapLinear(phi, 0, Math.PI, VerticalMin, VerticalMax);

        var targetPosition = target;
        var position = camera.Position;

        targetPosition.X = position.X + (float)(100 * Math.Sin(phi) * Math.Cos(theta));
        targetPosition.Y = position.Y + (float)(100 * Math.Cos(phi));
        targetPosition.Z = position.Z + (float)(100 * Math.Sin(phi) * Math.Sin(theta));

        camera.LookAt(targetPosition);
    }
}