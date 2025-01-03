namespace THREE;

[Serializable]
public class SceneUtils
{
    public static Group CreateMultiMaterialObject(Geometry geometry, List<Material> materials)
    {
        var group = new Group();

        for (var i = 0; i < materials.Count; i++) group.Add(new Mesh(geometry, materials[i]));
        return group;
    }
}