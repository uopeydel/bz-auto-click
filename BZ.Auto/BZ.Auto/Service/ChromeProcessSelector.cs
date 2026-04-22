using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

public class ChromeWindowInfo
{
	public int ProcessId { get; set; }
	public IntPtr Hwnd { get; set; }
	public string Title { get; set; } = "";
	public int ScreenIndex { get; set; }
}

public static class ChromeWindowManager
{
	// ====== CONFIG (เลือกจอที่ต้องการ) ======
	public static nint Hwnd_SELECTED = 0;
	 

	// ====== Win32 ======
	private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

	[DllImport("user32.dll")]
	private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

	[DllImport("user32.dll")]
	private static extern bool IsWindowVisible(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

	[DllImport("user32.dll")]
	private static extern int GetWindowTextLength(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

	[DllImport("user32.dll")]
	private static extern bool SetWindowPos(
		IntPtr hWnd,
		IntPtr hWndInsertAfter,
		int X,
		int Y,
		int cx,
		int cy,
		uint uFlags);

	private static readonly IntPtr HWND_TOP = IntPtr.Zero;
	private const uint SWP_NOSIZE = 0x0001;
	private const uint SWP_NOZORDER = 0x0004;

	[DllImport("user32.dll")]
	static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	const int SW_RESTORE = 9;


	[DllImport("user32.dll")]
	static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

	public struct RECT
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;
	}



	// ====== MAIN LOGIC ======
	public static List<ChromeWindowInfo> GetChromeWindows()
	{
		var chromePids = Process.GetProcessesByName("chrome")
								.Select(p => p.Id)
								.ToHashSet();

		var result = new List<ChromeWindowInfo>();

		EnumWindows((hWnd, lParam) =>
		{
			if (!IsWindowVisible(hWnd))
				return true;

			GetWindowThreadProcessId(hWnd, out uint pid);

			if (!chromePids.Contains((int)pid))
				return true;

			int length = GetWindowTextLength(hWnd);
			if (length == 0)
				return true;

			var sb = new StringBuilder(length + 1);
			GetWindowText(hWnd, sb, sb.Capacity);

			string title = sb.ToString();

			//// ====== FILTER TITLE ======
			//if (!title.EndsWith(" - Google Chrome"))
			//	return true;

			var screen = Screen.FromHandle(hWnd);
			int screenIndex = Array.IndexOf(Screen.AllScreens, screen);

			result.Add(new ChromeWindowInfo
			{
				ProcessId = (int)pid,
				Hwnd = hWnd,
				Title = title,
				ScreenIndex = screenIndex
			});

			return true;
		}, IntPtr.Zero);

		return result;
	}

	public static void MoveSelectedScreenWindowsToOrigin(List<ChromeWindowInfo> windows)
	{
		var screen = Screen.AllScreens[0];
		var bounds = screen.Bounds;
		Console.WriteLine($"Bonds X:{bounds.X} Y:{bounds.Y}");
		if (Hwnd_SELECTED == 0)
		{
			return;
		}
		ShowWindow(Hwnd_SELECTED, SW_RESTORE);
		SetWindowPos(Hwnd_SELECTED, HWND_TOP,
			 bounds.X -8,
			 bounds.Y,
			 0, 0, SWP_NOSIZE | SWP_NOZORDER);

		var w = windows.Where(x => x.Hwnd == Hwnd_SELECTED).FirstOrDefault();
		Console.WriteLine($"Move to origin Title: {w.Title} ProcessId : {w.ProcessId} | ScreenIndex : {w.ScreenIndex} | Hwnd : {w.Hwnd} | ");
		// ย้าย window ไป (0,0) ของ virtual desktop

	}
}

