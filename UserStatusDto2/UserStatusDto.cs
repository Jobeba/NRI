namespace NRI.Shared
{
    public class UserStatusDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public DateTime LastActivity { get; set; }
        public bool IsOnline { get; set; }

        public string Status => IsOnline ? "Online" : $"Last seen: {LastActivity:g}";
    }

  
}
