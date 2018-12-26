using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace PirateninselLevelExport
{
    class Level
    {
        public ushort[,] Tiles;

        internal static Level Read(BinaryReader binaryReader)
        {
            ushort[,] tiles = new ushort[20, 14];

            for (int y = 0; y < 14; y++)
                for (int x = 0; x < 20; x++)
                    tiles[x, y] = (ushort)((binaryReader.ReadByte() << 8) | binaryReader.ReadByte());

            return new Level() { Tiles = tiles };
        }

        internal void Write(BinaryWriter binaryWriter)
        {
            for (int y = 0; y < 14; y++)
                for (int x = 0; x < 20; x++)
                {
                    binaryWriter.Write((byte)(Tiles[x, y] >> 8));
                    binaryWriter.Write((byte)Tiles[x, y]);
                }
        }

        public static Level[] ReadLevelFile(string path)
        {
            using (FileStream fs = File.OpenRead(path))
            using (BinaryReader br = new BinaryReader(fs))
            {
                br.ReadChars(4);
                ushort numLevels = br.ReadUInt16();

                Level[] levels = new Level[numLevels];

                for (int level = 0; level < numLevels; level++)
                    levels[level] = Level.Read(br);

                return levels;
            }
        }

        public static void WriteLevelFile(string path, Level[] levels)
        {
            using (FileStream fs = File.Create(path))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write("RDST".ToCharArray());
                bw.Write((ushort)levels.Length);
                foreach (Level level in levels)
                    level.Write(bw);
            }
        }

        public Bitmap Draw(Bitmap[] sprites)
        {
            Bitmap levelBitmap = new Bitmap(20 * 32, 14 * 32, PixelFormat.Format32bppArgb);

            using (Graphics graphics = Graphics.FromImage(levelBitmap))
            {
                for (int y = 0; y < 14; y++)
                    for (int x = 0; x < 20; x++)
                    {
                        byte tile = (byte)Tiles[x, y];
                        byte flags = (byte)(Tiles[x, y] >> 8);
                        bool player2SpawnPoint = (flags & 0x80) != 0;

                        if (tile == 77)
                            graphics.FillRectangle(new SolidBrush(Color.Black), x * 32, y * 32, 32, 32);
                        else
                        {
                            graphics.DrawImage(sprites[79], new Point(x * 32, y * 32));
                            graphics.DrawImage(sprites[tile], new Point(x * 32, y * 32));
                        }

                        if (player2SpawnPoint)
                            graphics.DrawImage(sprites[1], new Point(x * 32, y * 32));
                    }

                return levelBitmap;
            }
        }
    }
}
