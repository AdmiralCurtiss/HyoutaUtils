using System;
using System.Collections.Generic;
using System.IO;
using HyoutaUtils.Checksum;
using zlib_sharp;

namespace HyoutaUtils.Image2D;

public class PngSerializer {
    struct ChunkInfo {
        public uint Length;
        public uint Type;
        public long Offset;

        public ChunkInfo(uint length, uint type, long offset) {
            Length = length;
            Type = type;
            Offset = offset;
        }
    }

    public static Bitmap Read(Stream stream, out uint[]? palette) {
        ulong magic = stream.ReadUInt64();
        if (magic != 0x0a1a0a0d474e5089) {
            throw new InvalidDataException("Invalid magic bytes for PNG");
        }

        ChunkInfo? ihdrChunk = null;
        ChunkInfo? plteChunk = null;
        List<ChunkInfo> idatChunks = new List<ChunkInfo>();
        List<ChunkInfo> otherChunks = new List<ChunkInfo>();
        while (stream.Position < stream.Length) {
            uint length = stream.ReadUInt32(EndianUtils.Endianness.BigEndian);
            uint type = stream.PeekUInt32(EndianUtils.Endianness.BigEndian);
            long offset = stream.Position + 4;
            CRC32 actualChecksum = stream.CalculateCRC32FromCurrentPosition((long)length + 4);
            CRC32 expectedChecksum = new CRC32(stream.ReadUInt32(EndianUtils.Endianness.BigEndian));
            if (actualChecksum != expectedChecksum) {
                throw new InvalidDataException("Checksum error for PNG chunk");
            }
            if (type == 0x49454E44) { // IEND
                break;
            }
            if (type == 0x49484452) { // IHDR
                if (ihdrChunk != null) {
                    throw new InvalidDataException("PNG: Multiple IHDR chunks");
                }
                if (length != 13) {
                    throw new InvalidDataException("PNG: Invalid IHDR chunk size");
                }
                ihdrChunk = new ChunkInfo(length, type, offset);
                continue;
            }
            if (type == 0x504C5445) { // PLTE
                if (plteChunk != null) {
                    throw new InvalidDataException("PNG: Multiple PLTE chunks");
                }
                if ((length % 3) != 0) {
                    throw new InvalidDataException("PNG: Invalid PLTE chunk size");
                }
                plteChunk = new ChunkInfo(length, type, offset);
                continue;
            }
            if (type == 0x49444154) { // IDAT
                idatChunks.Add(new ChunkInfo(length, type, offset));
            } else {
                otherChunks.Add(new ChunkInfo(length, type, offset));
            }
        }

        // read IHDR
        if (!ihdrChunk.HasValue) {
            throw new InvalidDataException("PNG: No IHDR chunk");
        }
        stream.Position = ihdrChunk.Value.Offset;
        uint width = stream.ReadUInt32(EndianUtils.Endianness.BigEndian);
        uint height = stream.ReadUInt32(EndianUtils.Endianness.BigEndian);
        if (width == 0 || height == 0) {
            throw new InvalidDataException("PNG: Invalid image dimensions");
        }
        byte bitDepth = stream.ReadUInt8();
        byte colorType = stream.ReadUInt8();
        byte compressionMethod = stream.ReadUInt8();
        byte filterMethod = stream.ReadUInt8();
        byte interlaceMethod = stream.ReadUInt8();
        bool paletted = false;
        bool grayscale = false;
        bool hasAlpha = false;
        switch (colorType) {
            case 0:
                if (!(bitDepth == 1 || bitDepth == 2 || bitDepth == 4 || bitDepth == 8 || bitDepth == 16)) {
                    throw new InvalidDataException("PNG: Invalid bit depth");
                }
                grayscale = true;
                break;
            case 2:
                if (!(bitDepth == 8 || bitDepth == 16)) {
                    throw new InvalidDataException("PNG: Invalid bit depth");
                }
                break;
            case 3:
                if (!(bitDepth == 1 || bitDepth == 2 || bitDepth == 4 || bitDepth == 8)) {
                    throw new InvalidDataException("PNG: Invalid bit depth");
                }
                paletted = true;
                break;
            case 4:
                if (!(bitDepth == 8 || bitDepth == 16)) {
                    throw new InvalidDataException("PNG: Invalid bit depth");
                }
                grayscale = true;
                hasAlpha = true;
                break;
            case 6:
                if (!(bitDepth == 8 || bitDepth == 16)) {
                    throw new InvalidDataException("PNG: Invalid bit depth");
                }
                hasAlpha = true;
                break;
            default:
                throw new InvalidDataException("PNG: Invalid color type");
        }
        if (compressionMethod != 0) {
            throw new InvalidDataException("PNG: Invalid compression method");
        }
        if (filterMethod != 0) {
            throw new InvalidDataException("PNG: Invalid filter method");
        }
        if (!(interlaceMethod == 0 || interlaceMethod == 1)) {
            throw new InvalidDataException("PNG: Invalid filter method");
        }

        palette = null;
        if (paletted) {
            if (plteChunk == null) {
                throw new InvalidDataException("PNG: No PLTE chunk for paletted image");
            }
            stream.Position = plteChunk.Value.Offset;
            uint maxAllowedColors = (1u << bitDepth);
            uint numPaletteEntries = plteChunk.Value.Length / 3;
            if (numPaletteEntries > maxAllowedColors) {
                throw new InvalidDataException("PNG: Too many colors in palette");
            }
            palette = new uint[numPaletteEntries];
            for (uint i = 0; i < numPaletteEntries; ++i) {
                int r = stream.ReadByte();
                int g = stream.ReadByte();
                int b = stream.ReadByte();
                palette[i] = Color.FromArgb(255, r, g, b).ColorRGBA;
            }
        }
        if (interlaceMethod == 1) {
            throw new NotImplementedException();
        }

        // decompress IDAT
        MemoryStream decompressedIDAT = new MemoryStream();
        {
            uint CHUNK = 0x4000;
            int ret;
            zlib_sharp.z_stream strm = new zlib_sharp.z_stream();
            strm.input_buffer = new byte[CHUNK];
            strm.output_buffer = new byte[CHUNK];
            strm.avail_in = 0;
            strm.next_in = 0;
            ret = zlib_sharp.zlib.inflateInit(strm);
            if (ret != zlib_sharp.zlib.Z_OK) {
                throw new Exception("zlib inflateInit() failed");
            }

            foreach (ChunkInfo chunk in idatChunks) {
                stream.Position = chunk.Offset;
                long rest = chunk.Length;
                while (rest > 0) {
                    int bytesRead = stream.Read(strm.input_buffer, 0, (int)Math.Min(rest, CHUNK));
                    if (bytesRead <= 0) {
                        break;
                    }
                    strm.avail_in = (uint)bytesRead;
                    strm.next_in = 0;

                    do {
                        strm.avail_out = CHUNK;
                        strm.next_out = 0;
                        ret = zlib_sharp.zlib.inflate(strm, zlib_sharp.zlib.Z_NO_FLUSH);
                        if (ret == zlib_sharp.zlib.Z_STREAM_ERROR
                            || ret == zlib_sharp.zlib.Z_NEED_DICT
                            || ret == zlib_sharp.zlib.Z_DATA_ERROR
                            || ret == zlib_sharp.zlib.Z_MEM_ERROR) {
                            throw new Exception("zlib inflate() failed");
                        }
                        uint have = CHUNK - strm.avail_out;
                        decompressedIDAT.Write(strm.output_buffer, 0, (int)have);
                    } while (strm.avail_out == 0);

                    rest -= bytesRead;
                }
            }

            zlib_sharp.zlib.inflateEnd(strm);
        }

        decompressedIDAT.Position = 0;
        Bitmap bmp = DecodePngIdat(decompressedIDAT, width, height, bitDepth, grayscale, palette, hasAlpha);
        return bmp;
    }

