// NuGet: OpenCvSharp4, OpenCvSharp4.runtime.win
using OpenCvSharp;

public class TemplateMatcher
{
	/// <summary>
	/// หาตำแหน่งของรูปเล็ก (template) ในรูปใหญ่ (screenshot)
	/// คืนค่า null ถ้าหาไม่เจอ
	/// </summary>
	public static MatchResult? FindTemplate(
		byte[] screenshotBytes,
		byte[] templateBytes,
		double threshold = 0.85,        // ความมั่นใจขั้นต่ำ 0-1
		bool tryMultiScale = true)      // ลองหลาย scale กรณี DPI ต่างกัน
	{
		using var screenshot = Cv2.ImDecode(screenshotBytes, ImreadModes.Color);
		using var template = Cv2.ImDecode(templateBytes, ImreadModes.Color);

		if (screenshot.Empty() || template.Empty())
			throw new ArgumentException("ไม่สามารถโหลดภาพได้");

		if (!tryMultiScale)
			return MatchSingle(screenshot, template, threshold);

		// ลอง scale 0.8 - 1.2 กรณี DPI หรือ resolution ต่างกันนิดหน่อย
		double[] scales = [1.0, 0.9, 1.1, 0.95, 1.05, 0.85, 1.15, 0.8, 1.2];

		MatchResult? best = null;

		foreach (var scale in scales)
		{
			using var resized = new Mat();
			int newW = (int)(template.Width * scale);
			int newH = (int)(template.Height * scale);

			if (newW < 10 || newH < 10) continue;
			if (newW > screenshot.Width || newH > screenshot.Height) continue;

			Cv2.Resize(template, resized, new Size(newW, newH),
				interpolation: InterpolationFlags.Lanczos4);

			var result = MatchSingle(screenshot, resized, threshold);
			if (result != null && (best == null || result.Confidence > best.Confidence))
			{
				result.Scale = scale;
				best = result;
			}

			// เจอ confidence สูงมากพอแล้ว หยุดได้
			if (best?.Confidence >= 0.97) break;
		}

		return best;
	}

	private static MatchResult? MatchSingle(Mat screenshot, Mat template, double threshold)
	{
		// ถ้า template ใหญ่กว่า screenshot
		if (template.Width > screenshot.Width || template.Height > screenshot.Height)
			return null;

		using var result = new Mat();

		// TM_CCOEFF_NORMED ทนต่อความแตกต่างของ brightness ได้ดีที่สุด
		Cv2.MatchTemplate(screenshot, template, result, TemplateMatchModes.CCoeffNormed);
		Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out Point maxLoc);

		if (maxVal < threshold)
			return null;

		return new MatchResult
		{
			X = maxLoc.X,
			Y = maxLoc.Y,
			Width = template.Width,
			Height = template.Height,
			CenterX = maxLoc.X + template.Width / 2,
			CenterY = maxLoc.Y + template.Height / 2,
			Confidence = maxVal,
			Scale = 1.0
		};
	}

	/// <summary>
	/// หาทุกตำแหน่งที่ตรง (กรณีรูปซ้ำหลายจุดบนหน้าจอ)
	/// </summary>
	public static List<MatchResult> FindAllTemplates(
		byte[] screenshotBytes,
		byte[] templateBytes,
		double threshold = 0.85)
	{
		using var screenshot = Cv2.ImDecode(screenshotBytes, ImreadModes.Color);
		using var template = Cv2.ImDecode(templateBytes, ImreadModes.Color);
		using var resultMat = new Mat();

		Cv2.MatchTemplate(screenshot, template, resultMat, TemplateMatchModes.CCoeffNormed);

		var matches = new List<MatchResult>();

		// วน suppress ตำแหน่งซ้ำซ้อน (Non-Maximum Suppression แบบง่าย)
		while (true)
		{
			Cv2.MinMaxLoc(resultMat, out _, out double maxVal, out _, out Point maxLoc);

			if (maxVal < threshold) break;

			matches.Add(new MatchResult
			{
				X = maxLoc.X,
				Y = maxLoc.Y,
				Width = template.Width,
				Height = template.Height,
				CenterX = maxLoc.X + template.Width / 2,
				CenterY = maxLoc.Y + template.Height / 2,
				Confidence = maxVal,
				Scale = 1.0
			});

			// ลบพื้นที่รอบตำแหน่งที่เจอแล้วออก ไม่ให้ซ้ำ
			var roi = new Rect(
				Math.Max(0, maxLoc.X - template.Width / 2),
				Math.Max(0, maxLoc.Y - template.Height / 2),
				Math.Min(template.Width, resultMat.Width - maxLoc.X),
				Math.Min(template.Height, resultMat.Height - maxLoc.Y));

			resultMat[roi].SetTo(Scalar.All(0));
		}

		return matches;
	}
}

public class MatchResult
{
	public int X { get; set; }          // มุมบนซ้าย
	public int Y { get; set; }
	public int Width { get; set; }
	public int Height { get; set; }
	public int CenterX { get; set; }    // จุดกึ่งกลาง (ใช้ click)
	public int CenterY { get; set; }
	public double Confidence { get; set; } // 0-1
	public double Scale { get; set; }   // scale ที่เจอ (1.0 = ขนาดเดิม)
}