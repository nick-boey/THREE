using System.Collections;
using Assimp;

namespace THREE;

public class AssimpLoader
{
    private readonly Material basicMaterial = new MeshBasicMaterial();
    private readonly HashSet<int> lineMaterialSet = new();
    private readonly HashSet<int> pointMaterialSet = new();
    private readonly Dictionary<int, Material> threeMaterials = new();
    private string FilePath;

    public Group Load(string path)
    {
        var importer = new AssimpContext();

        var scene = importer.ImportFile(path, PostProcessSteps.GenerateSmoothNormals | PostProcessSteps.Triangulate);
        FilePath = path;

        lineMaterialSet.Clear();
        pointMaterialSet.Clear();

        basicMaterial.Color = Color.Hex(0xffff00);

        foreach (var m in scene.Meshes)
            if (m.PrimitiveType == PrimitiveType.Point)
                pointMaterialSet.Add(m.MaterialIndex);
            else if (m.PrimitiveType == PrimitiveType.Line)
                lineMaterialSet.Add(m.MaterialIndex);

        CreateMaterial(scene, lineMaterialSet, pointMaterialSet);

        importer.Dispose();

        return ProcessNode(scene.RootNode, scene);
    }

    private Group ProcessNode(Assimp.Node node, Assimp.Scene scene)
    {
        var group = new Group();
        group.Name = node.Name;

        for (var i = 0; i < node.MeshCount; i++)
        {
            var mesh = scene.Meshes[node.MeshIndices[i]];
            group.Add(ProcessMesh(mesh, scene));
        }

        for (var i = 0; i < node.ChildCount; i++)
        {
            var child = ProcessNode(node.Children[i], scene);
            group.Add(child);
        }

        return group;
    }

    private Object3D ProcessMesh(Assimp.Mesh m, Assimp.Scene scene)
    {
        var verts = m.Vertices;
        var norms = m.Normals;
        var uvs = m.HasTextureCoords(0) ? m.TextureCoordinateChannels[0] : null;
        var mpositions = new List<float>();
        var mnormals = new List<float>();
        var muvs = new List<float>();
        var mindex = new List<int>();

        for (var i = 0; i < verts.Count; i++)
        {
            var pos = verts[i];
            var norm = m.HasNormals ? norms[i] : new Vector3D(0, 0, 0);
            var uv = m.HasTextureCoords(0) ? uvs[i] : new Vector3D(0, 0, 0);

            mpositions.Add(pos.X, pos.Y, pos.Z);
            mnormals.Add(norm.X, norm.Y, norm.Z);
            muvs.Add(uv.X, uv.Y);
        }

        List<Face> faces = m.Faces;
        for (var i = 0; i < faces.Count; i++)
        {
            var f = faces[i];
            if (f.IndexCount != 3)
            {
                mindex.Add(0, 0, 0);
                continue;
            }

            mindex.Add(f.Indices[0], f.Indices[1], f.Indices[2]);
        }

        var geometry = new BufferGeometry();
        geometry.SetIndex(new BufferAttribute<int>(mindex.ToArray(), 1));

        geometry.SetAttribute("position", new BufferAttribute<float>(mpositions.ToArray(), 3));

        if (m.HasNormals)
            geometry.SetAttribute("normal", new BufferAttribute<float>(mnormals.ToArray(), 3));

        if (m.HasTextureCoords(0))
            geometry.SetAttribute("uv", new BufferAttribute<float>(muvs.ToArray(), 2));

        var material = basicMaterial;
        if (scene.HasMaterials)
            material = threeMaterials[m.MaterialIndex];


        if (m.PrimitiveType == PrimitiveType.Line)
            return new LineSegments(geometry, material);
        if (m.PrimitiveType == PrimitiveType.Point)
            return new Points(geometry, material);
        //geometry.ComputeVertexNormals();
        return new Mesh(geometry, material);
    }

    private void CreateMaterial(Assimp.Scene scene, HashSet<int> linesSet, HashSet<int> pointsSet)
    {
        threeMaterials.Clear();

        for (var i = 0; i < scene.Materials.Count; i++)
        {
            var m = scene.Materials[i];
            if (!linesSet.Contains(i) && !pointsSet.Contains(i))
            {
                var parameter = new Hashtable
                {
                    { "name", m.Name },
                    { "side", m.HasTwoSided ? Constants.DoubleSide : Constants.FrontSide },
                    { "wireframe", m.IsWireFrameEnabled ? true : false },
                    {
                        "color",
                        m.HasColorDiffuse ? new Color(m.ColorDiffuse[0], m.ColorDiffuse[1], m.ColorDiffuse[2]) : null
                    },
                    {
                        "specular",
                        m.HasColorSpecular
                            ? new Color(m.ColorSpecular[0], m.ColorSpecular[1], m.ColorSpecular[2])
                            : null
                    },
                    {
                        "emissive",
                        m.HasColorEmissive
                            ? new Color(m.ColorEmissive[0], m.ColorEmissive[1], m.ColorEmissive[2])
                            : null
                    },
                    { "transparent", m.HasTransparencyFactor ? true : false }
                };
                if (m.HasShininess) parameter["shininess"] = m.Shininess;
                if (m.HasOpacity) parameter["opacity"] = m.Opacity;
                if (m.HasTextureDiffuse) parameter["map"] = LoadMaterialTexture(m.TextureDiffuse);
                if (m.HasTextureSpecular) parameter["specularMap"] = LoadMaterialTexture(m.TextureSpecular);
                if (m.HasTextureEmissive) parameter["emissiveMap"] = LoadMaterialTexture(m.TextureEmissive);
                if (m.HasTextureNormal) parameter["normalMap"] = LoadMaterialTexture(m.TextureNormal);
                var material = new MeshPhongMaterial(parameter);
                threeMaterials[i] = material;
            }
            else if (linesSet.Contains(i))
            {
                var material = new LineBasicMaterial();
                if (m.HasColorDiffuse)
                    material.Color = new Color(m.ColorDiffuse[0], m.ColorDiffuse[1], m.ColorDiffuse[2]);
                threeMaterials[i] = material;
            }
            else if (pointsSet.Contains(i))
            {
                var material = new PointsMaterial { Size = 10, SizeAttenuation = false };
                if (m.HasColorDiffuse)
                    material.Color = new Color(m.ColorDiffuse[0], m.ColorDiffuse[1], m.ColorDiffuse[2]);
                if (m.HasTextureDiffuse)
                {
                    var texture = LoadMaterialTexture(m.TextureDiffuse);
                    material.Map = texture;
                }

                threeMaterials[i] = material;
            }
        }
    }

    private Texture LoadMaterialTexture(TextureSlot slot)
    {
        var filePath = slot.FilePath;
        var texFilePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(FilePath), filePath);
        var texture = TextureLoader.Load(texFilePath);

        return texture;
    }
}