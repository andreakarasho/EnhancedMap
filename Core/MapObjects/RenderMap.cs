using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace EnhancedMap.Core.MapObjects
{
    public class Palette
    {
        const int PALETTE_LENGTH = 256;

        private int _steps = 3;

        public Palette()
        {
            Entries = new uint[PALETTE_LENGTH];
        }

        public uint[] Entries { get; }

        public bool Load(Bitmap bmp)
        {
            if (_steps > 0)
            {
                for (int i = 0; i < PALETTE_LENGTH; i++)
                    Entries[i] = (uint)(-16777216 | (bmp.Palette.Entries[i].R << 16) | (bmp.Palette.Entries[i].G << 8) | bmp.Palette.Entries[i].B);
                _steps--;

                return true;
            }
            return false;
        }

        public void Reset() => _steps = 3;
    }

    public class RenderMap : IRenderObject
    {
        private static readonly byte[] _bytesCode = { 0x57, 0x65, 0x6C, 0x63, 0x6F, 0x6D, 0x65, 0x20, 0x74, 0x6F, 0x20, 0x45, 0x6E, 0x68, 0x61, 0x6E, 0x63, 0x65, 0x64,
            0x4D ,0x61 ,0x70 ,0x21 ,0x20 ,0x54, 0x68 ,0x69 ,0x73 ,0x20 ,0x69, 0x73, 0x20 ,0x61, 0x20, 0x6E, 0x65, 0x77, 0x20 ,0x70, 0x72, 0x6F, 0x6A, 0x65 ,0x63, 0x74,
            0x20, 0x74 ,0x6F, 0x20, 0x65 ,0x78 ,0x70, 0x6C ,0x6F, 0x72, 0x65 ,0x20 ,0x55, 0x4F, 0x20, 0x57, 0x6F ,0x72 ,0x6C ,0x64, 0x21, 0x21 };

        private Bitmap _bmp;

        public RenderMap(MapEntry entry, string path)
        {
            MapEntry = entry;
            Path = path;
            IsVisible = true;
        }

        public string Path { get; private set; }
        public Bitmap Image { get; private set; }
        public Palette Palette { get; private set; }
        public bool IsDisposing { get; set; }
        public MapEntry MapEntry { get; }
        public bool IsVisible { get; set; }

        public void Unload()
        {
            if (Image != null)
            {
                Image.Dispose();
                Image = null;
            }

            Palette = null;
        }

        public void Load()
        {
            if (Image != null)
                Unload();      
            Image = Decode();
            Palette = new Palette();
            Palette.Load(Image);
        }

        public void Renew()
        {
            if (_bmp != null)
            {
                _bmp.Dispose();
                _bmp = null;
            }
        }

        public unsafe bool Render(Graphics g, int w, int h, float angle, short x = -1, short y = -1, float zoom = -1)
        {
            if (!IsVisible)
                return false;

            if (Image == null)
                Load();

            bool request = false;

            int playerCenterX = w / 2;
            int playerCenterY = h / 2;

            int canvasWidth;
            int canvasHeight;

            if (_bmp == null)
            {
                canvasWidth = w;
                canvasHeight = h;

                int size = (int)(Math.Max(w, h) * 1.75f);
                _bmp = new Bitmap(size, size, PixelFormat.Format32bppPArgb);
            }

            int width = Image.Width;
            int height = Image.Height;

            request |= Palette.Load(Image);

            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bitmapData = Image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            Rectangle rect2 = new Rectangle(0, 0, _bmp.Width, _bmp.Height);
            BitmapData data2 = _bmp.LockBits(rect2, ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
            byte* firstImgPtr = (byte*)bitmapData.Scan0.ToPointer();
            uint* secondImgPtr = (uint*)data2.Scan0.ToPointer();
            canvasWidth = _bmp.Width;
            canvasHeight = _bmp.Height;

            int iX = x > -1 ? x : (int)Global.X;
            int iY = y > -1 ? y : (int)Global.Y;
            float iZoom = zoom > -1 ? zoom : Global.Zoom;

            int playerLocX = (int)(iX / (1 << 0) - (int)(canvasWidth / iZoom / 2f));
            int playerLocY = (int)(iY / (1 << 0) - (int)(canvasHeight / iZoom / 2f));
            firstImgPtr += playerLocY * bitmapData.Stride + playerLocX;
            int baseX = playerLocX;
            byte* tempPtr = firstImgPtr;
            int zoomCalc = (int)(1f / iZoom * 65536f);

            int offset1 = 0;
            int offset2 = 0;

            fixed (uint* tempPtrPalette = Palette.Entries)
            {
                int j;
                for (j = 0; j < canvasHeight; j++)
                {
                    firstImgPtr = tempPtr;
                    int tempX = 0;
                    int k;
                    if (playerLocY >= 0 && playerLocY < height)
                    {
                        tempX = baseX;
                        k = 0;
                        offset2 = 0;
                        while (tempX < 0 && k < canvasWidth)
                        {
                            *secondImgPtr = 0u;
                            secondImgPtr++;
                            offset2 += zoomCalc;
                            firstImgPtr += offset2 >> 16;
                            tempX += offset2 >> 16;
                            offset2 &= ushort.MaxValue;
                            k++;
                        }
                        while (k < canvasWidth && tempX < width)
                        {
                            var b = *firstImgPtr;
                            *secondImgPtr = tempPtrPalette[b];
                            secondImgPtr++;
                            offset2 += zoomCalc;
                            firstImgPtr += offset2 >> 16;
                            tempX += offset2 >> 16;
                            offset2 &= ushort.MaxValue;
                            k++;
                        }
                        while (k < canvasWidth)
                        {
                            *secondImgPtr = 0u;
                            secondImgPtr++;
                            offset2 += zoomCalc;
                            firstImgPtr += offset2 >> 16;
                            tempX += offset2 >> 16;
                            offset2 &= ushort.MaxValue;
                            k++;
                        }
                    }
                    else
                    {
                        tempX = (int)(iX / (1 << 0) - (int)(canvasWidth / iZoom / 2f));
                        for (k = 0; k < canvasWidth; k++)
                        {
                            *secondImgPtr = 0u;
                            secondImgPtr++;
                            offset2 += zoomCalc;
                            firstImgPtr += offset2 >> 16;
                            tempX += offset2 >> 16;
                            offset2 &= ushort.MaxValue;
                        }
                    }
                    offset1 += zoomCalc;
                    if (offset1 >= 65536)
                    {
                        tempPtr += bitmapData.Stride * (offset1 >> 16);
                        playerLocY += offset1 >> 16;
                        offset1 &= ushort.MaxValue;
                    }
                }
            }

            Image.UnlockBits(bitmapData);
            _bmp.UnlockBits(data2);

            g.TranslateTransform(playerCenterX, playerCenterY);
            g.RotateTransform(angle, System.Drawing.Drawing2D.MatrixOrder.Prepend);
            g.DrawImageUnscaled(_bmp, -_bmp.Width / 2, -_bmp.Height / 2);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

            return request;
        }

        // png standard format. Thanks wikipedia :P
        private readonly static byte[] _png_header = new byte[4]
        {
            0x89, 0x50, 0x4e, 0x47
        };

        private Bitmap Decode()
        {
            using (FileStream fs = File.OpenRead(Path))
            {
                byte[] buffer = new byte[(int)fs.Length];
                fs.Read(buffer, 0, buffer.Length);

                bool ispng = true;
                for (int i = 0; i < _png_header.Length; i++)
                {
                    if (_png_header[i] != buffer[i])
                    {
                        ispng = false;
                        break;
                    }
                }

                if (!ispng)
                    ReadMapBytes(buffer, fs.Length);
                return (Bitmap)System.Drawing.Image.FromStream(new MemoryStream(buffer));
            }
        }

        private unsafe void ReadMapBytes(byte[] buffer, long len)
        {
            byte l = (byte)(len >> 2 & byte.MaxValue);
            fixed (byte* bufferPtr = buffer)
            {
                byte* tempPtr = bufferPtr;
                for (; len > 0L; --len)
                {
                    *tempPtr = (byte)(*tempPtr ^ (uint)(byte)(_bytesCode[(int)len & 63] & (uint)byte.MaxValue));
                    *tempPtr = (byte)(*tempPtr + 139U);
                    *tempPtr = (byte)(*tempPtr ^ (uint)l);
                    *tempPtr = (byte)(*tempPtr - 17U);

                    byte shifted = (byte)((uint)*tempPtr >> 3);
                    ++tempPtr;
                    l -= shifted;
                }
            }
        }

        public void Dispose()
        {
            IsDisposing = true;
        }
    }
}
