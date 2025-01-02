namespace THREE
{
    [Serializable]
    public class EllipseCurve : Curve
    {
        public float Ax;

        public float Ay;

        public float XRadius;

        public float YRadius;

        public float AStartAngle;

        public float AEndAngle;

        public bool ClockWise;

        public float Rotation;


        public EllipseCurve(float? aX = null, float? aY = null, float? xRadius = null, float? yRadius = null,
            float? aStartAngle = null, float? aEndAngle = null, bool? clockwise = null, float? rotation = null) : base()
        {
            this.Ax = aX != null ? aX.Value : 0;
            this.Ay = aY != null ? aY.Value : 0;

            this.XRadius = xRadius != null ? xRadius.Value : 1;
            this.YRadius = yRadius != null ? yRadius.Value : 1;

            this.AStartAngle = aStartAngle != null ? aStartAngle.Value : 0;
            this.AEndAngle = aEndAngle != null ? aEndAngle.Value : (float)(2 * System.Math.PI);

            this.ClockWise = clockwise != null ? clockwise.Value : false;

            this.Rotation = rotation != null ? rotation.Value : 0;
        }

        protected EllipseCurve(EllipseCurve source)
        {
            this.Ax = source.Ax;
            this.Ay = source.Ay;

            this.XRadius = source.XRadius;
            this.YRadius = source.YRadius;

            this.AStartAngle = source.AStartAngle;
            this.AEndAngle = source.AEndAngle;

            this.ClockWise = source.ClockWise;

            this.Rotation = source.Rotation;
        }

        public new object Clone()
        {
            return new EllipseCurve(this);
        }

        public override Vector3 GetPoint(float t, Vector3? optionalTarget = null)
        {
            var point = optionalTarget ?? new Vector3();

            const float twoPI = (float)System.Math.PI * 2;

            float deltaAngle = this.AEndAngle - this.AStartAngle;

            bool samePoints = (float)System.Math.Abs(deltaAngle) < float.Epsilon;

            while (deltaAngle < 0) deltaAngle += twoPI;
            while (deltaAngle > twoPI) deltaAngle -= twoPI;

            if (deltaAngle < float.Epsilon)
            {
                if (samePoints)
                {
                    deltaAngle = 0;
                }
                else
                {
                    deltaAngle = twoPI;
                }
            }

            if (this.ClockWise == true && !samePoints)
            {
                if (Math.Abs(deltaAngle - twoPI) < 1e-14)
                {
                    deltaAngle = -twoPI;
                }
                else
                {
                    deltaAngle = deltaAngle - twoPI;
                }
            }

            var angle = this.AStartAngle + t * deltaAngle;
            var x = this.Ax + this.XRadius * (float)System.Math.Cos(angle);
            var y = this.Ay + this.YRadius * (float)System.Math.Sin(angle);

            if (this.Rotation != 0)
            {
                var cos = (float)System.Math.Cos(this.Rotation);
                var sin = (float)System.Math.Sin(this.Rotation);

                var tx = x - this.Ax;
                var ty = y - this.Ay;

                // Rotate the point about the center of the ellipse.
                x = tx * cos - ty * sin + this.Ax;
                y = tx * sin + ty * cos + this.Ay;
            }

            return point.Set(x, y, 0);
        }
    }
}