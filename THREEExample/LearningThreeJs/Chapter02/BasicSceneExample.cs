using System;
using ImGuiNET;
using THREE;

namespace THREEExample.Learning.Chapter02;

[Example("01-Basic Scene", ExampleCategory.LearnThreeJS, "Chapter02")]
public class BasicSceneExample : Example
{
    public Mesh plane;
    public PlaneGeometry planeGeometry;
    private float rotationSpeed = 0.005f;
    private int numOfObjects;

#if WSL
        public override void Load(IThreeWindow glControl)
#else
    public override void Load(GLControl glControl)
#endif
    {
        base.Load(glControl);

        planeGeometry = new PlaneGeometry(60, 40, 1, 1);
        var planeMaterial = new MeshPhongMaterial { Color = new Color().SetHex(0xffffff) };

        plane = new Mesh(planeGeometry, planeMaterial);
        plane.ReceiveShadow = true;

        plane.Rotation.X = (float)(-0.5 * Math.PI);
        plane.Position.X = 0;
        plane.Position.Y = 0;
        plane.Position.Z = 0;

        scene.Add(plane);

        var ambientLight = new AmbientLight(new Color().SetHex(0x0c0c0c));
        scene.Add(ambientLight);

        var spotLight = new SpotLight(new Color().SetHex(0xffffff));
        spotLight.Position.Set(-40, 60, -10);
        spotLight.CastShadow = true;

        scene.Add(spotLight);

        AddGuiControlsAction = AddControls;
    }

    public override void Render()
    {
        scene.Traverse(o =>
        {
            if (o is Mesh && !plane.Equals(o))
            {
                o.Rotation.X += rotationSpeed;
                o.Rotation.Y += rotationSpeed;
                o.Rotation.Z += rotationSpeed;
            }
        });
        base.Render();
    }

    public void Add()
    {
        var cubeSize = (float)MathUtils.random.NextDouble() * 3;
        cubeSize = (int)Math.Ceiling((decimal)cubeSize);
        var cubeGeometry = new BoxGeometry(cubeSize, cubeSize, cubeSize);
        var cubeMaterial = new MeshPhongMaterial { Color = new Color().Random() };
        var cube = new Mesh(cubeGeometry, cubeMaterial);
        cube.CastShadow = true;
        //cube.Name = "cube-" + BasicScene.scene.Children.Count;

        cube.Position.X = -30 + (float)Math.Round(MathUtils.random.NextDouble() * planeGeometry.parameters.Width);
        cube.Position.Y = (float)Math.Round(MathUtils.random.NextDouble() * 5);
        cube.Position.Z = -20 + (float)Math.Round(MathUtils.random.NextDouble() * planeGeometry.parameters.Height);
        scene.Add(cube);
        numOfObjects++;
    }

    public void Remove()
    {
        var allChildren = scene.Children;
        var index = allChildren.Count - 1;

        if (index < 0) return;

        var lastObject = allChildren[allChildren.Count - 1];

        if (lastObject is Mesh) scene.Remove(lastObject);
        //NumberOfObjects.Content = BasicScene.scene.Children.Count.ToString();
        numOfObjects--;
    }

    private void AddControls()
    {
        ImGui.Text("This is Rotation Speed Control Box");

        ImGui.SliderFloat("RotationSpeed", ref rotationSpeed, 0.0f, 0.5f);

        if (ImGui.Button("Draw")) numOfObjects++;

        if (ImGui.Button("Add Cube")) Add();

        if (ImGui.Button("Remove Cube")) Remove();

        ImGui.Text("Number Of Objects:" + numOfObjects);
    }
}