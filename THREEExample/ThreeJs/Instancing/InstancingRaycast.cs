using System;
using ImGuiNET;
using THREE;
using Color = THREE.Color;

namespace THREEExample.ThreeJs.Instancing;

[Example("Instancing Raycast", ExampleCategory.ThreeJs, "Instancing")]
public class InstancingRaycast : Example
{
    private readonly int amount = 10;
    private readonly Color color = new(1, 1, 1);
    private readonly int count;
    private readonly Vector2 mouse = new(1, 1);
    private readonly Raycaster raycaster = new();
    private InstancedMesh mesh;

    public InstancingRaycast()
    {
        count = (int)Math.Pow(amount, 3);
    }

    public override void InitCamera()
    {
        camera = new PerspectiveCamera(60, glControl.AspectRatio, 0.1f, 100);
        camera.Position.Set(amount, amount, amount);
        camera.LookAt(0, 0, 0);
    }

    public override void InitLighting()
    {
        base.InitLighting();
        var light1 = new HemisphereLight(Color.Hex(0xffffff), Color.Hex(0x000088));
        light1.Position.Set(-1, 1.5f, 1);
        scene.Add(light1);

        var light2 = new HemisphereLight(Color.Hex(0xffffff), Color.Hex(0x880000), 0.5f);
        light2.Position.Set(-1, -1.5f, -1);
        scene.Add(light2);
    }

    public override void InitRenderer()
    {
        //base.InitRenderer();
        renderer.SetClearColor(Color.Hex(0x000000));
    }

    public override void Init()
    {
        base.Init();

        var geometry = new IcosahedronBufferGeometry(0.5f, 3);
        var material = new MeshPhongMaterial();
        mesh = new InstancedMesh(geometry, material, count);

        var i = 0;
        var offset = (amount - 1) / 2;

        var matrix = new Matrix4();

        for (var x = 0; x < amount; x++)
        for (var y = 0; y < amount; y++)
        for (var z = 0; z < amount; z++)
        {
            matrix.SetPosition(offset - x, offset - y, offset - z);

            mesh.SetMatrixAt(i, matrix);
            mesh.SetColorAt(i, color);
            i++;
        }

        scene.Add(mesh);
        AddGuiControlsAction = () => { ImGui.SliderInt("count", ref mesh.InstanceCount, 0, count); };
        MouseMove += OnMouseMove;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        mouse.X = e.X * 1.0f / ClientRectangle.Width * 2 - 1.0f;
        mouse.Y = -e.Y * 1.0f / ClientRectangle.Height * 2 + 1.0f;
    }

    public override void Render()
    {
        raycaster.SetFromCamera(mouse, camera);

        var intersection = raycaster.IntersectObject(mesh);

        if (intersection.Count > 0)
        {
            var instanceId = intersection[0].instanceId;

            mesh.SetColorAt(instanceId, new Color().Random());
            mesh.InstanceColor.NeedsUpdate = true;
        }

        base.Render();
    }
}