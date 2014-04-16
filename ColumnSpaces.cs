namespace Telerik.Sitefinity.Samples.Common
{
    public class ColumnSpaces
    {
        public ColumnSpaces(double top, double right, double bottom, double left)
        {
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
            this.Left = left;
        }

        public double Bottom { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
        public double Top { get; set; }
    }
}