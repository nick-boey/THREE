using THREE;

namespace THREEExample.ThreeJs.Loader;

[Example("ObjLoaderWithAssimp", ExampleCategory.ThreeJs, "loader")]
public class ObjLoaderWithAssimp : GradientBackgroundShaderExample
{
    public override void BuildScene()
    {
        var loader = new AssimpLoader();
        var group = loader.Load(@"../../../../assets/models/obj/O{22}.obj");

        var childMesh = group.Children[0];

        var box = new Box3().SetFromObject(group);

        var dimensions = new Vector3().SubVectors(box.Max, box.Min);
        var boxGeo = new BoxBufferGeometry(dimensions.X, dimensions.Y, dimensions.Z);

        var position = dimensions.AddVectors(box.Min, box.Max) * 0.5f;

        var boxMesh = new Mesh(boxGeo, new LineBasicMaterial { Color = Color.Hex(0xffff00), Wireframe = true });
        boxMesh.Position.Copy(group.Position + position);
        //scene.Add(GenerateEdges(mesh));

        var mat = new OutlineMaterial(30, true, Color.Hex(0x000));
        var angleThreshold = mat.AngleThreshold;
        var opacity = mat.Opacity;
        var edges = GenerateEdges(group, 10.1f);
        scene.Add(group);
        //scene.Add(boxMesh);
        //scene.Add(edges);
        FitModelToWindow(group, true);
    }
}