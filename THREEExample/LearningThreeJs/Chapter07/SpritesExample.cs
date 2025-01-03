using System;
using THREE;
using THREEExample;
using Color = THREE.Color;

namespace THREEDemo.Learning.Chapter07;

[Example("01.Sprites-Example", ExampleCategory.LearnThreeJS, "Chapter07")]
public class SpritesExample : Example
{
    public override void InitCamera()
    {
        base.InitCamera();
        camera.Position.Set(0, 0, 150);
    }

    public override void Init()
    {
        base.Init();

        CreateSprites();
    }

    private void CreateSprites()
    {
        var random = new Random();
        for (var x = -15; x < 15; x++)
        for (var y = -10; y < 10; y++)
        {
            var material = new SpriteMaterial { Color = new Color().Random() };

            var sprite = new Sprite(material);
            sprite.Position.Set(x * 4, y * 4, 0);
            scene.Add(sprite);
        }
    }
}