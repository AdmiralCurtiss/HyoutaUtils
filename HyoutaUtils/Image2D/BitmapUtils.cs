namespace HyoutaUtils.Image2D;

public static class BitmapUtils {
    public static bool HasAlpha(Bitmap bmp) {
        for (int y = 0; y < bmp.Height; ++y) {
            for (int x = 0; x < bmp.Width; ++x) {
                if (bmp.GetPixel(x, y).A != 255) {
                    return true;
                }
            }
        }
        return false;
    }
}
