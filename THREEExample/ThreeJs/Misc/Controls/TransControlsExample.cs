﻿using System.ComponentModel;
using THREE;

namespace THREEExample.Three.Misc.Controls;

[Example("Controls-Transform", ExampleCategory.Misc, "Controls")]
public class TransControlsExample : ControlsExampleTemplate
{
    private TransformControls transformControls;

    public override void Init()
    {
        base.Init();

        scene.Add(new GridHelper(1000, 10, 0x888888, 0x444444));


        var texture = TextureLoader.Load(@"../../../../assets/textures/crate.gif");
        texture.Anisotropy = renderer.Capabilities.GetMaxAnisotropy();

        var geometry = new BoxGeometry(200, 200, 200);
        var material = new MeshLambertMaterial();
        material.Transparent = true;
        material.Map = texture;
        material.DepthTest = false;
        material.DepthWrite = false;
        material.ToneMapped = false;

        var mesh = new Mesh(geometry, material);
        scene.Add(mesh);

        transformControls = new TransformControls(this, camera);
        transformControls.PropertyChanged += OnPropertyChanged;
        transformControls.Attach(mesh);

        scene.Add(transformControls);
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName.Equals("dragging"))
        {
            controls.state = TrackballControls.STATE.NONE;
            controls.Enabled = !(sender as TransformControls).dragging;
        }
    }
}