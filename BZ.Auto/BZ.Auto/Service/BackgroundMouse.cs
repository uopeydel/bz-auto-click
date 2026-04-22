using BZ.Auto.Service;
using System;
using System.Runtime.InteropServices;

public class BackgroundMouse
{
	// นำเข้าฟังก์ชัน PostMessage
	[DllImport("user32.dll")]
	public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

	// Constant สำหรับคำสั่งเมาส์
	private const uint WM_LBUTTONDOWN = 0x0201;
	private const uint WM_LBUTTONUP = 0x0202;

	#region MoveMainCrhomeToTopLeft
	[DllImport("user32.dll", SetLastError = true)]
	static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

	// Constant สำหรับ uFlags (บอกให้ Windows รู้ว่าเราจะขยับแค่ตำแหน่ง ไม่สนขนาด)
	private const uint SWP_NOSIZE = 0x0001; // ไม่เปลี่ยนขนาดหน้าต่าง (ใช้ค่าเดิม)
	private const uint SWP_NOZORDER = 0x0004; // ไม่เปลี่ยนลำดับการซ้อน (อยู่เลเยอร์เดิม)
	private const uint SWP_SHOWWINDOW = 0x0040; // แสดงหน้าต่างขึ้นมา
	#endregion

	public static async Task ClickAtBackground(IntPtr handle, int x, int y)
	{
		SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);

		// คำนวณพิกัด X, Y ให้อยู่ในรูปแบบที่ Windows เข้าใจ (LPARAM)
		IntPtr lParam = (IntPtr)((y << 16) | (x & 0xFFFF));

		// ส่งคำสั่งเมาส์ซ้ายกดลง (Down)
		PostMessage(handle, WM_LBUTTONDOWN, (IntPtr)1, lParam);


		await Task.Delay(MouseOperationsService.GenerateRandomMillisecond(1, 2));

		// ส่งคำสั่งเมาส์ซ้ายปล่อย (Up)
		PostMessage(handle, WM_LBUTTONUP, (IntPtr)0, lParam);
	}
}