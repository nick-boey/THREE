﻿/***
 *
 * */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace THREE;

internal class EmptyXmlAttribute : XmlAttribute
{
    public EmptyXmlAttribute(string prefix, string localName, string namespaceURI, XmlDocument doc) : base(prefix,
        localName, namespaceURI, doc)
    {
    }
}

public class X3DLoader
{
    private readonly Hashtable defines = new();
    private readonly string float2_pattern = @"([\d\.\+\-e]+)\s+([\d\.\+\-e]+)";
    private readonly string float3_pattern = @"([\d\.\+\-e]+)\s+([\d\.\+\-e]+)\s+([\d\.\+\-e]+)";
    private List<float> angles = new();
    private List<float> colors = new();
    private string float_pattern = @"(\b|\-|\+)([\d\.e]+)";
    private Object3D group;
    private List<List<int>> indexes = new();
    private bool isRecordingAngles;
    private bool isRecordingColors;
    private bool isRecordingFaces;

    private bool isRecordingPoints;
    private List<Vector3> points = new();
    private string recordingFieldname;
    private List<Vector2> uvPoints = new();

    public void Load(string fileName)
    {
        var text = File.ReadAllText(fileName);
        var x3dxml = new XmlDocument();
        x3dxml.LoadXml(text);

        XmlNode root = x3dxml.DocumentElement;
        var scene = root.SelectSingleNode("Scene");

        group = new Object3D();
        renderNode(getTree(scene), group);
    }

    public Object3D GetX3DObject()
    {
        return group;
    }

    private Hashtable getTree(XmlNode sceneNode)
    {
        var tree = new Hashtable { { "children", new List<Hashtable>() } };
        for (var i = 0; i < sceneNode.ChildNodes.Count; i++) parseChildren(sceneNode.ChildNodes[i], tree);
        return tree;
    }

    private float[] FlattenVector3(List<Vector3> points)
    {
        var result = new List<float>();
        for (var i = 0; i < points.Count; i++)
        {
            result.Add(points[i].X);
            result.Add(points[i].Y);
            result.Add(points[i].Z);
        }

        return result.ToArray();
    }

    private float[] FlattenVector2(List<Vector2> points)
    {
        var result = new List<float>();
        for (var i = 0; i < points.Count; i++)
        {
            result.Add(points[i].X);
            result.Add(points[i].Y);
        }

        return result.ToArray();
    }

    private void parseChildren(XmlNode parentNode, Hashtable parentResult)
    {
        for (var i = 0; i < parentNode.ChildNodes.Count; i++)
        {
            var currentNode = parentNode.ChildNodes[i];
            if (currentNode.NodeType != XmlNodeType.None && currentNode.NodeType != XmlNodeType.Comment)
            {
                if (currentNode.Attributes.Count <= 0) continue;
                var nodeAttr = currentNode.Attributes.Count <= 0
                    ? new EmptyXmlAttribute("", "", "", null)
                    : currentNode.Attributes[0];
                ;
                var newChild = new Hashtable();
                newChild["name"] = nodeAttr.Value;
                newChild["nodeType"] = currentNode.Name.ToLower();
                newChild["string"] = getNodeGroup(currentNode.Name) + " " + currentNode.Name.ToLower() + " " +
                                     nodeAttr.Name + " " + nodeAttr.Value;
                newChild["parent"] = parentResult;
                newChild["attributes"] = currentNode.Attributes;
                newChild["children"] = new List<Hashtable>();
                (parentResult["children"] as List<Hashtable>).Add(newChild);

                for (var j = 0; j < currentNode.Attributes.Count; j++)
                    parseAttribute(newChild, currentNode.Attributes[j].Name, currentNode.Attributes[j].Value);

                if (currentNode.ChildNodes.Count > 0) parseChildren(currentNode, newChild);
            }
        }
    }

