
using SkiaSharp;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BZ.Auto.Service
{
	public class ImageCompare
	{
		private static Rectangle GetBitmapBounds(Bitmap bitmap)
		{
			Rectangle bounds = Rectangle.Empty;
			bounds = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
			return bounds;
		}
		private static Bitmap CropBitmap(Bitmap source, Rectangle cropArea)
		{
			// Check if the crop area is within the bounds of the source bitmap
			if (!cropArea.IntersectsWith(GetBitmapBounds(source)))
			{
				throw new ArgumentException("Crop area is outside the bounds of the source bitmap");
			}

			// Create a new bitmap with the same dimensions as the crop area
			Bitmap croppedBitmap = new Bitmap(cropArea.Width, cropArea.Height);

			// Use Graphics to draw the cropped portion of the source bitmap onto the new bitmap
			using (Graphics g = Graphics.FromImage(croppedBitmap))
			{
				g.DrawImage(source, new Rectangle(0, 0, cropArea.Width, cropArea.Height), cropArea, GraphicsUnit.Pixel);
			}

			// Return the cropped bitmap
			return croppedBitmap;
		}

		public static bool IsLikely(byte[] bytes1, byte[] bytes2, Action<string> AppendLogs, int fource = 0)
		{
			double result = GetSimilaritySkia(bytes1, bytes2);

			if (fource == 0)
			{
				AppendLogs($"Percent : {result}");
			}
			if (result >= 90)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public static double GetSimilaritySkia(byte[] bytes1, byte[] bytes2)
		{
			using var bmp1 = SKBitmap.Decode(bytes1);
			using var bmp2 = SKBitmap.Decode(bytes2);

			if (bmp1 == null || bmp2 == null) return 0;
			if (bmp1.Width != bmp2.Width || bmp1.Height != bmp2.Height) return 0;

			int diffPixels = 0;

			// เข้าถึง Pixel Span โดยตรง (ปลอดภัยและเร็ว)
			var pixels1 = bmp1.GetPixelSpan();
			var pixels2 = bmp2.GetPixelSpan();

			for (int i = 0; i < pixels1.Length; i++)
			{
				if (pixels1[i] != pixels2[i])
				{
					diffPixels++;
				}
			}

			// ใน SkiaSharp 1 pixel (RGBA) จะมี 4 elements ใน span (หรือเทียบเป็นก้อนสีเลยก็ได้)
			// แต่เพื่อความแม่นยำเราจะหารด้วยจำนวนข้อมูลทั้งหมด
			double totalElements = pixels1.Length;
			return ((totalElements - diffPixels) / totalElements) * 100;
		}
	}
}
