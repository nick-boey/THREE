﻿using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class MeshMatcapMaterial : Material
{
    public Texture Matcap;

    public MeshMatcapMaterial()
    {
        type = "MeshMatcapMaterial";
    }

    public MeshMatcapMaterial(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}