    private static Bitmap DecodePngIdat(Stream idat, uint width, uint height,
        byte bitDepth, bool grayscale, uint[]? palette, bool hasAlpha) {
        uint bitsPerSample = bitDepth;
        if (!(palette != null || grayscale)) {
            bitsPerSample *= 3; // RGB for each sample
        }
        if (palette == null && hasAlpha) {
            bitsPerSample += bitDepth;
        }
        uint bytesPerCompletePixel = ((bitsPerSample + 7u) / 8u);
        int bytesPerScanline = (int)((((bitsPerSample * width) + 7u) / 8u));
        byte[] lastScanline = new byte[bytesPerScanline];
        byte[] thisScanline = new byte[bytesPerScanline];
        Bitmap bmp = new Bitmap((int)width, (int)height);
        for (uint y = 0; y < height; ++y) {
            int filterType = idat.ReadByte();
            if (idat.Read(thisScanline, 0, bytesPerScanline) != bytesPerScanline) {
                throw new InvalidDataException("PNG: Failed to read scanline");
            }
            switch (filterType) {
                case 0: { // None
                    DecodeScanline(bmp, thisScanline, width, y, bitDepth, grayscale, palette, hasAlpha);
                    break;
                }
                case 1: { // Sub
                    for (uint x = bytesPerCompletePixel; x < bytesPerScanline; ++x) {
                        uint self = thisScanline[x];
                        uint left = thisScanline[x - bytesPerCompletePixel];
                        thisScanline[x] = (byte)((self + left) & 255u);
                    }
                    DecodeScanline(bmp, thisScanline, width, y, bitDepth, grayscale, palette, hasAlpha);
                    break;
                }
                case 2: { // Up
                    for (uint x = 0; x < bytesPerScanline; ++x) {
                        uint self = thisScanline[x];
                        uint up = lastScanline[x];
                        thisScanline[x] = (byte)((self + up) & 255u);
                    }
                    DecodeScanline(bmp, thisScanline, width, y, bitDepth, grayscale, palette, hasAlpha);
                    break;
                }
                case 3: { // Average
                    for (uint x = 0; x < bytesPerScanline; ++x) {
                        uint self = thisScanline[x];
                        uint left = x >= bytesPerCompletePixel ? thisScanline[x - bytesPerCompletePixel] : 0u;
                        uint up = lastScanline[x];
                        thisScanline[x] = (byte)((self + ((left + up) / 2u)) & 255u);
                    }
                    DecodeScanline(bmp, thisScanline, width, y, bitDepth, grayscale, palette, hasAlpha);
                    break;
                }
                case 4: { // Paeth
                    for (uint x = 0; x < bytesPerScanline; ++x) {
                        int self = thisScanline[x];
                        int left = x >= bytesPerCompletePixel ? thisScanline[x - bytesPerCompletePixel] : 0;
                        int up = lastScanline[x];
                        int upLeft = x >= bytesPerCompletePixel ? lastScanline[x - bytesPerCompletePixel] : 0;
                        thisScanline[x] = (byte)((self + PaethPredictor(left, up, upLeft)) & 255);
                    }
                    DecodeScanline(bmp, thisScanline, width, y, bitDepth, grayscale, palette, hasAlpha);
                    break;
                }
                default:
                    throw new InvalidDataException("PNG: Invalid filter type for scanline");
            }

            // swap buffers for next scanline
            byte[] tmp = thisScanline;
            thisScanline = lastScanline;
            lastScanline = tmp;
        }
        return bmp;
    }

