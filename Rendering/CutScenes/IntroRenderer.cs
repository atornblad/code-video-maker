using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using CodeVideoMaker.Output;

namespace CodeVideoMaker.Rendering.CutScenes;

class IntroRenderer : IDisposable
{
    private readonly IOutput output;
    private readonly string title;
    private readonly string subtitle;
    private readonly int fps;
    private readonly Bitmap bitmap;
    private readonly Graphics graphics;
    private readonly Bitmap helloPhoto;
    private readonly Color background = Color.FromArgb(0x65, 0x8d, 0x4a); // #658d4a
    private readonly Brush backgroundBrush;
    private readonly Brush darkenBrush;
    private readonly Brush lightenBrush;
    private readonly Font titleFont;
    private readonly Font subtitleFont;
    private readonly Font nameFont;


    public IntroRenderer(IOutput output, string videoTitle, string videoSubtitle, int fps, int width, int height)
    {
        this.output = output;
        this.title = videoTitle;
        this.subtitle = videoSubtitle;
        this.fps = fps;
        this.bitmap = new Bitmap(width, height);
        this.graphics = Graphics.FromImage(bitmap);
        this.helloPhoto = new Bitmap("hello-photo.png");
        this.backgroundBrush = new SolidBrush(background);
        this.backgroundBrush = new LinearGradientBrush(
            new Rectangle(0, 0, width, height),
            Color.FromArgb(background.R + 16, background.G + 16, background.B + 16),
            Color.FromArgb(background.R - 16, background.G - 16, background.B - 16),
            LinearGradientMode.Vertical);
        this.darkenBrush = new SolidBrush(Color.FromArgb(0x10, 0x00, 0x00, 0x00));
        this.lightenBrush = new SolidBrush(Color.FromArgb(0x10, 0xff, 0xff, 0xff));

        titleFont = MakeFont(height * 0.12f, width * 0.8f, videoTitle, FontStyle.Bold, "Rockwell");
        subtitleFont = MakeFont(height * 0.06f, width * 0.8f, videoSubtitle, FontStyle.Regular, "Franklin Gothic Book");
        nameFont = MakeFont(height * 0.04f, width * 0.5f, "Anders Marzi Tornblad", FontStyle.Regular, "Franklin Gothic Book");
    }

    private Font MakeFont(float initialSize, float maxWidth, string text, FontStyle style, string fontFamily)
    {
        Font font;
        float fontSize = initialSize;
        font = new Font(fontFamily, fontSize, style);
        var titleSize = graphics.MeasureString(text, font);
        while (titleSize.Width > maxWidth)
        {
            fontSize *= 0.95f;
            font = new Font(fontFamily, fontSize, FontStyle.Bold);
            titleSize = graphics.MeasureString(text, font);
        }

        return font;
    }

    public void RenderBackground(TimeSpan span)
    {
        for (var i = TimeSpan.Zero; i < span; i += TimeSpan.FromSeconds(1.0 / fps))
        {
            DrawBackground(i.TotalSeconds, 0.0);
            output.AddFrame(bitmap);
        }
    }

    public void RenderTimeline()
    {
        graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
        // Fade in for 1 second
        for (double i = 0; i < 1.0; i += 1.0 / fps)
        {
            DrawBackground(i, i);
            using var faderBrush = new SolidBrush(Color.FromArgb((int)((1.0 - i) * 255), Color.Black));
            graphics.FillRectangle(faderBrush, 0, 0, bitmap.Width, bitmap.Height);
            output.AddFrame(bitmap);
        }

        // Title appearing takes 0.5 seconds
        for (double i = 1; i < 1.5; i += 1.0 / fps)
        {
            DrawBackground(i, 1.0);
            DrawTitle((i - 1.0) * 2.0);
            output.AddFrame(bitmap);
        }

        // Subtitle appearing takes 0.5 seconds
        for (double i = 1.5; i < 2.0; i += 1.0 / fps)
        {
            DrawBackground(i, 1.0);
            DrawTitle(1.0);
            DrawSubtitle((i - 1.5) * 2.0);
            output.AddFrame(bitmap);
        }

        // Show for 3 seconds
        for (double i = 2.0; i < 5.0; i+= 1.0 / fps)
        {
            DrawBackground(i, 1.0);
            DrawTitle(1.0);
            DrawSubtitle(1.0);
            output.AddFrame(bitmap);
        }

        // Title disapperaing takes 0.5 seconds
        for (double i = 5.0; i < 5.5; i += 1.0 / fps)
        {
            DrawBackground(i, 1.0);
            DrawTitle((i - 4.5) * 2.0);
            DrawSubtitle(1.0);
            output.AddFrame(bitmap);
        }

        // Title disapperaing takes 0.5 seconds
        for (double i = 5.5; i < 6.0; i += 1.0 / fps)
        {
            DrawBackground(i, 1.0);
            DrawSubtitle((i - 5.0) * 2.0);
            output.AddFrame(bitmap);
        }

        // Fade out for 1 seconds
        for (double i = 6.0; i < 7.0; i+= 1.0 / fps)
        {
            DrawBackground(i, 7.0 - i);
            using var faderBrush = new SolidBrush(Color.FromArgb((int)((i - 6.0) * 255), Color.Black));
            graphics.FillRectangle(faderBrush, 0, 0, bitmap.Width, bitmap.Height);
            output.AddFrame(bitmap);
        }
    }

