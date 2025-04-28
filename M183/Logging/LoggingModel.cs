namespace M183.Logging
{
    public class LoggingModel
    {
        private static int _id = 0;

        public int? Id { get; set; } = _id++; // Unique ID for the log entry
        public int UserId { get; set; } = 0; // ID of the user performing the action
        public string Username { get; set; } = string.Empty; // Username of the user
        public string Action { get; set; } = string.Empty; // Action performed (e.g., "Login Attempt")
        public string Detail { get; set; } = string.Empty; // Detailed description of the action
        public bool IsAdmin { get; set; } // Whether the user is an admin
        public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Timestamp of the action
        public string IPAddress { get; set; } = string.Empty; // IP address of the user
        public string Input { get; set; } = string.Empty; // Input data provided by the user
        public string Status { get; set; } = string.Empty; // Status of the action (e.g., "Success", "Failed")
        public string ErrorMessage { get; set; } = string.Empty; // Error message if the action failed
    }
}
