using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;

namespace EnhancedMap.Core
{
    public class MapEntry
    {
        public int FileIndex;
        public int Height;

        public int Index;
        public string Name;
        public int Width;

        public MapEntry(int index, int fileindex, int w, int h, string name)
        {
            Index = index;
            Width = w;
            Height = h;
            Name = name;

            FileIndex = fileindex;
        }

        public override string ToString()
        {
            return string.Format("Name: {0}\r\nIndex: {1}\r\nSize: {2}x{3}", Name, Index, Width, Height);
        }
    }

    public static class MapsManager
    {
        private const string MAPS_XML = "maps.xml";
        private static float[] _radPix0, _radPix1, _radPix2;
        private static readonly IColorQuantizer _activeQuantizer = new OctreeQuantizer();
        private static readonly DirectoryInfo _pathMaps = new DirectoryInfo("Maps");

        public static void RemoveMapsFiles()
        {
            if (File.Exists(MAPS_XML))
                File.Delete(MAPS_XML);
        }

        public static bool CheckMapExists()
        {
            if (!_pathMaps.Exists)
                _pathMaps.Create();

            if (!File.Exists(MAPS_XML))
            {
                foreach (FileInfo file in _pathMaps.EnumerateFiles("*.png"))
                    file.Delete();

                return false;
            }

            return _pathMaps.EnumerateFiles("*.png").Any(s => s.Name.ToLower().Contains("map"));
        }

        public static void LoadMaps()
        {
            Global.Maps.Clear();

            XmlDocument doc = new XmlDocument();
            doc.Load(MAPS_XML);
            XmlElement root = doc["maps"];

            foreach (XmlElement e in root.ChildNodes)
            {
                MapEntry map = new MapEntry(e.GetAttribute("index").ToInt(), e.HasAttribute("fileindex") ? e.GetAttribute("fileindex").ToInt() : e.GetAttribute("index").ToInt(), e.GetAttribute("width").ToInt(), e.GetAttribute("height").ToInt(), e.Name);
                Global.Maps[map.Index] = map;
            }
        }

        public static void SaveMaps()
        {
            using (StreamWriter wr = new StreamWriter(MAPS_XML))
            {
                XmlTextWriter xml = new XmlTextWriter(wr) {Formatting = Formatting.Indented, IndentChar = '\t', Indentation = 1};

                xml.WriteStartDocument(true);
                xml.WriteStartElement("maps");

                foreach (MapEntry map in Global.Maps)
                {
                    if (map != null)
                    {
                        xml.WriteStartElement(map.Name);
                        xml.WriteAttributeString("index", map.Index.ToString());
                        xml.WriteAttributeString("fileindex", map.FileIndex.ToString());
                        xml.WriteAttributeString("width", map.Width.ToString());
                        xml.WriteAttributeString("height", map.Height.ToString());
                        xml.WriteEndElement();
                    }
                }

                xml.WriteEndElement();
                xml.Close();
            }
        }

