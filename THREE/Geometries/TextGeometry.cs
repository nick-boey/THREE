﻿using System.Collections;

namespace THREE;

[Serializable]
public class TextGeometry : Geometry
{
    public Hashtable parameters;

    public TextGeometry(string text, Hashtable parameters)
    {
        Type = "TextGeometry";

        this.parameters = new Hashtable
        {
            { "text", text },
            { "parameters", parameters }
        };

        FromBufferGeometry(new TextBufferGeometry(text, parameters));
        MergeVertices();
    }
}

[Serializable]
public class TextBufferGeometry : ExtrudeBufferGeometry
{
    public TextBufferGeometry()
    {
    }

    public TextBufferGeometry(string text, Hashtable parameters)
    {
        Type = "TextBufferGeometry";

        this.parameters = new Hashtable
        {
            { "text", text },
            { "parameters", parameters }
        };

        if (!parameters.ContainsKey("font") || parameters["font"] == null)
        {
            Console.WriteLine("Error: TextGeometry font parameter is not an instance of THREE.Font");
            return;
        }

        var font = (Font)parameters["font"];

        var shapes = font.GenerateShapes(text, parameters.ContainsKey("size") ? (int?)parameters["size"] : null);

        if (!parameters.ContainsKey("bevelThickness") || parameters["bevelThickness"] == null)
            parameters["bevelThickness"] = 10;
        if (!parameters.ContainsKey("bevelSize") || parameters["bevelSize"] == null) parameters["bevelSize"] = 8;
        if (!parameters.ContainsKey("bevelEnabled") || parameters["bevelEnabled"] == null)
            parameters["bevelEnabled"] = false;

        parameters["depth"] = parameters.ContainsKey("height") ? (int)parameters["height"] : 50;

        Init(shapes, parameters);
    }
}