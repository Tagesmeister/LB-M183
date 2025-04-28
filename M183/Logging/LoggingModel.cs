namespace M183.Logging
{
    public class LoggingModel
    {
        private static int _id = 0;

        public int? Id { get; set; } = _id++; 
        public int UserId { get; set; } = 0; 
        public string Username { get; set; } = string.Empty; 
        public string Action { get; set; } = string.Empty; 
        public string Detail { get; set; } = string.Empty; 
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string IPAddress { get; set; } = string.Empty; 
        public string Input { get; set; } = string.Empty; 
        public string Status { get; set; } = string.Empty; 
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