        public static unsafe bool CreateImages(List<MapEntry> maps, bool detailed, IProgress<string> progress)
        {
            bool result = true;
            if (_radPix0 == null || _radPix1 == null || _radPix2 == null)
            {
                //if (!ReadRadarcol(progress))
                //    return false;
                result = ReadRadarcol(progress);
            }

            progress.Report("Maps founded: " + maps.Count);
            progress.Report("Starting map creation...");

            foreach (MapEntry map in maps)
            {
                progress.Report(new string('#', 38));

                progress.Report("Current map:\r\n" + map);

                bool isuop = false;

                string path = Path.Combine(Global.UOPath, string.Format("map{0}LegacyMUL.uop", map.Index));
                if (!File.Exists(path))
                {
                    progress.Report(path + " not founded.");
                    path = Path.Combine(Global.UOPath, string.Format("map{0}.mul", map.Index));
                    if (!File.Exists(path))
                    {
                        progress.Report(path + " not founded.");
                        path = Path.Combine(Global.UOPath, string.Format("facet0{0}.dds", map.Index));

                        if (File.Exists(path))
                        {
                            // patch
                            if (!detailed)
                                return true;

                            if (!result && ReadDDS(map, progress, path))
                            {
                                _activeQuantizer.Clear();
                                continue;
                            }

                            return false;
                        }

                        progress.Report(path + " not founded.");
                        continue;
                    }
                }
                else
                    isuop = true;

                progress.Report("reading: " + path);

                Bitmap image = new Bitmap(map.Width, map.Height, PixelFormat.Format32bppRgb);
                BitmapData data = image.LockBits(new Rectangle(0, 0, map.Width, map.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);

                ushort pix0, pix1, pix2;
                sbyte z;
                byte* ptr = (byte*) data.Scan0.ToPointer();

                UOPFile[] files = null;
                if (isuop)
                {
                    progress.Report("Unboxing " + path + "...");
                    files = ReadUOPFile(path, Path.GetFileName(path).Replace(".uop", "").ToLowerInvariant());
                    progress.Report(path + " unboxed.");
                }

                progress.Report("Pixels reading...");
                using (BinaryReader mapReader = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    using (BinaryReader staidxReader = new BinaryReader(File.Open(Path.Combine(Global.UOPath, string.Format("staidx{0}.mul", map.Index)), FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        using (BinaryReader staticsReader = new BinaryReader(File.Open(Path.Combine(Global.UOPath, string.Format("statics{0}.mul", map.Index)), FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                        {
                            int w = 0;
                            int h = 0;

                            while (w < map.Width)
                            {
                                long offset = (w / 8 * (map.Height >> 3) + h / 8) * 196 + 4;

                                if (isuop) offset = CalculateUOPOffset(files, mapReader, offset);

                                mapReader.BaseStream.Seek(offset, SeekOrigin.Begin);

                                ushort hue;
                                int x;
                                int y;

                                for (y = 0; y < 8; y++)
                                {
                                    for (x = 0; x < 8; x++)
                                    {
                                        hue = mapReader.ReadUInt16();
                                        z = mapReader.ReadSByte();

                                        *(ptr + (h + y) * data.Stride + (w + x) * 4 + 3) = (byte) z;
                                        *(ptr + (h + y) * data.Stride + (w + x) * 4 + 2) = (byte) (_radPix0[hue] * 255f);
                                        *(ptr + (h + y) * data.Stride + (w + x) * 4 + 1) = (byte) (_radPix1[hue] * 255f);
                                        *(ptr + (h + y) * data.Stride + (w + x) * 4) = (byte) (_radPix2[hue] * 255f);
                                    }
                                }

                                uint start = staidxReader.ReadUInt32();
                                uint end = staidxReader.ReadUInt32();
                                staidxReader.ReadUInt32(); // unknown

                                if (start != 0xFFFFFFFF)
                                {
                                    staticsReader.BaseStream.Seek(start, SeekOrigin.Begin);

                                    while (end > 0)
                                    {
                                        hue = (ushort) (staticsReader.ReadUInt16() + 0x4000);
                                        x = staticsReader.ReadByte();
                                        y = staticsReader.ReadByte();
                                        z = staticsReader.ReadSByte();
                                        staticsReader.ReadUInt16(); // unknownw

                                        sbyte ss = (sbyte) *(ptr + (h + y) * data.Stride + (w + x) * 4 + 3);

                                        if (z >= ss)
                                        {
                                            pix0 = (ushort) (_radPix0[hue] * 255f);
                                            pix1 = (ushort) (_radPix1[hue] * 255f);
                                            pix2 = (ushort) (_radPix2[hue] * 255f);

                                            if (pix0 > 15 || pix1 > 15 || pix2 > 15)
                                            {
                                                *(ptr + (h + y) * data.Stride + (w + x) * 4 + 3) = (byte) z;
                                                *(ptr + (h + y) * data.Stride + (w + x) * 4 + 2) = (byte) pix0;
                                                *(ptr + (h + y) * data.Stride + (w + x) * 4 + 1) = (byte) pix1;
                                                *(ptr + (h + y) * data.Stride + (w + x) * 4) = (byte) pix2;
                                            }
                                        }

                                        end -= 7;
                                    }
                                }

                                h += 8;
                                if (h < map.Height)
                                    continue;

                                h = 0;
                                w += 8;
                            }

                            for (h = 1; h < map.Height - 1; h++)
                            {
                                for (w = 1; w < map.Width - 1; w++)
                                {
                                    z = (sbyte) *(ptr + h * data.Stride + w * 4 + 3);


                                    // *(ptr + (h + 1) * data.Stride + (w + 1) * 4 + 3) = 255;

                                    sbyte ss = (sbyte) *(ptr + (h + 1) * data.Stride + (w - 1) * 4 + 3);
                                    if (z < ss)
                                    {
                                        pix0 = *(ptr + (h + 1) * data.Stride + (w + 1) * 4 + 2);
                                        pix1 = *(ptr + (h + 1) * data.Stride + (w + 1) * 4 + 1);
                                        pix2 = *(ptr + (h + 1) * data.Stride + (w + 1) * 4);

                                        pix0 = (ushort) (pix0 * 80 / 100);
                                        pix1 = (ushort) (pix1 * 80 / 100);
                                        pix2 = (ushort) (pix2 * 80 / 100);
                                    }
                                    else if (z > ss)
                                    {
                                        pix0 = *(ptr + (h + 1) * data.Stride + (w + 1) * 4 + 2);
                                        pix1 = *(ptr + (h + 1) * data.Stride + (w + 1) * 4 + 1);
                                        pix2 = *(ptr + (h + 1) * data.Stride + (w + 1) * 4);

                                        pix0 = (ushort) (pix0 * 100 / 80);
                                        pix1 = (ushort) (pix1 * 100 / 80);
                                        pix2 = (ushort) (pix2 * 100 / 80);
                                    }
                                    else
                                        continue;

                                    if (pix0 > 255)
                                        pix0 = 255;
                                    if (pix1 > 255)
                                        pix1 = 255;
                                    if (pix2 > 255)
                                        pix2 = 255;

                                    if (detailed)
                                    {
                                        *(ptr + (h + 1) * data.Stride + (w + 1) * 4 + 2) = (byte) pix0;
                                        *(ptr + (h + 1) * data.Stride + (w + 1) * 4 + 1) = (byte) pix1;
                                        *(ptr + (h + 1) * data.Stride + (w + 1) * 4) = (byte) pix2;
                                    }
                                }
                            }
                        }
                    }
                }

                progress.Report("Pixels readed.");
                image.UnlockBits(data);

                Bitmap mapb = new Bitmap(map.Width, map.Height, PixelFormat.Format32bppRgb);
                using (Graphics g = Graphics.FromImage(mapb))
                {
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.DrawImage(image, 0, 0, map.Width, map.Height);
                }

                progress.Report("Improve image quality and reducing file size...");
                Bitmap tosave = (Bitmap) Quantize(mapb);
                progress.Report("Done!");

                progress.Report("Saving on /Maps ...");
                using (MemoryStream ms = new MemoryStream())
                {
                    tosave.Save(ms, ImageFormat.Png);
                    using (BinaryWriter writer = new BinaryWriter(File.Open(Path.Combine("Maps", (detailed ? "2D" : "") + "map" + map.Index + ".png"), FileMode.Create)))
                    {
                        writer.Write(ms.GetBuffer(), 0, (int) ms.Length);
                    }
                }

                progress.Report("Done!");

                progress.Report("Cleaning...");
                image.Dispose();
                image = null;
                tosave.Dispose();
                tosave = null;
                mapb.Dispose();
                mapb = null;
                _activeQuantizer.Clear();

                progress.Report("Done!");
            }

            _radPix0 = null;
            _radPix1 = null;
            _radPix2 = null;

            return true;
        }

        private static bool ReadDDS(MapEntry map, IProgress<string> progress, string path)
        {
            Bitmap bmp = DDS.LoadImage(path, false);
            Bitmap mapb = new Bitmap(map.Width, map.Height, PixelFormat.Format32bppRgb);
            using (Graphics g = Graphics.FromImage(mapb))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(bmp, 0, 0, map.Width, map.Height);
            }

            progress.Report("Improve image quality and reducing file size...");
            Bitmap tosave = (Bitmap) Quantize(mapb);
            progress.Report("Done!");

            progress.Report("Saving on /Maps ...");
            using (MemoryStream ms = new MemoryStream())
            {
                tosave.Save(ms, ImageFormat.Png);
                using (BinaryWriter writer = new BinaryWriter(File.Open(Path.Combine("Maps", "2Dmap" + map.Index + ".png"), FileMode.Create)))
                {
                    writer.Write(ms.GetBuffer(), 0, (int) ms.Length);
                }
            }

            return true;
        }

        private static bool ReadRadarcol(IProgress<string> progress)
        {
            string radCol = Path.Combine(Global.UOPath, "radarcol.mul");

            progress.Report("Checking for 'radarcol.mul'...");
            if (!File.Exists(radCol))
            {
                progress.Report("'radarcol.mul' not found.");
                return false;
            }

            progress.Report("'radarcol.mul' founded!");

            using (BinaryReader reader = new BinaryReader(File.Open(radCol, FileMode.Open)))
            {
                int length = (int) reader.BaseStream.Length / 2;

                progress.Report("'radarcol.mul' file length: " + length * 2);
                progress.Report("'radarcol.mul' colors: " + length);

                _radPix0 = new float[length];
                _radPix1 = new float[length];
                _radPix2 = new float[length];

                for (int i = 0; i < length; i++)
                {
                    ushort pix = (ushort) (reader.ReadByte() | (reader.ReadByte() << 8));
                    _radPix0[i] = ((pix >> 10) & 31) / 31f;
                    _radPix1[i] = ((pix >> 5) & 31) / 31f;
                    _radPix2[i] = (pix & 31) / 31f;
                }
            }

            return true;
        }

        private static Image Quantize(Image image)
        {
            // checks whether a source image is valid
            if (image == null)
            {
                const string message = "Cannot quantize a null image.";
                throw new ArgumentNullException(message);
            }

            // locks the source image data
            var bitmap = (Bitmap) image;
            var bounds = Rectangle.FromLTRB(0, 0, bitmap.Width, bitmap.Height);
            var sourceData = bitmap.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                // initalizes the pixel read buffer
                var sourceBuffer = new int[image.Width];

                // sets the offset to the first pixel in the image
                var sourceOffset = sourceData.Scan0.ToInt64();

                for (var row = 0; row < image.Height; row++)
                {
                    // copies the whole row of pixels to the buffer
                    Marshal.Copy(new IntPtr(sourceOffset), sourceBuffer, 0, image.Width);

                    // scans all the colors in the buffer
                    foreach (var color in sourceBuffer.Select(Color.FromArgb))
                        _activeQuantizer.AddColor(color);

                    // increases a source offset by a row
                    sourceOffset += sourceData.Stride;
                }
            }
            catch
            {
                bitmap.UnlockBits(sourceData);
                throw;
            }

            var result = new Bitmap(image.Width, image.Height, PixelFormat.Format8bppIndexed);

            // calculates the palette
            try
            {
                var colorCount = 256;
                var palette = _activeQuantizer.GetPalette(colorCount);

                // sets our newly calculated palette to the target image
                var imagePalette = result.Palette;

                for (var index = 0; index < palette.Count; index++) imagePalette.Entries[index] = palette[index];

                result.Palette = imagePalette;
            }
            catch (Exception)
            {
                bitmap.UnlockBits(sourceData);
                throw;
            }

            // locks the target image data
            var targetData = result.LockBits(bounds, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            try
            {
                // initializes read/write buffers
                var targetBuffer = new byte[result.Width];
                var sourceBuffer = new int[image.Width];

                // sets the offsets on the beginning of both source and target image
                var sourceOffset = sourceData.Scan0.ToInt64();
                var targetOffset = targetData.Scan0.ToInt64();

                for (var row = 0; row < image.Height; row++)
                {
                    // reads the pixel row from the source image
                    Marshal.Copy(new IntPtr(sourceOffset), sourceBuffer, 0, image.Width);

                    // goes thru all the pixels, reads the color on the source image, and writes calculated palette index on the target
                    for (var index = 0; index < image.Width; index++)
                    {
                        var color = Color.FromArgb(sourceBuffer[index]);
                        targetBuffer[index] = (byte) _activeQuantizer.GetPaletteIndex(color);
                    }

                    // writes the pixel row to the target image
                    Marshal.Copy(targetBuffer, 0, new IntPtr(targetOffset), result.Width);

                    // increases the offsets (on both images) by a row
                    sourceOffset += sourceData.Stride;
                    targetOffset += targetData.Stride;
                }
            }
            finally
            {
                // releases the locks on both images
                bitmap.UnlockBits(sourceData);
                result.UnlockBits(targetData);
            }

            // returns the quantized image
            return result;
        }


        private static UOPFile[] ReadUOPFile(string path, string pattern)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                if (reader.ReadUInt32() != 0x50594D)
                    throw new ArgumentException("Bad UOP file.");
                reader.ReadInt64();
                long nextBlock = reader.ReadInt64();
                reader.ReadInt32();
                int count = reader.ReadInt32();

                UOPFile[] files = new UOPFile[count];
                Dictionary<ulong, int> hashes = new Dictionary<ulong, int>();

                for (int i = 0; i < count; i++)
                {
                    string file = string.Format("build/{0}/{1:D8}.dat", pattern, i);
                    ulong hash = HashFileName(file);
                    if (!hashes.ContainsKey(hash))
                        hashes.Add(hash, i);
                }

                reader.BaseStream.Seek(nextBlock, SeekOrigin.Begin);

                do
                {
                    int filesCount = reader.ReadInt32();
                    nextBlock = reader.ReadInt64();

                    for (int i = 0; i < filesCount; i++)
                    {
                        long offset = reader.ReadInt64();
                        int headerLength = reader.ReadInt32();
                        int compressedLength = reader.ReadInt32();
                        int decompressedLength = reader.ReadInt32();
                        ulong hash = reader.ReadUInt64();
                        reader.ReadInt32();
                        short flag = reader.ReadInt16();

                        int length = flag == 1 ? compressedLength : decompressedLength;
                        if (offset == 0)
                            continue;

                        if (hashes.TryGetValue(hash, out int idx))
                        {
                            if (idx < 0 || idx > files.Length)
                                throw new IndexOutOfRangeException("hashes dictionary and files collection have different count of entries!");

                            files[idx] = new UOPFile(offset + headerLength, length);
                        }
                        else
                            throw new ArgumentException(string.Format("File with hash 0x{0:X8} was not found in hashes dictionary! EA Mythic changed UOP format!", hash));
                    }
                } while (reader.BaseStream.Seek(nextBlock, SeekOrigin.Begin) != 0);

                return files;
            }
        }

        private static long CalculateUOPOffset(UOPFile[] files, BinaryReader reader, long offset)
        {
            long pos = 0;

            for (int i = 0; i < files.Length; i++)
            {
                long currpos = pos + files[i].Length;
                if (offset < currpos)
                    return files[i].Offset + (offset - pos);
                pos = currpos;
            }

            return reader.BaseStream.Length;
        }

        private static ulong HashFileName(string s)
        {
            uint eax, ecx, edx, ebx, esi, edi;

            eax = ecx = edx = ebx = esi = edi = 0;
            ebx = edi = esi = (uint) s.Length + 0xDEADBEEF;

            int i = 0;

            for (i = 0; i + 12 < s.Length; i += 12)
            {
                edi = (uint) ((s[i + 7] << 24) | (s[i + 6] << 16) | (s[i + 5] << 8) | s[i + 4]) + edi;
                esi = (uint) ((s[i + 11] << 24) | (s[i + 10] << 16) | (s[i + 9] << 8) | s[i + 8]) + esi;
                edx = (uint) ((s[i + 3] << 24) | (s[i + 2] << 16) | (s[i + 1] << 8) | s[i]) - esi;

                edx = (edx + ebx) ^ (esi >> 28) ^ (esi << 4);
                esi += edi;
                edi = (edi - edx) ^ (edx >> 26) ^ (edx << 6);
                edx += esi;
                esi = (esi - edi) ^ (edi >> 24) ^ (edi << 8);
                edi += edx;
                ebx = (edx - esi) ^ (esi >> 16) ^ (esi << 16);
                esi += edi;
                edi = (edi - ebx) ^ (ebx >> 13) ^ (ebx << 19);
                ebx += esi;
                esi = (esi - edi) ^ (edi >> 28) ^ (edi << 4);
                edi += ebx;
            }

            if (s.Length - i > 0)
            {
                switch (s.Length - i)
                {
                    case 12:
                        esi += (uint) s[i + 11] << 24;
                        goto case 11;
                    case 11:
                        esi += (uint) s[i + 10] << 16;
                        goto case 10;
                    case 10:
                        esi += (uint) s[i + 9] << 8;
                        goto case 9;
                    case 9:
                        esi += s[i + 8];
                        goto case 8;
                    case 8:
                        edi += (uint) s[i + 7] << 24;
                        goto case 7;
                    case 7:
                        edi += (uint) s[i + 6] << 16;
                        goto case 6;
                    case 6:
                        edi += (uint) s[i + 5] << 8;
                        goto case 5;
                    case 5:
                        edi += s[i + 4];
                        goto case 4;
                    case 4:
                        ebx += (uint) s[i + 3] << 24;
                        goto case 3;
                    case 3:
                        ebx += (uint) s[i + 2] << 16;
                        goto case 2;
                    case 2:
                        ebx += (uint) s[i + 1] << 8;
                        goto case 1;
                    case 1:
                        ebx += s[i];
                        break;
                }

                esi = (esi ^ edi) - ((edi >> 18) ^ (edi << 14));
                ecx = (esi ^ ebx) - ((esi >> 21) ^ (esi << 11));
                edi = (edi ^ ecx) - ((ecx >> 7) ^ (ecx << 25));
                esi = (esi ^ edi) - ((edi >> 16) ^ (edi << 16));
                edx = (esi ^ ecx) - ((esi >> 28) ^ (esi << 4));
                edi = (edi ^ edx) - ((edx >> 18) ^ (edx << 14));
                eax = (esi ^ edi) - ((edi >> 8) ^ (edi << 24));

                return ((ulong) edi << 32) | eax;
            }

            return ((ulong) esi << 32) | eax;
        }

        private struct UOPFile
        {
            public UOPFile(long offset, int length)
            {
                Offset = offset;
                Length = length;
            }

            public readonly long Offset;
            public readonly int Length;
        }
    }
}

public static class DDS
{
    /// <summary>
    ///     Loads a DDS image from a byte array, and returns a Bitmap object of the image.
    /// </summary>
    /// <param name="data">The image data, as a byte array.</param>
    /// <param name="alpha">Preserve the alpha channel or not. (default: true)</param>
    /// <returns>The Bitmap representation of the image.</returns>
    public static Bitmap LoadImage(byte[] data, bool alpha = true)
    {
        DDSImage im = new DDSImage(data, alpha);
        return im.BitmapImage;
    }

    /// <summary>
    ///     Loads a DDS image from a file, and returns a Bitmap object of the image.
    /// </summary>
    /// <param name="file">The image file.</param>
    /// <param name="alpha">Preserve the alpha channel or not. (default: true)</param>
    /// <returns>The Bitmap representation of the image.</returns>
    public static Bitmap LoadImage(string file, bool alpha = true)
    {
        byte[] data = File.ReadAllBytes(file);
        DDSImage im = new DDSImage(data, alpha);
        return im.BitmapImage;
    }

    /// <summary>
    ///     Loads a DDS image from a Stream, and returns a Bitmap object of the image.
    /// </summary>
    /// <param name="stream">The stream to read the image data from.</param>
    /// <param name="alpha">Preserve the alpha channel or not. (default: true)</param>
    /// <returns>The Bitmap representation of the image.</returns>
    public static Bitmap LoadImage(Stream stream, bool alpha = true)
    {
        DDSImage im = new DDSImage(stream, alpha);
        return im.BitmapImage;
    }
}

/// <summary>
///     Thrown when there is an unknown compressor used in the DDS file.
/// </summary>
public class UnknownFileFormatException : Exception
{
}

/// <summary>
///     Thrown when an invalid file header has been encountered.
/// </summary>
public class InvalidFileHeaderException : Exception
{
}

/// <summary>
///     Thrown when the data does not contain a DDS image.
/// </summary>
public class NotADDSImageException : Exception
{
}

public class DDSImage : IDisposable
{
    public DDSImage(byte[] ddsImage, bool preserveAlpha = true)
    {
        if (ddsImage == null)
            return;

        if (ddsImage.Length == 0)
            return;

        PreserveAlpha = preserveAlpha;

        using (MemoryStream stream = new MemoryStream(ddsImage.Length))
        {
            stream.Write(ddsImage, 0, ddsImage.Length);
            stream.Seek(0, SeekOrigin.Begin);

            using (BinaryReader reader = new BinaryReader(stream))
            {
                Parse(reader);
            }
        }
    }

    public DDSImage(Stream ddsImage, bool preserveAlpha = true)
    {
        if (ddsImage == null)
            return;

        if (!ddsImage.CanRead)
            return;

        PreserveAlpha = preserveAlpha;

        using (BinaryReader reader = new BinaryReader(ddsImage))
        {
            Parse(reader);
        }
    }

    public Bitmap BitmapImage { get; private set; }

    public bool IsValid { get; private set; }

    public bool PreserveAlpha { get; set; }

    public void Dispose()
    {
        if (BitmapImage != null)
        {
            BitmapImage.Dispose();
            BitmapImage = null;
        }
    }

    private void Parse(BinaryReader reader)
    {
        DDSStruct header = new DDSStruct();
        PixelFormatDDS pixelFormat = PixelFormatDDS.UNKNOWN;
        byte[] data = null;

        if (ReadHeader(reader, ref header))
        {
            IsValid = true;
            // patches for stuff
            if (header.depth == 0) header.depth = 1;

            uint blocksize = 0;
            pixelFormat = GetFormat(header, ref blocksize);
            if (pixelFormat == PixelFormatDDS.UNKNOWN) throw new InvalidFileHeaderException();

            data = ReadData(reader, header);
            if (data != null)
            {
                byte[] rawData = Decompressor.Expand(header, data, pixelFormat);
                BitmapImage = CreateBitmap((int) header.width, (int) header.height, rawData);
            }
        }
    }

    private byte[] ReadData(BinaryReader reader, DDSStruct header)
    {
        byte[] compdata = null;
        uint compsize = 0;

        if ((header.flags & Helper.DDSD_LINEARSIZE) > 1)
        {
            compdata = reader.ReadBytes((int) header.sizeorpitch);
            compsize = (uint) compdata.Length;
        }
        else
        {
            uint bps = header.width * header.pixelformat.rgbbitcount / 8;
            compsize = bps * header.height * header.depth;
            compdata = new byte[compsize];

            MemoryStream mem = new MemoryStream((int) compsize);

            byte[] temp;
            for (int z = 0; z < header.depth; z++)
            {
                for (int y = 0; y < header.height; y++)
                {
                    temp = reader.ReadBytes((int) bps);
                    mem.Write(temp, 0, temp.Length);
                }
            }

            mem.Seek(0, SeekOrigin.Begin);

            mem.Read(compdata, 0, compdata.Length);
            mem.Close();
        }

        return compdata;
    }

    private Bitmap CreateBitmap(int width, int height, byte[] rawData)
    {
        var pxFormat = PixelFormat.Format32bppRgb;
        if (PreserveAlpha)
            pxFormat = PixelFormat.Format32bppArgb;

        Bitmap bitmap = new Bitmap(width, height, pxFormat);

        BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, pxFormat);
        IntPtr scan = data.Scan0;
        int size = bitmap.Width * bitmap.Height * 4;

        unsafe
        {
            byte* p = (byte*) scan;
            for (int i = 0; i < size; i += 4)
            {
                // iterate through bytes.
                // Bitmap stores it's data in RGBA order.
                // DDS stores it's data in BGRA order.
                p[i] = rawData[i + 2]; // blue
                p[i + 1] = rawData[i + 1]; // green
                p[i + 2] = rawData[i]; // red
                p[i + 3] = rawData[i + 3]; // alpha
            }
        }

        bitmap.UnlockBits(data);
        return bitmap;
    }

    private bool ReadHeader(BinaryReader reader, ref DDSStruct header)
    {
        byte[] signature = reader.ReadBytes(4);
        if (!(signature[0] == 'D' && signature[1] == 'D' && signature[2] == 'S' && signature[3] == ' '))
            return false;

        header.size = reader.ReadUInt32();
        if (header.size != 124)
            return false;

        //convert the data
        header.flags = reader.ReadUInt32();
        header.height = reader.ReadUInt32();
        header.width = reader.ReadUInt32();
        header.sizeorpitch = reader.ReadUInt32();
        header.depth = reader.ReadUInt32();
        header.mipmapcount = reader.ReadUInt32();
        header.alphabitdepth = reader.ReadUInt32();

        header.reserved = new uint[10];
        for (int i = 0; i < 10; i++) header.reserved[i] = reader.ReadUInt32();

        //pixelfromat
        header.pixelformat.size = reader.ReadUInt32();
        header.pixelformat.flags = reader.ReadUInt32();
        header.pixelformat.fourcc = reader.ReadUInt32();
        header.pixelformat.rgbbitcount = reader.ReadUInt32();
        header.pixelformat.rbitmask = reader.ReadUInt32();
        header.pixelformat.gbitmask = reader.ReadUInt32();
        header.pixelformat.bbitmask = reader.ReadUInt32();
        header.pixelformat.alphabitmask = reader.ReadUInt32();

        //caps
        header.ddscaps.caps1 = reader.ReadUInt32();
        header.ddscaps.caps2 = reader.ReadUInt32();
        header.ddscaps.caps3 = reader.ReadUInt32();
        header.ddscaps.caps4 = reader.ReadUInt32();
        header.texturestage = reader.ReadUInt32();

        return true;
    }

    private PixelFormatDDS GetFormat(DDSStruct header, ref uint blocksize)
    {
        PixelFormatDDS format = PixelFormatDDS.UNKNOWN;
        if ((header.pixelformat.flags & Helper.DDPF_FOURCC) == Helper.DDPF_FOURCC)
        {
            blocksize = (header.width + 3) / 4 * ((header.height + 3) / 4) * header.depth;

            switch (header.pixelformat.fourcc)
            {
                case Helper.FOURCC_DXT1:
                    format = PixelFormatDDS.DXT1;
                    blocksize *= 8;
                    break;

                case Helper.FOURCC_DXT2:
                    format = PixelFormatDDS.DXT2;
                    blocksize *= 16;
                    break;

                case Helper.FOURCC_DXT3:
                    format = PixelFormatDDS.DXT3;
                    blocksize *= 16;
                    break;

                case Helper.FOURCC_DXT4:
                    format = PixelFormatDDS.DXT4;
                    blocksize *= 16;
                    break;

                case Helper.FOURCC_DXT5:
                    format = PixelFormatDDS.DXT5;
                    blocksize *= 16;
                    break;

                case Helper.FOURCC_ATI1:
                    format = PixelFormatDDS.ATI1N;
                    blocksize *= 8;
                    break;

                case Helper.FOURCC_ATI2:
                    format = PixelFormatDDS.THREEDC;
                    blocksize *= 16;
                    break;

                case Helper.FOURCC_RXGB:
                    format = PixelFormatDDS.RXGB;
                    blocksize *= 16;
                    break;

                case Helper.FOURCC_DOLLARNULL:
                    format = PixelFormatDDS.A16B16G16R16;
                    blocksize = header.width * header.height * header.depth * 8;
                    break;

                case Helper.FOURCC_oNULL:
                    format = PixelFormatDDS.R16F;
                    blocksize = header.width * header.height * header.depth * 2;
                    break;

                case Helper.FOURCC_pNULL:
                    format = PixelFormatDDS.G16R16F;
                    blocksize = header.width * header.height * header.depth * 4;
                    break;

                case Helper.FOURCC_qNULL:
                    format = PixelFormatDDS.A16B16G16R16F;
                    blocksize = header.width * header.height * header.depth * 8;
                    break;

                case Helper.FOURCC_rNULL:
                    format = PixelFormatDDS.R32F;
                    blocksize = header.width * header.height * header.depth * 4;
                    break;

                case Helper.FOURCC_sNULL:
                    format = PixelFormatDDS.G32R32F;
                    blocksize = header.width * header.height * header.depth * 8;
                    break;

                case Helper.FOURCC_tNULL:
                    format = PixelFormatDDS.A32B32G32R32F;
                    blocksize = header.width * header.height * header.depth * 16;
                    break;

                default:
                    format = PixelFormatDDS.UNKNOWN;
                    blocksize *= 16;
                    break;
            }
        }
        else
        {
            // uncompressed image
            if ((header.pixelformat.flags & Helper.DDPF_LUMINANCE) == Helper.DDPF_LUMINANCE)
            {
                if ((header.pixelformat.flags & Helper.DDPF_ALPHAPIXELS) == Helper.DDPF_ALPHAPIXELS)
                    format = PixelFormatDDS.LUMINANCE_ALPHA;
                else
                    format = PixelFormatDDS.LUMINANCE;
            }
            else
            {
                if ((header.pixelformat.flags & Helper.DDPF_ALPHAPIXELS) == Helper.DDPF_ALPHAPIXELS)
                    format = PixelFormatDDS.RGBA;
                else
                    format = PixelFormatDDS.RGB;
            }

            blocksize = header.width * header.height * header.depth * (header.pixelformat.rgbbitcount >> 3);
        }

        return format;
    }
}

internal class Decompressor
{
    internal static byte[] Expand(DDSStruct header, byte[] data, PixelFormatDDS pixelFormat)
    {
        Debug.WriteLine(pixelFormat);
        // allocate bitmap
        byte[] rawData = null;

        switch (pixelFormat)
        {
            case PixelFormatDDS.RGBA:
                rawData = DecompressRGBA(header, data, pixelFormat);
                break;

            case PixelFormatDDS.RGB:
                rawData = DecompressRGB(header, data, pixelFormat);
                break;

            case PixelFormatDDS.LUMINANCE:
            case PixelFormatDDS.LUMINANCE_ALPHA:
                rawData = DecompressLum(header, data, pixelFormat);
                break;

            case PixelFormatDDS.DXT1:
                rawData = DecompressDXT1(header, data, pixelFormat);
                break;

            case PixelFormatDDS.DXT2:
                rawData = DecompressDXT2(header, data, pixelFormat);
                break;

            case PixelFormatDDS.DXT3:
                rawData = DecompressDXT3(header, data, pixelFormat);
                break;

            case PixelFormatDDS.DXT4:
                rawData = DecompressDXT4(header, data, pixelFormat);
                break;

            case PixelFormatDDS.DXT5:
                rawData = DecompressDXT5(header, data, pixelFormat);
                break;

            case PixelFormatDDS.THREEDC:
                rawData = Decompress3Dc(header, data, pixelFormat);
                break;

            case PixelFormatDDS.ATI1N:
                rawData = DecompressAti1n(header, data, pixelFormat);
                break;

            case PixelFormatDDS.RXGB:
                rawData = DecompressRXGB(header, data, pixelFormat);
                break;

            case PixelFormatDDS.R16F:
            case PixelFormatDDS.G16R16F:
            case PixelFormatDDS.A16B16G16R16F:
            case PixelFormatDDS.R32F:
            case PixelFormatDDS.G32R32F:
            case PixelFormatDDS.A32B32G32R32F:
                rawData = DecompressFloat(header, data, pixelFormat);
                break;

            default:
                throw new UnknownFileFormatException();
        }

        return rawData;
    }

