using Microsoft.Data.Sqlite;
using System.Globalization;
using DBItem;
namespace DataService
{
	public class DataServices
	{
		public List<Habit> dataRecords = new List<Habit>();
		public bool recordsPresent = true;

		/// <summary>
		/// General methode used to recieve user input.
		/// </summary>
		/// <param name="messageToUser">Message to write to console</param>
		/// <returns>USer input as a string value</returns>
		public string UserUnput(string messageToUser)
		{
			Console.WriteLine(messageToUser);
			return Console.ReadLine();
		}

		/// <summary>
		/// Validates user date input and saves to variable if succesful
		/// </summary>
		/// <param name="messageToUser">Prompt displayed to user</param>
		/// <returns>Correct date value or null</returns>
		public string? DateInput(string messageToUser)
		{
			Console.Write(messageToUser);
			string? date = Console.ReadLine();

			if (date == "0")
				return null;

			while (!DateTime.TryParseExact(date, "dd-MM-yyyy", new CultureInfo("en-Us"), DateTimeStyles.None, out _))
			{
				Console.Write("\n Invalid date. Enter Date with format of 'dd-MM-yyyy', or type '0 to exit'\n");
				date = Console.ReadLine();

				if (date == "0")
					return null;
			}
			return date;
		}

		/// <summary>
		/// Creates an instance of the HabbitLogger Database if not present
		/// </summary>
		public void CreateDB()
		{
			var connectionString = @"Data Source = HabbitLogger.db";

			try
			{
				using (var connection = new SqliteConnection(connectionString))
				{
					connection.Open();
					var tableCmd = connection.CreateCommand();
					tableCmd.CommandText = @"CREATE TABLE IF NOT EXISTS habbit_logger (
											Id INTEGER PRIMARY KEY AUTOINCREMENT,
											Habit Text,
											Date Text,
											Quantity Integer						
											)";
					tableCmd.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				Console.Clear();
				Console.WriteLine(ex.Message);
				Console.WriteLine("Type 0 to exit");
			}
		}

		/// <summary>
		/// Inserts data into the table Database 
		/// </summary>
		public void InsertRecord()
		{
			Console.Clear();
			string habbit = UserUnput("\nEnter the habit name");
			string? date = DateInput("\nPlease enter the date ('dd-MM-yyyy'), or Type 0 to exit \n");

			if (date == null)
				return;

			string Qty = UserUnput("\nEnter the Quantity");

			var connectionString = @"Data Source = HabbitLogger.db";
			try
			{
				using (var connection = new SqliteConnection(connectionString))
				{
					connection.Open();

					var dbCommand = connection.CreateCommand();
					dbCommand.CommandText = $"INSERT INTO habbit_logger(habit, Date, Quantity) VALUES('{habbit}','{date}',{Qty})";
					dbCommand.ExecuteNonQuery();
					connection.Close();
				}
			}
			catch
			{
				Console.WriteLine("\nFailed to insert Habbit\n");


			}
		}

		/// <summary>
		/// Displays a list of all records
		/// </summary>
		public void GetAllRecords()
		{
			Console.Clear();
			dataRecords.Clear();

			var connectionString = @"Data Source = HabbitLogger.db";

			using (var conection = new SqliteConnection(connectionString))
			{
				conection.Open();
				var dbCommand = conection.CreateCommand();
				dbCommand.CommandText = $"SELECT * FROM habbit_logger";

				SqliteDataReader reader = dbCommand.ExecuteReader();

				if (reader.HasRows)
				{
					recordsPresent = true;
					while (reader.Read())
					{
						dataRecords.Add(new Habit()
						{
							Id = reader.GetInt32(0),
							Habbit = reader.GetString(1),
							Date = DateTime.ParseExact(reader.GetString(2), "dd-MM-yyyy", new CultureInfo("en-US")),
							Quantity = reader.GetInt32(3)
						});
					}
				}
				else
				{
					Console.WriteLine("No Rows Found");
					recordsPresent = false;
					return;
				}

				conection.Close();

				Console.WriteLine("\nRecords");
				Console.WriteLine("________________________________________________________________________________\n");
				foreach (Habit item in dataRecords)
				{
					Console.WriteLine($"{item.Id} - {item.Habbit} - {item.Date.ToString()} - Quantity: {item.Quantity}");

				}
				Console.WriteLine("\n________________________________________________________________________________\n");
			}
		}

		public void DisplayAllRecords()
		{
			GetAllRecords();
			Console.WriteLine("Type 0 to return to the main menu");
			if (Console.ReadLine() == "0")
				return;
		}


