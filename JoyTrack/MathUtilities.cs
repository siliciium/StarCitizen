using System;
using System.Windows.Media;

namespace JoyTrack
{
    internal class MathUtilities
    {
        public static double RawToPixels_TargetJoy(double x)
        {
            double startX = 0;
            double offset = 5;

            double xMin = ushort.MinValue;
            double xMax = ushort.MaxValue;

            double yMin = startX - offset;
            double yMax = startX + offset;

            double y = ((x - xMin) * (yMax - yMin) / (xMax - xMin)) + yMin;

            return y;
        }

        public static double RawToPixels_TargetHead(double x, double minRange, double maxRange)
        {
            double xMin = ushort.MinValue;
            double xMax = ushort.MaxValue;

            double yMin = minRange;
            double yMax = maxRange;

            double y = ((x - xMin) * (yMax - yMin) / (xMax - xMin)) + yMin;

            return y;

        }

        public static double RawToPixels_Head(double x)
        {

            double xMin = ushort.MinValue;
            double xMax = ushort.MaxValue;

            double yMin = -90;
            double yMax = 90;

            double y = ((x - xMin) * (yMax - yMin) / (xMax - xMin)) + yMin;

            return y;

        }

        public static double InvertValue(double number)
        {
            if (number < 0)
            {
                number = Math.Abs(number);
            }
            else if (number > 0)
            {
                number *= -1.0;
            }
            return number;
        }

        public static double ToDegrees(int joystate, double limit)
        {
            double r = 360;
            double fraction = (double)joystate / ushort.MaxValue;
            double result = fraction * r;
            result -= limit;

            return result;
        }

        public static void MoveShapes(TransformGroup tfg, double offsetX, double offsetY)
        {
            TranslateTransform translateTransform = tfg.Children[0] as TranslateTransform;
            if (translateTransform != null)
            {
                translateTransform.X = offsetX;
                translateTransform.Y = offsetY;
            }
        }

        /// <summary>
        /// Applies an exponential curve to a linear value.
        /// </summary>
        /// <param name="value">The linear value to transform.</param>
        /// <param name="minY">The minimum value of the range.</param>
        /// <param name="maxY">The maximum value of the range.</param>
        /// <param name="exponent">The exponent for the curve.</param>
        /// <returns>The transformed value in the original range.</returns>
        public static Int32 ApplyCurveToMax(double value, double minY, double maxY, double curve_exponent)
        {
            // Normalize the value
            double normalizedValue = (value - minY) / (maxY - minY);

            // Apply the exponential curve
            double curvedValue = Math.Pow(normalizedValue, curve_exponent);

            // Denormalize the value back to the original range
            double transformedValue = minY + curvedValue * (maxY - minY);

            return Convert.ToInt32(Clamp(transformedValue, minY, maxY));
        }

        /// <summary> 
        /// Applies an exponential curve to a linear value. 
        /// </summary>
        /// <param name="value">The linear value to transform.</param>
        /// <param name="minY">The minimum value of the range.</param>
        /// <param name="maxY">The maximum value of the range.</param>
        /// <param name="exponent">The exponent for the curve.</param>
        /// <returns>The transformed value in the original range.</returns>
        public static Int32 ApplyCurveToZero(double value, double minY, double maxY, double curve_exponent)
        {
            // Normalize the value
            double normalizedValue = (maxY - value) / (maxY - minY);

            // Apply the exponential curve
            double curvedValue = Math.Pow(normalizedValue, curve_exponent);

            // Denormalize the value back to the original range
            double transformedValue = maxY - curvedValue * (maxY - minY);

            return Convert.ToInt32(Clamp(transformedValue, minY, maxY));
        }

        /// <summary>
        /// Restricts a value to be within a specified range.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The minimum value of the range.</param>
        /// <param name="max">The maximum value of the range.</param>
        /// <returns>The clamped value.</returns>
        private static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }


        /// <summary> 
        /// Converts a value from one range to a corresponding value in another range using linear interpolation. 
        /// </summary> 
        /// <param name="x">The value to be converted.</param> 
        /// <param name="a">The lower bound of the original range.</param> 
        /// <param name="b">The upper bound of the original range.</param> 
        /// <param name="c">The lower bound of the target range.</param> 
        /// <param name="d">The upper bound of the target range.</param> 
        /// <returns>The converted value in the target range.</returns>
        public static Int32 ConvertRange(double x, double a, double b, double c, double d)
        {
            return Convert.ToInt32(c + ((x - a) * (d - c)) / (b - a));
        }
    }
}