    private static unsafe byte[] DecompressDXT1(DDSStruct header, byte[] data, PixelFormatDDS pixelFormat)
    {
        // allocate bitmap
        int bpp = (int) Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount);
        int bps = (int) (header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
        int sizeofplane = (int) (bps * header.height);
        int width = (int) header.width;
        int height = (int) header.height;
        int depth = (int) header.depth;

        // DXT1 decompressor
        byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

        Colour8888[] colours = new Colour8888[4];
        colours[0].alpha = 0xFF;
        colours[1].alpha = 0xFF;
        colours[2].alpha = 0xFF;

        fixed (byte* bytePtr = data)
        {
            byte* temp = bytePtr;
            for (int z = 0; z < depth; z++)
            {
                for (int y = 0; y < height; y += 4)
                {
                    for (int x = 0; x < width; x += 4)
                    {
                        ushort colour0 = *((ushort*) temp);
                        ushort colour1 = *((ushort*) (temp + 2));
                        Helper.DxtcReadColor(colour0, ref colours[0]);
                        Helper.DxtcReadColor(colour1, ref colours[1]);

                        uint bitmask = ((uint*) temp)[1];
                        temp += 8;

                        if (colour0 > colour1)
                        {
                            // Four-color block: derive the other two colors.
                            // 00 = color_0, 01 = color_1, 10 = color_2, 11 = color_3
                            // These 2-bit codes correspond to the 2-bit fields
                            // stored in the 64-bit block.
                            colours[2].blue = (byte) ((2 * colours[0].blue + colours[1].blue + 1) / 3);
                            colours[2].green = (byte) ((2 * colours[0].green + colours[1].green + 1) / 3);
                            colours[2].red = (byte) ((2 * colours[0].red + colours[1].red + 1) / 3);
                            //colours[2].alpha = 0xFF;

                            colours[3].blue = (byte) ((colours[0].blue + 2 * colours[1].blue + 1) / 3);
                            colours[3].green = (byte) ((colours[0].green + 2 * colours[1].green + 1) / 3);
                            colours[3].red = (byte) ((colours[0].red + 2 * colours[1].red + 1) / 3);
                            colours[3].alpha = 0xFF;
                        }
                        else
                        {
                            // Three-color block: derive the other color.
                            // 00 = color_0,  01 = color_1,  10 = color_2,
                            // 11 = transparent.
                            // These 2-bit codes correspond to the 2-bit fields
                            // stored in the 64-bit block.
                            colours[2].blue = (byte) ((colours[0].blue + colours[1].blue) / 2);
                            colours[2].green = (byte) ((colours[0].green + colours[1].green) / 2);
                            colours[2].red = (byte) ((colours[0].red + colours[1].red) / 2);
                            //colours[2].alpha = 0xFF;

                            colours[3].blue = (byte) ((colours[0].blue + 2 * colours[1].blue + 1) / 3);
                            colours[3].green = (byte) ((colours[0].green + 2 * colours[1].green + 1) / 3);
                            colours[3].red = (byte) ((colours[0].red + 2 * colours[1].red + 1) / 3);
                            colours[3].alpha = 0x00;
                        }

                        for (int j = 0, k = 0; j < 4; j++)
                        {
                            for (int i = 0; i < 4; i++, k++)
                            {
                                int select = (int) ((bitmask & (0x03 << (k * 2))) >> (k * 2));
                                Colour8888 col = colours[select];
                                if (x + i < width && y + j < height)
                                {
                                    uint offset = (uint) (z * sizeofplane + (y + j) * bps + (x + i) * bpp);
                                    rawData[offset + 0] = col.red;
                                    rawData[offset + 1] = col.green;
                                    rawData[offset + 2] = col.blue;
                                    rawData[offset + 3] = col.alpha;
                                }
                            }
                        }
                    }
                }
            }
        }

        return rawData;
    }

