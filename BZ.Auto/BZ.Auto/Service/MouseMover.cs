using System;
using System.Runtime.InteropServices;
using System.Threading;

public static class MouseMover
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

	[DllImport("user32.dll")]
	static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

	[DllImport("user32.dll")]
	static extern int GetSystemMetrics(int nIndex);

	const int SM_CXSCREEN = 0;
	const int SM_CYSCREEN = 1;

	static int NormalizeX(int x)
		=> (int)Math.Round(x * 65535.0 / GetSystemMetrics(SM_CXSCREEN));

	static int NormalizeY(int y)
		=> (int)Math.Round(y * 65535.0 / GetSystemMetrics(SM_CYSCREEN));

	public static void MoveAbsolute(int x, int y)
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

	public static void MoveSmooth(
	int x1,
	int y1,
	int x2,
	int y2,
	double speedPxPerSec = 1000,   // ความเร็ว (px/sec)
	double smoothness = 1.0,       // 0.5 = แข็ง / 1 = ปกติ / 2+ = เนียนมาก
	int minSteps = 10)
	{
		double distance = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
		if (distance == 0) return;

		// เวลา (วินาที)
		double durationSec = distance / speedPxPerSec;

		// steps ปรับตาม smoothness
		int baseSteps = (int)(durationSec * 60); // base 60fps
		int steps = Math.Max(minSteps, (int)(baseSteps * smoothness));

		// delay ต่อ step
		int delayMs = (int)Math.Max(1, durationSec * 1000 / steps);

		for (int i = 0; i <= steps; i++)
		{
			// t = 0 → 1
			double t = i / (double)steps;

			// ===== Easing (ease-in-out cubic) =====
			double ease;
			if (t < 0.5)
				ease = 4 * t * t * t;
			else
				ease = 1 - Math.Pow(-2 * t + 2, 3) / 2;

			int x = (int)Math.Round(x1 + (x2 - x1) * ease);
			int y = (int)Math.Round(y1 + (y2 - y1) * ease);

			MoveAbsolute(x, y);
			Thread.Sleep(delayMs);
		}
	}
}