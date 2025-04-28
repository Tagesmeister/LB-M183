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

                var logMessage = $"{model.Id}: {model.Timestamp} - User: {model.Username} - Action: {model.Action} - Status: {model.Status} - Details: {model.Detail} - Input: {model.Input} - IP: {model.IPAddress} - Error: {model.ErrorMessage}";
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
