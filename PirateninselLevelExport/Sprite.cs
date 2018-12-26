using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace PirateninselLevelExport
{
    class Sprite
    {
        internal static Bitmap Read(byte[] data, Color[] palette)
        {
            const byte fillColor = 0x00;

            List<byte> decoded = new List<byte>(32 * 32);

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == 0x66)
                {
                    if (data[i + 1] == 0xB8 && data[i + 4] == 0x66)
                    {
                        decoded.Add(data[i + 2]);
                        decoded.Add(data[i + 3]);
                    }
                    else if (data[i + 1] == 0xC7 && data[i + 2] == 0x07)
                    {
                        decoded.Add(data[i + 3]);
                        decoded.Add(data[i + 4]);
                    }
                    i += 4;
                }
                else if (data[i] == 0x81) // present when new row starts, but not always
                {
                    int filler2 = data[i + 3] / 5 * 2; // this formula appears to be correct    

                    for (int j = 0; j < filler2 * 32; j++)
                        decoded.Add(fillColor);

                    int filler;
                    if (data[i + 2] >= 0x60)
                        filler = data[i + 2] - 0x60;
                    else
                        filler = data[i + 2];
                    for (int j = 0; j < filler; j++)
                        decoded.Add(fillColor);

                    i += 5;
                }
                else if (data[i] == 0x83)
                {
                    for (int j = 0; j < data[i + 2]; j++)
                        decoded.Add(fillColor);
                    i += 2;
                }
                else if (data[i] == 0xB0) // one pixel follows
                {
                    decoded.Add(data[i + 1]);
                    i += 1;
                }
                else if (data[i] == 0xB8) // four pixels follow
                {
                    decoded.Add(data[i + 1]);
                    decoded.Add(data[i + 2]);
                    decoded.Add(data[i + 3]);
                    decoded.Add(data[i + 4]);
                    i += 4;
                }
                else if (data[i] == 0xAA || data[i] == 0xAB) // at the end of the four pixels after 0xB8
                {
                }
                else if (data[i] == 0xC3) // end of data
                {
                    if (i != data.Length - 1)
                        Console.WriteLine("Unexpected end of data marker");
                    while (decoded.Count < 32 * 32)
                        decoded.Add(fillColor);
                }
                else if (data[i] == 0xC6) // unknown
                {
                    decoded.Add(data[i + 2]); // ???
                    i += 2;
                }
                else if (data[i] == 0xC7) // one byte, then four pixels follow
                {
                    decoded.Add(data[i + 2]);
                    decoded.Add(data[i + 3]);
                    decoded.Add(data[i + 4]);
                    decoded.Add(data[i + 5]);
                    i += 5;
                }
                else
                {
                    Console.WriteLine("Unknown byte: {0:X2}", data[i]);
                }
            }

            if (decoded.Count < 32 * 32)
            {
                Console.WriteLine("Warning: decoded data is only {0} bytes long", decoded.Count);
                while (decoded.Count < 32 * 32)
                    decoded.Add(fillColor);
            }
            else if (decoded.Count > 32 * 32)
            {
                Console.WriteLine("Warning: decoded data is {0} bytes long", decoded.Count);
            }

            Bitmap bitmap = new Bitmap(32, 32);

            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 32; x++)
                {
                    byte a = decoded[y * 32 + x];
                    Color c = palette[a];
                    bitmap.SetPixel(x, y, c);
                }

            return bitmap;
        }

        private static Color[] ReadPaletteFile(string path)
        {
            Color[] palette = new Color[256];

            using (FileStream fs = File.OpenRead(path))
            using (BinaryReader br = new BinaryReader(fs))
            {
                for (int i = 0; i < 256; i++)
                {
                    byte r = br.ReadByte();
                    byte g = br.ReadByte();
                    byte b = br.ReadByte();

                    if (i == 0)
                        palette[i] = Color.Transparent;
                    else
                        palette[i] = Color.FromArgb(r * 4, g * 4, b * 4);
                }
            }

            return palette;
        }

        public static Bitmap[] ReadSpriteFile(string path, string palettePath)
        {
            Color[] palette = ReadPaletteFile(palettePath);

            using (FileStream fs = File.OpenRead(path))
            using (BinaryReader br = new BinaryReader(fs))
            {
                br.ReadChars(4);
                int numSprites = br.ReadInt32();
                int spritesDataLength = br.ReadInt32();

                int[] table = new int[numSprites];
                for (int i = 0; i < numSprites; i++)
                    table[i] = br.ReadInt32();

                byte[] payload = br.ReadBytes(spritesDataLength);

                Bitmap[] sprites = new Bitmap[numSprites];

                for (int i = 0; i < numSprites; i++)
                {
                    int index = table[i];
                    int length;

                    if (i < numSprites - 1)
                        length = table[i + 1] - index;
                    else
                        length = spritesDataLength - index;

                    byte[] data = payload.Skip(index).Take(length).ToArray();

                    sprites[i] = Sprite.Read(data, palette);
                }

                return sprites;
            }
        }
    }
}
