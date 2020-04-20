using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace Heatmap
{
    public class HeatmapModel
    {
        public List<Color> colorList { get; set; }

        private Bitmap _bitmap;
        private Graphics _graphics;

        public HeatmapModel(int width, int height)
        {
            colorList = ColorPalette.GetColorList();
            _bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            _graphics = Graphics.FromImage(_bitmap);
        }

        public BitmapImage BitmapToImageSource()
        {
            using (MemoryStream memory = new MemoryStream())
            {
                _bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                bitmapimage.Freeze();
                return bitmapimage;
            }
        }

        public void PaintHeatMapPoint(HeatmapPoint heatmapPoint)
        {
            var rect = new Rectangle(heatmapPoint.X, heatmapPoint.Y, 1, 1);
            SolidBrush brush = new SolidBrush(colorList[heatmapPoint.Value]);
            _graphics.FillRectangle(brush, rect);
        }
    }
}
