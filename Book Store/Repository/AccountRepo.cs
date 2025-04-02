using Book_Store.Data;
using Book_Store.Models;
using Book_Store.Repository.Interface;
using Book_Store.View_Models.Account;
using Book_Store.View_Models.UserDash;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace Book_Store.Repository
{
    public class AccountRepo : IAccountRepo
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHomeRepo _homeRepo;

        public AccountRepo(AppDbContext context,
                           UserManager<ApplicationUser> userManager,
                           SignInManager<ApplicationUser> signInManager,
                           IHomeRepo homeRepo) 
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _homeRepo = homeRepo;
        }


        //Login 
        public async Task<SignInResult> LoginAsync(LoginVM model)
        {
            var usernameformat = new EmailAddressAttribute().IsValid(model.Email)? new MailAddress(model.Email).User : model.Email;

            var User = await _userManager.FindByEmailAsync(usernameformat);
            if (User == null) return SignInResult.Failed;
            var result = await _signInManager.PasswordSignInAsync(User, model.Password, model.RememberMe, false);
            return result;
        }


        //Register
        public async Task<IdentityResult> CreateUserAsync(RegisterVM model)
        {
            var User = new ApplicationUser
            {
                Email = new MailAddress(model.Email).User,
                EmailConfirmed = true,
                UserName = new MailAddress(model.Email).User,
                FirstName = model.Firstname,
                LastName = model.Lastname,
                PhoneNumber = model.PhoneNumber,
                CreatedDate = DateTime.Now,
            };
            var result = await _userManager.CreateAsync(User,model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(User, "user");
                await MakeCookiesAsync(User);
            }
            return result;
        }


        //Make Cookies to New User 
        public async Task MakeCookiesAsync(ApplicationUser User)
        {
            await _signInManager.SignInAsync(User, false);
        }


        //LogOut 
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }



        //Change Password
        public async Task<IdentityResult> ChangePasswordAsync(ChangePassword model)
        {
            var userId = _homeRepo.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);

            var result = await _userManager.ChangePasswordAsync(user,model.OldPassword,model.NewPassword);
            return result;
        }


    }
}
