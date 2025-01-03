namespace THREE;

public class WireframeGeometry2 : LineSegmentsGeometry
{
    public WireframeGeometry2(BufferGeometry geometry)
    {
        type = "WireframeGeometry2";
        FromWireframeGeometry(new WireframeGeometry(geometry));
    }
}