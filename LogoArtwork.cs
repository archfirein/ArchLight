using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
namespace ArchLightBrand
{
    internal static class LogoArtwork
    {
        internal static Bitmap Render(int size)
        {
            if (size < 1 || size > 2048) throw new ArgumentOutOfRangeException("size");
            // Supersampling keeps the circular edge smooth even in the notification area.
            int canvas = Math.Max(256, size * 2);
            using (Bitmap large = new Bitmap(canvas, canvas, PixelFormat.Format32bppArgb))
            using (Graphics g = Graphics.FromImage(large))
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ArchLight.Logo.png"))
            {
                if (stream == null) throw new InvalidDataException("Embedded logo is missing.");
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                using (Brush background = new SolidBrush(Color.FromArgb(19, 25, 36)))
                    g.FillEllipse(background, canvas * .015f, canvas * .015f, canvas * .97f, canvas * .97f);
                using (Bitmap source = new Bitmap(stream))
                {
                    // Remove only the neutral charcoal backdrop; retain saturated red/blue artwork.
                    for (int y = 0; y < source.Height; y++)
                    for (int x = 0; x < source.Width; x++)
                    {
                        Color c = source.GetPixel(x, y);
                        int max = Math.Max(c.R, Math.Max(c.G, c.B));
                        int min = Math.Min(c.R, Math.Min(c.G, c.B));
                        if (max < 50 && max - min < 12) source.SetPixel(x, y, Color.Transparent);
                    }
                    float scale = canvas * .94f / Math.Max(source.Width, source.Height);
                    float width = source.Width * scale, height = source.Height * scale;
                    g.DrawImage(source, (canvas - width) / 2, (canvas - height) / 2, width, height);
                }
                Bitmap result = new Bitmap(size, size, PixelFormat.Format32bppArgb);
                using (Graphics output = Graphics.FromImage(result))
                {
                    output.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    output.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    output.DrawImage(large, new Rectangle(0, 0, size, size));
                }
                return result;
            }
        }
    }
}