    private static byte[] DecompressDXT2(DDSStruct header, byte[] data, PixelFormatDDS pixelFormat)
    {
        // allocate bitmap
        int width = (int) header.width;
        int height = (int) header.height;
        int depth = (int) header.depth;

        // Can do color & alpha same as dxt3, but color is pre-multiplied
        // so the result will be wrong unless corrected.
        byte[] rawData = DecompressDXT3(header, data, pixelFormat);
        Helper.CorrectPremult((uint) (width * height * depth), ref rawData);

        return rawData;
    }

    private static unsafe byte[] DecompressDXT3(DDSStruct header, byte[] data, PixelFormatDDS pixelFormat)
    {
        // allocate bitmap
        int bpp = (int) Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount);
        int bps = (int) (header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
        int sizeofplane = (int) (bps * header.height);
        int width = (int) header.width;
        int height = (int) header.height;
        int depth = (int) header.depth;

        // DXT3 decompressor
        byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];
        Colour8888[] colours = new Colour8888[4];

        fixed (byte* bytePtr = data)
        {
            byte* temp = bytePtr;
            for (int z = 0; z < depth; z++)
            {
                for (int y = 0; y < height; y += 4)
                {
                    for (int x = 0; x < width; x += 4)
                    {
                        byte* alpha = temp;
                        temp += 8;

                        Helper.DxtcReadColors(temp, ref colours);
                        temp += 4;

                        uint bitmask = ((uint*) temp)[1];
                        temp += 4;

                        // Four-color block: derive the other two colors.
                        // 00 = color_0, 01 = color_1, 10 = color_2, 11	= color_3
                        // These 2-bit codes correspond to the 2-bit fields
                        // stored in the 64-bit block.
                        colours[2].blue = (byte) ((2 * colours[0].blue + colours[1].blue + 1) / 3);
                        colours[2].green = (byte) ((2 * colours[0].green + colours[1].green + 1) / 3);
                        colours[2].red = (byte) ((2 * colours[0].red + colours[1].red + 1) / 3);
                        //colours[2].alpha = 0xFF;

                        colours[3].blue = (byte) ((colours[0].blue + 2 * colours[1].blue + 1) / 3);
                        colours[3].green = (byte) ((colours[0].green + 2 * colours[1].green + 1) / 3);
                        colours[3].red = (byte) ((colours[0].red + 2 * colours[1].red + 1) / 3);
                        //colours[3].alpha = 0xFF;

                        for (int j = 0, k = 0; j < 4; j++)
                        {
                            for (int i = 0; i < 4; k++, i++)
                            {
                                int select = (int) ((bitmask & (0x03 << (k * 2))) >> (k * 2));

                                if (x + i < width && y + j < height)
                                {
                                    uint offset = (uint) (z * sizeofplane + (y + j) * bps + (x + i) * bpp);
                                    rawData[offset + 0] = colours[select].red;
                                    rawData[offset + 1] = colours[select].green;
                                    rawData[offset + 2] = colours[select].blue;
                                }
                            }
                        }

                        for (int j = 0; j < 4; j++)
                        {
                            //ushort word = (ushort)(alpha[2 * j] + 256 * alpha[2 * j + 1]);
                            ushort word = (ushort) (alpha[2 * j] | (alpha[2 * j + 1] << 8));
                            for (int i = 0; i < 4; i++)
                            {
                                if (x + i < width && y + j < height)
                                {
                                    uint offset = (uint) (z * sizeofplane + (y + j) * bps + (x + i) * bpp + 3);
                                    rawData[offset] = (byte) (word & 0x0F);
                                    rawData[offset] = (byte) (rawData[offset] | (rawData[offset] << 4));
                                }

                                word >>= 4;
                            }
                        }
                    }
                }
            }
        }

