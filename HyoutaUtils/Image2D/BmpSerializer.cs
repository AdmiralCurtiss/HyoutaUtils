using System;
using System.IO;

namespace HyoutaUtils.Image2D;

public class BmpSerializer {
    struct BitInfo {
        public readonly byte Shift;
        public readonly byte Count;

        public BitInfo(byte shift, byte count) {
            Shift = shift;
            Count = count;
        }
    }

    // returns null if bits in mask are not consecutive
    static BitInfo? BitmaskToShiftAndSize(uint bitmask) {
        // find first set bit
        byte shift = 0;
        while (shift < 32u) {
            if (((bitmask >> shift) & 1u) != 0u) {
                break;
            }

            ++shift;
        }

        if (shift == 32u) {
            // nothing set
            return new BitInfo(0, 0);
        }

        // find first unset bit after set bit
        byte shift2 = (byte)(shift + 1u);
        while (shift2 < 32u) {
            if (((bitmask >> shift2) & 1u) == 0u) {
                break;
            }

            ++shift2;
        }

        byte count = (byte)(shift2 - shift);
        if (count == 32u) {
            // everything set
            return new BitInfo(0, 32);
        }

        // all remaining bits must be unset
        byte shift3 = (byte)(shift2 + 1u);
        while (shift3 < 32u) {
            if (((bitmask >> shift3) & 1u) != 0u) {
                return null;
            }

            ++shift3;
        }

        return new BitInfo(shift, count);
    }

    struct NormalizedBitInfo {
        public readonly byte RightShift;
        public readonly byte LeftShift;
        public readonly uint Mask;

        public NormalizedBitInfo(byte rightShift, byte leftShift, uint mask) {
            RightShift = rightShift;
            LeftShift = leftShift;
            Mask = mask;
        }
    }

    static uint BitCountToMask(uint count) {
        if (count >= 32u) {
            return 0xffffffffu;
        }

        return (1u << ((int)count)) - 1u;
    }

    static NormalizedBitInfo NormalizeBitsTo(BitInfo bits, byte count) {
        if (count == 0) {
            return new NormalizedBitInfo(0, 0, 0);
        }

        if (count <= bits.Count) {
            // want less (or same as) bits than we have, just right-shift more
            int diff = bits.Count - count;
            int rightShift = bits.Shift + diff;
            if (rightShift >= 32u) {
                return new NormalizedBitInfo(0, 0, 0);
            }

            return new NormalizedBitInfo((byte)rightShift, 0, BitCountToMask(count));
        }

        // want more bits than we have, need to add a left shift and also shift the mask
        uint oldMask = BitCountToMask(bits.Count);
        int leftShift = (count - bits.Count);
        return new NormalizedBitInfo(bits.Shift, (byte)leftShift, oldMask << leftShift);
    }

    struct PixelTranslator {
        uint colorMaskRed;
        uint colorMaskGreen;
        uint colorMaskBlue;
        uint colorMaskAlpha;
        int rightShiftRed;
        int rightShiftGreen;
        int rightShiftBlue;
        int rightShiftAlpha;
        int leftShiftRed;
        int leftShiftGreen;
        int leftShiftBlue;
        int leftShiftAlpha;
        bool hasAlpha;

        public PixelTranslator(
            uint colorMaskRed,
            uint colorMaskGreen,
            uint colorMaskBlue,
            uint colorMaskAlpha,
            int rightShiftRed,
            int rightShiftGreen,
            int rightShiftBlue,
            int rightShiftAlpha,
            int leftShiftRed,
            int leftShiftGreen,
            int leftShiftBlue,
            int leftShiftAlpha,
            bool hasAlpha
        ) {
            this.colorMaskRed = colorMaskRed;
            this.colorMaskGreen = colorMaskGreen;
            this.colorMaskBlue = colorMaskBlue;
            this.colorMaskAlpha = colorMaskAlpha;
            this.rightShiftRed = rightShiftRed;
            this.rightShiftGreen = rightShiftGreen;
            this.rightShiftBlue = rightShiftBlue;
            this.rightShiftAlpha = rightShiftAlpha;
            this.leftShiftRed = leftShiftRed;
            this.leftShiftGreen = leftShiftGreen;
            this.leftShiftBlue = leftShiftBlue;
            this.leftShiftAlpha = leftShiftAlpha;
            this.hasAlpha = hasAlpha;
        }

