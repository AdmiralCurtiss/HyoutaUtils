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
