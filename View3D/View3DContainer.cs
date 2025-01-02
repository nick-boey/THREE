using System;
using System.Collections.Generic;
using System.Text;
using THREE;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;
using System.Diagnostics;
using OpenTK.Windowing.Common;
using Rectangle = THREE.Rectangle;
using System.Windows.Controls;
using OpenTK.Wpf;

namespace View3D
{
    [Serializable]
    abstract public class View3DContainer : ControlsContainer
    {
        public GLRenderer Renderer;
        public Camera Camera;
        public Scene Scene;
        public OrbitControls Controls;
        protected readonly Random Random = new Random();

        protected readonly Stopwatch Stopwatch = new Stopwatch();
#if WSL
        public IThreeWindow glControl;
#else
        public GLWpfControl glControl;
#endif

        public Action AddGuiControlsAction;

        public View3DContainer()
        {
            Camera = new PerspectiveCamera();
            Scene = new Scene();
        }

        ~View3DContainer()
        {
            this.Dispose(false);
        }
#if WSL
        public virtual void Load(IThreeWindow control)
#else
        public virtual void Load(GLWpfControl control)
#endif
        {
            Debug.Assert(null != control);

            glControl = control;
            this.Renderer = new THREE.GLRenderer(control.Context, (int)control.RenderSize.Width,
                (int)control.RenderSize.Height);

            Init();

            Stopwatch.Start();
        }

        public override Rectangle GetClientRectangle()
        {
            return new Rectangle(0, 0, Renderer.Width, Renderer.Height);
        }

        public virtual void Init()
        {
            InitRenderer();

            InitCamera();

            InitCameraController();

            InitLighting();
        }


        public virtual void InitCamera()
        {
            Camera.Fov = 45.0f;
            Camera.Aspect = (float)(glControl.RenderSize.Width / glControl.RenderSize.Height);
            Camera.Near = 0.1f;
            Camera.Far = 1000.0f;
            Camera.Position.X = -30;
            Camera.Position.Y = 40;
            Camera.Position.Z = 30;
            Camera.LookAt(THREE.Vector3.Zero());
        }

        public virtual void InitRenderer()
        {
            this.Renderer.SetClearColor(new THREE.Color().SetHex(0xEEEEEE), 1);
            this.Renderer.ShadowMap.Enabled = true;
            this.Renderer.ShadowMap.type = Constants.PCFSoftShadowMap;
        }

        public virtual void InitCameraController()
        {
            Controls = new OrbitControls(this, Camera);
            Controls.RotateSpeed = 1.5f;
            Controls.ZoomSpeed = 1.5f;
            Controls.PanSpeed = 1.5f;
            Controls.Update();
        }

        public virtual void InitLighting()
        {
        }

        public float GetDelta()
        {
            return Stopwatch.ElapsedMilliseconds / 1000.0f;
        }

        public virtual void OnResize(ResizeEventArgs clientSize)
        {
            if (Renderer != null)
            {
                Renderer.Resize(clientSize.Width, clientSize.Height);
                Camera.Aspect = (float)(glControl.Width / glControl.Height);
                Camera.UpdateProjectionMatrix();
            }

            base.OnResize(clientSize);
        }

        public virtual void Render()
        {
            Controls?.Update();
            Renderer?.Render(Scene, Camera);
        }

        public virtual void Unload()
        {
            this.Renderer.Dispose();
        }

        public override void Dispose()
        {
            OnDispose();
        }

        public virtual void OnDispose()
        {
            Unload();
        }
    }
}