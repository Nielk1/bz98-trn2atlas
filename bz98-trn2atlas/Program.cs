using BZ1GeoEditor;
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
        static void Main(string[] args)
        {
            
            if(args.Length == 0)
            {
                Console.WriteLine("No TRN specified.");
                return;
            }
            string Filename = args[0];
            
            //string Filename = @"D:\Data\Programming\Projects\bz98-trn2atlas\bz98-trn2atlas\bin\Debug\alstforc.TRN";

            string PaletteFilename = null;
            string Luma = null;
            string Translucency = null;
            string Alpha = null;
            List<string> TextureNames = new List<string>();
            List<Image> Textures = new List<Image>();

            using (FileStream stream = File.OpenRead(Filename))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (Regex.IsMatch(line, @"^Palette[ ]*=")) { PaletteFilename = line.Split('=')[1].Trim(); }
                        if (Regex.IsMatch(line, @"^Luma[ ]*=")) { Luma = line.Split('=')[1].Trim(); }
                        if (Regex.IsMatch(line, @"^Translucency[ ]*=")) { Translucency = line.Split('=')[1].Trim(); }
                        if (Regex.IsMatch(line, @"^Alpha[ ]*=")) { Alpha = line.Split('=')[1].Trim(); }

                        if (Regex.IsMatch(line, @"^CapTo[0-7]_[A-D]0[ ]*=")) { TextureNames.Add(Path.GetFileNameWithoutExtension(line.Split('=')[1].Trim())); }
                        if (Regex.IsMatch(line, @"^DiagonalTo[0-7]_[A-D]0[ ]*=")) { TextureNames.Add(Path.GetFileNameWithoutExtension(line.Split('=')[1].Trim())); }
                        if (Regex.IsMatch(line, @"^Solid[A-D]0[ ]*=")) { TextureNames.Add(Path.GetFileNameWithoutExtension(line.Split('=')[1].Trim())); }
                    }
                }
            }

            string searchPath = Path.GetDirectoryName(Filename);
            PaletteFilename = Directory.GetFiles(searchPath, PaletteFilename, SearchOption.AllDirectories).FirstOrDefault();

            Color[] Palette = OpenAct(PaletteFilename);

            TextureNames.ForEach(dr =>
            {
                Image tmpImage;
                string imagePath = Directory.GetFiles(searchPath, dr + @"*.png;*.bmp", SearchOption.AllDirectories).FirstOrDefault();
                if (imagePath != null)
                {
                    tmpImage = Image.FromFile(imagePath);
                }
                else
                {
                    imagePath = Directory.GetFiles(searchPath, dr + @"*.map", SearchOption.AllDirectories).FirstOrDefault();

                    if (imagePath != null)
                    {
                        MapFile tmp = MapFile.FromFile(imagePath);
                        tmpImage = tmp.GetBitmap(Palette);
                    }
                    else
                    {
                        tmpImage = new Bitmap(256, 256);
                    }
                }

                Textures.Add(tmpImage);
            });

            int size = Math.Max(Textures.Max(dr => dr.Width), Textures.Max(dr => dr.Height));

            Bitmap atlas = new Bitmap(size * 8, size * 8);
            Graphics g = Graphics.FromImage(atlas);
            int index = 0;
            Textures.ForEach(dr =>
            {
                int y = index / 8;
                int x = index % 8;

                g.DrawImage(dr, x * size, y * size, size, size);

                index++;
            });

            atlas.Save(searchPath + Path.DirectorySeparatorChar + @"atlas.png", ImageFormat.Png);
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
    }
}
