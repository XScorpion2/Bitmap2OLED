using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Bitmap2Hex
{
    class Program
    {
        static List<byte> ConvertBitmap(string file, out int width, out int height)
        {
            var result = new List<byte>();
            var bitmap = new Bitmap(file);
            width = bitmap.Width;
            height = bitmap.Height;
            for (int y = 0; y < height; y += 8)
            {
                for (int x = 0; x < width; x++)
                {
                    byte col = 0;
                    for (int p = 0; p < 8; p++)
                    {
                        var c = bitmap.GetPixel(x, y + p);
                        var b = c.GetBrightness();
                        var r = (int)Math.Round(b);
                        var s = r << p;
                        col = (byte)(col | s);
                    }
                    result.Add(col);
                }
            }
            return result;
        }

        static void PrintResult(List<byte> result, int width, int height)
        {
            for (int y = 0; y < height / 8; y++)
            {
                for (int p = 0; p < 8; p += 2)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int i = x + y * width;
                        var b = result[i] >> p & 1;
                        var b2 = result[i] >> (p + 1) & 1;

                        if (b == 0 && b2 == 0)
                            Console.Write(" ");
                        else if (b == 1 && b2 == 0)
                            Console.Write("▀");
                        else if (b == 0 && b2 == 1)
                            Console.Write("▄");
                        else if (b == 1 && b2 == 1)
                            Console.Write("█");
                    }
                    Console.Write("\n");
                }
            }
        }

        static void WriteResult(List<byte> result, string fileName)
        {
            var codeName = Regex.Replace(Path.GetFileNameWithoutExtension(fileName), @"[^_a-zA-Z]+", string.Empty);
            StringBuilder output = new StringBuilder();
            output.Append($"static const char PROGMEM {codeName}[] = {{ ");
            output.Append(string.Join(", ", result));
            output.Append(" };");
            File.WriteAllText(fileName, output.ToString());
        }

        static void Main(string[] args)
        {
            if (args.Length > 4 || args.Length < 1)
            {
                Console.WriteLine("usage: Bitmap2OLED <bitmapPath> [<outputPath>] [--print]");
                Console.WriteLine();
                Console.WriteLine("Examples:");
                Console.WriteLine(@"    Bitmap2OLED OLED_display.bmp");
                Console.WriteLine(@"    Bitmap2OLED OLED_display.bmp --print");
                Console.WriteLine(@"    Bitmap2OLED OLED_display.bmp OLEDdisplay.c");
                Console.WriteLine(@"    Bitmap2OLED OLED_display.bmp OLEDdisplay.c --print");
                return;
            }

            var printIndex = Array.IndexOf(args, "--print");

            var result = ConvertBitmap(args[0], out int width, out int height);
            if (printIndex > -1)
                PrintResult(result, width, height);

            var fileName = Path.ChangeExtension(args[0], ".c");
            if (args.Length > 1 && printIndex > 1)
                fileName = args[1];

            WriteResult(result, fileName);
        }
    }
}