    // Time: from 0 to 1 - moving in from the left. 1 - still. 1 to 2 - moving out to the right.
    private void DrawTitle(double time)
    {
        var titleSize = graphics.MeasureString(title, titleFont);
        float startX = -titleSize.Width;
        float midX = bitmap.Width * 0.5f - titleSize.Width * 0.5f;
        float endX = bitmap.Width;
        float x;
        if (time <= 1.0)
        {
            x = startX + (midX - startX) * (float)Smooth(time);
        }
        else
        {
            x = midX + (endX - midX) * (float)Smooth(time - 1.0);
        }
        float y = bitmap.Height * 0.2f;
        float shadowOffsetX = bitmap.Height * 0.01f;
        float shadowOffsetY = bitmap.Height * 0.02f;
        using var shadow = new SolidBrush(Color.FromArgb(0xc0, 0x00, 0x00, 0x00));
        graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
        graphics.DrawString(title, titleFont, shadow, x + shadowOffsetX, y + shadowOffsetY);
        graphics.DrawString(title, titleFont, Brushes.White, x, y);
    }

    // Time: from 0 to 1 - moving in from the left. 1 - still. 1 to 2 - moving out to the right.
    private void DrawSubtitle(double time)
    {
        var subtitleSize = graphics.MeasureString(subtitle, subtitleFont);
        float startX = -subtitleSize.Width;
        float midX = bitmap.Width * 0.5f - subtitleSize.Width * 0.5f;
        float endX = bitmap.Width;
        float x;
        if (time <= 1.0)
        {
            x = startX + (midX - startX) * (float)Smooth(time);
        }
        else
        {
            x = midX + (endX - midX) * (float)Smooth(time - 1.0);
        }
        float y = bitmap.Height * 0.45f;
        float shadowOffsetX = bitmap.Height * 0.005f;
        float shadowOffsetY = bitmap.Height * 0.01f;
        using var shadow = new SolidBrush(Color.FromArgb(0xc0, 0x00, 0x00, 0x00));
        graphics.DrawString(subtitle, subtitleFont, shadow, x + shadowOffsetX, y + shadowOffsetY);
        graphics.DrawString(subtitle, subtitleFont, Brushes.White, x, y);
    }

    // Time: progressing from 0 to infinity - used for the background animation
    // Hello: from 0 to 1 - moving in from the left. 1 - still. 1 to 2 - moving out to the left.
    private void DrawBackground(double time, double hello)
    {
        //graphics.Clear(background);
        graphics.FillRectangle(backgroundBrush, 0, 0, bitmap.Width, bitmap.Height);
        DrawShapes(time);
        Gaussian(Math.Max(2, bitmap.Width / 200), 3);
        if (hello >= 0.0 && hello <= 2.0)
        {
            DrawFaceAndName(hello);
        }
    }

    private void DrawFaceAndName(double hello)
    {
        double helloPosRatio = Smooth(hello);
        float helloX = (float)(-helloPhoto.Width + helloPhoto.Width * helloPosRatio);
        float helloWidth = helloPhoto.Width * bitmap.Height / 1536.0f;
        float helloHeight = helloPhoto.Height * bitmap.Height / 1536.0f;
        float helloY = bitmap.Height - helloHeight;
        graphics.DrawImage(helloPhoto, helloX, helloY, helloWidth, helloHeight);

        float nameY = (float)(bitmap.Height * (1.0f - 0.1f * helloPosRatio));
        graphics.DrawString("Anders Marzi Tornblad", nameFont, Brushes.White, bitmap.Width * 0.2f, nameY + 0.01f);
    }

