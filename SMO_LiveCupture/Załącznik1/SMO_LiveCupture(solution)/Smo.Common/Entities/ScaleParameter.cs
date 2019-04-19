namespace SmoReader.Entities
{
    public class ScalingCoefficients
    {

        public ScalingCoefficients(double x1, double y1, double x2, double y2, string signalName)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
            SignalName = signalName;

        }

        private double X1 { get; set; }
        private double Y1 { get; set; }
        private double X2 { get; set; }
        private double Y2 { get; set; }

        public double a => (Y2 - Y1)/ (X2 - X1);
        public double b => Y1 - a * X1;

        public string SignalName { get; private set; }
    }
}