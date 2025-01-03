using System;
using THREE;
using THREEExample.Learning.Chapter01;

namespace THREEExample.Chapter01;

[Example("04-Materials-Light-Animation", ExampleCategory.LearnThreeJS, "Chapter01")]
public class MaterialsLightAnimationExample : MaterialsLightExample
{
    public float bouncingSpeed = 0.04f;
    public float rotationSpeed = 0.2f;
    public float step;

    public override void InitLighting()
    {
        // add subtle ambient lighting
        var ambienLight = new AmbientLight(0x353535);
        scene.Add(ambienLight);

        // add spotlight for the shadows
        var spotLight = new SpotLight(0xffffff);
        spotLight.Position.Set(-10, 20, -5);
        spotLight.CastShadow = true;
        scene.Add(spotLight);
    }

    public override void Render()
    {
        // rotate the cube around its axes
        cube.Rotation.X += rotationSpeed;
        cube.Rotation.Y += rotationSpeed;
        cube.Rotation.Z += rotationSpeed;

        // bounce the sphere up and down
        step += bouncingSpeed;
        sphere.Position.X = 20 + 10 * (float)Math.Cos(step);
        sphere.Position.Y = 2 + 10 * (float)Math.Abs(Math.Sin(step));

        base.Render();
    }
}