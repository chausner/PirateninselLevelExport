using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace PirateninselLevelExport
{
    static class Exporter
    {
        public static void GenerateTiledFiles(string gameRootPath, string destinationPath)
        {
            for (int world = 1; world <= 6; world++)
            {
                Directory.CreateDirectory(Path.Combine(destinationPath, $"world{world}"));

                GenerateTiledFiles(
                    Path.Combine(gameRootPath, $@"GDAT\W{world}.STO"),
                    Path.Combine(gameRootPath, $@"GDAT\W{world}ICON.SPR"),
                    Path.Combine(gameRootPath, $@"GDAT\W{world}.COL"),
                    Path.Combine(gameRootPath, $@"GDAT\BOY.SPR"),
                    Path.Combine(destinationPath, $"world{world}"));
            }
        }

        private static void GenerateTiledFiles(string worldFile, string worldSpritesFile, string worldPaletteFile, string playerSpritesFile, string destinationFolder)
        {
            Level[] levels = Level.ReadLevelFile(worldFile);
            Bitmap[] sprites = Sprite.ReadSpriteFile(worldSpritesFile, worldPaletteFile);
            Bitmap playerSprite = Sprite.ReadSpriteFile(playerSpritesFile, worldPaletteFile)[105];

            // generate tileset image
            Bitmap tileSetBitmap = GenerateTileSetBitmap(worldSpritesFile, worldPaletteFile);
            using (Graphics graphics = Graphics.FromImage(tileSetBitmap))
            {
                // replace placeholder with player sprite
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.FillRectangle(Brushes.Transparent, 1 * 32, 0 * 32, 32, 32);
                graphics.CompositingMode = CompositingMode.SourceOver;
                graphics.DrawImage(playerSprite, new Point(1 * 32, 0 * 32));
                // replace white placeholder with a black one
                graphics.FillRectangle(Brushes.Black, 17 * 32, 3 * 32, 32, 32);
            }
            string tileSetImagePath = "tileset.png";
            tileSetBitmap.Save(Path.Combine(destinationFolder, tileSetImagePath));

            // generate tileset TSX file
            XDocument tsxDocument = new XDocument(
                new XElement("tileset",
                    new XAttribute("name", "W1"),
                    new XAttribute("tilewidth", 32),
                    new XAttribute("tileheight", 32),
                    new XAttribute("tilecount", 260),
                    new XAttribute("columns", 20),
                    new XElement("image",
                        new XAttribute("source", tileSetImagePath),
                        new XAttribute("width", 640),
                        new XAttribute("height", 416))));
            string tileSetTsxPath = "tileset.tsx";
            tsxDocument.Save(Path.Combine(destinationFolder, tileSetTsxPath));

            // generate level TMX files
            XElement backgroundData = new XElement("data",
                Enumerable.Repeat(new XElement("tile", new XAttribute("gid", 80)), 20 * 14));

            for (int l = 0; l < levels.Length; l++)
            {
                XElement foregroundData = new XElement("data");
                for (int y = 0; y < 14; y++)
                    for (int x = 0; x < 20; x++)
                    {
                        ushort tile = levels[l].Tiles[x, y];
                        int gid = (tile & 0xFF) + 1;
                        foregroundData.Add(new XElement("tile", new XAttribute("gid", gid)));
                    }

                XDocument tmxDocument = new XDocument(
                    new XElement("map",
                        new XAttribute("version", "1.0"),
                        new XAttribute("tiledversion", "2018.01.01"),
                        new XAttribute("orientation", "orthogonal"),
                        new XAttribute("renderorder", "right-down"),
                        new XAttribute("width", 20),
                        new XAttribute("height", 14),
                        new XAttribute("tilewidth", 32),
                        new XAttribute("tileheight", 32),
                        new XAttribute("infinite", 0),
                        new XAttribute("nextobjectid", 1),
                        new XElement("tileset",
                            new XAttribute("firstgid", 1),
                            new XAttribute("source", "tileset.tsx")),
                         new XElement("layer",
                            new XAttribute("name", "Hintergrund"),
                            new XAttribute("width", 20),
                            new XAttribute("height", 14),
                            backgroundData),
                        new XElement("layer",
                            new XAttribute("name", "Vordergrund"),
                            new XAttribute("width", 20),
                            new XAttribute("height", 14),
                            foregroundData)));

                string tmxFilePath = $"level{l + 1}.tmx";
                tmxDocument.Save(Path.Combine(destinationFolder, tmxFilePath));
            }
        }

        public static void GenerateLevelImages(string gameRootPath, string destinationPath)
        {
            for (int world = 1; world <= 6; world++)
            {
                string stoPath = Path.Combine(gameRootPath, $@"GDAT\W{world}.STO");
                string worldSpritesFile = Path.Combine(gameRootPath, $@"GDAT\W{world}ICON.SPR");
                string worldPaletteFile = Path.Combine(gameRootPath, $@"GDAT\W{world}.COL");

                Level[] levels = Level.ReadLevelFile(stoPath);
                Bitmap[] sprites = Sprite.ReadSpriteFile(worldSpritesFile, worldPaletteFile);

                for (int i = 0; i < levels.Length; i++)
                    levels[i].Draw(sprites).Save(Path.Combine(destinationPath, $"W{world}_L{i + 1}.png"));
            }
        }

        public static void GenerateLevelFiles(string tiledFilesRootPath, string gameRootPath)
        {
            int[] levelsPerWorld = { 40, 20, 20, 20, 20, 20 };

            for (int world = 1; world <= 6; world++)
            {
                string stoPath = Path.Combine(gameRootPath, $@"GDAT\W{world}.STO");
                Level[] levels = Level.ReadLevelFile(stoPath);

                for (int level = 1; level <= levelsPerWorld[world - 1]; level++)
                {
                    string tmxPath = Path.Combine(tiledFilesRootPath, $@"world{world}\level{level}.tmx");

                    XDocument stoDocument = XDocument.Load(tmxPath);

                    int[] tiles =
                        stoDocument
                        .Element("map")
                        .Elements("layer")
                        .Single(layer => layer.Attribute("name").Value == "Vordergrund")
                        .Element("data")
                        .Elements("tile")
                        .Select(tile => Convert.ToInt32(tile.Attribute("gid").Value))
                        .ToArray();

                    for (int y = 0; y < 14; y++)
                        for (int x = 0; x < 20; x++)
                            levels[level - 1].Tiles[x, y] = (ushort)(tiles[y * 20 + x] - 1);
                }

                if (!File.Exists(stoPath + ".BAK"))
                    File.Copy(stoPath, stoPath + ".BAK");

                Level.WriteLevelFile(stoPath, levels);
            }
        }

        public static Bitmap GenerateTileSetBitmap(string spritePath, string palettePath)
        {
            Bitmap[] sprites = Sprite.ReadSpriteFile(spritePath, palettePath);

            int columns = 20;
            int rows = (sprites.Length + columns - 1) / columns;

            Bitmap tileSetBitmap = new Bitmap(columns * 32, rows * 32, PixelFormat.Format32bppArgb);

            using (Graphics graphics = Graphics.FromImage(tileSetBitmap))
            {
                for (int i = 0; i < sprites.Length; i++)
                {
                    int x = i % columns;
                    int y = i / columns;
                    graphics.DrawImage(sprites[i], new Point(x * 32, y * 32));
                }

                return tileSetBitmap;
            }
        }
    }
}
