using System;
using ImGuiNET;
using THREE;

namespace THREEExample.ThreeJs.Instancing;

[Example("Instancing Dynamic", ExampleCategory.ThreeJs, "Instancing")]
public class InstancingDynamic : Example
{
    private readonly int amount = 10;
    private readonly int count;
    private readonly Object3D dummy = new();
    private InstancedMesh mesh;

    public InstancingDynamic()
    {
        count = (int)Math.Pow(amount, 3);
    }

    public override void InitRenderer()
    {
        base.InitRenderer();
        renderer.SetClearColor(0x000000);
    }

    public override void InitCamera()
    {
        camera = new PerspectiveCamera(60, ClientRectangle.Width / ClientRectangle.Height, 0.1f, 100);
        camera.Position.Set(amount * 0.9f, amount * 0.9f, amount * 0.9f);
        camera.LookAt(0, 0, 0);
    }

    public override void Init()
    {
        base.Init();

        var loader = new BufferGeometryLoader();
        var geometry = loader.Load("../../../../assets/models/json/suzanne_buffergeometry.json");

        geometry.ComputeVertexNormals();
        geometry.Scale(0.5f, 0.5f, 0.5f);

        var material = new MeshNormalMaterial();
        mesh = new InstancedMesh(geometry, material, count);
        mesh.InstanceMatrix.SetUsage(Constants.DynamicDrawUsage); // will be updated every frame
        scene.Add(mesh);

        AddGuiControlsAction = () => { ImGui.SliderInt("count", ref mesh.InstanceCount, 0, count); };
    }

    public override void Render()
    {
        base.Render();
        SetInstanceMatrix(GetDelta());
    }

    private void SetInstanceMatrix(float delta)
    {
        var time = delta;

        mesh.Rotation.X = (float)Math.Sin(time / 4);
        mesh.Rotation.Y = (float)Math.Sin(time / 2);

        var i = 0;
        var offset = (amount - 1) / 2;

        for (var x = 0; x < amount; x++)
        for (var y = 0; y < amount; y++)
        for (var z = 0; z < amount; z++)
        {
            dummy.Position.Set(offset - x, offset - y, offset - z);
            dummy.Rotation.Y = (float)(Math.Sin(x / 4 + time) + Math.Sin(y / 4 + time) + Math.Sin(z / 4 + time));
            dummy.Rotation.Z = dummy.Rotation.Y * 2;

            dummy.UpdateMatrix();

            mesh.SetMatrixAt(i++, dummy.Matrix);
        }

        mesh.InstanceMatrix.NeedsUpdate = true;
    }
}