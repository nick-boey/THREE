using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;

namespace THREE;

[Serializable]
public class MTLLoader
{
    public string CrossOrigin;

    public MaterialCreatorOptions MaterialOptions;

    public MaterialCreator MultiMaterialCreator = new();

    public MaterialCreator Load(string filepath)
    {
        var creator = Parse(filepath);

        creator.Preload();

        if (creator.Materials != null && creator.Materials.Count > 0)
            foreach (string key in creator.Materials.Keys)
                if (!MultiMaterialCreator.Materials.ContainsKey(key))
                    MultiMaterialCreator.Materials[key] = creator.Materials[key];

        return creator;
    }

    public MaterialCreator Parse(string filepath)
    {
        var textAll = File.ReadAllText(filepath);

        var lines = textAll.Split('\n');

        Hashtable info = null;

        var delimiter_pattern = @"\s+";

        var materialsInfo = new Hashtable();

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            if (line.Length == 0 || line[0] == '#') continue;

            var pos = line.IndexOf(' ');

            var key = pos >= 0 ? line.Substring(0, pos) : line;

            key = key.ToLower();

            var value = pos >= 0 ? line.Substring(pos + 1) : "";

            if (key == "newmtl")
            {
                info = new Hashtable();
                info.Add("name", value);
                materialsInfo.Add(value, info);
            }
            else
            {
                if (key == "ka" || key == "kd" || key == "ks" || key == "ke")
                {
                    //value = value.Substring(3).Trim();
                    value = pos >= 0 ? line.Substring(pos - 1) : "";
                    value = value.Substring(pos).Trim();
                    var ss = Regex.Split(value, delimiter_pattern);
                    info.Add(key,
                        new[]
                        {
                            float.Parse(ss[0], CultureInfo.InvariantCulture.NumberFormat),
                            float.Parse(ss[1], CultureInfo.InvariantCulture.NumberFormat),
                            float.Parse(ss[2], CultureInfo.InvariantCulture.NumberFormat)
                        });
                }
                else
                {
                    info.Add(key, value);
                }
            }
        }

        var materialCreator = new MaterialCreator(filepath, MaterialOptions);

        materialCreator.SetCrossOrigin(CrossOrigin);
        materialCreator.SetMaterials(materialsInfo);

