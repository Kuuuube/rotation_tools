# Rotation Tools Plugin for [OpenTabletDriver](https://github.com/OpenTabletDriver/OpenTabletDriver)

Filters to help with barrel rotation on tablet pens.

## Rotation to Tilt

Some drawing software doesn't support rotation but almost all support tilt. Rotation can be converted cleanly into tilt.

- `Tilt Multiplier` (Default: `64`): Tilt generally caps out at `-64` to `64`. Using a multiplier of 64 gives the highest precision but if you'd like a less extreme angle on brushes you can lower this.

- `Convert to unit square instead of unit circle` (Default: `false`): Typically tilt values operate on a square but rotation is simply an angle so it could be converted to either a unit circle or unit square. In most cases you wont see any change from this option.

    Extreme tilt values such as `[64, 64]` are at one of the corners of the square. The same tilt value mapped to a circle is `[45.25, 45.25]`.

    Both of these values define the same angle but the unit circle is consistent in its distance whereas the unit square gets further from the center towards the corners.

- `Rotation Degrees Offset` (Default: `0`): Offsets the converted tilt angle. If the angle of your brush is incorrect, change this value. You likely want `90`, `180`, or `270`.
