using CommandLine;
using System.IO;

namespace PirateninselLevelExport
{
    static class Program
    {
        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<ExportOptions, ImportOptions, GenerateLevelImagesOptions>(args)
                .WithParsed<ExportOptions>(Export)
                .WithParsed<ImportOptions>(Import)
                .WithParsed<GenerateLevelImagesOptions>(GenerateLevelImages);
        }

        private static void Export(ExportOptions options)
        {
            Directory.CreateDirectory(options.TiledDirectory);
            Exporter.GenerateTiledFiles(options.GameDirectory, options.TiledDirectory);            
        }

        private static void Import(ImportOptions options)
        {
            Exporter.GenerateLevelFiles(options.TiledDirectory, options.GameDirectory);
        }

        private static void GenerateLevelImages(GenerateLevelImagesOptions options)
        {
            Directory.CreateDirectory(options.DestinationDirectory);
            Exporter.GenerateLevelImages(options.GameDirectory, options.DestinationDirectory);
        }
    }

    [Verb("export", HelpText = "Generate Tiled level files from the Pirateninsel game files")]
    class ExportOptions 
    {
        [Option("game-dir", MetaValue = "PATH", Required = true, HelpText = "Path to game installation folder (e.g. C:\\PIRAT)")]
        public string GameDirectory { get; set; }

        [Option("tiled-dir", MetaValue = "PATH", Required = true, HelpText = "Path where to load or store Tiled level files")]
        public string TiledDirectory { get; set; }
    }

    [Verb("import", HelpText = "Generate Pirateninsel game files from Tiled level files")]
    class ImportOptions 
    {
        [Option("game-dir", MetaValue = "PATH", Required = true, HelpText = "Path to game installation folder (e.g. C:\\PIRAT)")]
        public string GameDirectory { get; set; }

        [Option("tiled-dir", MetaValue = "PATH", Required = true, HelpText = "Path where to load or store Tiled level files")]
        public string TiledDirectory { get; set; }
    }

    [Verb("gen-level-images", HelpText = "Generate an image of each level in the game")]
    class GenerateLevelImagesOptions
    {
        [Option("game-dir", MetaValue = "PATH", Required = true, HelpText = "Path to game installation folder (e.g. C:\\PIRAT)")]
        public string GameDirectory { get; set; }

        [Option("dest-dir", MetaValue = "PATH", Required = true, HelpText = "Path where to store the generated images")]
        public string DestinationDirectory { get; set; }
    }
}
