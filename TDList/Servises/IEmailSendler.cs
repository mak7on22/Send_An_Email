namespace TDList.Servises
{
    public interface IEmailSendler
    {
        Task SendEmailAsync(string email,string subject,string message);
    }
}
