using Book_Store.View_Models.Email;

namespace Book_Store.Repository.Interface
{
    public interface IEmailRepo
    {
        //Send Email Settings
        Task SendEmail(UserEmailOptions emailOptions);
    }
}
