﻿using System.Collections.Generic;
using ImGuiNET;
using Rhino.DocObjects;
using THREE;

namespace THREEExample.ThreeJs.Loader;

[Example("loader_3dm", ExampleCategory.ThreeJs, "loader")]
public class Rhino3dmLoaderExample : Example
{
    public override void InitCamera()
    {
        camera.Fov = 60.0f;
        camera.Aspect = glControl.AspectRatio;
        camera.Near = 1.0f;
        camera.Far = 1000.0f;
        camera.Position.Set(26, -40, 5);
    }

    public override void InitLighting()
    {
        base.InitLighting();
        var directionalLight = new DirectionalLight(0xffffff, 2);
        directionalLight.Position.Set(0, 0, 2);
        scene.Add(directionalLight);
    }

    public override void Init()
    {
        Object3D.DefaultUp = new Vector3(0, 0, 1);

        base.Init();

        var filePath = "../../../../assets/models/3dm/Rhino_Logo.3dm";

        var loader = new Rhino3dmLoader();
        var object3d = loader.Load(filePath);

        scene.Add(object3d);

        var layerList = object3d.UserData["layers"] as List<Layer>;
        var visible = new bool[layerList.Count];
        for (var i = 0; i < layerList.Count; i++)
            visible[i] = layerList[i].IsVisible;

        AddGuiControlsAction = () =>
        {
            for (var i = 0; i < layerList.Count; i++)
            {
                var layer = layerList[i];
                var name = layer.Name;


                if (ImGui.Checkbox(name, ref visible[i]))
                    scene.Traverse(o =>
                    {
                        if (o.UserData.ContainsKey("attributes"))
                        {
                            var attributes = o.UserData["attributes"] as ObjectAttributes;
                            var layerName = layerList[attributes.LayerIndex].Name;
                            if (layerName == name)
                            {
                                o.Visible = visible[i];
                                layer.IsVisible = visible[i];
                            }
                        }
                    });
            }
        };
    }
}