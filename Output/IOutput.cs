using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace CodeVideoMaker.Output;

public interface IOutput : IDisposable
{
    void AddFrame(Bitmap bitmap);
}