        return materialCreator;
    }

    public void SetMaterialOptions(MaterialCreatorOptions value)
    {
        MaterialOptions = value;
    }

    public struct MaterialCreatorOptions
    {
        public int? Side;

        public int? Wrap;

        public bool? NormalizeRGB;

        public bool? ignoreZeroRGBs;

        public bool? invertTrproperty;
    }

    [Serializable]
    public struct MaterialInfo
    {
        public List<int> Ks;

        public List<int> Kd;

        public List<int> Ke;

        public string Map_kd;

        public string Map_ks;

        public string Map_ke;

        public string Norm;

        public string Map_Bump;

        public string Bump;

        public string Map_d;

        public int? Ns;

        public int? D;

        public int? Tr;
    }

    [Serializable]
    public struct TexParams
    {
        public Vector2 Scale;

        public Vector2 Offset;

        public string Url;
    }

    [Serializable]
    public class MaterialCreator
    {
        public string CrossOrigin = "anonymous";
        public string FilePath;

        public Hashtable Materials = new();

        public List<Material> MaterialsArray = new();

        public Hashtable MaterialsInfo = new();

        public Hashtable NameLookup = new();

        public MaterialCreatorOptions? Options;

        public int Side;

        public int Wrap;

        public MaterialCreator()
        {
        }

        public MaterialCreator(string path, MaterialCreatorOptions? options = null)
        {
            FilePath = System.IO.Path.GetDirectoryName(path);

            Options = options;

            Side = options != null && options.Value.Side != null ? Options.Value.Side.Value : Constants.FrontSide;

            Wrap = options != null && options.Value.Wrap != null ? Options.Value.Wrap.Value : Constants.RepeatWrapping;
        }

        public MaterialCreator SetCrossOrigin(string value)
        {
            CrossOrigin = value;

            return this;
        }

        public void SetMaterials(Hashtable materialsInfo)
        {
            MaterialsInfo = Convert(materialsInfo);
        }

        public Hashtable Convert(Hashtable materialsInfo)
        {
            if (Options == null) return materialsInfo;

            var Converted = new Hashtable();

            foreach (string mn in materialsInfo.Keys)
            {
                var mat = materialsInfo[mn] as Hashtable;

                var covmat = new Hashtable();

                Converted[mn] = covmat;

                foreach (string prop in mat.Keys)
                {
                    var save = true;
                    var value = mat[prop] as float[];
                    var lprop = prop.ToLower();

                    switch (lprop)
                    {
                        case "kd":
                        case "ka":
                        case "ks":
                            if (Options != null && Options.Value.NormalizeRGB != null &&
                                Options.Value.NormalizeRGB.Value)
                                value = new float[3] { value[0] / 255.0f, value[1] / 255.0f, value[2] / 255.0f };

                            if (Options != null && Options.Value.ignoreZeroRGBs != null &&
                                Options.Value.ignoreZeroRGBs.Value)
                                if (value[0] == 0 && value[1] == 0 && value[2] == 0)
                                    save = false;

                            break;
                    }

                    if (save) covmat[lprop] = mat[prop];
                }
            }

            return Converted;
        }

        public void Preload()
        {
            foreach (string mn in MaterialsInfo.Keys) Create(mn);
        }

        public int GetIndex(string materialName)
        {
            return (int)NameLookup[materialName];
        }

        public List<Material> GetAsArray()
        {
            var index = 0;

            MaterialsArray.Clear();

            foreach (string mn in MaterialsInfo.Keys)
            {
                MaterialsArray.Add(Create(mn));
                NameLookup[mn] = index;
                index++;
            }

            return MaterialsArray;
        }

        public Material Create(string materialName)
        {
            if (!Materials.ContainsKey(materialName)) CreateMaterial(materialName);

            return Materials[materialName] as Material;
        }

        public Material CreateMaterial(string materialName)
        {
            var mat = MaterialsInfo[materialName] as Hashtable;

            var parameter = new Hashtable
            {
                { "name", materialName },
                { "side", Side }
            };

            foreach (string prop in mat.Keys)
            {
                var value = mat[prop];
                float n;
                if (value is string && value.ToString() == "") continue;

                if (value == null) continue;
                switch (prop.ToLower())
                {
                    case "kd":
                        var colorArray = (float[])value;
                        parameter["color"] = new Color().FromArray((float[])value);
                        break;
                    case "ks":
                        parameter["specular"] = new Color().FromArray((float[])value);
                        break;
                    case "ke":
                        parameter["emissive"] = new Color().FromArray((float[])value);
                        break;
                    case "map_kd":
                        SetMapForType(parameter, "map", value);
                        break;
                    case "map_ks":
                        SetMapForType(parameter, "specularMap", value);
                        break;
                    case "map_ke":
                        SetMapForType(parameter, "emissiveMap", value);
                        break;
                    case "norm":
                        SetMapForType(parameter, "normalMap", value);
                        break;
                    case "map_bump":
                    case "bump":
                        SetMapForType(parameter, "bumpMap", value);
                        break;
                    case "map_d":
                        SetMapForType(parameter, "alphaMap", value);
                        parameter["transparent"] = true;
                        break;
                    case "ns":
                        parameter["shininess"] = float.Parse((string)value, CultureInfo.InvariantCulture.NumberFormat);
                        break;
                    case "d":
                        n = float.Parse((string)value, CultureInfo.InvariantCulture.NumberFormat);
                        if (n < 1)
                        {
                            parameter["opacity"] = n;
                            parameter["transparent"] = true;
                        }

                        break;
                    case "tr":
                        n = float.Parse((string)value, CultureInfo.InvariantCulture.NumberFormat);

                        if (Options != null && Options.Value.invertTrproperty != null &&
                            Options.Value.invertTrproperty.Value) n = 1 - n;

                        if (n > 0)
                        {
                            parameter["opacity"] = 1 - n;
                            parameter["transparent"] = true;
                        }

                        break;
                }
            }

            Materials[materialName] = new MeshPhongMaterial(parameter);

            return Materials[materialName] as Material;
        }

        private void SetMapForType(Hashtable parameter, string mapType, object value)
        {
            if (parameter.ContainsKey(mapType)) return;

            var texParams = GetTextureParams((string)value, parameter);
            var map = LoadTexture(System.IO.Path.Combine(FilePath, (string)texParams["url"]));

            map.Repeat.Copy((Vector2)texParams["scale"]);
            map.Offset.Copy((Vector2)texParams["offset"]);

            map.WrapS = Wrap;
            map.WrapT = Wrap;

            parameter[mapType] = map;
        }

        private Hashtable GetTextureParams(string value, Hashtable matParams)
        {
            var texParams = new Hashtable
            {
                { "scale", new Vector2(1, 1) },
                { "offset", new Vector2(0, 0) }
            };

            var pattern = @"\s+";
            List<string> items = Regex.Split(value, pattern).ToList();
            var pos = -1;
            for (var i = 0; i < items.Count; i++)
                if (items[i].IndexOf("-bm") > -1)
                {
                    pos = i;
                    break;
                }

            if (pos >= 0)
            {
                matParams["bumpScale"] = float.Parse(items[pos + 1], CultureInfo.InvariantCulture.NumberFormat);
                items.Splice(pos, 2);
            }

            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].IndexOf("-s") > -1)
                {
                    pos = i;
                    break;
                }

                pos = -1;
            }

            if (pos >= 0)
            {
                var scale = texParams["scale"] as Vector2;
                scale.Set(float.Parse(items[pos + 1]),
                    float.Parse(items[pos + 2], CultureInfo.InvariantCulture.NumberFormat));
                items.Splice(pos, 4);
            }

            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].IndexOf("-o") > -1)
                {
                    pos = i;
                    break;
                }

                pos = -1;
            }

            if (pos >= 0)
            {
                var offset = texParams["offset"] as Vector2;
                offset.Set(float.Parse(items[pos + 1]),
                    float.Parse(items[pos + 2], CultureInfo.InvariantCulture.NumberFormat));
                items.Splice(pos, 4);
            }

            texParams["url"] = string.Join(" ", items).Trim();

            return texParams;
        }

        public Texture LoadTexture(string filePath, int? mapping = null)
        {
            var texture = TextureLoader.Load(filePath);

            if (mapping != null) texture.Mapping = mapping.Value;

            return texture;
        }
    }
}