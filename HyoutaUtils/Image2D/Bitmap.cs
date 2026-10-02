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
            uint[]? palette;
            return BmpSerializer.Read(file, out palette);
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
