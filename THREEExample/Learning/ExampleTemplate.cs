﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using THREE;
using THREE.Cameras;
using THREE.Controls;
using THREE.Geometries;
using THREE.Objects;
using THREE.Scenes;
using THREE.Math;
using THREEExample.ThreeImGui;
using OpenTK;

namespace THREEExample.Learning
{
    public class ExampleTemplate : Example
    {
        public Camera camera;
        public Scene scene;
        public TrackballControls controls;
        public ImGuiManager imGuiManager;

        public ExampleTemplate() : base()
        {
            camera = new PerspectiveCamera();
            scene = new Scene();
        }
        public virtual void InitCamera()
        {
            camera.Fov = 45.0f;
            camera.Aspect = this.glControl.AspectRatio;
            camera.Near = 0.1f;
            camera.Far = 1000.0f;
            camera.Position.X = -30;
            camera.Position.Y = 40;
            camera.Position.Z = 30;
            camera.LookAt(THREE.Math.Vector3.Zero());
        }
        public virtual void InitRenderer()
        {
            this.renderer.SetClearColor(new THREE.Math.Color().SetHex(0xEEEEEE), 1);
            this.renderer.ShadowMap.Enabled = true;
            this.renderer.ShadowMap.type = Constants.PCFSoftShadowMap;
        }
        public  virtual void InitCameraController()
        {
            controls = new TrackballControls(this.glControl, camera);
            controls.StaticMoving = false;
            controls.RotateSpeed = 4.0f;
            controls.ZoomSpeed = 3;
            controls.PanSpeed = 3;
            controls.NoZoom = false;
            controls.NoPan = false;
            controls.NoRotate = false;
            controls.StaticMoving = true;
            controls.DynamicDampingFactor = 0.3f;
        }

        public override void Load(GLControl control)
        {
            base.Load(control);

            InitRenderer();
            InitCamera();
            InitCameraController();

            imGuiManager = new ImGuiManager(this.glControl);
        }
        public override void Render()
        {
            controls.Update();
            renderer.Render(scene, camera);
        }
        public override void Resize(System.Drawing.Size clientSize)
        {
            base.Resize(clientSize);
            camera.Aspect = this.glControl.AspectRatio;
            camera.UpdateProjectionMatrix();
        }
    }
}
