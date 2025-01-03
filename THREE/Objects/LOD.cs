using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public struct LevelStruct
{
    public float distance;
    public Object3D object3D;
}

[Serializable]
public class LOD : Object3D
{
    public bool AutoUpdate = true;


    public List<LevelStruct> Levels = new();

    private Vector3 v1 = Vector3.Zero();

    private Vector3 v2 = Vector3.Zero();

    public LOD()
    {
        type = "LOD";
    }

    public LOD(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    protected LOD(LOD other) : base(other)
    {
        var levels = other.Levels;

        for (var i = 0; i < levels.Count; i++)
        {
            var level = levels[i];
            AddLevel((Object3D)level.object3D.Clone(), level.distance);
        }

        AutoUpdate = other.AutoUpdate;
    }

    public LOD AddLevel(Object3D object3D, float? distance)
    {
        if (distance == null) distance = 0;

        distance = Math.Abs((float)distance);

        var levels = Levels;

        var l = 0;
        for (l = 0; l < levels.Count; l++)
            if (distance < levels[l].distance)
                break;

        var level = new LevelStruct { distance = distance.Value, object3D = object3D };

        if (l >= Levels.Count)
            Levels.Add(level);
        else
            Levels.Insert(l, level);

        Add(object3D);

        return this;
    }

    public Object3D GetObjectForDistance(float distance)
    {
        int i;
        if (Levels.Count > 0)
        {
            for (i = 0; i < Levels.Count; i++)
                if (distance < Levels[i].distance)
                    break;
            return Levels[i - 1].object3D;
        }

        return null;
    }

    public override void Raycast(Raycaster raycaster, List<Intersection> intersectionList)
    {
        if (Levels.Count > 0)
        {
            v1.SetFromMatrixPosition(MatrixWorld);
            var distance = raycaster.ray.origin.DistanceTo(v1);
            GetObjectForDistance(distance).Raycast(raycaster, intersectionList);
        }
    }

    public void Update(Camera camera)
    {
        var levels = Levels;

        if (levels.Count > 1)
        {
            v1.SetFromMatrixPosition(camera.MatrixWorld);
            v2.SetFromMatrixPosition(MatrixWorld);

            var distance = v1.DistanceTo(v2);

            levels[0].object3D.Visible = true;

            int i, l = levels.Count;
            for (i = 1; i < l; i++)
                if (distance >= levels[i].distance)
                {
                    levels[i - 1].object3D.Visible = false;
                    levels[i].object3D.Visible = true;
                }
                else
                {
                    break;
                }

            for (; i < l; i++) levels[i].object3D.Visible = false;
        }
    }
}