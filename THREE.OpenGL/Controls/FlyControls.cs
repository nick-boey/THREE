﻿using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace THREE;

[Serializable]
public class FlyControls
{
    public bool AutoForward = false;
    public Camera camera;

    protected IControlsContainer control;

    public bool DragToLook = false;

    private float EPS = 0.000001f;

    private Vector3 lastPosition = Vector3.Zero();

    private Quaternion lastQuaternion = new();

    private int mouseStatus;

    public float MovementSpeed = 1.0f;

    private float movementSpeedMultiplier;

    private MoveState moveState = new()
    {
        Up = 0,
        Down = 0,
        Left = 0,
        Forward = 0,
        Back = 0,
        PitchUp = 0,
        PitchDown = 0,
        YawLeft = 0,
        YawRight = 0,
        RollLeft = 0,
        RollRight = 0
    };

    private Vector3 moveVector = Vector3.Zero();

    public float RollSpeed = 0.005f;

    private Vector3 rotationVector = Vector3.Zero();

    private Quaternion tmpQuaternion = new();

    public FlyControls(IControlsContainer control, Camera camera)
    {
        this.control = control;

        this.camera = camera;

        control.KeyDown += Control_KeyDown;

        control.KeyUp += Control_KeyUp;

        control.MouseDown += Control_MouseDown;

        control.MouseMove += Control_MouseMove;

        control.MouseUp += Control_MouseUp;
    }

    private void Control_MouseUp(object sender, MouseEventArgs e)
    {
        if (DragToLook)
        {
            mouseStatus--;

            moveState.YawLeft = moveState.PitchDown = 0;
        }
        else
        {
            switch (e.Button)
            {
                case MouseButton.Left: moveState.Forward = 0; break;
                case MouseButton.Right: moveState.Back = 0; break;
            }

            UpdateMovementVector();
        }

        UpdateRotationVector();
    }

    private void Control_MouseMove(object sender, MouseEventArgs e)
    {
        if (!DragToLook || mouseStatus > 0)
        {
            //var container = this.getContainerDimensions();
            var halfWidth = control.ClientRectangle.Width / 2;
            var halfHeight = control.ClientRectangle.Height / 2;

            moveState.YawLeft = -(e.X - halfWidth) / (float)halfWidth;
            moveState.PitchDown = (e.Y - halfHeight) / (float)halfHeight;

            UpdateRotationVector();
        }
    }

    private void Control_MouseDown(object sender, MouseEventArgs e)
    {
        if (DragToLook)
        {
            mouseStatus++;
        }
        else
        {
            switch (e.Button)
            {
                case MouseButton.Left: moveState.Forward = 1; break;
                case MouseButton.Right: moveState.Back = 1; break;
            }

            UpdateMovementVector();
        }
    }

    private void Control_KeyUp(object sender, KeyboardKeyEventArgs e)
    {
        switch (e.Key)
        {
            case Keys.RightShift:
            case Keys.LeftShift: /* shift */ movementSpeedMultiplier = 1.1f; break;

            case Keys.W: /*W*/ moveState.Forward = 0; break;
            case Keys.S: /*S*/ moveState.Back = 0; break;

            case Keys.A: /*A*/ moveState.Left = 0; break;
            case Keys.D: /*D*/ moveState.Right = 0; break;

            case Keys.R: /*R*/ moveState.Up = 0; break;
            case Keys.F: /*F*/ moveState.Down = 0; break;

            case Keys.Up: /*up*/ moveState.PitchUp = 0; break;
            case Keys.Down: /*down*/ moveState.PitchDown = 0; break;

            case Keys.Left: /*left*/ moveState.YawLeft = 0; break;
            case Keys.Right: /*right*/ moveState.YawRight = 0; break;

            case Keys.Q: /*Q*/ moveState.RollLeft = 0; break;
            case Keys.E: /*E*/ moveState.RollRight = 0; break;
        }

        UpdateMovementVector();
        UpdateRotationVector();
    }

    private void Control_KeyDown(object sender, KeyboardKeyEventArgs e)
    {
        if (e.Key == Keys.LeftAlt || e.Key == Keys.RightAlt)
            return;

        switch (e.Key)
        {
            case Keys.RightShift:
            case Keys.LeftShift: /* shift */ movementSpeedMultiplier = 0.1f; break;

            case Keys.W: /*W*/ moveState.Forward = 1; break;
            case Keys.S: /*S*/ moveState.Back = 1; break;

            case Keys.A: /*A*/ moveState.Left = 1; break;
            case Keys.D: /*D*/ moveState.Right = 1; break;

            case Keys.R: /*R*/ moveState.Up = 1; break;
            case Keys.F: /*F*/ moveState.Down = 1; break;

            case Keys.Up: /*up*/ moveState.PitchUp = 1; break;
            case Keys.Down: /*down*/ moveState.PitchDown = 1; break;

            case Keys.Left: /*left*/ moveState.YawLeft = 1; break;
            case Keys.Right: /*right*/ moveState.YawRight = 1; break;

            case Keys.Q: /*Q*/ moveState.RollLeft = 1; break;
            case Keys.E: /*E*/ moveState.RollRight = 1; break;
        }

        UpdateMovementVector();
        UpdateRotationVector();
    }

    public void Update(float delta)
    {
        var moveMult = delta * MovementSpeed;
        var rotMult = delta * RollSpeed;

        camera.TranslateX(moveVector.X * moveMult);
        camera.TranslateY(moveVector.Y * moveMult);
        camera.TranslateZ(moveVector.Z * moveMult);

        tmpQuaternion.Set(rotationVector.X * rotMult, rotationVector.Y * rotMult, rotationVector.Z * rotMult, 1);
        tmpQuaternion.Normalize();
        camera.Quaternion.Multiply(tmpQuaternion);
        //camera.Rotation.SetFromQuaternion(camera.Quaternion, camera.Rotation.Order);
        if (
            lastPosition.DistanceToSquared(camera.Position) > EPS ||
            8 * (1 - lastQuaternion.Dot(camera.Quaternion)) > EPS
        )
        {
            //dispatchEvent(changeEvent);
            //camera.Rotation.SetFromQuaternion(camera.Quaternion, camera.Rotation.Order);
            lastQuaternion.Copy(camera.Quaternion);
            lastPosition.Copy(camera.Position);
        }
    }

    private void UpdateMovementVector()
    {
        var forward = moveState.Forward != 0 || (AutoForward && moveState.Back == 0) ? 1 : 0;

        moveVector.X = -moveState.Left + moveState.Right;
        moveVector.Y = -moveState.Down + moveState.Up;
        moveVector.Z = -forward + moveState.Back;
    }

    private void UpdateRotationVector()
    {
        rotationVector.X = -moveState.PitchDown + moveState.PitchUp;
        rotationVector.Y = -moveState.YawRight + moveState.YawLeft;
        rotationVector.Z = -moveState.RollRight + moveState.RollLeft;
    }

    public struct MoveState
    {
        public float Up;
        public float Down;
        public float Left;
        public float Right;
        public float Forward;
        public float Back;
        public float PitchUp;
        public float PitchDown;
        public float YawLeft;
        public float YawRight;
        public float RollLeft;
        public float RollRight;
    }
}