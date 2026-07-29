using System.Runtime.InteropServices;

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
			await Task.Delay(GenerateRandomMillisecond(0.10, 0.18)); 
			MouseEvent(MouseEventFlags.LeftUp);

			//  SystemSounds.Beep.Play();
			//(new System.Media.SoundPlayer(@"D:\Code\Git\AutoMouse\AutoCursorMoveStep\AutoCursorMoveStep\sound\click.wav")).Play(); ;
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

		/// <summary>
		/// Generate random milliseconds from a second range.
		/// </summary>
		/// <param name="startSecond">
		/// Start time in seconds.
		/// Example: 0.10
		/// </param>
		/// <param name="endSecond">
		/// End time in seconds.
		/// Example: 0.18
		/// </param>
		/// <param name="stepSecond">
		/// Increment step in seconds.
		/// Example:
		/// 0.01 = 10 ms
		/// 0.05 = 50 ms
		/// 0.10 = 100 ms
		/// </param>
		/// <returns>Random milliseconds.</returns>
		/// <example>
		/// GenerateRandomMillisecond(0.10, 0.18, 0.01)
		/// Possible results:
		/// 100, 110, 120, ..., 180
		///
		/// GenerateRandomMillisecond(0.08, 0.25, 0.01)
		/// Possible results:
		/// 80, 90, 100, ..., 250
		/// </example>
		public static int GenerateRandomMillisecond(
			double startSecond,
			double endSecond,
			double stepSecond = 0.01)
		{
			if (startSecond > endSecond)
				throw new ArgumentException("startSecond must be less than or equal to endSecond.");

			if (stepSecond <= 0)
				throw new ArgumentException("stepSecond must be greater than zero.");

			int count = (int)Math.Round((endSecond - startSecond) / stepSecond);

			int index = Random.Shared.Next(count + 1);

			double second = startSecond + (index * stepSecond);

			return (int)Math.Round(second * 1000);
		}


	}
}
