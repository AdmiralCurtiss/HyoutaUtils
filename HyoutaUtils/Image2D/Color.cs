using System;

namespace HyoutaUtils.Image2D;

public struct Color {
    public uint ColorRGBA { get; private set; }
    public int R { get { return (int)(ColorRGBA & 0xff); } }
    public int G { get { return (int)((ColorRGBA >> 8) & 0xff); } }
    public int B { get { return (int)((ColorRGBA >> 16) & 0xff); } }
    public int A { get { return (int)(ColorRGBA >> 24); } }

    public Color(uint colorRGBA) {
        this.ColorRGBA = colorRGBA;
    }

    public static Color FromArgb(int red, int green, int blue) {
        return FromArgb(255, red, green, blue);
    }

    public static Color FromArgb(int alpha, int red, int green, int blue) {
        if (alpha < 0 || alpha > 255) {
            throw new OverflowException("alpha out of range");
        }
        if (red < 0 || red > 255) {
            throw new OverflowException("red out of range");
        }
        if (green < 0 || green > 255) {
            throw new OverflowException("green out of range");
        }
        if (blue < 0 || blue > 255) {
            throw new OverflowException("blue out of range");
        }
        return new Color((((uint)alpha) << 24) | (((uint)blue) << 16) | (((uint)green) << 8) | (((uint)red)));
    }
}
