﻿using System;
using THREE;
using Color = THREE.Color;

namespace THREEExample.Learning.Chapter07;

[Example("09-Sprites", ExampleCategory.LearnThreeJS, "Chapter07")]
public class SpritesExample1 : Example
{
    public Camera cameraOrtho;
    public Scene sceneOrtho;
    private int sprite;


    private float step;

    public override void InitCamera()
    {
        base.InitCamera();
        camera.Fov = 45.0f;
        camera.Aspect = glControl.AspectRatio;
        camera.Near = 0.1f;
        camera.Far = 1000;
        camera.Position.Set(0, 0, 50);
        camera.LookAt(scene.Position);
    }

    public override void Init()
    {
        base.Init();

        renderer.SetClearColor(0x000000);
        sceneOrtho = new Scene();


        cameraOrtho = new OrthographicCamera();


        var material = new MeshNormalMaterial { Transparent = true, Opacity = 0.6f };

        var geom = new SphereGeometry(15, 20, 20);

        var mesh = new Mesh(geom, material);

        scene.Add(mesh);

        var texture = TextureLoader.Load(@"../../../../assets/textures/particles/sprite-sheet.png");

        CreateSprite(150, true, 0.6f, Color.Hex(0xffffff), sprite, texture);
    }

    public override void Render()
    {
        if (glControl != null)
        {
            if ((cameraOrtho as OrthographicCamera).CameraRight == 1 && glControl.Width != 0)
            {
                (cameraOrtho as OrthographicCamera).CameraRight = glControl.Width;
                (cameraOrtho as OrthographicCamera).Top = glControl.Height;
                (cameraOrtho as OrthographicCamera).Left = 0;
                (cameraOrtho as OrthographicCamera).Bottom = 0;
                (cameraOrtho as OrthographicCamera).Near = -10;
                (cameraOrtho as OrthographicCamera).Far = 10;

                (cameraOrtho as OrthographicCamera).View.Enabled = false;

                (cameraOrtho as OrthographicCamera).UpdateProjectionMatrix();
            }
        }
        else
        {
            return;
        }

        step += 0.01f;

        camera.Position.Y = (float)Math.Sin(step) * 20;

        sceneOrtho.Traverse(o =>
        {
            if (o is ParticleSprite)
            {
                var e = o as ParticleSprite;

                e.Position.X = e.Position.X + e.VelocityX;

                if (e.Position.X > glControl.Width)
                {
                    e.VelocityX = -5;
                    sprite += 1;
                    e.Material.Map.Offset.Set(1.0f / 5 * (sprite % 4), 0);
                }

                if (e.Position.X < 0) e.VelocityX = 5;
            }
        });
        renderer.Render(sceneOrtho, cameraOrtho);
        renderer.AutoClear = false;
        renderer.Render(scene, camera);
        renderer.AutoClear = true;
    }

    private void CreateSprite(float size, bool transparent, float opacity, Color color, int spriteNumber,
        Texture texture)
    {
        var spriteMaterial = new SpriteMaterial
        {
            Opacity = opacity,
            Color = color,
            Transparent = transparent,
            Map = texture
        };

        // we have 1 row, with five sprites
        spriteMaterial.Map.Offset = new Vector2(0.2f * spriteNumber, 0);
        spriteMaterial.Map.Repeat = new Vector2(1f / 5, 1);
        spriteMaterial.Blending = Constants.AdditiveBlending;
        // make sure the object is always rendered at the front
        spriteMaterial.DepthTest = false;

        var sprite = new ParticleSprite(spriteMaterial);
        sprite.Scale.Set(size, size, size);
        sprite.Position.Set(100, 50, -10);
        sprite.VelocityX = 5;

        sceneOrtho.Add(sprite);
    }

    public class ParticleSprite : Sprite
    {
        public float VelocityX;

        public ParticleSprite(Material material) : base(material)
        {
        }
    }
}