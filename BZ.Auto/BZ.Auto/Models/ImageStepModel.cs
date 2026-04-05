namespace BZ.Auto.Models
{
	public class ImageStepModel
	{
		public string BaseImage { get; set; }            // URL หรือ base64
		public string CurrentFromScreenSmallToCompare { get; set; }    // URL หรือ base64

		public int TopLeftX { get; set; }
		public int TopLeftY { get; set; }
		public int BotRightX { get; set; }
		public int BotRightY { get; set; }

		public int Interval { get; set; }
		public int AfterClick { get; set; }
		public int ReCheckInterval { get; set; }

		public int NextStepFound { get; set; }
		public int NextStepNotFound { get; set; }

		public bool Active { get; set; }



		public bool IsFound { get; set; }= false;
		public int FoundTopLeftX { get; set; }
		public int FoundTopLeftY { get; set; }
		public int FoundBotRightX { get; set; }
		public int FoundBotRightY { get; set; }

		public string CaptureWholeScreenImage { get; set; }	
	}
}
