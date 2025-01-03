using THREE;
using THREEExample.Learning.Utils;

namespace THREEExample.Learning.Chapter10;

[Example("01-basic-texture", ExampleCategory.LearnThreeJS, "Chapter10")]
public class BasicTextureExample : TemplateExample
{
    public override void SetGeometryWithTexture()
    {
        var groundPlane = DemoUtils.AddLargeGroundPlane(scene);
        groundPlane.Position.Y = -10;

        var texture = TextureLoader.Load("../../../../assets/textures/dds/test-dxt1.jpg");

        scene.Add(new AmbientLight(new Color(0x444444)));

        var polyhedron = new IcosahedronBufferGeometry(8, 0);
        polyhedronMesh = AddGeometry(scene, polyhedron, "polyhedron", texture);
        polyhedronMesh.Position.X = 20;

        var sphere = new SphereBufferGeometry(5, 20, 20);
        sphereMesh = AddGeometry(scene, sphere, "sphere", texture);

        var cube = new BoxBufferGeometry(10, 10, 10);
        cubeMesh = AddGeometry(scene, cube, "cube", texture);
        cubeMesh.Position.X = -20;
    }
}