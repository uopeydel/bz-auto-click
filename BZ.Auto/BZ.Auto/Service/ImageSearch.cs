using BZ.Auto.Models;
using System.Buffers.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BZ.Auto.Service
{

	public static class ImageSearch
	{
		public static double Confidence { get; set; } = 0.90;
		/*
	  🎯 int tolerance = 10

👉 ความต่างของสีที่ “ยอมรับได้” ต่อ pixel

0 = ต้องเหมือนเป๊ะ 100%
10 = ต่างได้นิดหน่อย (กันแสง/anti-alias)
ค่ายิ่งสูง = ยิ่ง “ยืดหยุ่น” แต่เสี่ยง match ผิด

📌 ใช้แก้ปัญหา:

สีเพี้ยนเล็กน้อย
UI เปลี่ยน shade นิด ๆ 
	  */

		/*
		 
		 🎯 double threshold = 0.95

👉 เปอร์เซ็นต์ความเหมือนขั้นต่ำ (0–1)

0.95 = ต้องเหมือน ≥ 95% ถึงจะถือว่า “เจอ”
1.0 = ต้องเหมือน 100%
0.8 = ยอมให้คล้าย ๆ ก็ผ่าน

📌 ใช้คุม:

ความ “มั่นใจ” ว่าใช่ภาพนั้นจริง
		 */
		public static (Point position, double similarity) FindImage(
	byte[] bigBytes,
	byte[] smallBytes,
	int tolerance = 10,
	double threshold = 0.95)
		{
			using var bmpBig = new Bitmap(new MemoryStream(bigBytes));
			using var bmpSmall = new Bitmap(new MemoryStream(smallBytes));

			var rectBig = new Rectangle(0, 0, bmpBig.Width, bmpBig.Height);
			var rectSmall = new Rectangle(0, 0, bmpSmall.Width, bmpSmall.Height);

			var dataBig = bmpBig.LockBits(rectBig, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			var dataSmall = bmpSmall.LockBits(rectSmall, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

			int strideBig = dataBig.Stride;
			int strideSmall = dataSmall.Stride;

			int bestX = -1, bestY = -1;
			double bestScore = 0;

			unsafe
			{
				byte* ptrBig = (byte*)dataBig.Scan0;
				byte* ptrSmall = (byte*)dataSmall.Scan0;

				for (int y = 0; y <= bmpBig.Height - bmpSmall.Height; y++)
				{
					for (int x = 0; x <= bmpBig.Width - bmpSmall.Width; x++)
					{
						int matchCount = 0;
						int total = bmpSmall.Width * bmpSmall.Height * 4;

						for (int j = 0; j < bmpSmall.Height; j++)
						{
							byte* rowBig = ptrBig + (y + j) * strideBig + x * 4;
							byte* rowSmall = ptrSmall + j * strideSmall;

							for (int i = 0; i < bmpSmall.Width * 4; i++)
							{
								if (Math.Abs(rowBig[i] - rowSmall[i]) <= tolerance)
									matchCount++;
							}
						}

						double similarity = (double)matchCount / total;

						if (similarity > bestScore)
						{
							bestScore = similarity;
							bestX = x;
							bestY = y;
						}
					}
				}
			}

			bmpBig.UnlockBits(dataBig);
			bmpSmall.UnlockBits(dataSmall);

			if (bestScore >= threshold)
				return (new Point(bestX, bestY), bestScore);

			return (new Point(-1, -1), bestScore);
		}


		public static (Point position, bool similarity) FindBitmapSmallPosition(byte[] bigBytes, byte[] smallBytes)
		{
			using var bitmapBig = new Bitmap(new MemoryStream(bigBytes));
			using var bitmapSmall = new Bitmap(new MemoryStream(smallBytes));


			// Check if bitmapSmall is smaller than bitmapBig
			if (bitmapSmall.Width > bitmapBig.Width || bitmapSmall.Height > bitmapBig.Height)
			{
				throw new ArgumentException("bitmapSmall must be smaller than bitmapBig");
			}

			// Iterate through each pixel of bitmapBig
			for (int x = 0; x < bitmapBig.Width - bitmapSmall.Width; x++)
			{
				for (int y = 0; y < bitmapBig.Height - bitmapSmall.Height; y++)
				{
					// Check if the current pixel in bitmapBig matches the corresponding pixel in bitmapSmall
					bool matches = true;
					for (int i = 0; i < bitmapSmall.Width; i++)
					{
						for (int j = 0; j < bitmapSmall.Height; j++)
						{
							Color pixelBig = bitmapBig.GetPixel(x + i, y + j);
							Color pixelSmall = bitmapSmall.GetPixel(i, j);

							if (pixelBig != pixelSmall)
							{
								matches = false;
								break;
							}
						}

						if (!matches)
						{
							break;
						}
					}

					// If all pixels match, then bitmapSmall is found at the current position
					if (matches)
					{
						return (new Point(x, y), true);
					}
				}
			}
			// If bitmapSmall is not found, return (-1, -1)
			return (new Point(-1, -1), false);
		}



		//เอาไว้ใช้ค้นหาภาพจากทั้งหน้าจอ
		public static ImageStepModel SearchEqualImageInScreen(ImageStepModel imageStep)
		{
			imageStep.Confidence = 0;
			imageStep.IsFound = false;
			imageStep.FoundTopLeftX = -1;
			imageStep.FoundTopLeftY = -1;
			imageStep.FoundBotRightX = -1;
			imageStep.FoundBotRightY = -1;


			//กำหนดให้กว้างได้สูงสุด 600 ป้องกันมันมามองหาเจอหน้าเว็บที่เปิดอยู่ทางขวา
			int screenCaptureWidth = 600;
			;
			//capture ภาพจากหน้าจอเกมทั้งจอทางซ้ายของสกรีน
			Rectangle captureAreaForSearch = new Rectangle((int)0, (int)0, screenCaptureWidth, ImageCapture.GetHeight());
			var screenShortBytes = ImageCapture.Capture(screenCaptureWidth, ImageCapture.GetHeight());

			byte[] imageBaseCurrentStep = Convert.FromBase64String(imageStep.BaseImage);

			var rectWidth = imageStep.BotRightX - imageStep.TopLeftX;
			var rectHeight = imageStep.BotRightY - imageStep.TopLeftY;

			//แคปเจอแค่จากตรงที่ x y ที่กำหนด
			var imgCaptureRegion = ImageCapture.CaptureRegion(
				imageStep.TopLeftX,
				imageStep.TopLeftY,
				imageStep.BotRightX,
				imageStep.BotRightY
			);
			imageStep.CurrentFromScreenSmallToCompare = Convert.ToBase64String(imgCaptureRegion);
			imageStep.CaptureWholeScreenImage = Convert.ToBase64String(screenShortBytes);

			#region TestRegion
			// แคปทั้งจอ
			var screenshot = screenShortBytes;// ScreenCapture.Capture(screenW, screenH);

			// แคป template จาก region ที่ระบุ
			var template = imageBaseCurrentStep;// ScreenCapture.CaptureRegion(tx1, ty1, tx2, ty2);

			var match = TemplateMatcher.FindTemplate(screenshot, template, threshold: Confidence);
			var isFound = match != null;
			if (isFound)
			{
				var diffX = Math.Abs(imageStep.TopLeftX - match.X);
				var diffY = Math.Abs(imageStep.TopLeftY - match.Y);

				Console.WriteLine();
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine($"Diff X:{imageStep.TopLeftX - match.X}");
				Console.WriteLine($"Diff Y:{imageStep.TopLeftY - match.Y}");
				Console.ResetColor();
				Console.WriteLine();
				Console.WriteLine($"Confidence:{match.Confidence}");
				Console.WriteLine($"Scale:{match.Scale}");
				Console.WriteLine();

				// config สำหรับตรวจความใกล้เคียง
				// Confidence < 0.95 ความเหมือนน้อยกว่า 95 เปอเซน ไม่ผ่าน
				// ถ้าตำแหน่งห่างกันเกิน 100 px ไม่ผ่าน
				if (match.Confidence < Confidence && (diffX > 100 || diffY > 100))
				{
					imageStep.IsFound = false;

					imageStep.Confidence = match.Confidence;
					imageStep.Scale = match.Scale;

					imageStep.FoundTopLeftX = match.X;
					imageStep.FoundTopLeftY = match.Y;
					imageStep.FoundBotRightX = match.X + match.Width;
					imageStep.FoundBotRightY = match.Y + match.Height;
				}
				else
				{ 
					imageStep.IsFound = true;
					imageStep.FoundTopLeftX = match.X;
					imageStep.FoundTopLeftY = match.Y;
					imageStep.FoundBotRightX = match.X + match.Width;
					imageStep.FoundBotRightY = match.Y + match.Height;

					imageStep.Confidence = match.Confidence;
					imageStep.Scale = match.Scale;

					var imgCaptureRegionFound = ImageCapture.CaptureRegion(
						imageStep.FoundTopLeftX,
						imageStep.FoundTopLeftY,
						imageStep.FoundBotRightX,
						imageStep.FoundBotRightY
					);
					imageStep.CurrentFromScreenSmallToCompare = Convert.ToBase64String(imgCaptureRegionFound); 
				}
				//imageStep.CurrentFromScreenSmallToCompare
			}

			return imageStep;
			#endregion
			//var result = ImageSearch.FindImage(screenShortBytes, imageBaseCurrentStep, 15, 0.90);
			var result = FindBitmapSmallPosition(screenShortBytes, imageBaseCurrentStep);
			//เป็นจริง คลิกเม้า
			imageStep.IsFound = (result.position.X != -1);
			if (imageStep.IsFound)
			{
				imageStep.FoundTopLeftX = result.position.X;
				imageStep.FoundTopLeftY = result.position.Y;
				imageStep.FoundBotRightX = result.position.X + rectWidth;
				imageStep.FoundBotRightY = result.position.Y + rectHeight;
			}
			else
			{
				/*
				// แคปทั้งจอ
				var screenshot = screenShortBytes;// ScreenCapture.Capture(screenW, screenH);

				// แคป template จาก region ที่ระบุ
				var template = imageBaseCurrentStep;// ScreenCapture.CaptureRegion(tx1, ty1, tx2, ty2);

				var match = TemplateMatcher.FindTemplate(screenshot, template, threshold: 0.85);
				var isFound = match != null;
				if (isFound)
				{
					imageStep.IsFound = true;
					imageStep.FoundTopLeftX = match.X;
					imageStep.FoundTopLeftY = match.Y;
					imageStep.FoundBotRightX = match.X + match.Width;
					imageStep.FoundBotRightY = match.Y + match.Height;
				}
				*/
				/*
				using var bitmapBig = new Bitmap(new MemoryStream(screenShortBytes));
				using var bitmapSmall = new Bitmap(new MemoryStream(imageBaseCurrentStep));


				var isLike = IsLikely(bitmapBig, bitmapSmall);

				if (isLike)
				{
					imageStep.FoundTopLeftX = imageStep.TopLeftX;
					imageStep.FoundTopLeftY = imageStep.TopLeftY;
					imageStep.FoundBotRightX = imageStep.BotRightX;
					imageStep.FoundBotRightY = imageStep.BotRightY;
					imageStep.IsFound = true;
				}
				else
				{
					//using var bitmapCaptureCurrentTime = new Bitmap(new MemoryStream(imageBaseCurrentStep));
					var like = FindBitmapSmallPosition(imgCaptureRegion, imageBaseCurrentStep);
					imageStep.IsFound = (like.position.X != -1);
					if (isLike)
					{
						imageStep.FoundTopLeftX = imageStep.TopLeftX;
						imageStep.FoundTopLeftY = imageStep.TopLeftY;
						imageStep.FoundBotRightX = imageStep.BotRightX;
						imageStep.FoundBotRightY = imageStep.BotRightY;
						imageStep.IsFound = true;
					}
				}
				*/

			}

			return imageStep;
		}

		public static Point FindBitmapSmallPosition(Bitmap bitmapBig, Bitmap bitmapSmall)
		{
			// Check if bitmapSmall is smaller than bitmapBig
			if (bitmapSmall.Width > bitmapBig.Width || bitmapSmall.Height > bitmapBig.Height)
			{
				throw new ArgumentException("bitmapSmall must be smaller than bitmapBig");
			}

			// Iterate through each pixel of bitmapBig
			for (int x = 0; x < bitmapBig.Width - bitmapSmall.Width; x++)
			{
				for (int y = 0; y < bitmapBig.Height - bitmapSmall.Height; y++)
				{
					// Check if the current pixel in bitmapBig matches the corresponding pixel in bitmapSmall
					bool matches = true;
					for (int i = 0; i < bitmapSmall.Width; i++)
					{
						for (int j = 0; j < bitmapSmall.Height; j++)
						{
							Color pixelBig = bitmapBig.GetPixel(x + i, y + j);
							Color pixelSmall = bitmapSmall.GetPixel(i, j);

							if (pixelBig != pixelSmall)
							{
								matches = false;
								break;
							}
						}

						if (!matches)
						{
							break;
						}
					}

					// If all pixels match, then bitmapSmall is found at the current position
					if (matches)
					{
						return new Point(x, y);
					}
				}
			}
			// If bitmapSmall is not found, return (-1, -1)
			return new Point(-1, -1);
		}
		//public static bool IsLikely(Bitmap pictureBox1, Bitmap pictureBox2)
		//{
		//	double result = GetSimilarityPercentage(pictureBox1, pictureBox2);

		//	if (result >= 98)
		//	{
		//		return true;
		//	}
		//	else
		//	{
		//		return false;
		//	}

		//}



		//public static double GetSimilarityPercentage(Bitmap bmp1, Bitmap bmp2)
		//{
		//	if (bmp1.Size != bmp2.Size) return 0; // ขนาดไม่เท่ากัน ให้ความคล้ายเป็น 0

		//	int width = bmp1.Width;
		//	int height = bmp1.Height;
		//	int diffPixels = 0;

		//	// ใช้ LockBits เพื่อเข้าถึงข้อมูลใน Memory โดยตรง (เร็วกว่า GetPixel 100 เท่า)
		//	BitmapData data1 = bmp1.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
		//	BitmapData data2 = bmp2.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

		//	int size = data1.Stride * data1.Height;
		//	byte[] bytes1 = new byte[size];
		//	byte[] bytes2 = new byte[size];

		//	Marshal.Copy(data1.Scan0, bytes1, 0, size);
		//	Marshal.Copy(data2.Scan0, bytes2, 0, size);

		//	bmp1.UnlockBits(data1);
		//	bmp2.UnlockBits(data2);

		//	// เปรียบเทียบทีละ Byte (BGRA)
		//	for (int i = 0; i < size; i += 4)
		//	{
		//		// เช็คว่าค่าสี Blue, Green, Red ต่างกันไหม (ข้าม Alpha)
		//		if (bytes1[i] != bytes2[i] || bytes1[i + 1] != bytes2[i + 1] || bytes1[i + 2] != bytes2[i + 2])
		//		{
		//			diffPixels++;
		//		}
		//	}

		//	int totalPixels = width * height;
		//	double similarity = ((double)(totalPixels - diffPixels) / totalPixels) * 100;

		//	return similarity;
		//}
		public static Point FindImagePosition(byte[] bigImageBytes, byte[] smallImageBytes)
		{
			using var msBig = new MemoryStream(bigImageBytes);
			using var msSmall = new MemoryStream(smallImageBytes);

			using var bmpBig = new Bitmap(msBig);
			using var bmpSmall = new Bitmap(msSmall);

			return FindBitmapPosition(bmpBig, bmpSmall);
		}

		public static Point FindBitmapPosition(Bitmap bmpBig, Bitmap bmpSmall)
		{
			if (bmpSmall.Width > bmpBig.Width || bmpSmall.Height > bmpBig.Height)
				return new Point(-1, -1);

			var rectBig = new Rectangle(0, 0, bmpBig.Width, bmpBig.Height);
			var rectSmall = new Rectangle(0, 0, bmpSmall.Width, bmpSmall.Height);

			var dataBig = bmpBig.LockBits(rectBig, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			var dataSmall = bmpSmall.LockBits(rectSmall, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

			int strideBig = dataBig.Stride;
			int strideSmall = dataSmall.Stride;

			unsafe
			{
				byte* ptrBig = (byte*)dataBig.Scan0;
				byte* ptrSmall = (byte*)dataSmall.Scan0;

				for (int y = 0; y <= bmpBig.Height - bmpSmall.Height; y++)
				{
					for (int x = 0; x <= bmpBig.Width - bmpSmall.Width; x++)
					{
						bool match = true;

						for (int j = 0; j < bmpSmall.Height && match; j++)
						{
							byte* rowBig = ptrBig + (y + j) * strideBig + x * 4;
							byte* rowSmall = ptrSmall + j * strideSmall;

							for (int i = 0; i < bmpSmall.Width * 4; i++)
							{
								if (rowBig[i] != rowSmall[i])
								{
									match = false;
									break;
								}
							}
						}

						if (match)
						{
							bmpBig.UnlockBits(dataBig);
							bmpSmall.UnlockBits(dataSmall);
							return new Point(x, y);
						}
					}
				}
			}

			bmpBig.UnlockBits(dataBig);
			bmpSmall.UnlockBits(dataSmall);

			return new Point(-1, -1);
		}
	}
}
