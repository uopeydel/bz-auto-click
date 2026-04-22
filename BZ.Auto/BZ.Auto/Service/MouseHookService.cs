using System.Runtime.InteropServices;

public class MouseHookService : IDisposable
{
	private const int WH_KEYBOARD_LL = 13;
	private const int WM_KEYDOWN = 0x0100;
	private const int VK_SPACE = 0x20;

	private delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll")]
	private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);

	[DllImport("user32.dll")]
	private static extern bool UnhookWindowsHookEx(IntPtr hhk);

	[DllImport("user32.dll")]
	private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll")]
	private static extern bool GetCursorPos(out POINT lpPoint);

	[DllImport("user32.dll")]
	private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

	[DllImport("user32.dll")]
	private static extern bool TranslateMessage(ref MSG lpMsg);

	[DllImport("user32.dll")]
	private static extern IntPtr DispatchMessage(ref MSG lpMsg);

	[DllImport("user32.dll")]
	private static extern void PostThreadMessage(uint idThread, uint msg, IntPtr wParam, IntPtr lParam);

	[DllImport("kernel32.dll")]
	private static extern uint GetCurrentThreadId();

	[StructLayout(LayoutKind.Sequential)]
	public struct POINT { public int X; public int Y; }

	[StructLayout(LayoutKind.Sequential)]
	public struct MSG
	{
		public IntPtr hwnd;
		public uint message;
		public IntPtr wParam;
		public IntPtr lParam;
		public uint time;
		public POINT pt;
	}

	private const uint WM_QUIT = 0x0012;

	// ── State ──────────────────────────────────────────────
	private IntPtr _hookId = IntPtr.Zero;
	private LowLevelProc? _proc;
	private Thread? _hookThread;
	private uint _hookThreadId;
	private bool _isActive = false;

	public event Action<int, int>? OnSpacePressed;
	public bool IsActive => _isActive;

	// ── Start ──────────────────────────────────────────────
	public void Start()
	{
		if (_isActive) return;

		_proc = KeyboardHookCallback;

		// ✅ รัน Hook + Message Loop ใน Thread แยก
		_hookThread = new Thread(() =>
		{
			_hookThreadId = GetCurrentThreadId();

			// Install hook ใน thread นี้
			_hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, IntPtr.Zero, 0);

			if (_hookId == IntPtr.Zero)
			{
				Console.WriteLine("Hook failed to install");
				return;
			}

			Console.WriteLine("Hook installed, running message loop...");

			// ✅ Message Loop — จำเป็นสำหรับ Low Level Hook
			while (GetMessage(out MSG msg, IntPtr.Zero, 0, 0))
			{
				TranslateMessage(ref msg);
				DispatchMessage(ref msg);
			}

			UnhookWindowsHookEx(_hookId);
			_hookId = IntPtr.Zero;

			Console.WriteLine("Hook removed");
		});

		_hookThread.IsBackground = true;
		_hookThread.SetApartmentState(ApartmentState.STA);
		_hookThread.Start();

		// รอให้ thread id พร้อม
		Thread.Sleep(100);
		_isActive = true;
	}

	// ── Stop ───────────────────────────────────────────────
	public void Stop()
	{
		if (!_isActive) return;

		// ส่ง WM_QUIT เพื่อหยุด Message Loop
		PostThreadMessage(_hookThreadId, WM_QUIT, IntPtr.Zero, IntPtr.Zero);

		_hookThread?.Join(1000);
		_isActive = false;
	}

	// ── Callback ───────────────────────────────────────────
	private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
	{
		if (nCode >= 0 && wParam == WM_KEYDOWN)
		{
			int vkCode = Marshal.ReadInt32(lParam);

			if (vkCode == VK_SPACE)
			{
				GetCursorPos(out POINT p);
				//POINT p = GetRelativeCoordinates();
				Console.WriteLine($"Space pressed at X={p.X}, Y={p.Y}");
				OnSpacePressed?.Invoke(p.X, p.Y);
			}
		}

		return CallNextHookEx(_hookId, nCode, wParam, lParam);
	}


	#region MouseTracker
	  
	[DllImport("user32.dll")]
	static extern IntPtr WindowFromPoint(POINT point);

	[DllImport("user32.dll")]
	public static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

	public POINT GetRelativeCoordinates()
	{
		// ดึงพิกัดปัจจุบัน (ได้เป็น POINT ของเราเอง)
		POINT screenPoint = GetCurrentCursorPosition();

		// หา Handle ของหน้าต่างที่เมาส์ชี้อยู่
		IntPtr hWnd = WindowFromPoint(screenPoint);

		if (hWnd != IntPtr.Zero)
		{
			// ใช้ POINT ตัวเดิมในการแปลงค่า
			POINT clientPoint = screenPoint;
			ScreenToClient(hWnd, ref clientPoint);

			// ตอนนี้ clientPoint.X และ Y จะกลายเป็นพิกัดเทียบกับหน้าต่างนั้นๆ แล้ว
			Console.WriteLine($"Window Handle: {hWnd}");
			Console.WriteLine($"Relative X: {clientPoint.X}, Y: {clientPoint.Y}");

			return clientPoint;
		}
		else
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"NOT FOUND WINDOWS");
			Console.ResetColor();
			return screenPoint;
		}
		
	}

	private POINT GetCurrentCursorPosition()
	{
		GetCursorPos(out POINT p);
		return p;
	}
	#endregion

	public void Dispose() => Stop();
}