﻿using System.Collections;
using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class MeshBasicMaterial : Material
{
    // IMap

    //public Texture Map { get; set; }

    //public Texture AlphaMap { get; set; }

    //public Texture SpecularMap { get; set; }

    //public Texture EnvMap { get; set; }

    //public Texture NormalMap { get; set; } // TODO: not in ThreeJs, just to be an IMap. Must be NULL

    //public Texture BumpMap { get; set; } // TODO: not in ThreeJs, just to be an IMap.  Must be NULL

    //public Texture LightMap { get; set; }

    //public float LightMapIntensity = 1.0f;

    //public Texture AoMap { get; set; }

    //public float AoMapIntensity = 1.0f;

    //public Color Color = Color.White;

    //public int Combine =(int)THREES.MultiplyOperation;

    //public float Reflectivity =1.0f;

    //public float RefractionRatio = 0.98f;

    //public int Shading;

    // IWireFrameable

    //public bool Wireframe {get;set;}

    //public float WireframeLineWidth {get;set;}

    //public bool MorphTargets { get; set; }
    //


    //public bool Skinning = false;


    //public int NumSupportedMorphTargets;

    public MeshBasicMaterial(Hashtable parameters = null)
    {
        type = "MeshBasicMaterial";

        Map = null;

        Color = THREE.Color.Hex(0xffffff); // emissive

        //this.LightMap = null;

        SpecularMap = null;

        AlphaMap = null;

        EnvMap = null;


        Wireframe = false;

        WireframeLineWidth = 1;

        WireframeLineCap = "round";

        WireframeLineJoin = "round";
        // this.Fog = true;

        //this.Shading = (int)THREES.SmoothShading;


        SetValues(parameters);
    }

    public MeshBasicMaterial(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    protected MeshBasicMaterial(MeshBasicMaterial other)
        : base(other)
    {
    }

    public new MeshBasicMaterial Clone()
    {
        return new MeshBasicMaterial(this);
    }
}