        public uint Translate(uint inputPixel) {
            uint r = ((inputPixel >> rightShiftRed) << leftShiftRed) & colorMaskRed;
            uint g = ((inputPixel >> rightShiftGreen) << leftShiftGreen) & colorMaskGreen;
            uint b = ((inputPixel >> rightShiftBlue) << leftShiftBlue) & colorMaskBlue;
            uint a = hasAlpha ? (((inputPixel >> rightShiftAlpha) << leftShiftAlpha) & colorMaskAlpha) : 255u;
            uint outputPixel = (r | (g << 8) | (b << 16) | (a << 24));
            return outputPixel;
        }
    }

    public static Bitmap Read(Stream stream, out uint[]? palette) {
        long startPos = stream.Position;

        ushort magic = stream.ReadUInt16();
        if (magic != 0x4d42) {
            throw new InvalidDataException("Invalid magic bytes for BMP");
        }
        uint filesize = stream.ReadUInt32();
        ushort reserved1 = stream.ReadUInt16();
        ushort reserved2 = stream.ReadUInt16();
        uint offsetPixelArray = stream.ReadUInt32();

        uint headerSize = stream.ReadUInt32();
        uint width;
        uint height;
        uint planes;
        uint bpp;
        uint compressionMethod;
        uint sizeImage;
        uint numberOfColors;
        uint colorMaskRed = 0;
        uint colorMaskGreen = 0;
        uint colorMaskBlue = 0;
        uint colorMaskAlpha = 0;
        int rightShiftRed = 0;
        int rightShiftGreen = 0;
        int rightShiftBlue = 0;
        int rightShiftAlpha = 0;
        int leftShiftRed = 0;
        int leftShiftGreen = 0;
        int leftShiftBlue = 0;
        int leftShiftAlpha = 0;
        uint bytesPerColorInPalette = 4;
        bool paletted = false;
        bool flipped = false;
        bool hasAlpha = false;
        if (headerSize == 12 || headerSize == 40) {
            if (headerSize == 12) {
                // BITMAPCOREHEADER
                bytesPerColorInPalette = 3;
                width = stream.ReadUInt16();
                height = stream.ReadUInt16();
                planes = stream.ReadUInt16();
                bpp = stream.ReadUInt16();
                compressionMethod = 0;
                sizeImage = 0;
                numberOfColors = 0;
            } else {
                // BITMAPINFOHEADER
                int w = stream.ReadInt32();
                int h = stream.ReadInt32();
                if (w < 0) {
                    throw new InvalidDataException("Negative width is not allowed in BMP");
                }

                width = (uint)w;
                if (h < 0) {
                    flipped = true;
                    height = (uint)(-(long)h);
                } else {
                    height = (uint)h;
                }

                planes = stream.ReadUInt16();
                bpp = stream.ReadUInt16();
                compressionMethod = stream.ReadUInt32();
                sizeImage = stream.ReadUInt32();
                stream.ReadUInt32(); // pixels per meter x
                stream.ReadUInt32(); // pixels per meter y
                numberOfColors = stream.ReadUInt32();
                stream.ReadUInt32(); // clrImportant
            }

            if (numberOfColors == 0 && bpp <= 8) {
                numberOfColors = (1u << (int)bpp);
            }

            switch (bpp) {
                case 1:
                case 4:
                case 8:
                    paletted = true;
                    colorMaskBlue = 0xff;
                    colorMaskGreen = 0xff;
                    rightShiftGreen = 8;
                    colorMaskRed = 0xff;
                    rightShiftRed = 16;
                    break;
                case 16:
                    colorMaskBlue = 0x1f;
                    leftShiftBlue = 3;
                    colorMaskGreen = 0x1f;
                    rightShiftGreen = 2;
                    colorMaskRed = 0x1f;
                    rightShiftRed = 7;
                    break;
                case 24:
                case 32:
                    colorMaskBlue = 0xff;
                    rightShiftBlue = 0;
                    colorMaskGreen = 0xff;
                    rightShiftGreen = 8;
                    colorMaskRed = 0xff;
                    rightShiftRed = 16;
                    break;
                default:
                    throw new InvalidDataException("BMP with " + bpp + " bits per pixel is undefined");
            }

            if (compressionMethod == 3 || compressionMethod == 6) {
                if (!(bpp == 16 || bpp == 32)) {
                    throw new InvalidDataException("BMP with " + bpp +
                                                   " bits per pixel cannot have BI_BITFIELDS compression");
                }

                colorMaskRed = stream.ReadUInt32();
                colorMaskGreen = stream.ReadUInt32();
                colorMaskBlue = stream.ReadUInt32();
                BitInfo? bitsR = BitmaskToShiftAndSize(colorMaskRed);
                if (bitsR == null) {
                    throw new InvalidDataException("Invalid red mask: 0x" + colorMaskRed.ToString("x8"));
                }
                BitInfo? bitsG = BitmaskToShiftAndSize(colorMaskGreen);
                if (bitsG == null) {
                    throw new InvalidDataException("Invalid red mask: 0x" + colorMaskGreen.ToString("x8"));
                }
                BitInfo? bitsB = BitmaskToShiftAndSize(colorMaskBlue);
                if (bitsB == null) {
                    throw new InvalidDataException("Invalid red mask: 0x" + colorMaskBlue.ToString("x8"));
                }
                NormalizedBitInfo normR = NormalizeBitsTo(bitsR.Value, 8);
                NormalizedBitInfo normG = NormalizeBitsTo(bitsG.Value, 8);
                NormalizedBitInfo normB = NormalizeBitsTo(bitsB.Value, 8);

                colorMaskRed = normR.Mask;
                rightShiftRed = normR.RightShift;
                leftShiftRed = normR.LeftShift;
                colorMaskGreen = normG.Mask;
                rightShiftGreen = normG.RightShift;
                leftShiftGreen = normG.LeftShift;
                colorMaskBlue = normB.Mask;
                rightShiftBlue = normB.RightShift;
                leftShiftBlue = normB.LeftShift;

                if (compressionMethod == 6) {
                    colorMaskAlpha = stream.ReadUInt32();
                    BitInfo? bitsA = BitmaskToShiftAndSize(colorMaskAlpha);
                    if (bitsA == null) {
                        throw new InvalidDataException("Invalid alpha mask: 0x" + colorMaskAlpha.ToString("x8"));
                    }
                    NormalizedBitInfo normA = NormalizeBitsTo(bitsA.Value, 8);

                    colorMaskAlpha = normA.Mask;
                    rightShiftRed = normA.RightShift;
                    leftShiftRed = normA.LeftShift;
                }

                compressionMethod = 0;
            }
        } else {
            // there's other kinds too but I don't think I need this...
            throw new NotImplementedException("BMP header of length " + headerSize + " not implemented");
        }

        if (compressionMethod != 0) {
            throw new NotImplementedException("BMP compression not implemented");
        }
        if (planes != 1) {
            throw new InvalidDataException("BMP must have exactly 1 plane");
        }

        PixelTranslator pixelTranslator = new PixelTranslator(
            colorMaskRed,
            colorMaskGreen,
            colorMaskBlue,
            colorMaskAlpha,
            rightShiftRed,
            rightShiftGreen,
            rightShiftBlue,
            rightShiftAlpha,
            leftShiftRed,
            leftShiftGreen,
            leftShiftBlue,
            leftShiftAlpha,
            hasAlpha
        );

        uint pixelBytesPerRow = (((bpp * width) + 31u) / 32u) * 4u;
        uint pixelBytesPerRowPadded = pixelBytesPerRow.Align(4);
        palette = null;
        if (paletted) {
            if (bytesPerColorInPalette == 3) {
                palette = new uint[numberOfColors];
                for (uint i = 0; i < numberOfColors; ++i) {
                    palette[i] = stream.ReadUInt24();
                }
            } else {
                palette = stream.ReadUInt32Array(numberOfColors);
            }
            for (uint i = 0; i < numberOfColors; ++i) {
                palette[i] = pixelTranslator.Translate(palette[i]);
            }
        }

        stream.Position = (startPos + offsetPixelArray);
        Bitmap bmp = new Bitmap((int)width, (int)height);
        if (width == 0 || height == 0) {
            return bmp;
        }

        byte[] rowbytes = new byte[pixelBytesPerRowPadded];
        for (uint ry = 0; ry < height; ++ry) {
            if (stream.Read(rowbytes, 0, (int)pixelBytesPerRowPadded) < pixelBytesPerRow) {
                throw new InvalidDataException("Unexpected end-of-file in pixel data of BMP");
            }
            int y = flipped ? (int)ry : (int)(height - ry - 1u);
            switch (bpp) {
                case 1:
                    if (palette == null) {
                        throw new Exception("Internal error");
                    }
                    for (uint x = 0; x < width; ++x) {
                        uint pidx = (rowbytes[x / 8u] & (1u << (7 - (int)(x % 8u)))) != 0 ? 1u : 0u;
                        bmp.SetPixel((int)x, y, new Color(palette[pidx]));
                    }
                    break;
                case 4:
                    if (palette == null) {
                        throw new Exception("Internal error");
                    }
                    for (uint x = 0; x < width; ++x) {
                        uint pidx = ((x % 2u) == 0u ? ((uint)rowbytes[x / 2u] >> 4) : (rowbytes[x / 2u])) & 0xfu;
                        bmp.SetPixel((int)x, y, new Color(palette[pidx]));
                    }
                    break;
                case 8:
                    if (palette == null) {
                        throw new Exception("Internal error");
                    }
                    for (uint x = 0; x < width; ++x) {
                        uint pidx = rowbytes[x];
                        bmp.SetPixel((int)x, y, new Color(palette[pidx]));
                    }
                    break;
                case 16:
                    for (uint x = 0; x < width; ++x) {
                        uint b0 = rowbytes[x * 2u];
                        uint b1 = rowbytes[x * 2u + 1u];
                        uint rawcolor = (b0 | (b1 << 8));
                        bmp.SetPixel((int)x, y, new Color(pixelTranslator.Translate(rawcolor)));
                    }
                    break;
                case 24:
                    for (uint x = 0; x < width; ++x) {
                        uint b0 = rowbytes[x * 3u];
                        uint b1 = rowbytes[x * 3u + 1u];
                        uint b2 = rowbytes[x * 3u + 2u];
                        uint rawcolor = (b0 | (b1 << 8) | (b2 << 16));
                        bmp.SetPixel((int)x, y, new Color(pixelTranslator.Translate(rawcolor)));
                    }
                    break;
                case 32:
                    for (uint x = 0; x < width; ++x) {
                        uint b0 = rowbytes[x * 4u];
                        uint b1 = rowbytes[x * 4u + 1u];
                        uint b2 = rowbytes[x * 4u + 2u];
                        uint b3 = rowbytes[x * 4u + 3u];
                        uint rawcolor = (b0 | (b1 << 8) | (b2 << 16) | (b3 << 24));
                        bmp.SetPixel((int)x, y, new Color(pixelTranslator.Translate(rawcolor)));
                    }
                    break;
                default:
                    throw new InvalidDataException("BMP with " + bpp + " bits per pixel is undefined");
            }
        }

        return bmp;
    }

