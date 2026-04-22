using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public class DirectScreenOverlay
{
	// ดึง Device Context (DC) ของหน้าจอ
	[DllImport("user32.dll")]
	static extern IntPtr GetDC(IntPtr hWnd);

	[DllImport("user32.dll")]
	static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

	[DllImport("user32.dll")]
	static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

	public static async Task DrawStickerAsync(string imagePath, int x, int y, int durationSeconds)
	{
		// 1. ดึง Handle ของหน้าจอทั้งหมด (IntPtr.Zero คือทั้ง Desktop)
		IntPtr desktopPtr = GetDC(IntPtr.Zero);

		using (Graphics g = Graphics.FromHdc(desktopPtr))
		{
			try
			{
				using (Image img = Image.FromFile(imagePath))
				{
					// 2. วาดรูปลงไปที่พิกัด X, Y
					g.DrawImage(img, x, y, img.Width, img.Height);

					// 3. รอตามเวลาที่กำหนด (หน่วยวินาที)
					await Task.Delay(durationSeconds * 1000);
				}
			}
			finally
			{
				// 4. ล้างหน้าจอ (สั่งให้ Windows วาดหน้าจอใหม่ทับรอยที่เราวาดไว้)
				ReleaseDC(IntPtr.Zero, desktopPtr);
				// สั่งล้างพิกัดนั้นๆ เพื่อให้ภาพหายไป
				InvalidateRect(IntPtr.Zero, IntPtr.Zero, true);
			}
		}
	}
}