    private void Gaussian(int pixels, int noise)
    {
        var random = new Random();
        var gauss = new Bitmap(bitmap.Width, bitmap.Height);
        int count = pixels * 2 + 1;
        int[] reds = new int[count];
        int[] greens = new int[count];
        int[] blues = new int[count];
        int index = 0;
        int red = 0;
        int green = 0;
        int blue = 0;

        var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        byte[] bytes = new byte[bitmapData.Stride * bitmapData.Height];
        Marshal.Copy(bitmapData.Scan0, bytes, 0, bytes.Length);

        int dataIndex = 0;
        int writeIndex = 0;
        
        for (int y = 0; y < bitmap.Height; ++y)
        {
            for (int x = -pixels; x < bitmap.Width + pixels; ++x)
            {
                red -= reds[index];
                green -= greens[index];
                blue -= blues[index];
                

                reds[index] = 0;
                greens[index] = 0;
                blues[index] = 0;
                if (x >= 0 && x < bitmap.Width)
                {
                    //var pixel = bitmap.GetPixel(x, y);
                    red += (reds[index] = bytes[dataIndex++]);
                    green += (greens[index] = bytes[dataIndex++]);
                    blue += (blues[index] = bytes[dataIndex++]);
                    ++dataIndex;
                }
                if (x >= pixels)
                {
                    bytes[writeIndex++] = (byte)(red / count + random.Next(-noise, noise + 1));
                    bytes[writeIndex++] = (byte)(green / count + random.Next(-noise, noise + 1));
                    bytes[writeIndex++] = (byte)(blue / count + random.Next(-noise, noise + 1));
                    bytes[writeIndex++] = 255;
                }
                index = (index + 1) % count;
            }
        }
        Marshal.Copy(bytes, 0, bitmapData.Scan0, bytes.Length);
        bitmap.UnlockBits(bitmapData);
    }

    private void DrawShapes(double time)
    {
        var random = new Random(12345);
        for (int i = 0; i < 50; ++i)
        {
            int angles = random.Next(5, 8);
            double size = random.NextDouble() * 0.1 + 0.1;
            double xRoot = random.NextDouble();
            double yRoot = random.NextDouble();
            double xPhaseSpeed = random.NextDouble() * 0.1 + 0.05;
            double yPhaseSpeed = random.NextDouble() * 0.1 + 0.05;
            double xPhaseOffset = random.NextDouble() * 2 * Math.PI;
            double yPhaseOffset = random.NextDouble() * 2 * Math.PI;
            double xPhase = time * xPhaseSpeed + xPhaseOffset;
            double yPhase = time * yPhaseSpeed + yPhaseOffset;
            double xPhaseWeight = random.NextDouble() * 0.5 - 0.25;
            double yPhaseWeight = random.NextDouble() * 0.4 - 0.2;
            double timeSpeed = random.NextDouble() * 0.3 + 0.1;

            DrawShape(angles, size, xPhaseWeight * Math.Sin(xPhase) + xRoot, yPhaseWeight * Math.Sin(yPhase) + yRoot, time * timeSpeed, (i % 2) == 0);
        }
    }

    private void DrawShape(int corners, double size, double x, double y, double angle, bool brighten)
    {
        var points = new PointF[corners];
        double angleStep = Math.PI * 2.0 / corners;
        for (int i = 0; i < corners; ++i)
        {
            double cornerAngle = angle + i * angleStep;
            double cornerX = x * bitmap.Width + size * Math.Cos(cornerAngle) * bitmap.Height;
            double cornerY = y * bitmap.Height + size * Math.Sin(cornerAngle) * bitmap.Height;
            points[i] = new PointF((float)cornerX, (float)cornerY);
        }
        graphics.FillPolygon(brighten ? lightenBrush : darkenBrush, points);
    }

    private double Smooth(double ratio)
    {
        return ((1.0 - Math.Cos(ratio * Math.PI)) * 0.5);
    }

    public void Dispose()
    {
        backgroundBrush.Dispose();
        graphics.Dispose();
        bitmap.Dispose();
    }
}