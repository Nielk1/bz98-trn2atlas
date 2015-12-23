using BZ1GeoEditor;
using StumDE.Misc;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace bz98_trn2atlas
{
    class Program
    {
        static StringBuilder bld = new StringBuilder();

        static void Main(string[] args)
        {
            
            if (args.Length == 0)
            {
                WriteLogLine("No TRN specified");
                return;
            }
            string Filename = args[0];
            
            //string Filename = @"D:\Data\Programming\Projects\bz98-trn2atlas\bz98-trn2atlas\bin\Debug\alstforc.TRN";

            string WorkingPath = Path.GetDirectoryName(Filename);
            if (Path.GetDirectoryName(Filename).Trim().Length == 0)
            {
                WorkingPath = Directory.GetCurrentDirectory();
            }
            Filename = Path.GetFileName(Filename);

            try
            {
                if (!File.Exists(Filename))
                {
                    WriteLogLine("Could not find file \"" + Filename + "\"");
                    return;
                }
            }
            catch (Exception e)
            {
                WriteLogLine("Error occured checking for file");
                WriteLogLine(e.ToString());
                return;
            }

            string PaletteFilename = null;
            string Luma = null;
            string Translucency = null;
            string Alpha = null;
            List<string> TextureNames = new List<string>();
            List<Image> Textures = new List<Image>();

            WriteLogLine("Working Path: \"" + WorkingPath + "\"");
            WriteLogLine("TRN file: \"" + Filename + "\"");

            WriteLogLine("--------------------------------");

            WriteLogLine("Beginning TRN Scrape");
            using (FileStream stream = File.OpenRead(Path.Combine(WorkingPath, Filename)))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (Regex.IsMatch(line, @"^Palette[ ]*="))
                        {
                            PaletteFilename = line.Split('=')[1].Trim();
                            WriteLogLine("       Palette: {0}", PaletteFilename);
                        }
                        if (Regex.IsMatch(line, @"^Luma[ ]*="))
                        {
                            Luma = line.Split('=')[1].Trim();
                            WriteLogLine("          Luma: {0}", Luma);
                        }
                        if (Regex.IsMatch(line, @"^Translucency[ ]*="))
                        {
                            Translucency = line.Split('=')[1].Trim();
                            WriteLogLine("  Translucency: {0}", Translucency);
                        }
                        if (Regex.IsMatch(line, @"^Alpha[ ]*="))
                        {
                            Alpha = line.Split('=')[1].Trim();
                            WriteLogLine("         Alpha: {0}", Alpha);
                        }

                        if (Regex.IsMatch(line, @"^CapTo[0-7]_[A-D]0[ ]*="))
                        {
                            string name = line.Split('=')[0].Trim();
                            string value = Path.GetFileNameWithoutExtension(line.Split('=')[1].Trim());
                            TextureNames.Add(value);
                            WriteLogLine("{0,14}: {1}", name, value);
                        }
                        if (Regex.IsMatch(line, @"^DiagonalTo[0-7]_[A-D]0[ ]*="))
                        {
                            string name = line.Split('=')[0].Trim();
                            string value = Path.GetFileNameWithoutExtension(line.Split('=')[1].Trim());
                            TextureNames.Add(value);
                            WriteLogLine("{0,14}: {1}", name, value);
                        }
                        if (Regex.IsMatch(line, @"^Solid[A-D]0[ ]*="))
                        {
                            string name = line.Split('=')[0].Trim();
                            string value = Path.GetFileNameWithoutExtension(line.Split('=')[1].Trim());
                            TextureNames.Add(value);
                            WriteLogLine("{0,14}: {1}", name, value);
                        }
                    }
                }
            }
            WriteLogLine("TRN Scrape Complete");

            WriteLogLine("--------------------------------");

            WriteLogLine("Checking Existing Atlases");
            bool ExistingAtlas = false;
            //string TRNBase = Path.Combine(Directory.GetCurrentDirectory(), "trn", Path.ChangeExtension(PaletteFilename, "trn"));
            string KnownTrnPath = Path.Combine(System.IO.Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath), "known");
            if (Directory.Exists(KnownTrnPath))
            {
                string[] TRNBases = Directory.GetFiles(KnownTrnPath, "*.trn");

                IniFile ini = new IniFile(Path.Combine(WorkingPath, Filename));

                foreach (string TRNBase in TRNBases)
                {
                    Dictionary<string, string> ExistingTextureNames = new Dictionary<string, string>();

                    IniFile templateTRN = new IniFile(TRNBase);

                    ExistingAtlas = ini.GetSection("Color").ContentEquals(templateTRN.GetSection("Color")) &&
                                    ini.GetSection("TextureType0").ContentEquals(templateTRN.GetSection("TextureType0")) &&
                                    ini.GetSection("TextureType1").ContentEquals(templateTRN.GetSection("TextureType1")) &&
                                    ini.GetSection("TextureType2").ContentEquals(templateTRN.GetSection("TextureType2")) &&
                                    ini.GetSection("TextureType3").ContentEquals(templateTRN.GetSection("TextureType3")) &&
                                    ini.GetSection("TextureType4").ContentEquals(templateTRN.GetSection("TextureType4")) &&
                                    ini.GetSection("TextureType5").ContentEquals(templateTRN.GetSection("TextureType5")) &&
                                    ini.GetSection("TextureType6").ContentEquals(templateTRN.GetSection("TextureType6")) &&
                                    ini.GetSection("TextureType7").ContentEquals(templateTRN.GetSection("TextureType7"));

                    if (ExistingAtlas)
                    {
                        WriteLogLine("Found Existing Atlas \"{0}\"", templateTRN.GetValue("Atlases", "MaterialName").Trim());
                        break;
                    }
                }
            }
            if (!ExistingAtlas)
            {
                WriteLogLine("Existing Atlas Not Found");
            }

            WriteLogLine("--------------------------------");

            if (ExistingAtlas)
            {

            }
            else
            {
                string PaletteFullpath = null;
                WriteLogLine("Finding Palette");
                PaletteFullpath = Directory.GetFiles(WorkingPath, PaletteFilename, SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (PaletteFullpath == null)
                {
                    PaletteFullpath = Directory.GetFiles(WorkingPath, PaletteFilename, SearchOption.AllDirectories).FirstOrDefault();
                }
                if (PaletteFullpath != null)
                {
                    WriteLogLine("Found Pallet \"{0}\"", PaletteFullpath);
                }
                else
                {
                    WriteLogLine("Pallet not found, using default");
                }

                Color[] Palette = OpenAct(PaletteFullpath);

                WriteLogLine("--------------------------------");

                WriteLogLine("Finding Textures");
                TextureNames.ForEach(dr =>
                {
                    Image tmpImage;
                    string imagePath = Directory.GetFiles(WorkingPath, dr + @"*.png;*.bmp", SearchOption.TopDirectoryOnly).FirstOrDefault();
                    if (imagePath != null)
                    {
                        WriteLogLine("{0}:\t{1}\t\"{2}\"", dr, Path.GetExtension(imagePath) == ".png" ? "PNG" : "BMP", imagePath);
                        tmpImage = Image.FromFile(imagePath);
                    }
                    else
                    {
                        imagePath = Directory.GetFiles(WorkingPath, dr + @"*.png;*.bmp", SearchOption.AllDirectories).FirstOrDefault();
                        if (imagePath != null)
                        {
                            WriteLogLine("{0}:\t{1}\t\"{2}\"", dr, Path.GetExtension(imagePath) == ".png" ? "PNG" : "BMP", imagePath);
                            tmpImage = Image.FromFile(imagePath);
                        }
                        else
                        {
                            imagePath = Directory.GetFiles(WorkingPath, dr + @"*.map", SearchOption.TopDirectoryOnly).FirstOrDefault();

                            if (imagePath != null)
                            {
                                MapFile tmp = MapFile.FromFile(imagePath);
                                WriteLogLine("{0}:\t{1}\t\"{2}\"", dr, tmp.IsPalletized ? "PMAP" : "MAP", imagePath);
                                tmpImage = tmp.GetBitmap(Palette);
                            }
                            else
                            {
                                imagePath = Directory.GetFiles(WorkingPath, dr + @"*.map", SearchOption.AllDirectories).FirstOrDefault();

                                if (imagePath != null)
                                {
                                    MapFile tmp = MapFile.FromFile(imagePath);
                                    WriteLogLine("{0}:\t{1}\t\"{2}\"", dr, tmp.IsPalletized ? "PMAP" : "MAP", imagePath);
                                    tmpImage = tmp.GetBitmap(Palette);
                                }
                                else
                                {
                                    WriteLogLine("{0}:\tXXX\t[NOT_FOUND]", dr);
                                    tmpImage = new Bitmap(256, 256);
                                }
                            }
                        }
                    }

                    Textures.Add(tmpImage);
                });
                WriteLogLine("Textures Found");

                WriteLogLine("--------------------------------");

                int size = Math.Max(Textures.Max(dr => dr.Width), Textures.Max(dr => dr.Height));
                WriteLogLine("Max Size: {0}", size);

                Bitmap atlas = new Bitmap(size * 8, size * 8);
                Graphics g = Graphics.FromImage(atlas);
                int index = 0;

                IEnumerable<Color> avgColors = Textures.Select(dr => getDominantColor((Bitmap)dr));
                Color col = Color.FromArgb((int)avgColors.Average(dr => dr.R), (int)avgColors.Average(dr => dr.G), (int)avgColors.Average(dr => dr.B));

                WriteLogLine("Average Color: {0} {1} {2}", col.R, col.G, col.B);

                WriteLogLine("--------------------------------");

                WriteLogLine("Rendering Atlas");
                g.Clear(col);
                Textures.ForEach(dr =>
                {
                    int y = index / 8;
                    int x = index % 8;

                    g.DrawImage(dr, x * size, y * size, size, size);

                    index++;
                });

                atlas.Save(Path.Combine(WorkingPath, Path.ChangeExtension(Path.GetFileNameWithoutExtension(Filename) + "-atlas", ".png")), ImageFormat.Png);
            }

            File.WriteAllText(Path.Combine(WorkingPath, Path.ChangeExtension(Filename, ".log")), bld.ToString());
        }

        private static void WriteLogLine(string format, params object[] arg)
        {
            Console.WriteLine(format, arg);
            bld.AppendLine(string.Format(format, arg));
        }

        public static Color getDominantColor(Bitmap bmp)
        {

            //Used for tally
            int r = 0;
            int g = 0;
            int b = 0;

            int total = 0;

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color clr = bmp.GetPixel(x, y);

                    r += clr.R;
                    g += clr.G;
                    b += clr.B;

                    total++;
                }
            }

            //Calculate average
            r /= total;
            g /= total;
            b /= total;

            return Color.FromArgb(r, g, b);
        }

        static Color[] OpenAct(string filePath)
        {
            if (filePath != null && File.Exists(filePath))
            {
                return MapFile.PalletFromFile(filePath);
            }

            Color[] colorArray = new Color[256];
            for (int x = 0; x < 256; x++)
            {
                colorArray[x] = Color.FromArgb(x, x, x);
            }
            return colorArray;
        }

        static void Pause()
        {
            WriteLogLine("Press any key to continue.");
            Console.ReadKey(true);
        }
    }

    public static class DictionaryExtensionMethods
    {
        public static bool ContentEquals<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Dictionary<TKey, TValue> otherDictionary)
        {
            return (otherDictionary ?? new Dictionary<TKey, TValue>())
                .OrderBy(kvp => kvp.Key)
                .SequenceEqual((dictionary ?? new Dictionary<TKey, TValue>())
                                   .OrderBy(kvp => kvp.Key));
        }
    }
}
