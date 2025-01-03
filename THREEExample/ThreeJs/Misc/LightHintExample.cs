using System;
using THREE;
using THREE.Objects;

namespace THREEExample;

[Example("LightHint", ExampleCategory.ThreeJs, "Misc")]
public class LightHintExample : Example
{
    private readonly float _fov;
    private readonly Vector3 _up;

    private readonly Material lineMaterial;
    private Vector3 _cameraEye;
    private Vector3 _center;
    public AmbientLight ambientLight;
    public DirectionalLight directionalLight;

    public LightHintExample()
    {
        _cameraEye = new Vector3(-1.5f, -3.0f, 2.0f);
        _center = new Vector3(0, 0, 0);
        _up = new Vector3(0, 0, 1);
        _fov = 45f;

        lineMaterial = new LineBasicMaterial { Color = Color.Hex(0x000000), LineWidth = 2 };
    }

    public override void InitLighting()
    {
        ambientLight = new AmbientLight(Color.Hex(0x888888));
        directionalLight = new DirectionalLight { Color = Color.Hex(0x888888) };

        scene.Add(ambientLight);
        scene.Add(directionalLight);
    }

    private Vector3 Offset(Vector3 source, Vector3 direction, float distance)
    {
        var normal = direction.Clone().Normalize();
        source.X += normal.X * distance;
        source.Y += normal.Y * distance;
        source.Z += normal.Z * distance;
        return source;
    }

    private void AdjustClippingPlanesToSphere(Sphere boundingSphere)
    {
        if (boundingSphere.Radius <= 0.0000f) return;
        if (boundingSphere.Radius <= 10.0f)
        {
            camera.Near = 0.01f;
            camera.Far = 100;
        }
        else if (boundingSphere.Radius <= 100.0f)
        {
            camera.Near = 0.1f;
            camera.Far = 1000f;
        }
        else if (boundingSphere.Radius <= 1000.0f)
        {
            camera.Near = 10.0f;
            camera.Far = 10000f;
        }
        else
        {
            camera.Near = 100.0f;
            camera.Far = 1000000f;
        }
    }

    private void FitToSphere(Vector3 center, float radius)
    {
        var offsetToOrigin = _center - center;
        _cameraEye = _cameraEye - offsetToOrigin;
        _center = center.Clone();
        var centerEyeDirection = (_cameraEye - _center).Normalize();
        var fieldOfView = _fov / 2.0f;
        if (glControl.Width < glControl.Height) fieldOfView = fieldOfView * glControl.Width / glControl.Height;
        var distance = radius / (float)Math.Sin(fieldOfView * MathUtils.DEG2RAD);
        var centerCloned = _center.Clone();
        centerCloned = Offset(centerCloned, centerEyeDirection, distance);
        _cameraEye = centerCloned;
    }

    private void FitSphereToWindow(Sphere boundingSphere)
    {
        var center = boundingSphere.Center.Clone();
        var radius = boundingSphere.Radius;
        FitToSphere(center, radius);
    }

    public void FitModelToWindow(Object3D model, bool nearFarChange)
    {
        var boundingBox = new Box3();
        boundingBox = boundingBox.SetFromObject(model);
        var boundingSphere = new Sphere();
        boundingSphere = boundingBox.GetBoundingSphere(boundingSphere);
        if (nearFarChange) AdjustClippingPlanesToSphere(boundingSphere);
        FitSphereToWindow(boundingSphere);

        camera.Position = _cameraEye;
        camera.Up = _up;
        camera.LookAt(_center.X, _center.Y, _center.Z);
        camera.Aspect = glControl.AspectRatio;
        camera.Fov = _fov;
        camera.UpdateProjectionMatrix();

        var lightDir = _cameraEye - _center;
        directionalLight.Position.Set(lightDir.X, lightDir.Y, lightDir.Z);
    }

    public Object3D GenerateEdges(Object3D group, float threshold = 0.6f)
    {
        var edges = new Object3D();
        group.Traverse(o =>
        {
            if (o is Mesh)
            {
                var mesh = (Mesh)o;

                if (mesh.Geometry != null)
                {
                    var edgeGeometry = new EdgesGeometry(mesh.Geometry, threshold);
                    var edgeObject = new LineSegments(edgeGeometry, lineMaterial);
                    edges.Add(edgeObject);
                }
            }
        });
        return edges;
    }

    public virtual void BuildScene()
    {
        var loader = new OBJLoader();
        var mesh = loader.Parse(@"../../../../assets/models/obj/O{22}.obj");

        var meshMaterial = new MeshPhongMaterial();
        meshMaterial.Color = Color.Hex(0x7777ff);
        meshMaterial.Name = "MeshPhongMaterial";
        meshMaterial.FlatShading = false;

        mesh.Traverse(o =>
        {
            if (o is Mesh) mesh.Material = meshMaterial;
        });
        var childMesh = mesh.Children[0];

        // generate out box line
        var box = new Box3().SetFromObject(mesh);
        var dimensions = new Vector3().SubVectors(box.Max, box.Min);
        var boxGeo = new BoxBufferGeometry(dimensions.X, dimensions.Y, dimensions.Z);
        var position = dimensions.AddVectors(box.Min, box.Max) * 0.5f;

        var boxMesh = new Mesh(boxGeo, new LineBasicMaterial { Color = Color.Hex(0xffff00), Wireframe = true });
        boxMesh.RenderOrder = 99;
        boxMesh.Position.Copy(mesh.Position + position);
        boxMesh.Visible = false;
        scene.Add(boxMesh);

        // generate outline
        var mat = new OutlineMaterial(60, true, Color.Hex(0x000000));
        mesh.Traverse(o =>
        {
            if (o is Mesh)
            {
                var mesh = (Mesh)o;
                var outlineMesh = new OutlineMesh(mesh, mat);
                scene.Add(outlineMesh);
            }
        });

        FitModelToWindow(mesh, true);
        scene.Add(mesh);
    }

    public override void Init()
    {
        base.Init();
        BuildScene();
        //AddGuiControlsAction = AddControls;
    }
}