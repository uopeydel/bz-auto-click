using System.Text;
using System.Text.Json;

namespace BZ.Auto.Service
{
	public class DiscordNotificationService
	{
		public static async Task SendNotification(string webhookUrl,string name ,int RoundRunning)
		{
			using var client = new HttpClient();

			// สร้างโครงสร้างข้อมูลสำหรับ Embed (เลียนแบบหน้าตา Spidey Hook)
			var payload = new
			{
				username = "Force stop loop",
				avatar_url = "https://devblogs.microsoft.com/aspnet/wp-content/uploads/sites/16/2019/04/BrandBlazor_nohalo_1000x.png", // ใส่ URL รูปโปรไฟล์ของ Bot
				content = $"@everyone {name} บอทหยุดแล้ว", // ข้อความนอกกรอบ
				embeds = new[]
				{
					new
					{
						title = $"🚀 Bot ของคุณ {name} หยุดการทำงานแล้ว",
						description = "ติด Fource Stop Loop",
						color = 15158332,// 3447003, // รหัสสี Decimal (สีฟ้า)
						fields = new[]
						{
							new { name = "Program", value = "GBF Auto", inline = true },
							new { name = "Status", value = "Stop ✅", inline = true },
							new { name = "RoundRunning", value = $"{RoundRunning}", inline = true }
						},
						footer = new { text = "อย่าลืมไปกรอก Capcha" },
						timestamp = DateTime.UtcNow // ใส่เวลาปัจจุบัน
					}
				}
			};

			// แปลง Object เป็น JSON
			var json = JsonSerializer.Serialize(payload);
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			// ส่ง Request ไปยัง Discord
			var response = await client.PostAsync(webhookUrl, content);

			if (response.IsSuccessStatusCode)
			{
				Console.WriteLine("Notification sent successfully!");
			}
			else
			{
				Console.WriteLine($"Error: {response.StatusCode}");
			}
		}
	}
}