        return rawData;
    }

    private static byte[] DecompressDXT4(DDSStruct header, byte[] data, PixelFormatDDS pixelFormat)
    {
        // allocate bitmap
        int width = (int) header.width;
        int height = (int) header.height;
        int depth = (int) header.depth;

        // Can do color & alpha same as dxt5, but color is pre-multiplied
        // so the result will be wrong unless corrected.
        byte[] rawData = DecompressDXT5(header, data, pixelFormat);
        Helper.CorrectPremult((uint) (width * height * depth), ref rawData);

        return rawData;
    }

    private static unsafe byte[] DecompressDXT5(DDSStruct header, byte[] data, PixelFormatDDS pixelFormat)
    {
        // allocate bitmap
        int bpp = (int) Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount);
        int bps = (int) (header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
        int sizeofplane = (int) (bps * header.height);
        int width = (int) header.width;
        int height = (int) header.height;
        int depth = (int) header.depth;

        byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];
        Colour8888[] colours = new Colour8888[4];
        ushort[] alphas = new ushort[8];

        fixed (byte* bytePtr = data)
        {
            byte* temp = bytePtr;
            for (int z = 0; z < depth; z++)
            {
                for (int y = 0; y < height; y += 4)
                {
                    for (int x = 0; x < width; x += 4)
                    {
                        if (y >= height || x >= width)
                            break;

                        alphas[0] = temp[0];
                        alphas[1] = temp[1];
                        byte* alphamask = temp + 2;
                        temp += 8;

                        Helper.DxtcReadColors(temp, ref colours);
                        uint bitmask = ((uint*) temp)[1];
                        temp += 8;

                        // Four-color block: derive the other two colors.
                        // 00 = color_0, 01 = color_1, 10 = color_2, 11	= color_3
                        // These 2-bit codes correspond to the 2-bit fields
                        // stored in the 64-bit block.
                        colours[2].blue = (byte) ((2 * colours[0].blue + colours[1].blue + 1) / 3);
                        colours[2].green = (byte) ((2 * colours[0].green + colours[1].green + 1) / 3);
                        colours[2].red = (byte) ((2 * colours[0].red + colours[1].red + 1) / 3);
                        //colours[2].alpha = 0xFF;

                        colours[3].blue = (byte) ((colours[0].blue + 2 * colours[1].blue + 1) / 3);
                        colours[3].green = (byte) ((colours[0].green + 2 * colours[1].green + 1) / 3);
                        colours[3].red = (byte) ((colours[0].red + 2 * colours[1].red + 1) / 3);
                        //colours[3].alpha = 0xFF;

                        int k = 0;
                        for (int j = 0; j < 4; j++)
                        {
                            for (int i = 0; i < 4; k++, i++)
                            {
                                int select = (int) ((bitmask & (0x03 << (k * 2))) >> (k * 2));
                                Colour8888 col = colours[select];
                                // only put pixels out < width or height
                                if (x + i < width && y + j < height)
                                {
                                    uint offset = (uint) (z * sizeofplane + (y + j) * bps + (x + i) * bpp);
                                    rawData[offset] = col.red;
                                    rawData[offset + 1] = col.green;
                                    rawData[offset + 2] = col.blue;
                                }
                            }
                        }

                        // 8-alpha or 6-alpha block?
                        if (alphas[0] > alphas[1])
                        {
                            // 8-alpha block:  derive the other six alphas.
                            // Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
                            alphas[2] = (ushort) ((6 * alphas[0] + 1 * alphas[1] + 3) / 7); // bit code 010
                            alphas[3] = (ushort) ((5 * alphas[0] + 2 * alphas[1] + 3) / 7); // bit code 011
                            alphas[4] = (ushort) ((4 * alphas[0] + 3 * alphas[1] + 3) / 7); // bit code 100
                            alphas[5] = (ushort) ((3 * alphas[0] + 4 * alphas[1] + 3) / 7); // bit code 101
                            alphas[6] = (ushort) ((2 * alphas[0] + 5 * alphas[1] + 3) / 7); // bit code 110
                            alphas[7] = (ushort) ((1 * alphas[0] + 6 * alphas[1] + 3) / 7); // bit code 111
                        }
                        else
                        {
                            // 6-alpha block.
                            // Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
                            alphas[2] = (ushort) ((4 * alphas[0] + 1 * alphas[1] + 2) / 5); // Bit code 010
                            alphas[3] = (ushort) ((3 * alphas[0] + 2 * alphas[1] + 2) / 5); // Bit code 011
                            alphas[4] = (ushort) ((2 * alphas[0] + 3 * alphas[1] + 2) / 5); // Bit code 100
                            alphas[5] = (ushort) ((1 * alphas[0] + 4 * alphas[1] + 2) / 5); // Bit code 101
                            alphas[6] = 0x00; // Bit code 110
                            alphas[7] = 0xFF; // Bit code 111
                        }

                        // Note: Have to separate the next two loops,
                        // it operates on a 6-byte system.

                        // First three bytes
                        //uint bits = (uint)(alphamask[0]);
                        uint bits = (uint) (alphamask[0] | (alphamask[1] << 8) | (alphamask[2] << 16));
                        for (int j = 0; j < 2; j++)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                // only put pixels out < width or height
                                if (x + i < width && y + j < height)
                                {
                                    uint offset = (uint) (z * sizeofplane + (y + j) * bps + (x + i) * bpp + 3);
                                    rawData[offset] = (byte) alphas[bits & 0x07];
                                }

                                bits >>= 3;
                            }
                        }

                        // Last three bytes
                        //bits = (uint)(alphamask[3]);
                        bits = (uint) (alphamask[3] | (alphamask[4] << 8) | (alphamask[5] << 16));
                        for (int j = 2; j < 4; j++)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                // only put pixels out < width or height
                                if (x + i < width && y + j < height)
                                {
                                    uint offset = (uint) (z * sizeofplane + (y + j) * bps + (x + i) * bpp + 3);
                                    rawData[offset] = (byte) alphas[bits & 0x07];
                                }

                                bits >>= 3;
                            }
                        }
                    }
                }
            }
        }

        return rawData;
    }

    private static unsafe byte[] DecompressRGB(DDSStruct header, byte[] data, PixelFormatDDS pixelFormat)
    {
        // allocate bitmap
        int bpp = (int) Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount);
        int bps = (int) (header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
        int sizeofplane = (int) (bps * header.height);
        int width = (int) header.width;
        int height = (int) header.height;
        int depth = (int) header.depth;

        byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

        uint valMask = (uint) (header.pixelformat.rgbbitcount == 32 ? ~0 : (1 << (int) header.pixelformat.rgbbitcount) - 1);
        uint pixSize = (uint) (((int) header.pixelformat.rgbbitcount + 7) / 8);
        int rShift1 = 0;
        int rMul = 0;
        int rShift2 = 0;
        Helper.ComputeMaskParams(header.pixelformat.rbitmask, ref rShift1, ref rMul, ref rShift2);
        int gShift1 = 0;
        int gMul = 0;
        int gShift2 = 0;
        Helper.ComputeMaskParams(header.pixelformat.gbitmask, ref gShift1, ref gMul, ref gShift2);
        int bShift1 = 0;
        int bMul = 0;
        int bShift2 = 0;
        Helper.ComputeMaskParams(header.pixelformat.bbitmask, ref bShift1, ref bMul, ref bShift2);

        int offset = 0;
        int pixnum = width * height * depth;
        fixed (byte* bytePtr = data)
        {
            byte* temp = bytePtr;
            while (pixnum-- > 0)
            {
                uint px = *((uint*) temp) & valMask;
                temp += pixSize;
                uint pxc = px & header.pixelformat.rbitmask;
                rawData[offset + 0] = (byte) (((pxc >> rShift1) * rMul) >> rShift2);
                pxc = px & header.pixelformat.gbitmask;
                rawData[offset + 1] = (byte) (((pxc >> gShift1) * gMul) >> gShift2);
                pxc = px & header.pixelformat.bbitmask;
                rawData[offset + 2] = (byte) (((pxc >> bShift1) * bMul) >> bShift2);
                rawData[offset + 3] = 0xff;
                offset += 4;
            }
        }

        return rawData;
    }

    private static unsafe byte[] DecompressRGBA(DDSStruct header, byte[] data, PixelFormatDDS pixelFormat)
    {
        // allocate bitmap
        int bpp = (int) Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount);
        int bps = (int) (header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
        int sizeofplane = (int) (bps * header.height);
        int width = (int) header.width;
        int height = (int) header.height;
        int depth = (int) header.depth;

        byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

        uint valMask = (uint) (header.pixelformat.rgbbitcount == 32 ? ~0 : (1 << (int) header.pixelformat.rgbbitcount) - 1);
        // Funny x86s, make 1 << 32 == 1
        uint pixSize = (header.pixelformat.rgbbitcount + 7) / 8;
        int rShift1 = 0;
        int rMul = 0;
        int rShift2 = 0;
        Helper.ComputeMaskParams(header.pixelformat.rbitmask, ref rShift1, ref rMul, ref rShift2);
        int gShift1 = 0;
        int gMul = 0;
        int gShift2 = 0;
        Helper.ComputeMaskParams(header.pixelformat.gbitmask, ref gShift1, ref gMul, ref gShift2);
        int bShift1 = 0;
        int bMul = 0;
        int bShift2 = 0;
        Helper.ComputeMaskParams(header.pixelformat.bbitmask, ref bShift1, ref bMul, ref bShift2);
        int aShift1 = 0;
        int aMul = 0;
        int aShift2 = 0;
        Helper.ComputeMaskParams(header.pixelformat.alphabitmask, ref aShift1, ref aMul, ref aShift2);

        int offset = 0;
        int pixnum = width * height * depth;
        fixed (byte* bytePtr = data)
        {
            byte* temp = bytePtr;

            while (pixnum-- > 0)
            {
                uint px = *((uint*) temp) & valMask;
                temp += pixSize;
                uint pxc = px & header.pixelformat.rbitmask;
                rawData[offset + 0] = (byte) (((pxc >> rShift1) * rMul) >> rShift2);
                pxc = px & header.pixelformat.gbitmask;
                rawData[offset + 1] = (byte) (((pxc >> gShift1) * gMul) >> gShift2);
                pxc = px & header.pixelformat.bbitmask;
                rawData[offset + 2] = (byte) (((pxc >> bShift1) * bMul) >> bShift2);
                pxc = px & header.pixelformat.alphabitmask;
                rawData[offset + 3] = (byte) (((pxc >> aShift1) * aMul) >> aShift2);
                offset += 4;
            }
        }

        return rawData;
    }

    private static unsafe byte[] Decompress3Dc(DDSStruct header, byte[] data, PixelFormatDDS pixelFormat)
    {
        // allocate bitmap
        int bpp = (int) Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount);
        int bps = (int) (header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
        int sizeofplane = (int) (bps * header.height);
        int width = (int) header.width;
        int height = (int) header.height;
        int depth = (int) header.depth;

        byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];
        byte[] yColours = new byte[8];
        byte[] xColours = new byte[8];

        int offset = 0;
        fixed (byte* bytePtr = data)
        {
            byte* temp = bytePtr;
            for (int z = 0; z < depth; z++)
            {
                for (int y = 0; y < height; y += 4)
                {
                    for (int x = 0; x < width; x += 4)
                    {
                        byte* temp2 = temp + 8;

                        //Read Y palette
                        int t1 = yColours[0] = temp[0];
                        int t2 = yColours[1] = temp[1];
                        temp += 2;
                        if (t1 > t2)
                        {
                            for (int i = 2; i < 8; ++i)
                                yColours[i] = (byte) (t1 + (t2 - t1) * (i - 1) / 7);
                        }
                        else
                        {
                            for (int i = 2; i < 6; ++i)
                                yColours[i] = (byte) (t1 + (t2 - t1) * (i - 1) / 5);
                            yColours[6] = 0;
                            yColours[7] = 255;
                        }

                        // Read X palette
                        t1 = xColours[0] = temp2[0];
                        t2 = xColours[1] = temp2[1];
                        temp2 += 2;
                        if (t1 > t2)
                        {
                            for (int i = 2; i < 8; ++i)
                                xColours[i] = (byte) (t1 + (t2 - t1) * (i - 1) / 7);
                        }
                        else
                        {
                            for (int i = 2; i < 6; ++i)
                                xColours[i] = (byte) (t1 + (t2 - t1) * (i - 1) / 5);
                            xColours[6] = 0;
                            xColours[7] = 255;
                        }

                        //decompress pixel data
                        int currentOffset = offset;
                        for (int k = 0; k < 4; k += 2)
                        {
                            // First three bytes
                            uint bitmask = ((uint) temp[0] << 0) | ((uint) temp[1] << 8) | ((uint) temp[2] << 16);
                            uint bitmask2 = ((uint) temp2[0] << 0) | ((uint) temp2[1] << 8) | ((uint) temp2[2] << 16);
                            for (int j = 0; j < 2; j++)
                            {
                                // only put pixels out < height
                                if (y + k + j < height)
                                {
                                    for (int i = 0; i < 4; i++)
                                    {
                                        // only put pixels out < width
                                        if (x + i < width)
                                        {
                                            int t;
                                            byte tx, ty;

                                            t1 = currentOffset + (x + i) * 3;
                                            rawData[t1 + 1] = ty = yColours[bitmask & 0x07];
                                            rawData[t1 + 0] = tx = xColours[bitmask2 & 0x07];

                                            //calculate b (z) component ((r/255)^2 + (g/255)^2 + (b/255)^2 = 1
                                            t = 127 * 128 - (tx - 127) * (tx - 128) - (ty - 127) * (ty - 128);
                                            if (t > 0)
                                                rawData[t1 + 2] = (byte) (Math.Sqrt(t) + 128);
                                            else
                                                rawData[t1 + 2] = 0x7F;
                                        }

                                        bitmask >>= 3;
                                        bitmask2 >>= 3;
                                    }

                                    currentOffset += bps;
                                }
                            }

                            temp += 3;
                            temp2 += 3;
                        }

                        //skip bytes that were read via Temp2
                        temp += 8;
                    }

                    offset += bps * 4;
                }
            }
        }

        return rawData;
    }

    private static unsafe byte[] DecompressAti1n(DDSStruct header, byte[] data, PixelFormatDDS pixelFormat)
    {
        // allocate bitmap
        int bpp = (int) Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount);
        int bps = (int) (header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
        int sizeofplane = (int) (bps * header.height);
        int width = (int) header.width;
        int height = (int) header.height;
        int depth = (int) header.depth;

        byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];
        byte[] colours = new byte[8];

        uint offset = 0;
        fixed (byte* bytePtr = data)
        {
            byte* temp = bytePtr;
            for (int z = 0; z < depth; z++)
            {
                for (int y = 0; y < height; y += 4)
                {
                    for (int x = 0; x < width; x += 4)
                    {
                        //Read palette
                        int t1 = colours[0] = temp[0];
                        int t2 = colours[1] = temp[1];
                        temp += 2;
                        if (t1 > t2)
                        {
                            for (int i = 2; i < 8; ++i)
                                colours[i] = (byte) (t1 + (t2 - t1) * (i - 1) / 7);
                        }
                        else
                        {
                            for (int i = 2; i < 6; ++i)
                                colours[i] = (byte) (t1 + (t2 - t1) * (i - 1) / 5);
                            colours[6] = 0;
                            colours[7] = 255;
                        }

                        //decompress pixel data
                        uint currOffset = offset;
                        for (int k = 0; k < 4; k += 2)
                        {
                            // First three bytes
                            uint bitmask = ((uint) temp[0] << 0) | ((uint) temp[1] << 8) | ((uint) temp[2] << 16);
                            for (int j = 0; j < 2; j++)
                            {
                                // only put pixels out < height
                                if (y + k + j < height)
                                {
                                    for (int i = 0; i < 4; i++)
                                    {
                                        // only put pixels out < width
                                        if (x + i < width)
                                        {
                                            t1 = (int) (currOffset + (x + i));
                                            rawData[t1] = colours[bitmask & 0x07];
                                        }

                                        bitmask >>= 3;
                                    }

                                    currOffset += (uint) bps;
                                }
                            }

                            temp += 3;
                        }
                    }

                    offset += (uint) (bps * 4);
                }
            }
        }

        return rawData;
    }

    private static unsafe byte[] DecompressLum(DDSStruct header, byte[] data, PixelFormatDDS pixelFormat)
    {
        // allocate bitmap
        int bpp = (int) Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount);
        int bps = (int) (header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
        int sizeofplane = (int) (bps * header.height);
        int width = (int) header.width;
        int height = (int) header.height;
        int depth = (int) header.depth;

        byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

        int lShift1 = 0;
        int lMul = 0;
        int lShift2 = 0;
        Helper.ComputeMaskParams(header.pixelformat.rbitmask, ref lShift1, ref lMul, ref lShift2);

        int offset = 0;
        int pixnum = width * height * depth;
        fixed (byte* bytePtr = data)
        {
            byte* temp = bytePtr;
            while (pixnum-- > 0)
            {
                byte px = *temp++;
                rawData[offset + 0] = (byte) (((px >> lShift1) * lMul) >> lShift2);
                rawData[offset + 1] = (byte) (((px >> lShift1) * lMul) >> lShift2);
                rawData[offset + 2] = (byte) (((px >> lShift1) * lMul) >> lShift2);
                rawData[offset + 3] = (byte) (((px >> lShift1) * lMul) >> lShift2);
                offset += 4;
            }
        }

        return rawData;
    }

    private static unsafe byte[] DecompressRXGB(DDSStruct header, byte[] data, PixelFormatDDS pixelFormat)
    {
        // allocate bitmap
        int bpp = (int) Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount);
        int bps = (int) (header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
        int sizeofplane = (int) (bps * header.height);
        int width = (int) header.width;
        int height = (int) header.height;
        int depth = (int) header.depth;

        byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];

        Colour565 color_0 = new Colour565();
        Colour565 color_1 = new Colour565();
        Colour8888[] colours = new Colour8888[4];
        byte[] alphas = new byte[8];

        fixed (byte* bytePtr = data)
        {
            byte* temp = bytePtr;
            for (int z = 0; z < depth; z++)
            {
                for (int y = 0; y < height; y += 4)
                {
                    for (int x = 0; x < width; x += 4)
                    {
                        if (y >= height || x >= width)
                            break;
                        alphas[0] = temp[0];
                        alphas[1] = temp[1];
                        byte* alphamask = temp + 2;
                        temp += 8;

                        Helper.DxtcReadColors(temp, ref color_0, ref color_1);
                        temp += 4;

                        uint bitmask = ((uint*) temp)[1];
                        temp += 4;

                        colours[0].red = (byte) (color_0.red << 3);
                        colours[0].green = (byte) (color_0.green << 2);
                        colours[0].blue = (byte) (color_0.blue << 3);
                        colours[0].alpha = 0xFF;

                        colours[1].red = (byte) (color_1.red << 3);
                        colours[1].green = (byte) (color_1.green << 2);
                        colours[1].blue = (byte) (color_1.blue << 3);
                        colours[1].alpha = 0xFF;

                        // Four-color block: derive the other two colors.
                        // 00 = color_0, 01 = color_1, 10 = color_2, 11 = color_3
                        // These 2-bit codes correspond to the 2-bit fields
                        // stored in the 64-bit block.
                        colours[2].blue = (byte) ((2 * colours[0].blue + colours[1].blue + 1) / 3);
                        colours[2].green = (byte) ((2 * colours[0].green + colours[1].green + 1) / 3);
                        colours[2].red = (byte) ((2 * colours[0].red + colours[1].red + 1) / 3);
                        colours[2].alpha = 0xFF;

                        colours[3].blue = (byte) ((colours[0].blue + 2 * colours[1].blue + 1) / 3);
                        colours[3].green = (byte) ((colours[0].green + 2 * colours[1].green + 1) / 3);
                        colours[3].red = (byte) ((colours[0].red + 2 * colours[1].red + 1) / 3);
                        colours[3].alpha = 0xFF;

                        int k = 0;
                        for (int j = 0; j < 4; j++)
                        {
                            for (int i = 0; i < 4; i++, k++)
                            {
                                int select = (int) ((bitmask & (0x03 << (k * 2))) >> (k * 2));
                                Colour8888 col = colours[select];

                                // only put pixels out < width or height
                                if (x + i < width && y + j < height)
                                {
                                    uint offset = (uint) (z * sizeofplane + (y + j) * bps + (x + i) * bpp);
                                    rawData[offset + 0] = col.red;
                                    rawData[offset + 1] = col.green;
                                    rawData[offset + 2] = col.blue;
                                }
                            }
                        }

                        // 8-alpha or 6-alpha block?
                        if (alphas[0] > alphas[1])
                        {
                            // 8-alpha block:  derive the other six alphas.
                            // Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
                            alphas[2] = (byte) ((6 * alphas[0] + 1 * alphas[1] + 3) / 7); // bit code 010
                            alphas[3] = (byte) ((5 * alphas[0] + 2 * alphas[1] + 3) / 7); // bit code 011
                            alphas[4] = (byte) ((4 * alphas[0] + 3 * alphas[1] + 3) / 7); // bit code 100
                            alphas[5] = (byte) ((3 * alphas[0] + 4 * alphas[1] + 3) / 7); // bit code 101
                            alphas[6] = (byte) ((2 * alphas[0] + 5 * alphas[1] + 3) / 7); // bit code 110
                            alphas[7] = (byte) ((1 * alphas[0] + 6 * alphas[1] + 3) / 7); // bit code 111
                        }
                        else
                        {
                            // 6-alpha block.
                            // Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
                            alphas[2] = (byte) ((4 * alphas[0] + 1 * alphas[1] + 2) / 5); // Bit code 010
                            alphas[3] = (byte) ((3 * alphas[0] + 2 * alphas[1] + 2) / 5); // Bit code 011
                            alphas[4] = (byte) ((2 * alphas[0] + 3 * alphas[1] + 2) / 5); // Bit code 100
                            alphas[5] = (byte) ((1 * alphas[0] + 4 * alphas[1] + 2) / 5); // Bit code 101
                            alphas[6] = 0x00; // Bit code 110
                            alphas[7] = 0xFF; // Bit code 111
                        }

                        // Note: Have to separate the next two loops,
                        //	it operates on a 6-byte system.
                        // First three bytes
                        uint bits = *((uint*) alphamask);
                        for (int j = 0; j < 2; j++)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                // only put pixels out < width or height
                                if (x + i < width && y + j < height)
                                {
                                    uint offset = (uint) (z * sizeofplane + (y + j) * bps + (x + i) * bpp + 3);
                                    rawData[offset] = alphas[bits & 0x07];
                                }

                                bits >>= 3;
                            }
                        }

                        // Last three bytes
                        bits = *((uint*) &alphamask[3]);
                        for (int j = 2; j < 4; j++)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                // only put pixels out < width or height
                                if (x + i < width && y + j < height)
                                {
                                    uint offset = (uint) (z * sizeofplane + (y + j) * bps + (x + i) * bpp + 3);
                                    rawData[offset] = alphas[bits & 0x07];
                                }

                                bits >>= 3;
                            }
                        }
                    }
                }
            }
        }

        return rawData;
    }

    private static unsafe byte[] DecompressFloat(DDSStruct header, byte[] data, PixelFormatDDS pixelFormat)
    {
        // allocate bitmap
        int bpp = (int) Helper.PixelFormatToBpp(pixelFormat, header.pixelformat.rgbbitcount);
        int bps = (int) (header.width * bpp * Helper.PixelFormatToBpc(pixelFormat));
        int sizeofplane = (int) (bps * header.height);
        int width = (int) header.width;
        int height = (int) header.height;
        int depth = (int) header.depth;

        byte[] rawData = new byte[depth * sizeofplane + height * bps + width * bpp];
        int size = 0;
        fixed (byte* bytePtr = data)
        {
            byte* temp = bytePtr;
            fixed (byte* destPtr = rawData)
            {
                byte* destData = destPtr;
                switch (pixelFormat)
                {
                    case PixelFormatDDS.R32F: // Red float, green = blue = max
                        size = width * height * depth * 3;
                        for (int i = 0, j = 0; i < size; i += 3, j++)
                        {
                            ((float*) destData)[i] = ((float*) temp)[j];
                            ((float*) destData)[i + 1] = 1.0f;
                            ((float*) destData)[i + 2] = 1.0f;
                        }

                        break;

                    case PixelFormatDDS.A32B32G32R32F: // Direct copy of float RGBA data
                        Array.Copy(data, rawData, data.Length);
                        break;

                    case PixelFormatDDS.G32R32F: // Red float, green float, blue = max
                        size = width * height * depth * 3;
                        for (int i = 0, j = 0; i < size; i += 3, j += 2)
                        {
                            ((float*) destData)[i] = ((float*) temp)[j];
                            ((float*) destData)[i + 1] = ((float*) temp)[j + 1];
                            ((float*) destData)[i + 2] = 1.0f;
                        }

                        break;

                    case PixelFormatDDS.R16F: // Red float, green = blue = max
                        size = width * height * depth * bpp;
                        Helper.ConvR16ToFloat32((uint*) destData, (ushort*) temp, (uint) size);
                        break;

                    case PixelFormatDDS.A16B16G16R16F: // Just convert from half to float.
                        size = width * height * depth * bpp;
                        Helper.ConvFloat16ToFloat32((uint*) destData, (ushort*) temp, (uint) size);
                        break;

                    case PixelFormatDDS.G16R16F: // Convert from half to float, set blue = max.
                        size = width * height * depth * bpp;
                        Helper.ConvG16R16ToFloat32((uint*) destData, (ushort*) temp, (uint) size);
                        break;

                    default:
                        break;
                }
            }
        }

        return rawData;
    }
}

