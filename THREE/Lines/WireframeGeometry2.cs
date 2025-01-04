namespace THREE;

public class WireframeGeometry2 : LineSegmentsGeometry
{
    public WireframeGeometry2(BufferGeometry geometry)
    {
        Type = "WireframeGeometry2";
        FromWireframeGeometry(new WireframeGeometry(geometry));
    }
}