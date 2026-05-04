using OpenTabletDriver;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

namespace rotation_tools;

public abstract class RotationToTiltBase : IPositionedPipelineElement<IDeviceReport>
{
    public uint? GetMaxRotation()
    {
        if (output_mode_type == OutputModeType.absolute && absolute_output_mode != null)
        {
            return absolute_output_mode.Tablet.Properties.Specifications.Pen.MaxRotation;
        }

        if (output_mode_type == OutputModeType.relative && relative_output_mode != null)
        {
            return relative_output_mode.Tablet.Properties.Specifications.Pen.MaxRotation;
        }

        TryResolveOutputMode();
        return default;
    }

    [Resolved]
    public IDriver? driver;
    private OutputModeType output_mode_type;
    private AbsoluteOutputMode? absolute_output_mode;
    private RelativeOutputMode? relative_output_mode;
    private void TryResolveOutputMode()
    {
        if (driver is Driver drv)
        {
            IOutputMode? output = drv.InputDevices
                .Where(dev => dev?.OutputMode?.Elements?.Contains(this) ?? false)
                .Select(dev => dev?.OutputMode).FirstOrDefault();

            if (output is AbsoluteOutputMode abs_output)
            {
                absolute_output_mode = abs_output;
                output_mode_type = OutputModeType.absolute;
                return;
            }
            if (output is RelativeOutputMode rel_output)
            {
                relative_output_mode = rel_output;
                output_mode_type = OutputModeType.relative;
                return;
            }
            output_mode_type = OutputModeType.unknown;
        }
    }

    public abstract event Action<IDeviceReport> Emit;
    public abstract void Consume(IDeviceReport value);
    public abstract PipelinePosition Position { get; }
}
enum OutputModeType
{
    absolute,
    relative,
    unknown
}