public class Helper
{
    // iCompFormatToBpp
    internal static uint PixelFormatToBpp(PixelFormatDDS pf, uint rgbbitcount)
    {
        switch (pf)
        {
            case PixelFormatDDS.LUMINANCE:
            case PixelFormatDDS.LUMINANCE_ALPHA:
            case PixelFormatDDS.RGBA:
            case PixelFormatDDS.RGB:
                return rgbbitcount / 8;

            case PixelFormatDDS.THREEDC:
            case PixelFormatDDS.RXGB:
                return 3;

            case PixelFormatDDS.ATI1N:
                return 1;

            case PixelFormatDDS.R16F:
                return 2;

            case PixelFormatDDS.A16B16G16R16:
            case PixelFormatDDS.A16B16G16R16F:
            case PixelFormatDDS.G32R32F:
                return 8;

            case PixelFormatDDS.A32B32G32R32F:
                return 16;

            default:
                return 4;
        }
    }

    // iCompFormatToBpc
    internal static uint PixelFormatToBpc(PixelFormatDDS pf)
    {
        switch (pf)
        {
            case PixelFormatDDS.R16F:
            case PixelFormatDDS.G16R16F:
            case PixelFormatDDS.A16B16G16R16F:
                return 4;

            case PixelFormatDDS.R32F:
            case PixelFormatDDS.G32R32F:
            case PixelFormatDDS.A32B32G32R32F:
                return 4;

            case PixelFormatDDS.A16B16G16R16:
                return 2;

            default:
                return 1;
        }
    }

