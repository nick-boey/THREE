using System;
using ImGuiNET;
using THREE;
using Color = THREE.Color;

namespace THREEExample.ThreeJs.Instancing;

[Example("Instancing Raycast", ExampleCategory.ThreeJs, "Instancing")]
public class InstancingRaycast : Example
{
    public const int Amount = 10;
    private readonly Color _color = new(1, 1, 1);
    private readonly int _count;
    private readonly Vector2 _mouse = new(1, 1);
    private readonly Raycaster _raycaster = new();
    private InstancedMesh _mesh;

    public InstancingRaycast()
    {
        _count = (int)Math.Pow(Amount, 3);
    }

    public override void InitCamera()
    {
        camera = new PerspectiveCamera(60, glControl.AspectRatio, 0.1f, 100);
        camera.Position.Set(Amount, Amount, Amount);
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
        _mesh = new InstancedMesh(geometry, material, _count);

        var i = 0;
        var offset = (Amount - 1) / 2;

        var matrix = new Matrix4();

        for (var x = 0; x < Amount; x++)
        for (var y = 0; y < Amount; y++)
        for (var z = 0; z < Amount; z++)
        {
            matrix.SetPosition(offset - x, offset - y, offset - z);

            _mesh.SetMatrixAt(i, matrix);
            _mesh.SetColorAt(i, _color);
            i++;
        }

        scene.Add(_mesh);
        AddGuiControlsAction = () => { ImGui.SliderInt("count", ref _mesh.InstanceCount, 0, _count); };
        MouseMove += OnMouseMove;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        _mouse.X = e.X * 1.0f / ClientRectangle.Width * 2 - 1.0f;
        _mouse.Y = -e.Y * 1.0f / ClientRectangle.Height * 2 + 1.0f;
    }

    public override void Render()
    {
        _raycaster.SetFromCamera(_mouse, camera);

        var intersection = _raycaster.IntersectObject(_mesh);

        if (intersection.Count > 0)
        {
            var instanceId = intersection[0].InstanceId;

            _mesh.SetColorAt(instanceId, new Color().Random());
            _mesh.InstanceColor.NeedsUpdate = true;
        }

        base.Render();
    }
}