    private static int PaethPredictor(int a, int b, int c) {
        // a = left, b = above, c = upper left
        int p = a + b - c; // initial estimate
        int pa = Math.Abs(p - a); // distances to a, b, c
        int pb = Math.Abs(p - b);
        int pc = Math.Abs(p - c);
        // return nearest of a,b,c,
        // breaking ties in order a,b,c.
        if (pa <= pb && pa <= pc) {
            return a;
        } else if (pb <= pc) {
            return b;
        } else {
            return c;
        }
    }

    private static void DecodeScanline(Bitmap bmp, byte[] scanline, uint width, uint y,
        byte bitDepth, bool grayscale, uint[]? palette, bool hasAlpha) {
        if (palette != null) {
            throw new NotImplementedException();
        } else {
            if (grayscale) {
                if (hasAlpha) {
                    switch (bitDepth) {
                        case 8:
                            for (uint x = 0; x < width; ++x) {
                                int c = scanline[x * 2u];
                                int a = scanline[x * 2u + 1u];
                                bmp.SetPixel((int)x, (int)y, Color.FromArgb(a, c, c, c));
                            }
                            break;
                        case 16:
                            throw new NotImplementedException();
                        default:
                            throw new InvalidDataException("PNG: Invalid bit depth");
                    }
                } else {
                    switch (bitDepth) {
                        case 1:
                            for (uint x = 0; x < width; ++x) {
                                int c = (scanline[x / 8u] & (1u << (int)((7u - x) % 8u))) != 0 ? 255 : 0;
                                bmp.SetPixel((int)x, (int)y, Color.FromArgb(255, c, c, c));
                            }
                            break;
                        case 2:
                            for (uint x = 0; x < width; ++x) {
                                uint r = ((scanline[x / 4u] & (3u << (int)(((3u - x) % 4u) * 2u))) << (int)((x % 4u) * 2u));
                                r = (r | (r >> 2));
                                r = (r | (r >> 4));
                                int c = (int)r;
                                bmp.SetPixel((int)x, (int)y, Color.FromArgb(255, c, c, c));
                            }
                            break;
                        case 4:
                            for (uint x = 0; x < width; ++x) {
                                uint r = ((scanline[x / 2u] & (15u << (int)(((1u - x) % 2u) * 4u))) << (int)((x % 2u) * 4u));
                                r = (r | (r >> 4));
                                int c = (int)r;
                                bmp.SetPixel((int)x, (int)y, Color.FromArgb(255, c, c, c));
                            }
                            break;
                        case 8:
                            for (uint x = 0; x < width; ++x) {
                                int c = scanline[x];
                                bmp.SetPixel((int)x, (int)y, Color.FromArgb(255, c, c, c));
                            }
                            break;
                        case 16:
                            throw new NotImplementedException();
                        default:
                            throw new InvalidDataException("PNG: Invalid bit depth");
                    }
                }
            } else {
                switch (bitDepth) {
                    case 8:
                        if (hasAlpha) {
                            for (uint x = 0; x < width; ++x) {
                                int r = scanline[x * 4u];
                                int g = scanline[x * 4u + 1u];
                                int b = scanline[x * 4u + 2u];
                                int a = scanline[x * 4u + 3u];
                                bmp.SetPixel((int)x, (int)y, Color.FromArgb(a, r, g, b));
                            }
                        } else {
                            for (uint x = 0; x < width; ++x) {
                                int r = scanline[x * 3u];
                                int g = scanline[x * 3u + 1u];
                                int b = scanline[x * 3u + 2u];
                                bmp.SetPixel((int)x, (int)y, Color.FromArgb(255, r, g, b));
                            }
                        }
                        break;
                    case 16:
                        throw new NotImplementedException();
                    default:
                        throw new InvalidDataException("PNG: Invalid bit depth");
                }
            }
        }
    }

