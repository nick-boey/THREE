﻿using THREE;
using Color = THREE.Color;

namespace THREEExample.Learning.Chapter07;

[Example("08.Snowy-Scene", ExampleCategory.LearnThreeJS, "Chapter07")]
public class SnowySceneExample : Example
{
    private Points cloud;

    private float step = 0.0f;

    public override void Init()
    {
        base.Init();

        camera.Position.Set(20, 40, 110);

        camera.LookAt(new Vector3(20, 30, 0));

        renderer.SetClearColor(0x000000);

        CreatePointInstance(10, true, 0.6f, true, Color.Hex(0xffffff));
    }

    public override void Render()
    {
        base.Render();

        scene.Traverse(p =>
        {
            if (p is Points)
            {
                var vertices = p.Geometry.Vertices;
                vertices.ForEach(delegate(Vector3 v1)
                {
                    var v = v1 as Particle;
                    v.Y = v.Y - v.VelocityY;
                    v.X = v.X - v.VelocityX;
                    v.Z = v.Z - v.VelocityX;

                    if (v.Y <= 0) v.Y = 60;
                    if (v.X <= -20 || v.X >= 20) v.VelocityX = v.VelocityX * -1;
                    if (v.Z <= -20 || v.Z >= 20) v.VelocityZ = v.VelocityZ * -1;
                });
                p.Geometry.VerticesNeedUpdate = true;
            }
        });
    }

    private Points CreatePointCloud(string name, Texture texture, float size, bool transparent, float opacity,
        bool sizeAttenuation, Color color)
    {
        var geom = new Geometry();

        color.SetHSL(color.GetHSL().H, color.GetHSL().S, (float)random.NextDouble() * color.GetHSL().L);

        var material = new PointsMaterial
        {
            Size = size,
            Transparent = transparent,
            Opacity = opacity,
            Map = texture,
            Blending = Constants.AdditiveBlending,
            DepthWrite = false,
            SizeAttenuation = sizeAttenuation,
            Color = color
        };

        var range = 40;
        for (var i = 0; i < 150; i++)
        {
            var particle = new Particle(
                (float)random.NextDouble() * range - range / 2.0f,
                (float)random.NextDouble() * range * 1.5f,
                (float)random.NextDouble() * range - range / 2.0f);

            particle.VelocityY = 0.1f + (float)random.NextDouble() / 5;
            particle.VelocityX = (float)(random.NextDouble() - 0.5f) / 3;
            particle.VelocityZ = (float)(random.NextDouble() - 0.5f) / 3;
            geom.Vertices.Add(particle);
        }

        var system = new Points(geom, material);
        system.Name = name;

        return system;
    }

    private void CreatePointInstance(float size, bool transparent, float opacity, bool sizeAttenuation, Color color)
    {
        var texture1 = TextureLoader.Load(@"../../../../assets/textures/particles/snowflake1_t.png");
        var texture2 = TextureLoader.Load(@"../../../../assets/textures/particles/snowflake2_t.png");
        var texture3 = TextureLoader.Load(@"../../../../assets/textures/particles/snowflake3_t.png");
        var texture4 = TextureLoader.Load(@"../../../../assets/textures/particles/snowflake5_t.png");

        scene.Add(CreatePointCloud("system1", texture1, size, transparent, opacity, sizeAttenuation, color));
        scene.Add(CreatePointCloud("system2", texture2, size, transparent, opacity, sizeAttenuation, color));
        scene.Add(CreatePointCloud("system3", texture3, size, transparent, opacity, sizeAttenuation, color));
        scene.Add(CreatePointCloud("system4", texture4, size, transparent, opacity, sizeAttenuation, color));
    }

    public class Particle : Vector3
    {
        public float VelocityX;

        public float VelocityY;

        public float VelocityZ;

        public Particle(float x, float y, float z) : base(x, y, z)
        {
        }
    }
}