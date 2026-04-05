using BZ.Auto.Models;
using Microsoft.Data.Sqlite;
using System;

namespace BZ.Auto.Service
{
	public static class SQLiteContext
	{
		public static async Task InsertImageStep(List<ImageStepModel> models , string tableNamee = "ImageSteps")
		{ 
			using var connection = new SqliteConnection("Data Source=app.db");
			await connection.OpenAsync();

			// 🔥 DROP TABLE
			string dropTableQuery = $"DROP TABLE IF EXISTS {tableNamee}";
			var command1 = connection.CreateCommand();
			command1.CommandText = dropTableQuery;
			command1.ExecuteNonQuery();


			// 🧱 CREATE TABLE
			string createTableQuery = $@"
    
   CREATE TABLE IF NOT EXISTS {tableNamee} (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    BaseImage TEXT,
    CurrentFromScreen TEXT,
    TopLeftX INTEGER,
    TopLeftY INTEGER,
    BotRightX INTEGER,
    BotRightY INTEGER,
    Interval INTEGER,
	AfterClick INTEGER,
    ReCheckInterval INTEGER,
    NextStepFound INTEGER,
    NextStepNotFound INTEGER,
    Active INTEGER
);";

			var command2 = connection.CreateCommand();
			command2.CommandText = createTableQuery;
			command2.ExecuteNonQuery();

			for (int i = 0; i < models.Count; i++)
			{

				var command = connection.CreateCommand();
				command.CommandText = $@"
        INSERT INTO {tableNamee} (
            BaseImage,
            CurrentFromScreen,
            TopLeftX,
            TopLeftY,
            BotRightX,
            BotRightY,
            Interval,
            AfterClick,
            ReCheckInterval,
            NextStepFound,
            NextStepNotFound,
            Active
        )
        VALUES (
            @BaseImage,
            @CurrentFromScreen,
            @TopLeftX,
            @TopLeftY,
            @BotRightX,
            @BotRightY,
            @Interval,
            @AfterClick,
            @ReCheckInterval,
            @NextStepFound,
            @NextStepNotFound,
            @Active
        );
    ";

				command.Parameters.AddWithValue("@BaseImage", models[i].BaseImage ?? "");
				command.Parameters.AddWithValue("@CurrentFromScreen", models[i].CurrentFromScreenSmallToCompare ?? "");
				command.Parameters.AddWithValue("@TopLeftX", models[i].TopLeftX);
				command.Parameters.AddWithValue("@TopLeftY", models[i].TopLeftY);
				command.Parameters.AddWithValue("@BotRightX", models[i].BotRightX);
				command.Parameters.AddWithValue("@BotRightY", models[i].BotRightY);
				command.Parameters.AddWithValue("@Interval", models[i].Interval);
				command.Parameters.AddWithValue("@AfterClick", models[i].AfterClick);
				command.Parameters.AddWithValue("@ReCheckInterval", models[i].ReCheckInterval);
				command.Parameters.AddWithValue("@NextStepFound", models[i].NextStepFound);
				command.Parameters.AddWithValue("@NextStepNotFound", models[i].NextStepNotFound);
				command.Parameters.AddWithValue("@Active", models[i].Active ? 1 : 0);

				await command.ExecuteNonQueryAsync();
			}
		}

		public static async Task<List<ImageStepModel>> GetAll(string tableNamee = "ImageSteps")
		{ 
			var list = new List<ImageStepModel>();

			using var connection = new SqliteConnection("Data Source=app.db");
			await connection.OpenAsync();

			var command = connection.CreateCommand();
			command.CommandText = $@"
			SELECT  
				BaseImage,
				CurrentFromScreen,
				TopLeftX,
				TopLeftY,
				BotRightX,
				BotRightY,
				Interval,
				AfterClick,
				ReCheckInterval,
				NextStepFound,
				NextStepNotFound,
				Active 
			FROM {tableNamee}";

			using var reader = await command.ExecuteReaderAsync();

			while (await reader.ReadAsync())
			{

				// ของเดิม (ผิด) -> ของใหม่ (ถูก)
				var BaseImage = reader.GetString(0); // Index 0 = BaseImage
				var CurrentFromScreenSmallToCompare = reader.GetString(1); // Index 1 = CurrentFromScreen
				var TopLeftX = reader.GetInt32(2);
				var TopLeftY = reader.GetInt32(3);
				var BotRightX = reader.GetInt32(4);
				var BotRightY = reader.GetInt32(5);
				var Interval = reader.GetInt32(6);
				var AfterClick = reader.GetInt32(7);
				var ReCheckInterval = reader.GetInt32(8);
				var NextStepFound = reader.GetInt32(9);
				var NextStepNotFound = reader.GetInt32(10);
				var Active = reader.GetInt32(11) == 1; // อ่านค่าจริงจาก DB (Index 11 คือ Active)

				list.Add(new ImageStepModel
				{ 
					BaseImage = BaseImage,
					CurrentFromScreenSmallToCompare = CurrentFromScreenSmallToCompare,
					TopLeftX = TopLeftX,
					TopLeftY = TopLeftY,
					BotRightX = BotRightX,
					BotRightY = BotRightY,
					Interval = Interval,
					AfterClick = AfterClick,
					ReCheckInterval = ReCheckInterval,
					NextStepFound = NextStepFound,
					NextStepNotFound = NextStepNotFound,
					Active = Active
				});
			}

			return list;
		}


	}
}
