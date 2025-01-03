using System;
using THREE;

namespace THREEExample.Chapter02;

[Example("08.Cameras-LookAt", ExampleCategory.LearnThreeJS, "Chapter02")]
public class CameraLookAtExample : BothCameraExample
{
    private Mesh lookAtMesh;
    private float step;

    public override void BuildScene()
    {
        base.BuildScene();
        var lookAtGeom = new SphereGeometry(2, 20, 20);
        lookAtMesh = new Mesh(lookAtGeom, new MeshLambertMaterial { Color = Color.Hex(0x00ff00) });
        scene.Add(lookAtMesh);
    }

    public override void Render()
    {
        step += 0.02f;

        var x = 10 + 100 * (float)Math.Sin(step);
        camera.LookAt(new Vector3(x, 10, 0));
        lookAtMesh.Position.Set(x, 10, 0);
        base.Render();
        //renderer.Render(scene, camera);
    }
}