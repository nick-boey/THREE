﻿namespace THREE
{
    [Serializable]
    public class CubicBezierCurve3 : Curve
    {
        public Vector3 V0, V1, V2, V3;

        public CubicBezierCurve3(Vector3? v0 = null, Vector3? v1 = null, Vector3? v2 = null, Vector3? v3 = null)
        {
            V0 = v0 ?? new Vector3();
            V1 = v1 ?? new Vector3();
            V2 = v2 ?? new Vector3();
            V3 = v3 ?? new Vector3();
        }

        protected CubicBezierCurve3(CubicBezierCurve3 source)
        {
            V0?.Copy(source.V0);
            V1?.Copy(source.V1);
            V2?.Copy(source.V2);
            V3?.Copy(source.V3);
        }

        public new object Clone()
        {
            return new CubicBezierCurve3(this);
        }

        public override Vector3 GetPoint(float t, Vector3? optionalTarget = null)
        {
            var point = optionalTarget ?? new Vector3();


            point.Set(
                Interpolations.CubicBezier(t, V0.X, V1.X, V2.X, V3.X),
                Interpolations.CubicBezier(t, V0.Y, V1.Y, V2.Y, V3.Y),
                Interpolations.CubicBezier(t, V0.Z, V1.Z, V2.Z, V3.Z)
            );

            return point;
        }
    }
}