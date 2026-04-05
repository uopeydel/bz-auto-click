using BZ.Auto.Models;
using System.Drawing;

namespace BZ.Auto.Service
{
	public class MouseClickHelper
	{
		public static int RandomIntBetween(int minValue, int maxValue)
		{
			if (minValue > maxValue)
			{
				throw new ArgumentException("minValue cannot be greater than maxValue");
			}

			Random random = new Random();
			return random.Next(minValue * 1000, maxValue * 1000);
		}
		public static int ConvertSecondToMilisecond(decimal seconds)
		{
			// TimeSpan.FromSeconds(seconds);
			int milliseconds = (int)(seconds * 1000);
			return milliseconds;
		}
		public static Point GetRandomPointInRectangle(ImageStepModel item)
		{
			var rectWidth = item.FoundBotRightX - item.FoundTopLeftX;
			var rectHeight = item.FoundBotRightY - item.FoundTopLeftY;
			Rectangle rectangle = new Rectangle(item.FoundTopLeftX, item.FoundTopLeftY, (int)rectWidth, (int)rectHeight);

			Random random = new Random();
			int x = random.Next(rectangle.Left, rectangle.Right);
			int y = random.Next(rectangle.Top, rectangle.Bottom);

			Point randomPoint = new Point(x, y);
			return randomPoint;
		}
	}
}