    public static void Write(Stream stream, Bitmap bitmap) {
        // very basic: always truecolor 8bpp, no filter, no interlace
        uint width = (uint)bitmap.Width;
        uint height = (uint)bitmap.Height;
        bool hasAlpha = true;

        stream.WriteUInt64(0x0a1a0a0d474e5089);

        // IHDR
        MemoryStream chunkData = new MemoryStream();
        chunkData.WriteUInt32(0x49484452, EndianUtils.Endianness.BigEndian);
        chunkData.WriteUInt32(width, EndianUtils.Endianness.BigEndian);
        chunkData.WriteUInt32(height, EndianUtils.Endianness.BigEndian);
        chunkData.WriteUInt8(8); // bit depth
        chunkData.WriteUInt8((byte)(hasAlpha ? 6 : 2)); // color type
        chunkData.WriteUInt8(0); // compression method
        chunkData.WriteUInt8(0); // filter method
        chunkData.WriteUInt8(0); // interlace method
        WriteChunk(stream, chunkData);

        // IDAT
        chunkData.SetLength(0);
        chunkData.WriteUInt32(0x49444154, EndianUtils.Endianness.BigEndian);
        {
            uint bytesPerScanline = (width * (hasAlpha ? 4u : 3u)) + 1u;
            byte[] buffer = new byte[bytesPerScanline * height];
            long p = 0;
            for (uint y = 0; y < height; ++y) {
                buffer[p] = 0;
                ++p;
                for (uint x = 0; x < width; ++x) {
                    Color c = bitmap.GetPixel((int)x, (int)y);
                    buffer[p] = (byte)c.R;
                    ++p;
                    buffer[p] = (byte)c.G;
                    ++p;
                    buffer[p] = (byte)c.B;
                    ++p;
                    if (hasAlpha) {
                        buffer[p] = (byte)c.A;
                        ++p;
                    }
                }
            }

            ulong insize = (ulong)buffer.Length;
            ulong bound = zlib.compressBound(insize);
            byte[] compressedData = new byte[bound];
            ulong size = bound;
            int result = zlib.compress2(compressedData, 0, ref size, buffer, 0, insize, zlib.Z_DEFAULT_COMPRESSION);
            if (result != zlib.Z_OK) {
                throw new Exception("PNG: zlib compression error");
            }
            chunkData.Write(compressedData, 0, (int)size);
        }
        WriteChunk(stream, chunkData);

        // IEND
        chunkData.SetLength(0);
        chunkData.WriteUInt32(0x49454E44, EndianUtils.Endianness.BigEndian);
        WriteChunk(stream, chunkData);
    }

    private static void WriteChunk(Stream stream, Stream chunk) {
        chunk.Position = 0;
        CRC32 crc = chunk.CalculateCRC32FromCurrentPosition(chunk.Length);
        chunk.Position = 0;
        stream.WriteUInt32((uint)(chunk.Length - 4), EndianUtils.Endianness.BigEndian);
        chunk.CopyTo(stream);
        stream.WriteUInt32(crc.Value, EndianUtils.Endianness.BigEndian);
    }
}
