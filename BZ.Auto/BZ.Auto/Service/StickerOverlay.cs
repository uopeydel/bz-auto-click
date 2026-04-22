using System.Runtime.InteropServices;

public class StickerOverlay : Form
{
	// Windows API สำหรับตั้งค่าให้คลิกทะลุ (Click-through)
	[DllImport("user32.dll")]
	static extern int GetWindowLong(IntPtr hWnd, int nIndex);
	[DllImport("user32.dll")]
	static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

	public StickerOverlay(string imagePath, int x, int y, int durationSeconds)
	{
		this.FormBorderStyle = FormBorderStyle.None;
		this.ShowInTaskbar = false;
		this.TopMost = true;
		this.StartPosition = FormStartPosition.Manual;
		this.Location = new Point(x, y);

		// โหลดรูปภาพ
		Image img = Image.FromFile(imagePath);
		this.Size = img.Size;
		this.BackColor = Color.Fuchsia; // สีที่จะใช้เป็นกุญแจความโปร่งใส
		this.TransparencyKey = Color.Fuchsia;

		PictureBox pb = new PictureBox
		{
			Image = img,
			Dock = DockStyle.Fill,
			SizeMode = PictureBoxSizeMode.AutoSize
		};
		this.Controls.Add(pb);

		// ตั้งค่าให้โปร่งใสและคลิกทะลุได้ (WS_EX_LAYERED | WS_EX_TRANSPARENT)
		int initialStyle = GetWindowLong(this.Handle, -20);
		SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);

		// ตัวจับเวลาปิดสติกเกอร์
		System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
		timer.Interval = durationSeconds * 1000;
		timer.Tick += (s, e) => { this.Close(); };
		timer.Start();
	}
}