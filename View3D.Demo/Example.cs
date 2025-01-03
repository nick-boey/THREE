using OpenTK.Windowing.Common;
using OpenTK.Wpf;
using THREE;
using Color = THREE.Color;
using DirectionalLight = THREE.DirectionalLight;

namespace View3D;

public class Example : ViewContainer
{
    public override void Load(GLWpfControl glControl)
    {
        base.Load(glControl);

        Scene.Background = Color.Hex(0x000000);

        var axes = new AxesHelper(20);

        Scene.Add(axes);

        var planeGeometry = new PlaneGeometry(60, 20);
        var planeMaterial = new MeshBasicMaterial { Color = Color.Hex(0xcccccc) };
        var plane = new Mesh(planeGeometry, planeMaterial);

        plane.Rotation.X = (float)(-0.5 * Math.PI);
        plane.Position.Set(15, 0, 0);

        Scene.Add(plane);

        // create a directional light
        var light = new DirectionalLight();
        light.Color = Color.Hex(0xff00ff);
        light.Position = new Vector3(30, 30, 30);
        Scene.Add(light);


        // create a cube
        var cubeGeometry = new BoxGeometry(4, 4, 4);

        var cubeMaterial = new OutlineMaterial { Color = Color.Hex(0xff0000) };
        var cube = new Mesh(cubeGeometry, cubeMaterial);

        // position the cube
        cube.Position.Set(-4, 3, 0);

        // add the cube to the scene

        Scene.Add(cube);

        // create a sphere
        var sphereGeometry = new SphereGeometry(4, 20, 20);
        var sphereMaterial = new MeshBasicMaterial { Color = Color.Hex(0x7777ff) };
        var sphere = new Mesh(sphereGeometry, sphereMaterial);

        // position the sphere
        sphere.Position.Set(20, 4, 2);

        // add the sphere to the scene
        Scene.Add(sphere);
    }

    public override void OnResize(ResizeEventArgs clientSize)
    {
        // TODO: Work out why this is required for rendering, as it appears to do nothing
        if (GLControl == null) return;

        base.OnResize(clientSize);
        Camera.Aspect = (float)(GLControl.RenderSize.Width / GLControl.RenderSize.Height);
        Camera.UpdateProjectionMatrix();
    }
}