    internal static bool Check16BitComponents(DDSStruct header)
    {
        if (header.pixelformat.rgbbitcount != 32)
            return false;
        // a2b10g10r10 format
        if (header.pixelformat.rbitmask == 0x3FF00000 && header.pixelformat.gbitmask == 0x000FFC00 && header.pixelformat.bbitmask == 0x000003FF && header.pixelformat.alphabitmask == 0xC0000000)
            return true;
        // a2r10g10b10 format
        if (header.pixelformat.rbitmask == 0x000003FF && header.pixelformat.gbitmask == 0x000FFC00 && header.pixelformat.bbitmask == 0x3FF00000 && header.pixelformat.alphabitmask == 0xC0000000)
            return true;

        return false;
    }

    internal static void CorrectPremult(uint pixnum, ref byte[] buffer)
    {
        for (uint i = 0; i < pixnum; i++)
        {
            byte alpha = buffer[i + 3];
            if (alpha == 0) continue;
            int red = (buffer[i] << 8) / alpha;
            int green = (buffer[i + 1] << 8) / alpha;
            int blue = (buffer[i + 2] << 8) / alpha;

            buffer[i] = (byte) red;
            buffer[i + 1] = (byte) green;
            buffer[i + 2] = (byte) blue;
        }
    }

    internal static void ComputeMaskParams(uint mask, ref int shift1, ref int mul, ref int shift2)
    {
        shift1 = 0;
        mul = 1;
        shift2 = 0;
        if (mask == 0 || mask == uint.MaxValue)
            return;
        while ((mask & 1) == 0)
        {
            mask >>= 1;
            shift1++;
        }

        uint bc = 0;
        while ((mask & (1 << (int) bc)) != 0) bc++;
        while (mask * mul < 255)
            mul = (mul << (int) bc) + 1;
        mask *= (uint) mul;

        while ((mask & ~0xff) != 0)
        {
            mask >>= 1;
            shift2++;
        }
    }

