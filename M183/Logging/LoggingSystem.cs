using M183.Models;
using System;

namespace M183.Logging
{
    public class LoggingSystem
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "AuditTrail.txt");

        static LoggingSystem()
        {
            // Ensure the Logs directory exists
            var logDirectory = Path.GetDirectoryName(LogFilePath);
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }

        public static void Log(LoggingModel model)
        {
            try
            {
                model.Id++;

                var logMessage = $"{model.Id} - Time {model.Timestamp} - User: {model.Username} - IsAdmin: {model.IsAdmin} - Action: {model.Action} - Details: {model.Detail}";
                File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Handle logging errors (optional)
                Console.Error.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }

    }
}
