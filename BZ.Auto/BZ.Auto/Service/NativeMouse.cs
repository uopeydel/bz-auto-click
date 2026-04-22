using System;
using System.Runtime.InteropServices;
using System.Threading;

public class NativeMouse
{
	[StructLayout(LayoutKind.Sequential)]
	struct INPUT
	{
		public int type;
		public MOUSEINPUT mi;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct MOUSEINPUT
	{
		public int dx;
		public int dy;
		public int mouseData;
		public int dwFlags;
		public int time;
		public IntPtr dwExtraInfo;
	}

	const int INPUT_MOUSE = 0;

	const int MOUSEEVENTF_MOVE = 0x0001;
	const int MOUSEEVENTF_ABSOLUTE = 0x8000;
	const int MOUSEEVENTF_LEFTDOWN = 0x0002;
	const int MOUSEEVENTF_LEFTUP = 0x0004;

	[DllImport("user32.dll")]
	static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

	[DllImport("user32.dll")]
	static extern int GetSystemMetrics(int nIndex);

	const int SM_CXSCREEN = 0;
	const int SM_CYSCREEN = 1;

	static int NormalizeX(int x)
	{
		return (int)Math.Round(x * 65535.0 / GetSystemMetrics(SM_CXSCREEN));
	}

	static int NormalizeY(int y)
	{
		return (int)Math.Round(y * 65535.0 / GetSystemMetrics(SM_CYSCREEN));
	}

	public static void Move(int x, int y)
	{
		var input = new INPUT
		{
			type = INPUT_MOUSE,
			mi = new MOUSEINPUT
			{
				dx = NormalizeX(x),
				dy = NormalizeY(y),
				dwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE
			}
		};

		SendInput(1, new[] { input }, Marshal.SizeOf(typeof(INPUT)));
	}

	public static void LeftClick()
	{
		var down = new INPUT
		{
			type = INPUT_MOUSE,
			mi = new MOUSEINPUT { dwFlags = MOUSEEVENTF_LEFTDOWN }
		};

		var up = new INPUT
		{
			type = INPUT_MOUSE,
			mi = new MOUSEINPUT { dwFlags = MOUSEEVENTF_LEFTUP }
		};

		SendInput(2, new[] { down, up }, Marshal.SizeOf(typeof(INPUT)));
	}
}