using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;

namespace rotation_tools;

[PluginName("Rotation to Tilt")]
public class RotationToTilt : IPositionedPipelineElement<IDeviceReport>
{
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

    public static float RotationToRadians(int rotation, int minRotation, uint maxRotation, uint degreesOffset)
    {
        float normalizedRotation = ((float)rotation - minRotation) / (maxRotation - minRotation);
        float degrees = normalizedRotation * 360f + degreesOffset;
        float radians = MathF.PI / 180 * degrees;
        return radians;
    }

    public Vector2 ConvertRotation(int rotation, int minRotation, uint maxRotation)
    {
        float radians = RotationToRadians(rotation, minRotation, maxRotation, RotationDegreesOffset);
        Vector2 unitCircleTilt = new(MathF.Cos(radians), MathF.Sin(radians));
        if (UnitSquare)
        {
            unitCircleTilt = CircleToSquare(unitCircleTilt);
        }
        return unitCircleTilt * TiltMultiplier;
    }

    public event Action<IDeviceReport>? Emit;

    public void Consume(IDeviceReport? value)
    {
        if (value == null) return;

        int? minRotation = TabletReference?.Properties.Specifications.Pen.MinRotation;
        uint? maxRotation = TabletReference?.Properties.Specifications.Pen.MaxRotation;
        if (value is IAbsolutePositionReport report && report is IRotationReport rotationReport && report is ITiltReport tiltReport && maxRotation != null && minRotation != null)
        {
            tiltReport.Tilt = ConvertRotation(rotationReport.Rotation, (int)minRotation, (uint)maxRotation);
            value = report;
        }

        Emit?.Invoke(value);
    }

    public PipelinePosition Position => PipelinePosition.PostTransform;

    [TabletReference]
    public TabletReference? TabletReference { set; get; }

    [Property("Tilt Multiplier"), DefaultPropertyValue(64u)]
    public uint TiltMultiplier { set; get; }

    [Property("Rotation Degrees Offset"), DefaultPropertyValue(0u), Unit("°")]
    public uint RotationDegreesOffset { set; get; }

    [BooleanProperty("Convert to unit square instead of unit circle", "")]
    public bool UnitSquare { set; get; }
}