    private void parseAttribute(Hashtable node, string name, string value)
    {
        var fieldName = name;
        var parts = new List<string>();
        var index = new List<int>();
        parts.Add(name);
        var valuePattern = @"[\s,\[\]]+";
        fieldName = name;
        parts.AddRange(Regex.Split(value, valuePattern).ToList());
        // trigger several recorders
        switch (fieldName)
        {
            case "skyAngle":
            case "groundAngle":
                recordingFieldname = fieldName;
                isRecordingAngles = true;
                angles = new List<float>();
                break;
            case "skyColor":
            case "groundColor":
                recordingFieldname = fieldName;
                isRecordingColors = true;
                colors = new List<float>();
                break;
            case "point":
                recordingFieldname = fieldName;
                isRecordingPoints = true;
                points = new List<Vector3>();
                uvPoints = new List<Vector2>();
                break;
            case "coordIndex":
            case "texCoordIndex":
                recordingFieldname = fieldName;
                isRecordingFaces = true;
                indexes = new List<List<int>>();
                break;
        }

        if (isRecordingFaces)
        {
            if (parts.Count > 0)
            {
                index = new List<int>();
                for (var ind = 0; ind < parts.Count; ind++)
                {
                    if (!int.TryParse(parts[ind], out var val)) continue;

                    // end of current face
                    if (parts[ind] == "-1")
                    {
                        if (index.Count > 0) indexes.Add(index);

                        // start new one
                        index = new List<int>();
                    }
                    else
                    {
                        index.Add(int.Parse(parts[ind]));
                    }
                }
            }

            isRecordingFaces = false;
            node[recordingFieldname] = indexes;
        }
        else if (isRecordingPoints)
        {
            if (node["nodeType"] as string == "coordinate")
            {
                var coords = Regex.Split(value, float3_pattern).ToList();
                coords.RemoveAll(s => string.IsNullOrWhiteSpace(s));
                var list = coords.Select(s => float.Parse(s, CultureInfo.InvariantCulture.NumberFormat)).ToList();
                for (var i = 0; i < list.Count; i += 3) points.Add(new Vector3(list[i], list[i + 1], list[i + 2]));
                node["points"] = points;
            }

            if (node["nodeType"] as string == "texturecoordinate")
            {
                var coords = Regex.Split(value, float2_pattern).ToList();
                coords.RemoveAll(s => string.IsNullOrWhiteSpace(s));
                var list = coords.Select(s => float.Parse(s, CultureInfo.InvariantCulture.NumberFormat)).ToList();
                for (var i = 0; i < list.Count; i += 2) uvPoints.Add(new Vector2(list[i], list[i + 1]));
                node["points"] = uvPoints;
            }

            isRecordingPoints = false;
        }
        else if (isRecordingAngles)
        {
            if (parts.Count > 0)
                for (var ind = 0; ind < parts.Count; ind++)
                {
                    if (!int.TryParse(parts[ind], out var val)) continue;
                    angles.Add(float.Parse(parts[ind], CultureInfo.InvariantCulture.NumberFormat));
                }

            isRecordingAngles = false;
            node[recordingFieldname] = angles;
        }
        else if (isRecordingColors)
        {
            var colorArray = Regex.Split(value, float3_pattern).ToList();
            colorArray.RemoveAll(s => string.IsNullOrWhiteSpace(s));
            colors.AddRange(colorArray.Select(s => float.Parse(s, CultureInfo.InvariantCulture.NumberFormat)).ToList());


            isRecordingColors = false;
            node[recordingFieldname] = colors;
        }
        else if (parts[parts.Count - 1] != null && fieldName != "children")
        {
            switch (fieldName)
            {
                case "diffuseColor":
                case "emissiveColor":
                case "specularColor":
                case "color":

                    if (parts.Count != 4)
                    {
                        Console.WriteLine("Invalid color format detected for " + fieldName);
                        break;
                    }

                    var color = new Color(float.Parse(parts[1], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(parts[2], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(parts[3], CultureInfo.InvariantCulture.NumberFormat));
                    node[fieldName] = color;
                    break;

                case "translation":
                case "scale":
                case "size":
                case "position":
                    if (parts.Count != 4)
                    {
                        Console.WriteLine("Invalid vector format detected for " + fieldName);
                        break;
                    }

                    var vector = new Vector3(float.Parse(parts[1], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(parts[2], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(parts[3], CultureInfo.InvariantCulture.NumberFormat));
                    node[fieldName] = vector;
                    break;

                case "radius":
                case "topRadius":
                case "bottomRadius":
                case "height":
                case "transparency":
                case "shininess":
                case "ambientIntensity":
                case "creaseAngle":
                case "fieldOfView":
                    if (parts.Count != 2)
                    {
                        Console.WriteLine("Invalid single float value specification detected for " + fieldName);
                        break;
                    }

                    var fieldOfView = float.Parse(parts[1], CultureInfo.InvariantCulture.NumberFormat);
                    node[fieldName] = fieldOfView;
                    break;

                case "rotation":
                case "orientation":
                    if (parts.Count != 5)
                    {
                        Console.WriteLine("Invalid quaternion format detected for " + fieldName);
                        break;
                    }

                    var vec4 = new Vector4(float.Parse(parts[1], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(parts[2], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(parts[3], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(parts[4], CultureInfo.InvariantCulture.NumberFormat));
                    node[fieldName] = vec4;
                    break;

                case "ccw":
                case "solid":
                case "colorPerVertex":
                case "convex":
                    if (parts.Count != 2)
                    {
                        Console.WriteLine("Invalid format detected for " + fieldName);
                        break;
                    }

                    var convex = parts[1] == "TRUE" ? true : false;
                    node[fieldName] = convex;
                    break;

                case "url":
                    var url = "";
                    if (parts.Count >= 3)
                        url = parts[1] + "," + parts[2];
                    else
                        url = parts[1];
                    node[fieldName] = url;
                    break;
            }
        }
    }

    private void renderNode(Hashtable data, Object3D parent)
    {
        var stringValue = "";
        if (data.Contains("string"))
        {
            stringValue = data["string"] as string;

            if (stringValue.Contains(" USE "))
            {
                var match = Regex.Match(stringValue, @"USE\s+?([-\w]+)");
                var defineKey = match.Success ? match.Groups[1].Value : null;

                if (!defines.ContainsKey(defineKey))
                {
#if DEBUG
                    Console.WriteLine("{0} is not defined", defineKey);
#endif
                }
                else
                {
                    if (stringValue.Contains("appearance ") && defineKey != null)
                    {
                        parent.Material = defines[defineKey] as Material;
                    }
                    else if (stringValue.Contains("geometry ") && defineKey != null)
                    {
                        parent.Geometry = (defines[defineKey] as BufferGeometry).Clone();
                    }
                    else if (defineKey != null)
                    {
                        var obj = (defines[defineKey] as Object3D).Clone();
                        (obj as Object3D).Name = (data["parent"] as Hashtable)["name"] as string;
                        parent.Add(obj as Object3D);
                    }
                }

                return;
            }
        }

        var currentObject = parent;
        if ("viewpoint" == data["nodeType"] as string)
        {
            var p = data["position"] as Vector3;
            parent.UserData["cameraPosition"] = p.Clone();

            var r = data["orientation"] as Vector3;
            parent.UserData["cameraOrientation"] = r.Clone();

            parent.UserData["cameraFieldOfView"] = (float)data["fieldOfView"];
        }
        else if ("transform" == (string)data["nodeType"] || "group" == (string)data["nodeType"])
        {
            currentObject = new Object3D();
            stringValue = data["string"] as string;
            if (stringValue.Contains(" DEF"))
            {
                var match = Regex.Match(stringValue, @"DEF\s+(\w+)");
                currentObject.Name = match.Success ? match.Groups[1].Value : "";
                defines[currentObject.Name] = currentObject;
            }
            else if ((data["attributes"] as XmlAttributeCollection).Count > 1)
            {
                var def = (data["attributes"] as XmlAttributeCollection)[1].Name;
                if (def == "DEF")
                {
                    currentObject.Name = (data["attributes"] as XmlAttributeCollection)[1].Value;
                    defines[currentObject.Name] = currentObject;
                }
            }

            if (data.ContainsKey("translation"))
            {
                var t = (Vector3)data["translation"];

                currentObject.Position.Set(t.X, t.Y, t.Z);
            }

            if (data.ContainsKey("rotation"))
            {
                var r = (Vector4)data["rotation"];

                currentObject.Quaternion.SetFromAxisAngle(new Vector3(r.X, r.Y, r.Z), r.W);
            }

            if (data.ContainsKey("scale"))
            {
                var s = (Vector3)data["scale"];

                currentObject.Scale.Set(s.X, s.Y, s.Z);
            }

            parent.Add(currentObject);
        }
        else if ("shape" == (string)data["nodeType"])
        {
            stringValue = data["string"] as string;
            currentObject = new Mesh();
            if (stringValue.Contains(" DEF"))
            {
                var match = Regex.Match(stringValue, @"DEF\s+(\w+)");
                currentObject.Name = match.Success ? match.Groups[1].Value : "";
                defines[currentObject.Name] = currentObject;
            }


            parent.Add(currentObject);
        }
        else if ("background" == (string)data["nodeType"])
        {
            var segments = 20;

            // sky (full sphere):

            var radius = 2e4;

            var skyGeometry = new SphereBufferGeometry((float)radius, segments, segments);
            var skyMaterial = new MeshBasicMaterial { Fog = false, Side = Constants.BackSide };

            if (data["skyColor"] != null && (data["skyColor"] as List<float>).Count > 1)
            {
                //PaintFaces(skyGeometry, radius, data.skyAngle, data.skyColor, true);

                skyMaterial.VertexColors = true;
            }
            else
            {
                var color = (Color)data["skyColor"];
                skyMaterial.Color.Value.SetRGB(color.R, color.G, color.B);
            }

            group.Add(new Mesh(skyGeometry, skyMaterial));

            // ground (half sphere):

            if (data.ContainsKey("groundColor"))
            {
                radius = 1.2e4;

                var groundGeometry = new SphereBufferGeometry((float)radius, segments, segments, 0,
                    (float)(2 * Math.PI), (float)(0.5 * Math.PI), (float)(1.5 * Math.PI));
                var groundMaterial = new MeshBasicMaterial
                    { Fog = false, Side = Constants.BackSide, VertexColors = true };

                //paintFaces(groundGeometry, radius, data.groundAngle || [], data.groundColor, false);

                group.Add(new Mesh(groundGeometry, groundMaterial));
            }
        }
        else if (data["string"] != null && (data["string"] as string).Contains("geometry "))
        {
            if ("box" == (string)data["nodeType"])
            {
                var s = (Vector3)data["size"];
                if (s != null)
                    parent.Geometry = new BoxBufferGeometry(s.X, s.Y, s.Z);
                else
                    parent.Geometry = new BoxBufferGeometry();
            }
            else if ("cylinder" == (string)data["nodeType"])
            {
                parent.Geometry = new CylinderBufferGeometry((float)data["radius"], (float)data["radius"],
                    (float)data["height"]);
            }
            else if ("cone" == (string)data["nodeType"])
            {
                parent.Geometry = new CylinderBufferGeometry((float)data["topRadius"], (float)data["bottomRadius"],
                    (float)data["height"]);
            }
            else if ("sphere" == (string)data["nodeType"])
            {
                parent.Geometry = new SphereBufferGeometry((float)data["radius"]);
            }
            else if ("indexedfaceset" == (string)data["nodeType"])
            {
                var geometry = new BufferGeometry();

                List<Vector3> points = null;
                List<Vector2> uvs = null;
                List<int> indexes, uvIndexes;

                for (var i = 0; i < (data["children"] as List<Hashtable>).Count; i++)
                {
                    var child = (data["children"] as List<Hashtable>)[i];


                    if ("texturecoordinate" == (string)child["nodeType"]) uvs = (List<Vector2>)child["points"];


                    if ("coordinate" == (string)child["nodeType"])
                    {
                        if (child.ContainsKey("points"))
                        {
                            points = (List<Vector3>)child["points"];
                            geometry.SetAttribute("position", new BufferAttribute<float>(FlattenVector3(points), 3));
                        }

                        var stringValue1 = (string)data["string"];
                        if (stringValue1.IndexOf("DEF") > -1)
                        {
                            var match = Regex.Match(stringValue1, @"DEF\s+(\w+)");
                            var name = match.Success ? match.Groups[1].Value : "";
                            defines[name] = geometry.Attributes["position"];
                        }

                        if (stringValue1.IndexOf("USE") > -1)
                        {
                            var match = Regex.Match(stringValue1, @"USE\s+(\w+)");
                            var defineKey = match.Success ? match.Groups[1].Value : "";
                            geometry.Attributes["position"] = defines[defineKey];
                        }
                    }
                }

                var skip = 0;

                // some shapes only have vertices for use in other shapes
                if (data.ContainsKey("coordIndex"))
                {
                    var coordIndex = data["coordIndex"] as List<List<int>>;
                    var indexedList = new List<int>();
                    var uvsList = new List<float>();
                    // read this: http://math.hws.edu/eck/cs424/notes2013/16_Threejs_Advanced.html
                    for (var i = 0; i < coordIndex.Count; i++)
                    {
                        indexes = coordIndex[i];
                        uvIndexes = coordIndex[i];

                        skip = 0;

                        // Face3 only works with triangles, but IndexedFaceSet allows shapes with more then three vertices, build them of triangles
                        while (indexes.Count >= 3 && skip < indexes.Count - 2)
                        {
                            indexedList.Add(indexes[0],
                                indexes[skip + (data["ccw"] != null && (bool)data["ccw"] ? 1 : 2)],
                                indexes[skip + (data["ccw"] != null && (bool)data["ccw"] ? 2 : 1)]);

                            if (uvs != null && uvs.Count > 0 && uvIndexes.Count > 0)
                            {
                                uvsList.Add(uvs[uvIndexes[0]].X, uvs[uvIndexes[0]].Y);
                                uvsList.Add(uvs[uvIndexes[skip + (data["ccw"] != null && (bool)data["ccw"] ? 1 : 2)]].X,
                                    uvs[uvIndexes[skip + (data["ccw"] != null && (bool)data["ccw"] ? 1 : 2)]].Y);
                                uvsList.Add(uvs[uvIndexes[skip + (data["ccw"] != null && (bool)data["ccw"] ? 2 : 1)]].X,
                                    uvs[uvIndexes[skip + (data["ccw"] != null && (bool)data["ccw"] ? 2 : 1)]].Y);
                            }

                            skip++;
                        }
                    }

                    if (indexedList.Count > 0) geometry.SetIndex(indexedList);
                    if (uvsList.Count > 0)
                        geometry.SetAttribute("uv", new BufferAttribute<float>(uvsList.ToArray(), 2));
                }
                else
                {
                    // do not add dummy mesh to the scene
                    parent.Parent.Remove(parent);
                }

                if (data["solid"] != null && (bool)data["solid"] == false) parent.Material.Side = Constants.DoubleSide;

                // we need to store it on the geometry for use with defines
                geometry.UserData["solid"] = data["solid"];

                geometry.ComputeVertexNormals();
                geometry.ComputeBoundingSphere();
                stringValue = (string)data["string"];
                // see if it's a define
                if (stringValue.IndexOf("DEF") > -1)
                {
                    var match = Regex.Match(stringValue, @"DEF\s+(\w+)");
                    geometry.Name = match.Success ? match.Groups[1].Value : "";
                    defines[geometry.Name] = geometry;
                }


                parent.Geometry = geometry;
                //parent.geometry = geometry;
            }

            return;
        }
        else if (stringValue.IndexOf("appearance") > -1)
        {
            var children = data["children"] as List<Hashtable>;
            for (var i = 0; i < children.Count; i++)
            {
                var child = children[i];

                if ("material" == (string)child["nodeType"])
                {
                    var material = new MeshPhongMaterial();

                    if (null != child["diffuseColor"])
                    {
                        var d = (Color)child["diffuseColor"];

                        material.Color = d;
                    }

                    if (null != child["emissiveColor"])
                    {
                        var e = (Color)child["emissiveColor"];

                        material.Emissive = e;
                    }

                    if (null != child["specularColor"])
                    {
                        var s = (Color)child["specularColor"];

                        material.Specular = s;
                    }

                    if (null != child["transparency"])
                    {
                        var t = (float)child["transparency"];

                        // transparency is opposite of opacity
                        material.Opacity = Math.Abs(1 - t);

                        material.Transparent = true;
                    }

                    if (stringValue.IndexOf("DEF ") > -1)
                    {
                        var match = Regex.Match(stringValue, @"DEF (\w+)");
                        material.Name = match.Success ? match.Groups[1].Value : "";
                        defines[material.Name] = material;
                    }

                    parent.Material = material;
                }

                var useImageTexture = false;
                if ("imagetexture" == (string)child["nodeType"] && useImageTexture)
                {
                    //var tex = THREE.ImageUtils.loadTexture("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAIAAACQkWg2AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABYSURBVDhPxc9BCsAgDERRj96j9WZpyI+CYxCKlL6VJfMXbfbSX8Ed8mOmAdMr8M5DNwVj2gJvaYqANXbBuoY0B4FbG1m7s592fh4Z7zx0GqCcog42vg7MHh1jhetTOqUmAAAAAElFTkSuQmCC");
                    var tex = TextureLoader.Load((string)child["url"]);
                    tex.WrapS = Constants.RepeatWrapping;
                    tex.WrapT = Constants.RepeatWrapping;

                    parent.Material.Map = tex;
                }
            }

            return;
        }


        for (var i = 0; i < (data["children"] as List<Hashtable>).Count; i++)
        {
            var child = (data["children"] as List<Hashtable>)[i];
            renderNode(child, currentObject);
        }
    }


    private string getNodeGroup(string nodeName)
    {
        string group = null;
        switch (nodeName.ToLower())
        {
            case "box":
            case "cylinder":
            case "cone":
            case "sphere":
            case "indexedfaceset":
                group = "geometry";
                break;
            case "material":
            case "imagetexture":
                group = "appearance";
                break;
            case "coordinate":
                group = "coord";
                break;
            default:
                group = "";
                break;
        }

        return group;
    }
}