		/// <summary>
		/// Upadates a record as per record ID
		/// </summary>
		public void UpdateRecords()
		{
			Console.Clear();
			GetAllRecords();

			if (recordsPresent == false)
			{
				Console.WriteLine("Type 0 to return to the main menu");
				if (Console.ReadLine() == "0")
					return;
			}
			else
			{
				var recordToUpdate = UserUnput("\nEnter the ID of the record you would like to update, Type 0 to exit");
				int id = Convert.ToInt32(recordToUpdate);

				if (id == 0)
					return;

				var connectionString = @"Data Source = HabbitLogger.db";

				try
				{
					using (var connection = new SqliteConnection(connectionString))
					{
						connection.Open();
						var command = connection.CreateCommand();
						command.CommandText = $"SELECT EXISTS(SELECT 1 FROM  habbit_logger WHERE id = {recordToUpdate})";
						int? checkIfPresent = Convert.ToInt32(command.ExecuteScalar());

						if (checkIfPresent == 0)
						{
							Console.WriteLine($"\nRecord with ID of - {recordToUpdate} does not exist ");
							connection.Close();
							UpdateRecords();
						}

						string habbit = UserUnput("\nEnter the habit name");
						string? date = DateInput("\nPlease enter the date ('dd-MM-yyyy'), or Type 0 to exit \n");
						string Qty = UserUnput("\nEnter the Quantity");

						var updteCmd = connection.CreateCommand();
						updteCmd.CommandText = $"UPDATE habbit_logger SET habit = '{habbit}' , Date = '{date}', Quantity = '{Qty}' WHERE Id = {recordToUpdate}";
						updteCmd.ExecuteNonQuery();

						connection.Close();
					}

				}
				catch
				{
					Console.WriteLine("The Record was not updated");

				}


			}

		}

		/// <summary>
		/// Deletes a Record based on ID
		/// </summary>
		public void DeleteRecord()
		{
			Console.Clear();
			GetAllRecords();

			if (recordsPresent == false)
			{
				Console.WriteLine("Type 0 to return to the main menu");
				if (Console.ReadLine() == "0")
					return;
			}
			else
			{
				var recordToDelete = UserUnput("\nEnter the ID of the record you would like to remove, or type 0 to exit\n");
				int id = Convert.ToInt32(recordToDelete);

				if (id == 0)
					return;

				var connectionString = @"Data Source = HabbitLogger.db";

				try
				{
					using (var connection = new SqliteConnection(connectionString))
					{
						connection.Open();
						var deleteCmd = connection.CreateCommand();
						deleteCmd.CommandText = $"DELETE FROM habbit_logger WHERE Id ={recordToDelete} ";
						deleteCmd.ExecuteNonQuery();
						Console.WriteLine($"\nRecord with ID of {recordToDelete} was succesfully deleted\n");
						connection.Close();
					}
				}
				catch
				{
					Console.WriteLine("No Record has been deleted please enter a valid ID");

				}
			}
		}

		/// <summary>
		/// Generates a simple report on the selected habbit
		/// </summary>
		public void GenerateReport()
		{


			Console.Clear();
			GetAllRecords();
			string habitName = UserUnput("\nEnter the name of the Habit to generate the report");

			int? sum = 0;
			int? numberOfEntries = 0;


			foreach (Habit entry in dataRecords)
			{
				if (entry.Habbit == habitName)
				{
					numberOfEntries++;
					sum = sum + entry.Quantity;
				}
			}

			float? average = sum / numberOfEntries;


			if (numberOfEntries == 0)
			{
				Console.WriteLine("\nEnter a valid habbit\n");
				GenerateReport();
			}
			else
			{
				Console.Clear();
				Console.WriteLine($"\nReport of {habitName}");
				Console.WriteLine("______________________________________________________________________________________________________\n");
				Console.WriteLine($"Habit Name : {habitName}");
				Console.WriteLine($"Number of Entries : {numberOfEntries}");
				Console.WriteLine($"Total Sum of Entries : {sum}");
				Console.WriteLine($"Average vaue per entry: {average}");
				Console.WriteLine("\n\nEntries: \n");
				Console.WriteLine("________________\n");

				foreach (Habit item in dataRecords)
				{
					if (item.Habbit == habitName)
						Console.WriteLine($"{item.Habbit} - {item.Date.ToString()} - Quantity: {item.Quantity}");
				}



				Console.WriteLine("\n_____________________________________________________________________________________________________\n");


				Console.WriteLine("Type 0 to return to the main menu");
				if (Console.ReadLine() == "0")
					return;

			}


		}

	}
}
