using System;
using System.Collections;
using OpenTK.Windowing.Common;
using THREE;
using Color = THREE.Color;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

namespace THREEExample.Three.Loaders;

[Example("loader ttf", ExampleCategory.ThreeJs, "loader")]
public class LoaderTTFExample : Example
{
    private readonly bool bevelEnabled = true;
    private readonly int bevelSegments = 3;
    private readonly float bevelSize = 1.5f;
    private readonly int bevelThickness = 2;
    private readonly Vector3 cameraTarget = new(0, 150, 0);
    private readonly int curveSegments = 4;
    private readonly int height = 20;
    private readonly int hover = 30;
    private readonly bool mirror = true;

    private readonly int size = 70;
    private readonly int steps = 1;

    private bool firstLetter = true;
    private Group group;
    private Material material;

    private Hashtable options;

    private int pointerX;
    private int pointerXOnPointerDown;

    private float targetRotation;
    private float targetRotationOnPointerDown;

    private string text = "three.js";
    private TextBufferGeometry textGeo;

    private Mesh textMesh1, textMesh2;
    private int windowHalfX;

    public LoaderTTFExample()
    {
        scene.Background = Color.Hex(0x000000);
        scene.Fog = new Fog(0x000000, 250, 1400);
    }

    public override void InitRenderer()
    {
        base.InitRenderer();
    }

    public override void InitCamera()
    {
        base.InitCamera();
        camera.Fov = 30;
        camera.Near = 0.1f;
        camera.Far = 1500;
        camera.Position.Set(0, 400, 700);
    }

    public override void InitLighting()
    {
        var dirLight = new DirectionalLight(0xffffff, 0.125f);
        dirLight.Position.Set(0, 0, 1).Normalize();
        scene.Add(dirLight);

        var pointLight = new PointLight(0xffffff, 1.5f);
        pointLight.Position.Set(0, 100, 90);
        pointLight.Color.SetHSL(MathUtils.NextFloat(), 1, 0.5f);
        scene.Add(pointLight);
    }

    public override void Init()
    {
        base.Init();

        windowHalfX = glControl.Width / 2;

        group = new Group();
        group.Position.Y = 100;


        material = new MeshPhongMaterial { Color = Color.Hex(0xffffff), FlatShading = true };


        CreateText();

        scene.Add(group);

        var plane = new Mesh(
            new PlaneBufferGeometry(10000, 10000),
            new MeshBasicMaterial { Color = Color.Hex(0xffffff), Opacity = 0.5f, Transparent = true }
        );
        plane.Position.Y = 100;
        plane.Rotation.X = -(float)Math.PI / 2;
        scene.Add(plane);

        KeyDown += OnKeyDown;
        KeyPress += OnKeyPress;
        MouseDown += OnMouseDown;
    }

    private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
    {
        if (firstLetter)
        {
            firstLetter = false;
            text = "";
        }

        var keyCode = e.Key;

        if (keyCode == Keys.Backspace)
        {
            text = text.Substring(0, text.Length - 1);

            RefreshText();
        }
    }

    private void OnKeyPress(object sender, KeyPressEventArgs e)
    {
        var k = (int)e.Key[0];
        var b = (int)Keys.Backspace;
        if (e.Key[0] == 8) //(int)Keys.Backspace)
        {
            if (!string.IsNullOrEmpty(text))
                text = text.Substring(0, text.Length - 1);

            RefreshText();
        }
        else
        {
            text = text + e.Key;

            RefreshText();
        }
    }

    private void OnMouseDown(object sender, MouseEventArgs e)
    {
        pointerXOnPointerDown = e.X - windowHalfX;
        targetRotationOnPointerDown = targetRotation;

        MouseMove += OnMouseMove;
        MouseUp += OnMouseUp;
    }

    private void OnMouseUp(object sender, MouseEventArgs e)
    {
        MouseMove -= OnMouseMove;
        MouseUp -= OnMouseUp;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        pointerX = e.X - windowHalfX;

        targetRotation = targetRotationOnPointerDown + (pointerX - pointerXOnPointerDown) * 0.02f;
    }


    private void CreateText()
    {
        var ttfFont = new TTFFont(@"../../../../assets/fonts/ttf/kenpixel.ttf");
        //var geometry = CreateTextGeometry();
        options = new Hashtable
        {
            { "size", size },
            { "height", height },
            { "bevelThickness", bevelThickness },
            { "bevelSize", bevelSize },
            { "bevelSegments", bevelSegments },
            { "bevelEnabled", bevelEnabled },
            { "curveSegments", curveSegments },
            { "steps", steps }
        };
        textGeo = ttfFont.CreateTextGeometry(text, options);
        //textGeo.ApplyMatrix4(new Matrix4().MakeScale(0.05f, 0.05f, 0.05f));

        textGeo.ComputeBoundingBox();
        textGeo.ComputeVertexNormals();

        var centerOffset = -0.5f * (textGeo.BoundingBox.Max.X - textGeo.BoundingBox.Min.X);

        textMesh1 = new Mesh(textGeo, material);

        textMesh1.Position.X = centerOffset;
        textMesh1.Position.Y = hover;
        textMesh1.Position.Z = 0;


        textMesh1.Rotation.X = 0;
        textMesh1.Rotation.Y = (float)Math.PI * 2;


        group.Add(textMesh1);

        if (mirror)
        {
            textMesh2 = new Mesh(textGeo, material);

            textMesh2.Position.X = centerOffset;
            textMesh2.Position.Y = -hover;
            textMesh2.Position.Z = height;

            textMesh2.Rotation.X = (float)Math.PI;
            textMesh2.Rotation.Y = (float)Math.PI * 2;

            group.Add(textMesh2);
        }
    }

    public override void Render()
    {
        group.Rotation.Y += (targetRotation - group.Rotation.Y) * 0.05f;

        camera.LookAt(cameraTarget);

        renderer.Render(scene, camera);
    }

    private void RefreshText()
    {
        group.Remove(textMesh1);
        if (mirror) group.Remove(textMesh2);
        if (text == null || text.Length == 0) return;

        CreateText();
    }

    public override void OnResize(ResizeEventArgs clientSize)
    {
        base.OnResize(clientSize);

        windowHalfX = glControl.Width / 2;
    }
}