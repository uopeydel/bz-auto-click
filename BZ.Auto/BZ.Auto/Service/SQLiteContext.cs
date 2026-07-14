using BZ.Auto.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace BZ.Auto.Service
{
	public static class SQLiteContext
	{
		public static async Task<List<string>> DropTable(string tableToDrop)
		{
			using var connection = new SqliteConnection("Data Source=app.db");
			await connection.OpenAsync();

			// 🔥 DROP TABLE
			string dropTableQuery = $"DROP TABLE IF EXISTS {tableToDrop}";
			var command1 = connection.CreateCommand();
			command1.CommandText = dropTableQuery;
			command1.ExecuteNonQuery();

			return await ListTableName();
		}

		public static async Task<List<string>> ListTableName()
		{
			var list = new List<string>();

			using var connection = new SqliteConnection("Data Source=app.db");
			await connection.OpenAsync();
			var sqlCommand = "SELECT name FROM sqlite_schema WHERE type='table' ORDER BY name;";
			var command = connection.CreateCommand();
			command.CommandText = sqlCommand;

			using var reader = await command.ExecuteReaderAsync();

			while (await reader.ReadAsync())
			{
				var name = reader.GetString(0);
				list.Add(name);
			}

			list = list.Where(w => w != "sqlite_sequence").ToList();
			return list;
		}
		public static async Task InsertImageStep(List<ImageStepModel> models, string tableNamee = "ImageSteps")
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
    NextStepEveryToStep INTEGER,
    NextStepEveryRound INTEGER,
    Active INTEGER,
	IsClick  INTEGER,
    FourceStopLoop INTEGER
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
            NextStepEveryToStep,
            NextStepEveryRound,
            Active,
			IsClick,
			FourceStopLoop 
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
            @NextStepEveryToStep,
            @NextStepEveryRound,
            @Active,
			@IsClick,
			@FourceStopLoop  
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
				command.Parameters.AddWithValue("@NextStepEveryToStep", models[i].NextStepEveryToStep);
				command.Parameters.AddWithValue("@NextStepEveryRound", models[i].NextStepEveryRound);
				command.Parameters.AddWithValue("@Active", models[i].Active ? 1 : 0);
				command.Parameters.AddWithValue("@IsClick", models[i].IsClick ? 1 : 0);

				command.Parameters.AddWithValue("@FourceStopLoop", models[i].FourceStopLoop ? 1 : 0);

				await command.ExecuteNonQueryAsync();
			}
		}

		public static async Task<List<ImageStepModel>> GetAll(string tableNamee = "ImageSteps")
		{
			try
			{
				var list = new List<ImageStepModel>();

				using var connection = new SqliteConnection("Data Source=app.db");
				await connection.OpenAsync();

				var command = connection.CreateCommand();

				//				command.CommandText = $@"
				//SELECT  
				//    t.BaseImage,
				//    t.CurrentFromScreen,
				//    t.TopLeftX,
				//    t.TopLeftY,
				//    t.BotRightX,
				//    t.BotRightY,
				//    t.Interval,
				//    t.AfterClick,
				//    t.ReCheckInterval,
				//    t.NextStepFound,
				//    t.NextStepNotFound, 
				//    t.NextStepEveryToStep,
				//    t.NextStepEveryRound,
				//    t.Active,
				//	t.IsClick,
				//    t.FourceStopLoop
				//FROM {tableNamee} t ";

				command.CommandText = $"SELECT * FROM {tableNamee}";

				using var reader = await command.ExecuteReaderAsync();

				while (await reader.ReadAsync())
				{
					list.Add(new ImageStepModel
					{
						BaseImage = reader.GetStringOrDefault("BaseImage"),
						CurrentFromScreenSmallToCompare = reader.GetStringOrDefault("CurrentFromScreen"),
						TopLeftX = reader.GetInt32OrDefault("TopLeftX"),
						TopLeftY = reader.GetInt32OrDefault("TopLeftY"),
						BotRightX = reader.GetInt32OrDefault("BotRightX"),
						BotRightY = reader.GetInt32OrDefault("BotRightY"),
						Interval = reader.GetInt32OrDefault("Interval"),
						AfterClick = reader.GetInt32OrDefault("AfterClick"),
						ReCheckInterval = reader.GetInt32OrDefault("ReCheckInterval"),
						NextStepFound = reader.GetInt32OrDefault("NextStepFound"),
						NextStepNotFound = reader.GetInt32OrDefault("NextStepNotFound"),
						NextStepEveryToStep = reader.GetInt32OrDefault("NextStepEveryToStep"),
						NextStepEveryRound = reader.GetInt32OrDefault("NextStepEveryRound"),
						Active = reader.GetBoolOrDefault("Active"),
						IsClick = reader.GetBoolOrDefault("IsClick"),
						FourceStopLoop = reader.GetBoolOrDefault("FourceStopLoop")
					});
				}

				return list;
			}
			catch (Exception ex)
			{
				return new List<ImageStepModel> { };
			}
		}


	}

	public static class SqliteDataReaderExtension
	{
		public static bool HasColumn(this SqliteDataReader reader, string columnName)
		{
			for (int i = 0; i < reader.FieldCount; i++)
			{
				if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
					return true;
			}

			return false;
		}

		public static string GetStringOrDefault(this SqliteDataReader reader, string columnName, string defaultValue = "")
		{
			if (!reader.HasColumn(columnName))
				return defaultValue;

			int index = reader.GetOrdinal(columnName);

			if (reader.IsDBNull(index))
				return defaultValue;

			return reader.GetString(index);
		}

		public static int GetInt32OrDefault(this SqliteDataReader reader, string columnName, int defaultValue = 0)
		{
			if (!reader.HasColumn(columnName))
				return defaultValue;

			int index = reader.GetOrdinal(columnName);

			if (reader.IsDBNull(index))
				return defaultValue;

			return reader.GetInt32(index);
		}

		public static bool GetBoolOrDefault(this SqliteDataReader reader, string columnName, bool defaultValue = false)
		{
			if (!reader.HasColumn(columnName))
				return defaultValue;

			int index = reader.GetOrdinal(columnName);

			if (reader.IsDBNull(index))
				return defaultValue;

			return reader.GetInt32(index) == 1;
		}
	}
}
