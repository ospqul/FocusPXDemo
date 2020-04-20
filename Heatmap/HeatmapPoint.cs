namespace Heatmap
{
    public class HeatmapPoint
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Value { get; set; }

        public HeatmapPoint(int x, int y, int value)
        {
            X = x;
            Y = y;
            Value = value;
        }
    }
}
