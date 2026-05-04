using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;

namespace rotation_tools;

[PluginName("Rotation to Tilt")]
public class RotationToTilt : RotationToTiltBase {
    // Uses simple stretch mapping
    public static Vector2 CircleToSquare(Vector2 point)
    {
        double u = point.X;
        double v = point.Y;

        double u2 = u * u;
        double v2 = v * v;

        double sgnu = Math.Sign(u);
        double sgnv = Math.Sign(v);

        if (u2 >= v2)
        {
            return new(
                (float)(sgnu * Math.Sqrt(u2 + v2)),
                (float)(sgnu * (v / u) * Math.Sqrt(u2 + v2))
            );
        }
        else
        {
            return new(
                (float)(sgnv * (u / v) * Math.Sqrt(u2 + v2)),
                (float)(sgnv * Math.Sqrt(u2 + v2))
            );
        }
    }

    public static float RotationToRadians(uint rotation, uint maxRotation, uint degreesOffset)
    {
        float degrees = (float)rotation / (float)maxRotation * 360f + degreesOffset;
        float radians = MathF.PI / 180 * degrees;
        return radians;
    }

    public Vector2 ConvertRotation(uint rotation, uint maxRotation) {
        float radians = RotationToRadians(rotation, maxRotation, RotationDegreesOffset);
        Vector2 unitCircleTilt = new(MathF.Cos(radians), MathF.Sin(radians));
        if (UnitSquare) {
            unitCircleTilt = CircleToSquare(unitCircleTilt);
        }
        return unitCircleTilt * TiltMultiplier;
    }

    public override event Action<IDeviceReport>? Emit;

    public override void Consume(IDeviceReport value) {
        uint? maxRotation = GetMaxRotation();
        if (value is IAbsolutePositionReport report && report is IRotationReport rotationReport && report is ITiltReport tiltReport && maxRotation != null) {
            tiltReport.Tilt = ConvertRotation(rotationReport.Rotation, (uint)maxRotation);
            value = report;
        }

        Emit?.Invoke(value);
    }

    public override PipelinePosition Position => PipelinePosition.PostTransform;

    [Property("Tilt Multiplier"), DefaultPropertyValue(64u)]
    public uint TiltMultiplier { set; get; }

    [BooleanProperty("Convert to unit square instead of unit circle", "")]
    public bool UnitSquare { set; get; }

    [Property("Rotation Degrees Offset"), DefaultPropertyValue(0u), Unit("°")]
    public uint RotationDegreesOffset { set; get; }
}
