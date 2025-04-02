using Book_Store.Repository.Interface;
using Book_Store.View_Models.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Book_Store.Models;
using System.Net.Mail;

namespace Book_Store.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountRepo _accountRepo;
        public AccountController(IAccountRepo accountRepo)
        {
            _accountRepo = accountRepo;
        }


        #region Login

        //Return Login Page
        [HttpGet]
        public IActionResult Login(string? returnURL)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Landing","Home");
            }

            LoginVM model = new LoginVM()
            {
                returnURL = returnURL,
            };
            return View(model);
        }


        //Login Action ==> Logic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.returnURL ??= Url.Action("/");
            var result = await _accountRepo.LoginAsync(model);
            if (result == Microsoft.AspNetCore.Identity.SignInResult.Failed)
            {
                ModelState.AddModelError("", "Something wrong happens...");
                TempData["res"] = true;
                return View(model);
            }
            if (result.Succeeded)
            {
                if (model.returnURL == "/Account/%2F") return RedirectToAction("Landing", "Home");
                return LocalRedirect(model.returnURL);
            }
            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "Your email has been locked...");
                TempData["res"] = true;
                return View(model);
            }
            return RedirectToAction("Landing", "Home");
        }

        #endregion


        #region Register
        //Return Register Page
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Landing", "Home");
            }
            return View();
        }


        //Register Logic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _accountRepo.CreateUserAsync(model);
            if (result.Succeeded)
            {
                return RedirectToAction("Landing", "Home");
            }
            else
            {
                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }
            }
            TempData["res"] = true;
            return View(model);
        }
        #endregion

        //Logout 
        [HttpPost]
        public async Task<IActionResult> LogOut()
        {
            await _accountRepo.LogoutAsync();
            return RedirectToAction("Landing","Home");
        }
    }
}
