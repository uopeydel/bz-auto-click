using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Shapes;

namespace BZ.Auto.Service
{
	public static class MouseOperationsService
	{
		[Flags]
		public enum MouseEventFlags
		{
			LeftDown = 0x00000002,
			LeftUp = 0x00000004,
			MiddleDown = 0x00000020,
			MiddleUp = 0x00000040,
			Move = 0x00000001,
			Absolute = 0x00008000,
			RightDown = 0x00000008,
			RightUp = 0x00000010,
			MOUSEEVENTF_WHEEL = 0x0800
		}

		[DllImport("user32.dll", EntryPoint = "SetCursorPos")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetCursorPos(int x, int y);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetCursorPos(out MousePoint lpMousePoint);

		[DllImport("user32.dll")]
		private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

		public static void SetCursorPosition(int x, int y)
		{
			SetCursorPos(x, y);
		}

		public static void SetCursorPosition(MousePoint point)
		{
			SetCursorPos(point.X, point.Y);
		}

		public static MousePoint GetCursorPosition()
		{
			MousePoint currentMousePoint;
			var gotPoint = GetCursorPos(out currentMousePoint);
			if (!gotPoint) { currentMousePoint = new MousePoint(0, 0); }
			return currentMousePoint;
		}

		public static void MouseEvent(MouseEventFlags value)
		{
			MousePoint position = GetCursorPosition();

			mouse_event
				((int)value,
				 position.X,
				 position.Y,
				 0,
				 0)
				;
		}
		public static void MouseEvent(MouseEventFlags value, int x, int y)
		{
			mouse_event
				((int)value,
				 x,
				 y,
				 0,
				 0)
				;
		}


		[StructLayout(LayoutKind.Sequential)]
		public struct MousePoint
		{
			public int X;
			public int Y;

			public MousePoint(int x, int y)
			{
				X = x;
				Y = y;
			}
		}

		public static async Task LeftMouseClick()
		{
			MouseEvent(MouseEventFlags.LeftDown);
			await Task.Delay(GenerateRandomMillisecond(1, 2));
			//  SystemSounds.Beep.Play();
			//(new System.Media.SoundPlayer(@"D:\Code\Git\AutoMouse\AutoCursorMoveStep\AutoCursorMoveStep\sound\click.wav")).Play(); ;
			MouseEvent(MouseEventFlags.LeftUp);
		}


		
		public static string filePath { get; set; }=  "";
		public static async Task LeftMouseClickBackGround(int x , int y)
		{
			Process[] processes = Process.GetProcessesByName("chrome");
			 
			await BackgroundMouse.ClickAtBackground(processes[0].MainWindowHandle, x, y);

			//string fileName = "favicon.png";
			//string filePath = Path.Combine(AppContext.BaseDirectory, "wwwroot", fileName);

			// ตรวจสอบก่อนเพื่อความชัวร์
			if (File.Exists(filePath))
			{

				#region DrawSticker

				//_ = DirectScreenOverlay.DrawStickerAsync(filePath, x, y, 3);

				// ต้องรัน Form ใน Thread ใหม่ที่เป็น STA
				Thread thread = new Thread(() =>
				{
					Application.Run(new StickerOverlay(filePath, x, y, 3));
				});

				thread.SetApartmentState(ApartmentState.STA); // สำคัญมากสำหรับ WinForms
				thread.Start();
				#endregion
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine(filePath + " Exist");
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(filePath + " Not Exist");
			}
			Console.ResetColor();

			//MouseEvent(MouseEventFlags.LeftDown);
			//await Task.Delay(GenerateRandomMillisecond(1, 2)); 
			//MouseEvent(MouseEventFlags.LeftUp);
		}

		public static void MouseEventWheelDown()
		{
			return;
			MousePoint position = GetCursorPosition();

			mouse_event
				((int)MouseEventFlags.MOUSEEVENTF_WHEEL,
				 0,
				 0,
				 -800,
				 0)
				;
		}

		public static int GenerateRandomMillisecond(int start, int end)
		{
			if (start > end)
			{
				throw new ArgumentException("Start value must be less than or equal to end value.");
			}

			Random random = new Random();
			double randomRange = (double)(end - start);
			double randomSecond = 0.0;

			while (randomSecond % 0.3 != 0)
			{
				randomSecond = random.NextDouble() * randomRange;
				randomSecond += start;
			}

			int randomMillisecond = (int)randomSecond * 1000;
			return randomMillisecond;
		}


	}
}