    internal static unsafe void DxtcReadColors(byte* data, ref Colour8888[] op)
    {
        byte r0, g0, b0, r1, g1, b1;

        b0 = (byte) (data[0] & 0x1F);
        g0 = (byte) (((data[0] & 0xE0) >> 5) | ((data[1] & 0x7) << 3));
        r0 = (byte) ((data[1] & 0xF8) >> 3);

        b1 = (byte) (data[2] & 0x1F);
        g1 = (byte) (((data[2] & 0xE0) >> 5) | ((data[3] & 0x7) << 3));
        r1 = (byte) ((data[3] & 0xF8) >> 3);

        op[0].red = (byte) ((r0 << 3) | (r0 >> 2));
        op[0].green = (byte) ((g0 << 2) | (g0 >> 3));
        op[0].blue = (byte) ((b0 << 3) | (b0 >> 2));

        op[1].red = (byte) ((r1 << 3) | (r1 >> 2));
        op[1].green = (byte) ((g1 << 2) | (g1 >> 3));
        op[1].blue = (byte) ((b1 << 3) | (b1 >> 2));
    }

    internal static void DxtcReadColor(ushort data, ref Colour8888 op)
    {
        byte r, g, b;

        b = (byte) (data & 0x1f);
        g = (byte) ((data & 0x7E0) >> 5);
        r = (byte) ((data & 0xF800) >> 11);

        op.red = (byte) ((r << 3) | (r >> 2));
        op.green = (byte) ((g << 2) | (g >> 3));
        op.blue = (byte) ((b << 3) | (r >> 2));
    }

    internal static unsafe void DxtcReadColors(byte* data, ref Colour565 color_0, ref Colour565 color_1)
    {
        color_0.blue = (byte) (data[0] & 0x1F);
        color_0.green = (byte) (((data[0] & 0xE0) >> 5) | ((data[1] & 0x7) << 3));
        color_0.red = (byte) ((data[1] & 0xF8) >> 3);

        color_0.blue = (byte) (data[2] & 0x1F);
        color_0.green = (byte) (((data[2] & 0xE0) >> 5) | ((data[3] & 0x7) << 3));
        color_0.red = (byte) ((data[3] & 0xF8) >> 3);
    }

    internal static void GetBitsFromMask(uint mask, ref uint shiftLeft, ref uint shiftRight)
    {
        uint temp, i;

        if (mask == 0)
        {
            shiftLeft = shiftRight = 0;
            return;
        }

        temp = mask;
        for (i = 0; i < 32; i++, temp >>= 1)
        {
            if ((temp & 1) != 0)
                break;
        }

        shiftRight = i;

        // Temp is preserved, so use it again:
        for (i = 0; i < 8; i++, temp >>= 1)
        {
            if ((temp & 1) == 0)
                break;
        }

        shiftLeft = 8 - i;
    }

    // This function simply counts how many contiguous bits are in the mask.
    internal static uint CountBitsFromMask(uint mask)
    {
        uint i, testBit = 0x01, count = 0;
        bool foundBit = false;

        for (i = 0; i < 32; i++, testBit <<= 1)
        {
            if ((mask & testBit) != 0)
            {
                if (!foundBit)
                    foundBit = true;
                count++;
            }
            else if (foundBit)
                return count;
        }

        return count;
    }

    internal static uint HalfToFloat(ushort y)
    {
        int s = (y >> 15) & 0x00000001;
        int e = (y >> 10) & 0x0000001f;
        int m = y & 0x000003ff;

        if (e == 0)
        {
            if (m == 0)
            {
                //
                // Plus or minus zero
                //
                return (uint) (s << 31);
            }

            //
            // Denormalized number -- renormalize it
            //
            while ((m & 0x00000400) == 0)
            {
                m <<= 1;
                e -= 1;
            }

            e += 1;
            m &= ~0x00000400;
        }
        else if (e == 31)
        {
            if (m == 0)
            {
                //
                // Positive or negative infinity
                //
                return (uint) ((s << 31) | 0x7f800000);
            }

            //
            // Nan -- preserve sign and significand bits
            //
            return (uint) ((s << 31) | 0x7f800000 | (m << 13));
        }

        //
        // Normalized number
        //
        e = e + (127 - 15);
        m = m << 13;

        //
        // Assemble s, e and m.
        //
        return (uint) ((s << 31) | (e << 23) | m);
    }

    internal static unsafe void ConvFloat16ToFloat32(uint* dest, ushort* src, uint size)
    {
        uint i;
        for (i = 0; i < size; ++i, ++dest, ++src)
            //float: 1 sign bit, 8 exponent bits, 23 mantissa bits
            //half: 1 sign bit, 5 exponent bits, 10 mantissa bits
            *dest = HalfToFloat(*src);
    }

    internal static unsafe void ConvG16R16ToFloat32(uint* dest, ushort* src, uint size)
    {
        uint i;
        for (i = 0; i < size; i += 3)
        {
            //float: 1 sign bit, 8 exponent bits, 23 mantissa bits
            //half: 1 sign bit, 5 exponent bits, 10 mantissa bits
            *dest++ = HalfToFloat(*src++);
            *dest++ = HalfToFloat(*src++);
            *((float*) dest++) = 1.0f;
        }
    }

    internal static unsafe void ConvR16ToFloat32(uint* dest, ushort* src, uint size)
    {
        uint i;
        for (i = 0; i < size; i += 3)
        {
            //float: 1 sign bit, 8 exponent bits, 23 mantissa bits
            //half: 1 sign bit, 5 exponent bits, 10 mantissa bits
            *dest++ = HalfToFloat(*src++);
            *((float*) dest++) = 1.0f;
            *((float*) dest++) = 1.0f;
        }
    }

    #region Constants

    // DDSStruct flags
    public const int DDSD_CAPS = 0x00000001;

    public const int DDSD_HEIGHT = 0x00000002;
    public const int DDSD_WIDTH = 0x00000004;
    public const int DDSD_PITCH = 0x00000008;
    public const int DDSD_PIXELFORMAT = 0x00001000;
    public const int DDSD_MIPMAPCOUNT = 0x00020000;
    public const int DDSD_LINEARSIZE = 0x00080000;
    public const int DDSD_DEPTH = 0x00800000;

    // PixelFormat values
    public const int DDPF_ALPHAPIXELS = 0x00000001;

    public const int DDPF_FOURCC = 0x00000004;
    public const int DDPF_RGB = 0x00000040;
    public const int DDPF_LUMINANCE = 0x00020000;

    // DDSCaps
    public const int DDSCAPS_COMPLEX = 0x00000008;

    public const int DDSCAPS_TEXTURE = 0x00001000;
    public const int DDSCAPS_MIPMAP = 0x00400000;
    public const int DDSCAPS2_CUBEMAP = 0x00000200;
    public const int DDSCAPS2_CUBEMAP_POSITIVEX = 0x00000400;
    public const int DDSCAPS2_CUBEMAP_NEGATIVEX = 0x00000800;
    public const int DDSCAPS2_CUBEMAP_POSITIVEY = 0x00001000;
    public const int DDSCAPS2_CUBEMAP_NEGATIVEY = 0x00002000;
    public const int DDSCAPS2_CUBEMAP_POSITIVEZ = 0x00004000;
    public const int DDSCAPS2_CUBEMAP_NEGATIVEZ = 0x00008000;
    public const int DDSCAPS2_VOLUME = 0x00200000;

    // FOURCC
    public const uint FOURCC_DXT1 = 0x31545844;

    public const uint FOURCC_DXT2 = 0x32545844;
    public const uint FOURCC_DXT3 = 0x33545844;
    public const uint FOURCC_DXT4 = 0x34545844;
    public const uint FOURCC_DXT5 = 0x35545844;
    public const uint FOURCC_ATI1 = 0x31495441;
    public const uint FOURCC_ATI2 = 0x32495441;
    public const uint FOURCC_RXGB = 0x42475852;
    public const uint FOURCC_DOLLARNULL = 0x24;
    public const uint FOURCC_oNULL = 0x6f;
    public const uint FOURCC_pNULL = 0x70;
    public const uint FOURCC_qNULL = 0x71;
    public const uint FOURCC_rNULL = 0x72;
    public const uint FOURCC_sNULL = 0x73;
    public const uint FOURCC_tNULL = 0x74;

    #endregion Constants
}

[StructLayout(LayoutKind.Sequential)]
public struct Colour8888
{
    public byte red;
    public byte green;
    public byte blue;
    public byte alpha;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct Colour565
{
    public ushort blue; //: 5;
    public ushort green; //: 6;
    public ushort red; //: 5;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public struct DDSStruct
{
    public uint size; // equals size of struct (which is part of the data file!)
    public uint flags;
    public uint height;
    public uint width;
    public uint sizeorpitch;
    public uint depth;
    public uint mipmapcount;
    public uint alphabitdepth;

    //[MarshalAs(UnmanagedType.U4, SizeConst = 11)]
    public uint[] reserved; //[11];

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct pixelformatstruct
    {
        public uint size; // equals size of struct (which is part of the data file!)
        public uint flags;
        public uint fourcc;
        public uint rgbbitcount;
        public uint rbitmask;
        public uint gbitmask;
        public uint bbitmask;
        public uint alphabitmask;
    }

    public pixelformatstruct pixelformat;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct ddscapsstruct
    {
        public uint caps1;
        public uint caps2;
        public uint caps3;
        public uint caps4;
    }

    public ddscapsstruct ddscaps;
    public uint texturestage;
}

public enum PixelFormatDDS
{
    RGBA,
    RGB,
    DXT1,
    DXT2,
    DXT3,
    DXT4,
    DXT5,
    THREEDC,
    ATI1N,
    LUMINANCE,
    LUMINANCE_ALPHA,
    RXGB,
    A16B16G16R16,
    R16F,
    G16R16F,
    A16B16G16R16F,
    R32F,
    G32R32F,
    A32B32G32R32F,
    UNKNOWN
}

public class TestHelper
{
    public static int[] ComputeMaskParams(uint mask)
    {
        int rShift1 = 0;
        int rMul = 0;
        int rShift2 = 0;
        Helper.ComputeMaskParams(mask, ref rShift1, ref rMul, ref rShift2);
        return new[] {rShift1, rMul, rShift2};
    }
}