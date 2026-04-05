using SkiaSharp;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;



namespace BZ.Auto.Service
{
	public static class ImageCapture
	{
		public static string Default = @"iVBORw0KGgoAAAANSUhEUgAAAQAAAAEACAIAAADTED8xAAADMElEQVR4nOzVwQnAIBQFQYXff81RUkQCOyDj1YOPnbXWPmeTRef+/3O/OyBjzh3CD95BfqICMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK0CMK1CMO0TAAD//2Anhf4QtqobAAAAAElFTkSuQmCC";

		[DllImport("user32.dll")]
		static extern bool SetProcessDPIAware();

		[DllImport("user32.dll")]
		static extern IntPtr GetDesktopWindow();

		[DllImport("user32.dll")]
		static extern IntPtr GetWindowDC(IntPtr hWnd);

		[DllImport("user32.dll")]
		static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC); // ✅ เพิ่ม

		[DllImport("gdi32.dll")]
		static extern IntPtr CreateCompatibleDC(IntPtr hDC);

		[DllImport("gdi32.dll")]
		static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

		[DllImport("gdi32.dll")]
		static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

		[DllImport("gdi32.dll")]
		static extern bool BitBlt(IntPtr hDC, int nXDest, int nYDest, int nWidth, int nHeight,
			IntPtr hSrcDC, int nXSrc, int nYSrc, int dwRop);

		[DllImport("gdi32.dll")]
		static extern bool DeleteObject(IntPtr hObject);

		[DllImport("gdi32.dll")]
		static extern bool DeleteDC(IntPtr hDC);

		private const int SRCCOPY = 0x00CC0020;

		// เรียกครั้งเดียวใน Program.cs หรือ Startup
		public static void Initialize() => SetProcessDPIAware();

		public static byte[] Capture(int width, int height)
		{
			IntPtr desktop = GetDesktopWindow();
			IntPtr desktopDC = GetWindowDC(desktop);
			IntPtr memoryDC = CreateCompatibleDC(desktopDC);
			IntPtr bitmap = CreateCompatibleBitmap(desktopDC, width, height);
			IntPtr oldBitmap = SelectObject(memoryDC, bitmap); // ✅ เก็บ old

			try
			{
				BitBlt(memoryDC, 0, 0, width, height, desktopDC, 0, 0, SRCCOPY);
				using var img = Image.FromHbitmap(bitmap);
				using var ms = new MemoryStream();
				img.Save(ms, ImageFormat.Png);
				return ms.ToArray();
			}
			finally
			{
				SelectObject(memoryDC, oldBitmap); // ✅ restore
				DeleteObject(bitmap);
				DeleteDC(memoryDC);
				ReleaseDC(desktop, desktopDC); // ✅ ใช้ ReleaseDC ไม่ใช่ DeleteDC
			}
		}

		public static byte[] CaptureRegion(int x1, int y1, int x2, int y2)
		{
			int width = x2 - x1;
			int height = y2 - y1;
			if (width <= 0 || height <= 0)
				throw new ArgumentException("Invalid coordinates");

			IntPtr desktop = GetDesktopWindow();
			IntPtr desktopDC = GetWindowDC(desktop);
			IntPtr memoryDC = CreateCompatibleDC(desktopDC);
			IntPtr bitmap = CreateCompatibleBitmap(desktopDC, width, height);
			IntPtr oldBitmap = SelectObject(memoryDC, bitmap); // ✅

			try
			{
				BitBlt(memoryDC, 0, 0, width, height, desktopDC, x1, y1, SRCCOPY);
				using var img = Image.FromHbitmap(bitmap);
				using var ms = new MemoryStream();
				img.Save(ms, ImageFormat.Png);
				return ms.ToArray();
			}
			finally
			{
				SelectObject(memoryDC, oldBitmap); // ✅
				DeleteObject(bitmap);
				DeleteDC(memoryDC);
				ReleaseDC(desktop, desktopDC); // ✅
			}
		}



		[DllImport("user32.dll")]
		private static extern int GetSystemMetrics(int nIndex);

		private const int SM_CXSCREEN = 0; // width
		private const int SM_CYSCREEN = 1; // height

		public static int GetWidth()
		{
			return GetSystemMetrics(SM_CXSCREEN);
		}

		public static int GetHeight()
		{
			return GetSystemMetrics(SM_CYSCREEN);
		}
	}
}
