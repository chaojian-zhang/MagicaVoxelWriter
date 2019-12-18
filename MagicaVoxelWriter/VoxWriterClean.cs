using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MagicaVoxelWriter
{
    public struct Voxel
    {
        public byte X;
        public byte Y;
        public byte Z;
        /// <summary>
        /// Index of color in MagicaVoxel, 1-255 are valid color palette values, 0 is for empty cell
        /// </summary>
        public byte Index;

        public Voxel(byte x, byte y, byte z, byte index)
        {
            X = x;
            Y = y;
            Z = z;
            Index = index;
        }
        public string GetKey()
            => X + "_" + Y + "_" + Z;
    }

    public struct RGBA
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public RGBA(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }

    /// <summary>
    /// A custom, more structured implementation
    /// </summary>
    public class VoxWriterClean
    {
        #region Constructor
        /// <param name="maxX">Size in terms of coordinate index from 0-255</param>
        /// <param name="maxY">Size in terms of coordinate index from 0-255</param>
        /// <param name="maxZ">Size in terms of coordinate index from 0-255</param>
        public VoxWriterClean(byte maxX, byte maxY, byte maxZ)
        {
            X = maxX;
            Y = maxY;
            Z = maxZ;
            VCount = 0;
            Voxels = new Dictionary<string, Voxel>();
            Palette = new List<RGBA>();

            // Initialize palette
            // Notice 0 represent no voxel and is included as the first element, i from 256-0 inclusive
            // The end result here is to generate from [255, 255, 255, 255] to [0, 0, 0, 255], the last one being "index 0"
            for (int i = 255; i >= 0; i--)
            {
                byte b = (byte)i;
                Palette.Add(new RGBA(b, b, b, 255));
            }
        }
        public byte X { get; }
        public byte Y { get; }
        public byte Z { get; }
        public uint VCount { get; private set; }
        public Dictionary<string, Voxel> Voxels { get; private set; }
        /// <summary>
        /// Palette of RGB colors
        /// </summary>
        /// <remarks>The color palette can be written directly, format is 0xAARRGGBB; 
        /// Note that the palette values are offset by 1, so setting palette[0] will change the color index #1 (as in MagicaVoxel)
        /// Clearly palette[255] aka. index #256 is the empty voxel</remarks>
        public List<RGBA> Palette { get; private set; }
        #endregion

        #region Sub Routines
        private void AppendString(BinaryWriter data, string str)
        {
            for (var i = 0; i < str.Length; ++i)
                data.Write((byte)str[i]);
        }
        private void AppendUInt32(BinaryWriter data, uint n)
        {
            data.Write((byte)(n & 0xff));
            data.Write((byte)((n >> 8) & 0xff));
            data.Write((byte)((n >> 16) & 0xff));
            data.Write((byte)((n >> 24) & 0xff));
        }
        private void AppendRGBA(BinaryWriter data, RGBA rgba)
        {
            data.Write(rgba.R);
            data.Write(rgba.G);
            data.Write(rgba.B);
            data.Write(rgba.A);
        }
        private void AppendVoxel(BinaryWriter data, Voxel voxel)
        {
            data.Write(voxel.X);
            data.Write(voxel.Y);
            data.Write(voxel.Z);
            data.Write(voxel.Index);
        }
        #endregion

        #region Interface
        /// <summary>
        /// Set or remove a voxel
        /// </summary>
        /// <param name="v">Index 0 will clear the voxel</param>
        public void SetVoxel(Voxel v)
        {
            if (v.X <= this.X && v.Y <= this.Y && v.Z <= this.Z)
            {
                string key = v.GetKey();
                // Set
                if (v.Index != 0)
                {
                    if (!Voxels.ContainsKey(key))
                        VCount++;
                    Voxels[key] = v;
                }
                // Clear
                else
                {
                    if (Voxels.ContainsKey(key))
                        VCount--;
                    Voxels.Remove(key);
                }
            }
        }
        public void Export(string filePath)
        {
            using (FileStream file = new FileStream(filePath, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(file))
            {
                AppendString(writer, "VOX ");
                AppendUInt32(writer, 150);
                AppendString(writer, "MAIN");
                AppendUInt32(writer, 0);
                AppendUInt32(writer, VCount * 4 + 0x434);

                AppendString(writer, "SIZE");
                AppendUInt32(writer, 12);
                AppendUInt32(writer, 0);
                AppendUInt32(writer, (uint)X + 1);
                AppendUInt32(writer, (uint)Y + 1);
                AppendUInt32(writer, (uint)Z + 1);
                AppendString(writer, "XYZI");
                AppendUInt32(writer, 4 + VCount * 4);
                AppendUInt32(writer, 0);
                AppendUInt32(writer, VCount);
                foreach (var voxel in Voxels.Values)
                    AppendVoxel(writer, voxel);
                AppendString(writer, "RGBA");
                AppendUInt32(writer, 0x400);
                AppendUInt32(writer, 0);
                for (var i = 0; i < 256; i++)
                    AppendRGBA(writer, Palette[i]);
            }
        }
        #endregion
    }
}