    public static void Write(Stream stream, Bitmap bitmap) {
        uint width = (uint)bitmap.Width;
        uint height = (uint)bitmap.Height;
        bool hasAlpha = false;
        uint headerSize = 40;
        uint offsetPixelArray = headerSize + 14u + (hasAlpha ? 16u : 0u);
        ushort bpp = hasAlpha ? (ushort)32u : (ushort)24u;
        uint pixelBytesPerRow = (((bpp * width) + 31u) / 32u) * 4u;
        uint pixelBytesPerRowPadded = pixelBytesPerRow.Align(4);
        uint padding = pixelBytesPerRowPadded - pixelBytesPerRow;
        uint pixelBytesTotal = pixelBytesPerRowPadded * height;
        uint filesize = pixelBytesTotal + offsetPixelArray;

        stream.WriteUInt16(0x4d42);
        stream.WriteUInt32(filesize);
        stream.WriteUInt16(0);
        stream.WriteUInt16(0);
        stream.WriteUInt32(offsetPixelArray);

        stream.WriteUInt32(headerSize);
        stream.WriteUInt32(width);
        stream.WriteUInt32(height);
        stream.WriteUInt16(1);
        stream.WriteUInt16(bpp);
        stream.WriteUInt32(hasAlpha ? 6u : 0u);
        stream.WriteUInt32(pixelBytesTotal);
        stream.WriteUInt32(0);
        stream.WriteUInt32(0);
        stream.WriteUInt32(0);
        stream.WriteUInt32(0);

        if (hasAlpha) {
            stream.WriteUInt32(0x00ff0000u);
            stream.WriteUInt32(0x0000ff00u);
            stream.WriteUInt32(0x000000ffu);
            stream.WriteUInt32(0xff000000u);
        }

        for (uint ry = 0; ry < height; ++ry) {
            int y = (int)(height - ry - 1u);
            for (uint x = 0; x < width; ++x) {
                Color c = bitmap.GetPixel((int)x, y);
                stream.WriteUInt8((byte)c.B);
                stream.WriteUInt8((byte)c.G);
                stream.WriteUInt8((byte)c.R);
                if (hasAlpha) {
                    stream.WriteUInt8((byte)c.A);
                }
            }
            for (uint p = 0; p < padding; ++p) {
                stream.WriteUInt8(0);
            }
        }
    }
}
