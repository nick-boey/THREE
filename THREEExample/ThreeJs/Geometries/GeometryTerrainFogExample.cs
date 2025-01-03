using THREE;

namespace THREEExample.Three.Geometries;

[Example("Terrain Fog", ExampleCategory.ThreeJs, "geometry")]
public class GeometryTerrainFogExample : GeometryTerrainExample
{
    public GeometryTerrainFogExample()
    {
        scene.Fog = new Fog(0xefd1b5);
    }

    public override void InitCamera()
    {
        camera = new PerspectiveCamera(60, glControl.AspectRatio, 1, 10000);
        camera.Position.Set(100, 800, -800);
        camera.LookAt(-100, 810, -800);
    }
}