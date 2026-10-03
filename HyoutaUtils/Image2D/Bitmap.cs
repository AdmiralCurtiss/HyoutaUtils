using System;
using System.IO;

namespace HyoutaUtils.Image2D;

public class Bitmap {
    public int Width { get; private set; }
    public int Height { get; private set; }
    private uint[] Data;

    public Bitmap(int width, int height) {
        this.Width = width;
        this.Height = height;
        this.Data = new uint[width * height];
    }

    public Bitmap(Bitmap copy) {
        this.Width = copy.Width;
        this.Height = copy.Height;
        this.Data = new uint[copy.Data.Length];
        Array.Copy(copy.Data, this.Data, copy.Data.Length);
    }

    public static Bitmap ReadFromFile(string path) {
        using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
            if (file.PeekUInt64() == 0x0a1a0a0d474e5089) {
                uint[]? palette;
                return PngSerializer.Read(file, out palette);
            }
            if (file.PeekUInt16() == 0x4d42) {
                uint[]? palette;
                return BmpSerializer.Read(file, out palette);
            }
            throw new NotImplementedException("Unknown file type");
        }
    }

    public void SetPixel(int x, int y, Color color) {
        if (x < 0 || x >= Width || y < 0 || y >= Height) {
            throw new ArgumentOutOfRangeException();
        }
        this.Data[y * Width + x] = color.ColorRGBA;
    }

    public Color GetPixel(int x, int y) {
        if (x < 0 || x >= Width || y < 0 || y >= Height) {
            throw new ArgumentOutOfRangeException();
        }
        return new Color(this.Data[y * Width + x]);
    }

    private Color GetInterpolatedPixel(double x, double y) {
        int leftX = Math.Clamp((int)Math.Round(x, MidpointRounding.ToNegativeInfinity), 0, Width - 1);
        int rightX = Math.Clamp((int)Math.Round(x, MidpointRounding.ToPositiveInfinity), 0, Width - 1);
        int topY = Math.Clamp((int)Math.Round(y, MidpointRounding.ToNegativeInfinity), 0, Height - 1);
        int bottomY = Math.Clamp((int)Math.Round(y, MidpointRounding.ToPositiveInfinity), 0, Height - 1);
        Color pixelTL = GetPixel(leftX, topY);
        Color pixelTR = GetPixel(rightX, topY);
        Color pixelBL = GetPixel(leftX, bottomY);
        Color pixelBR = GetPixel(rightX, bottomY);
        double distanceLeft = (x - leftX);
        double distanceTop = (y - topY);
        double weightTL = (1.0 - distanceLeft) * (1.0 - distanceTop);
        double weightTR = distanceLeft * (1.0 - distanceTop);
        double weightBL = (1.0 - distanceLeft) * distanceTop;
        double weightBR = distanceLeft * distanceTop;
        double r = weightTL * pixelTL.R + weightTR * pixelTR.R + weightBL * pixelBL.R + weightBR * pixelBR.R;
        double g = weightTL * pixelTL.G + weightTR * pixelTR.G + weightBL * pixelBL.G + weightBR * pixelBR.G;
        double b = weightTL * pixelTL.B + weightTR * pixelTR.B + weightBL * pixelBL.B + weightBR * pixelBR.B;
        double a = weightTL * pixelTL.A + weightTR * pixelTR.A + weightBL * pixelBL.A + weightBR * pixelBR.A;
        int cr = Math.Clamp((int)Math.Round(r, MidpointRounding.ToEven), 0, 255);
        int cg = Math.Clamp((int)Math.Round(g, MidpointRounding.ToEven), 0, 255);
        int cb = Math.Clamp((int)Math.Round(b, MidpointRounding.ToEven), 0, 255);
        int ca = Math.Clamp((int)Math.Round(a, MidpointRounding.ToEven), 0, 255);
        return Color.FromArgb(ca, cr, cg, cb);
    }

    public void Scale(int newWidth, int newHeight) {
        if (newWidth <= 0 || newHeight <= 0) {
            throw new ArgumentOutOfRangeException();
        }

        double oldMaxXFloat = Width - 1;
        double oldMaxYFloat = Height - 1;
        double newMaxXFloat = newWidth - 1;
        double newMaxYFloat = newHeight - 1;
        double ratioWidth = (oldMaxXFloat / newMaxXFloat);
        double ratioHeight = (oldMaxYFloat / newMaxYFloat);

        uint[] dst = new uint[newWidth * newHeight];
        for (int newY = 0; newY < newHeight; ++newY) {
            for (int newX = 0; newX < newWidth; ++newX) {
                double oldX = Math.Clamp(newX * ratioWidth, 0.0, oldMaxXFloat);
                double oldY = Math.Clamp(newY * ratioHeight, 0.0, oldMaxYFloat);
                dst[newY * newWidth + newX] = GetInterpolatedPixel(oldX, oldY).ColorRGBA;
            }
        }

        Width = newWidth;
        Height = newHeight;
        Data = dst;
    }

    public void RotateFlip(RotateFlipType type) {
        int w = Width;
        int h = Height;
        uint[] src = Data;
        uint[] dst;
        switch (type) {
            case RotateFlipType.RotateNoneFlipNone:
            case RotateFlipType.Rotate180FlipXY:
                // no change
                break;
            case RotateFlipType.Rotate90FlipNone:
            case RotateFlipType.Rotate270FlipXY:
                // a b
                // c d -> e c a
                // e f    f d b
                dst = new uint[w * h];
                for (int sy = 0; sy < h; ++sy) {
                    int dx = h - sy - 1;
                    for (int sx = 0; sx < w; ++sx) {
                        int dy = sx;
                        dst[dy * h + dx] = src[sy * w + sx];
                    }
                }
                Width = h;
                Height = w;
                Data = dst;
                break;
            case RotateFlipType.Rotate180FlipNone:
            case RotateFlipType.RotateNoneFlipXY:
                // a b    f e
                // c d -> d c
                // e f    b a
                dst = new uint[w * h];
                for (int sy = 0; sy < h; ++sy) {
                    int dy = h - sy - 1;
                    for (int sx = 0; sx < w; ++sx) {
                        int dx = w - sx - 1;
                        dst[dy * w + dx] = src[sy * w + sx];
                    }
                }
                Data = dst;
                break;
            case RotateFlipType.Rotate270FlipNone:
            case RotateFlipType.Rotate90FlipXY:
                // a b
                // c d -> b d f
                // e f    a c e
                dst = new uint[w * h];
                for (int sy = 0; sy < h; ++sy) {
                    int dx = sy;
                    for (int sx = 0; sx < w; ++sx) {
                        int dy = w - sx - 1;
                        dst[dy * h + dx] = src[sy * w + sx];
                    }
                }
                Width = h;
                Height = w;
                Data = dst;
                break;
            case RotateFlipType.RotateNoneFlipX:
            case RotateFlipType.Rotate180FlipY:
                // a b    b a
                // c d -> d c 
                // e f    f e
                dst = new uint[w * h];
                for (int sy = 0; sy < h; ++sy) {
                    int dy = sy;
                    for (int sx = 0; sx < w; ++sx) {
                        int dx = w - sx - 1;
                        dst[dy * w + dx] = src[sy * w + sx];
                    }
                }
                Data = dst;
                break;
            case RotateFlipType.Rotate90FlipX:
            case RotateFlipType.Rotate270FlipY:
                // a b
                // c d -> a c e
                // e f    b d f
                dst = new uint[w * h];
                for (int sy = 0; sy < h; ++sy) {
                    int dx = sy;
                    for (int sx = 0; sx < w; ++sx) {
                        int dy = sx;
                        dst[dy * h + dx] = src[sy * w + sx];
                    }
                }
                Width = h;
                Height = w;
                Data = dst;
                break;
            case RotateFlipType.Rotate180FlipX:
            case RotateFlipType.RotateNoneFlipY:
                // a b    e f
                // c d -> c d
                // e f    a b
                dst = new uint[w * h];
                for (int sy = 0; sy < h; ++sy) {
                    int dy = h - sy - 1;
                    for (int sx = 0; sx < w; ++sx) {
                        int dx = sx;
                        dst[dy * w + dx] = src[sy * w + sx];
                    }
                }
                Data = dst;
                break;
            case RotateFlipType.Rotate270FlipX:
            case RotateFlipType.Rotate90FlipY:
                // a b
                // c d -> f d b
                // e f    e c a
                dst = new uint[w * h];
                for (int sy = 0; sy < h; ++sy) {
                    int dx = h - sy - 1;
                    for (int sx = 0; sx < w; ++sx) {
                        int dy = w - sx - 1;
                        dst[dy * h + dx] = src[sy * w + sx];
                    }
                }
                Width = h;
                Height = w;
                Data = dst;
                break;
        }
    }

    public void Save(string path) {
        if (path.EndsWith(".bmp", StringComparison.InvariantCultureIgnoreCase)) {
            Save(path, ImageFormat.Bmp);
        } else {
            throw new NotImplementedException();
        }
    }

    public void Save(string path, ImageFormat format) {
        using (var file = new FileStream(path, FileMode.Create, FileAccess.Write)) {
            Save(file, format);
        }
    }

    public void Save(Stream stream, ImageFormat format) {
        switch (format) {
            case ImageFormat.Bmp: BmpSerializer.Write(stream, this); break;
            default: throw new NotImplementedException();
        }
    }
}
