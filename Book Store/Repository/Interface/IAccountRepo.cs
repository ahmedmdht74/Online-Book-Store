using Book_Store.Models;
using Book_Store.View_Models.Account;
using Book_Store.View_Models.UserDash;
using Microsoft.AspNetCore.Identity;

namespace Book_Store.Repository.Interface
{
    public interface IAccountRepo
    {

        //Login 
        Task<SignInResult> LoginAsync(LoginVM model);



        //Register
        Task<IdentityResult> CreateUserAsync(RegisterVM model);
        //Make Cookies to New User 
        Task MakeCookiesAsync(ApplicationUser User);



        //LogOut 
        Task LogoutAsync();




        //Change Password
        Task<IdentityResult> ChangePasswordAsync(ChangePassword model);
    }
}
