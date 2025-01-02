﻿using System;
using THREE;
using OpenTK;
using OpenTK.Windowing.Common;
using Color = THREE.Color;
using System.Windows.Media.Media3D;
using OpenTK.Wpf;

namespace View3D
{
    public class Example3DContainer : View3DContainer
    {
        public Example3DContainer() : base()
        {
        }
#if WSL
        public override void Load(IThreeWindow glControl)
#else
        public override void Load(GLWpfControl glControl)
#endif
        {
            base.Load(glControl);


            Scene.Background = Color.Hex(0x000000);

            var axes = new AxesHelper(20);

            Scene.Add(axes);

            var planeGeometry = new PlaneGeometry(60, 20);
            var planeMaterial = new MeshBasicMaterial() { Color = Color.Hex(0xcccccc) };
            var plane = new Mesh(planeGeometry, planeMaterial);

            plane.Rotation.X = (float)(-0.5 * Math.PI);
            plane.Position.Set(15, 0, 0);

            Scene.Add(plane);

            // create a cube
            var cubeGeometry = new BoxGeometry(4, 4, 4);
            var cubeMaterial = new MeshBasicMaterial() { Color = Color.Hex(0xff0000), Wireframe = true };
            var cube = new Mesh(cubeGeometry, cubeMaterial);

            // position the cube
            cube.Position.Set(-4, 3, 0);

            // add the cube to the scene

            Scene.Add(cube);

            //      // create a sphere
            var sphereGeometry = new SphereGeometry(4, 20, 20);
            var sphereMaterial = new MeshBasicMaterial() { Color = Color.Hex(0x7777ff), Wireframe = true };
            var sphere = new Mesh(sphereGeometry, sphereMaterial);

            //      // position the sphere
            sphere.Position.Set(20, 4, 2);

            //      // add the sphere to the scene
            Scene.Add(sphere);
        }

        public override void Render()
        {
            base.Render();
        }

        public override void OnResize(ResizeEventArgs clientSize)
        {
            base.OnResize(clientSize);
            Camera.Aspect = (float)(this.glControl.RenderSize.Width / this.glControl.RenderSize.Height);
            Camera.UpdateProjectionMatrix();
        }
    }
}