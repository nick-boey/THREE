namespace THREE;

[Serializable]
public class ArrayCamera : PerspectiveCamera
{
    public List<Camera> Cameras = new();


    public ArrayCamera()
    {
    }

    public ArrayCamera(List<Camera> array)
    {
        Cameras = array;
    }
}