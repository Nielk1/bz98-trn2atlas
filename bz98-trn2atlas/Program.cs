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
        static string WorkingPath;

        static void Main(string[] args)
        {
            
            if (args.Length == 0)
            {
                WriteLogLine("bz98-trn2atlas.exe trn [output]");
                return;
            }
            if (args.Length > 1)
            {
                Process(args[0], args[1]);
            }
            else
            {
                Process(args[0], null);
            }

            //Process(@"D:\Data\Programming\Projects\bz98-trn2atlas\bz98-trn2atlas\bin\Release\y\misn01.trn");


        }
        static void Process(string Filename, string output)
        {
            WorkingPath = Path.GetDirectoryName(Filename);
            if (Path.GetDirectoryName(Filename).Trim().Length == 0)
            {
                WorkingPath = Directory.GetCurrentDirectory();
            }
            Filename = Path.GetFileName(Filename);

            try
            {
                if (!File.Exists(Path.Combine(WorkingPath, Filename)))
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
            List<Image> NormalTextures = new List<Image>();
            List<Image> EmissiveTextures = new List<Image>();
            List<Image> SpecularTextures = new List<Image>();

            WriteLogLine("Working Path: \"" + WorkingPath + "\"");
            WriteLogLine("TRN file: \"" + Filename + "\"");

            WriteLogLine("--------------------------------");

            WriteLogLine("Beginning TRN Scrape");

            IniFile ini = new IniFile(Path.Combine(WorkingPath, Filename));
            WriteLogLine("[Color]");
            PaletteFilename = ini.GetValue("Color", "Palette");
            WriteLogLine("       Palette: {0}", PaletteFilename);
            Luma = ini.GetValue("Color", "Luma");
            WriteLogLine("          Luma: {0}", Luma);
            Translucency = ini.GetValue("Color", "Translucency");
            WriteLogLine("  Translucency: {0}", Translucency);
            Alpha = ini.GetValue("Color", "Alpha");
            WriteLogLine("         Alpha: {0}", Alpha);

            for(int headerIdx = 0; headerIdx < 8; headerIdx++)
            {
                string headerSection = string.Format("TextureType{0}", headerIdx);
                WriteLogLine("[{0}]", headerSection);

                string x1 = ProcessTrnItem(ini, headerSection, "SolidA0");
                string x2 = ProcessTrnItem(ini, headerSection, "SolidB0");
                string x3 = ProcessTrnItem(ini, headerSection, "SolidC0");
                string x4 = ProcessTrnItem(ini, headerSection, "SolidD0");

                if (x1 != null) TextureNames.Add(Path.GetFileNameWithoutExtension(x1));
                if (x2 != null) TextureNames.Add(Path.GetFileNameWithoutExtension(x2));
                if (x3 != null) TextureNames.Add(Path.GetFileNameWithoutExtension(x3));
                if (x4 != null) TextureNames.Add(Path.GetFileNameWithoutExtension(x4));

                for (char subSectionLet = 'A'; subSectionLet <= 'D'; subSectionLet++)
                    for (int subSectionIdx = 0; subSectionIdx < 8; subSectionIdx++)
                    {
                        string n1 = string.Format("CapTo{0}_{1}0", subSectionIdx, subSectionLet);
                        string m1 = ProcessTrnItem(ini, headerSection, n1);
                        if (m1 != null) TextureNames.Add(Path.GetFileNameWithoutExtension(m1));
                    }

                for (char subSectionLet = 'A'; subSectionLet <= 'D'; subSectionLet++)
                    for (int subSectionIdx = 0; subSectionIdx < 8; subSectionIdx++)
                    {
                        string n1 = string.Format("DiagonalTo{0}_{1}0", subSectionIdx, subSectionLet);
                        string m1 = ProcessTrnItem(ini, headerSection, n1);
                        if (m1 != null) TextureNames.Add(Path.GetFileNameWithoutExtension(m1));
                    }
            }

            //using (FileStream stream = File.OpenRead(Path.Combine(WorkingPath, Filename)))
            //{
            //    using (StreamReader reader = new StreamReader(stream))
            //    {
            //        string line;
            //        while ((line = reader.ReadLine()) != null)
            //        {
            //            if (Regex.IsMatch(line, @"^Palette[ ]*=", RegexOptions.IgnoreCase))
            //            {
            //                PaletteFilename = line.Split('=')[1].Trim();
            //                WriteLogLine("       Palette: {0}", PaletteFilename);
            //            }
            //            if (Regex.IsMatch(line, @"^Luma[ ]*=", RegexOptions.IgnoreCase))
            //            {
            //                Luma = line.Split('=')[1].Trim();
            //                WriteLogLine("          Luma: {0}", Luma);
            //            }
            //            if (Regex.IsMatch(line, @"^Translucency[ ]*=", RegexOptions.IgnoreCase))
            //            {
            //                Translucency = line.Split('=')[1].Trim();
            //                WriteLogLine("  Translucency: {0}", Translucency);
            //            }
            //            if (Regex.IsMatch(line, @"^Alpha[ ]*=", RegexOptions.IgnoreCase))
            //            {
            //                Alpha = line.Split('=')[1].Trim();
            //                WriteLogLine("         Alpha: {0}", Alpha);
            //            }

            //            if (Regex.IsMatch(line, @"^CapTo[0-7]_[A-D]0[ ]*=", RegexOptions.IgnoreCase))
            //            {
            //                string name = line.Split('=')[0].Trim();
            //                string value = Path.GetFileNameWithoutExtension(line.Split('=')[1].Trim());
            //                TextureNames.Add(value);
            //                WriteLogLine("{0,14}: {1}", name, value);
            //            }
            //            if (Regex.IsMatch(line, @"^DiagonalTo[0-7]_[A-D]0[ ]*=", RegexOptions.IgnoreCase))
            //            {
            //                string name = line.Split('=')[0].Trim();
            //                string value = Path.GetFileNameWithoutExtension(line.Split('=')[1].Trim());
            //                TextureNames.Add(value);
            //                WriteLogLine("{0,14}: {1}", name, value);
            //            }
            //            if (Regex.IsMatch(line, @"^Solid[A-D]0[ ]*=", RegexOptions.IgnoreCase))
            //            {
            //                string name = line.Split('=')[0].Trim();
            //                string value = Path.GetFileNameWithoutExtension(line.Split('=')[1].Trim());
            //                TextureNames.Add(value);
            //                WriteLogLine("{0,14}: {1}", name, value);
            //            }
            //        }
            //    }
            //}
            WriteLogLine("TRN Scrape Complete");

            WriteLogLine("--------------------------------");

            WriteLogLine("Checking Existing Atlases");
            bool ExistingAtlas = false;
            //string TRNBase = Path.Combine(Directory.GetCurrentDirectory(), "trn", Path.ChangeExtension(PaletteFilename, "trn"));
            string KnownTrnPath = Path.Combine(System.IO.Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath), "known");
            WriteLogLine("Checking path \"{0}\"", KnownTrnPath);
            if (Directory.Exists(KnownTrnPath))
            {
                string[] TRNBases = Directory.GetFiles(KnownTrnPath, "*.trn");

                foreach (string TRNBase in TRNBases)
                {
                    WriteLogLine("Checking file \"{0}\"", TRNBase);

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
                int loadedDiffuse = 0;
                int loadedNormal = 0;
                int loadedEmissive = 0;
                int loadedSpecular = 0;
                TextureNames.ForEach(dr =>
                {
                    Textures.Add(GetImage(dr, ref loadedDiffuse, Palette));
                    NormalTextures.Add(GetImage(dr + @"_n", ref loadedNormal));
                    EmissiveTextures.Add(GetImage(dr + @"_e", ref loadedEmissive));
                    SpecularTextures.Add(GetImage(dr + @"_s", ref loadedSpecular));
                });
                WriteLogLine("Textures Found");
                WriteLogLine("Diffuse: {0}", loadedDiffuse);
                WriteLogLine("Normal: {0}", loadedNormal);
                WriteLogLine("Emissive: {0}", loadedEmissive);
                WriteLogLine("Specular: {0}", loadedSpecular);
                WriteLogLine("================================");

                if (loadedDiffuse > 0)
                {
                    StitchTextures(Filename, "Diffuse", "D", output, Textures, null);
                }
                else
                {
                    WriteLogLine("Skipping Diffuse");
                }
                WriteLogLine("================================");
                if (loadedNormal > 0)
                {
                    StitchTextures(Filename, "Normal", "N", output, NormalTextures, Color.FromArgb(126, 127, 255));
                }
                else
                {
                    WriteLogLine("Skipping Normal");
                }
                WriteLogLine("================================");
                if (loadedEmissive > 0)
                {
                    StitchTextures(Filename, "Emissive", "E", output, EmissiveTextures, Color.FromArgb(0, 0, 0));
                }
                else
                {
                    WriteLogLine("Skipping Emissive");
                }
                WriteLogLine("================================");
                if (loadedSpecular > 0)
                {
                    StitchTextures(Filename, "Specular", "S", output, SpecularTextures, Color.FromArgb(128, 128, 128));
                }
                else
                {
                    WriteLogLine("Skipping Specular");
                }
            }

            File.WriteAllText(Path.Combine(WorkingPath, Path.ChangeExtension(Filename, ".log")), bld.ToString());
            bld.Clear();
        }

        private static void StitchTextures(string Filename, string typeForLog, string appendName, string output, List<Image> Textures, Color? fillColor)
        {
            int size = Math.Max(Textures.Max(dr => dr.Width), Textures.Max(dr => dr.Height));
            WriteLogLine("Max {0} Size: {1}", typeForLog, size);

            Bitmap atlas = new Bitmap(size * 8, size * 8);
            Graphics g = Graphics.FromImage(atlas);
            int index = 1;

            if (!fillColor.HasValue)
            {
                IEnumerable<Color> avgColors = Textures.Select(dr => getDominantColor((Bitmap)dr));
                //fillColor = Color.FromArgb((int)avgColors.Average(dr => dr.R), (int)avgColors.Average(dr => dr.G), (int)avgColors.Average(dr => dr.B));
                fillColor = avgColors.First();

                WriteLogLine("Dominant Color: {0} {1} {2}", fillColor.Value.R, fillColor.Value.G, fillColor.Value.B);
            }
            else
            {
                WriteLogLine("Fill Color: {0} {1} {2}", fillColor.Value.R, fillColor.Value.G, fillColor.Value.B);
            }

            WriteLogLine("--------------------------------");

            WriteLogLine("Rendering {0} Atlas", typeForLog);
            g.Clear(fillColor.Value);


            Image defaultTile = Textures.First();
            if (defaultTile != null)
            {
                g.DrawImage(defaultTile, 0, 0, size, size);
            }

            Textures.ForEach(dr =>

            {
                int y = index / 8;
                int x = index % 8;

                g.DrawImage(dr, x * size, y * size, size, size);

                index++;
            });

            string outputPathVal = Path.Combine(WorkingPath, Path.ChangeExtension(Path.GetFileNameWithoutExtension(Filename) + "-atlas_" + appendName, ".png"));
            if (output != null)
            {
                if (Path.IsPathRooted(output))
                {
                    outputPathVal = Path.ChangeExtension(Path.GetFileNameWithoutExtension(output) + @"_" + appendName, ".png");
                }
                else
                {
                    outputPathVal = Path.Combine(WorkingPath, Path.ChangeExtension(Path.GetFileNameWithoutExtension(output) + "_" + appendName, ".png"));
                }
            }
            atlas.Save(outputPathVal, ImageFormat.Png);
            atlas.Dispose();
        }

        private static Image GetImage(string dr, ref int FoundImage, Color[] Palette = null)
        {
            Image tmpImage;
            string imagePath = Directory.GetFiles(WorkingPath, dr + @".*", SearchOption.TopDirectoryOnly).Where(s => s.ToLowerInvariant().EndsWith(".png") || s.ToLowerInvariant().EndsWith(".bmp")).FirstOrDefault();
            if (imagePath != null)
            {
                WriteLogLine("{0}:\t{1}\t\"{2}\"", dr, Path.GetExtension(imagePath) == ".png" ? "PNG" : "BMP", imagePath);
                tmpImage = Image.FromFile(imagePath);
                FoundImage++;
            }
            else
            {
                imagePath = Directory.GetFiles(WorkingPath, dr + @".*", SearchOption.AllDirectories).Where(s => s.ToLowerInvariant().EndsWith(".png") || s.ToLowerInvariant().EndsWith(".bmp")).FirstOrDefault();
                if (imagePath != null)
                {
                    WriteLogLine("{0}:\t{1}\t\"{2}\"", dr, Path.GetExtension(imagePath) == ".png" ? "PNG" : "BMP", imagePath);
                    tmpImage = Image.FromFile(imagePath);
                    FoundImage++;
                }
                else
                {
                    imagePath = Directory.GetFiles(WorkingPath, dr + @".map", SearchOption.TopDirectoryOnly).FirstOrDefault();

                    if (imagePath != null)
                    {
                        MapFile tmp = MapFile.FromFile(imagePath);
                        WriteLogLine("{0}:\t{1}\t\"{2}\"", dr, tmp.IsPalletized ? "PMAP" : "MAP", imagePath);
                        tmpImage = tmp.IsPalletized ? tmp.GetBitmap(Palette) : tmp.GetBitmap();
                        FoundImage++;
                    }
                    else
                    {
                        imagePath = Directory.GetFiles(WorkingPath, dr + @".map", SearchOption.AllDirectories).FirstOrDefault();

                        if (imagePath != null)
                        {
                            MapFile tmp = MapFile.FromFile(imagePath);
                            WriteLogLine("{0}:\t{1}\t\"{2}\"", dr, tmp.IsPalletized ? "PMAP" : "MAP", imagePath);
                            tmpImage = tmp.IsPalletized ? tmp.GetBitmap(Palette) : tmp.GetBitmap();
                            FoundImage++;
                        }
                        else
                        {
                            WriteLogLine("{0}:\tXXX\t[NOT_FOUND]", dr);
                            tmpImage = new Bitmap(256, 256);
                        }
                    }
                }
            }

            return tmpImage;
        }

        private static string ProcessTrnItem(IniFile ini, string headerSection, string keyname)
        {
            string val = ini.GetValue(headerSection, keyname);
            if(val != null && val.Length > 0)
            {
                WriteLogLine("{0,14}: {1}", keyname, val);
                return val;
            }
            return null;
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
