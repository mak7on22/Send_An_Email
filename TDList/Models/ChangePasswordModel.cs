namespace TDList.Models
{
    public class ChangePasswordModel
    {
        public int Id { get; set; }
        public string PasswordHash { get; set; }
        public string NewPassword { get; set; }
    }
}
