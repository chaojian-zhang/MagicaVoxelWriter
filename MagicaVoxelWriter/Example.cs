using System;

namespace MagicaVoxelWriter
{
    class Example
    {
        /// <summary>
        /// Generates a random walk
        /// </summary>
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Missing parameter: output file path.");
                Console.Write("Enter file path now or enter return to exit: ");
                string path = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(path))
                    WaveClean(path);
            }
            else
                WaveClean(args[0]);
        }

        private static void RandomWalk(string filePath)
        {
            // Create a new Voxel volume (maximum dimensions are 256x256x256 right now)
            var vox = new VoxWriter(256, 256, 256);
            // Just some random X/Y/Z walk
            Random rand = new Random();
            var x = 128;
            var y = 128;
            var z = 0;
            for (var i = 0; i < 12000; i++)
            {
                // This sets a voxel at the x/y/z coordinate with color palette index c
                // note that index 0 is an empty cell and will delete a voxel in case
                // there is one already at that position
                byte colorIndex = (byte)(rand.NextDouble() < 0.01 ? 2 : 1);
                vox.SetVoxel(x, y, z, colorIndex);
                vox.SetVoxel(255 - x, y, z, colorIndex);
                vox.SetVoxel(x, 255 - y, z, colorIndex);
                vox.SetVoxel(255 - x, 255 - y, z, colorIndex);
                // Above creates a symmetrical pattern

                int[,] steps = new int[,] {{ 1, 0, 0 },
                    { -1, 0, 0 },
                    { 0, 1, 0 },
                    { 0, -1, 0 },
                    { 1, 0, 0 },
                    { -1, 0, 0 },
                    { 0, 1, 0 },
                    { 0, -1, 0 },
                    { 0, 0, 1 },
                    { 0, 0, -1 }
                };
                int groupIndex = (int)Math.Floor(rand.NextDouble() * 10);
                x = (x + steps[groupIndex, 0]) % 256;
                y = (y + steps[groupIndex, 1]) % 256;
                z = (z + steps[groupIndex, 2]);
                if (z < 0)
                    z = 0;
            }
            // Set color index #2
            vox.Palette[1] = 0xffff8000;

            // Save to file
            vox.Export(filePath);
        }
        private static void Test(string filePath)
        {
            var vox = new VoxWriterClean(255, 255, 25);
            for (int x = 0; x < 256; x++)
            {
                vox.SetVoxel(new Voxel((byte)x, 0, 0, 2));
            }
            vox.Palette[1] = new RGBA(0xff, 0x80, 0x00, 0xff);
            vox.Export(filePath);
        }
        private static void WaveClean(string filePath)
        {
            // Create a new Voxel volume (maximum dimensions are 256x256x256 right now)
            var vox = new VoxWriterClean(125, 125, 125);
            // Wave
            var cx = 63;
            var cy = 63;
            var cz = 63;

            byte colorIndex = 2;
            for (byte x = 0; x < 126; x++)
            {
                for (byte y = 0; y < 126; y++)
                {
                    for (byte z = 0; z < 126; z++)
                    {
                        double t = Math.Sqrt(Math.Pow(x - cx, 2) + Math.Pow(y - cy, 2));
                        double s = cz * (Math.Cos(t * 3.14159 * 0.1) * 0.25 + 1.0);
                        if(Math.Abs(z - s) <= 2.0)
                        {
                            vox.SetVoxel(new Voxel(x, y, z, colorIndex));
                            Console.WriteLine($"{x} {y} {z}");
                        }
                    }
                }
            }

            // Set color index #2
            vox.Palette[1] = new RGBA(0xff, 0x80, 0x00, 0xff);

            // Save to file
            vox.Export(filePath);
        }
    }
}
