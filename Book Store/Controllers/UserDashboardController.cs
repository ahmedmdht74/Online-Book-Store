using Book_Store.Repository.Interface;
using Book_Store.View_Models.UserDash;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PdfSharp.Pdf;

namespace Book_Store.Controllers
{
    [Authorize]
    public class UserDashboardController : Controller
    {
        private readonly IUserDashboardRepo _userDashboard;
        private readonly IAccountRepo _accountRepo;

        public UserDashboardController(IUserDashboardRepo userDashboard,
                                       IAccountRepo accountRepo)
        {
            _userDashboard = userDashboard;
            _accountRepo = accountRepo;
        }



        #region User Dashboard
        //Return the User Home page
        public async Task<IActionResult> UserDashboard()
        {
            var model = await _userDashboard.UserHomeDataAsync();
            return View(model);
        }

        #endregion


        #region User Favorites
        //Return User Favorites Page
        public async Task<IActionResult> UserFavorites()
        {
            var model = await _userDashboard.UserFavoriteDataAsync();
            return View(model);
        }



        //Clear All User Favorite List
        [HttpPost]
        public async Task<IActionResult> ClearUserFavoriteList()
        {
            var result = await _userDashboard.ClearUserFavoriteListAsync();
            if (result)
                return RedirectToAction("UserFavorites");


            return BadRequest();
        }



        //Add All User Favorite List to Cart
        [HttpPost]
        public async Task<IActionResult> AddUserFavoriteToCart()
        {
            var result = await _userDashboard.AddAllUserFavoriteListToCartAsync();
            return Json(result);

        }



        //Send Favorite to Friend
        public async Task<IActionResult> SendFavoriteToFriend(string email)
        {
            if (email == null)
                return NotFound("Username Is null");

            var result = await _userDashboard.SendFavoriteToFriendAsync(email);
            return Json(result);
        }
        #endregion


        #region User Orders
        //Return User Profile
        public async Task<IActionResult> UserOrders()
        {
            var model = await _userDashboard.UserOrdersDataAsync();   
            return View(model);
        }
        #endregion



        #region User Order Details
        //Return User Order Details
        public async Task<IActionResult> UserOrderDetails(int OrderId)
        {
            if(OrderId == 0)
            {
                TempData["result"] = false;
                TempData["message"] = "Wrong Data , try again....";
                return RedirectToAction("UserOrders");
            }
            var model = await _userDashboard.UserOrderDetailsDataAsync(OrderId);
            return View(model);
        }
        #endregion


        #region Invice
        //Make Order Invoice
        public async Task<IActionResult> MakeOrderInvice(int OrderId)
        {
            PdfDocument document = await _userDashboard.MakeOrderInvice(OrderId);

            MemoryStream stream = new MemoryStream();
            document.Save(stream);

            Response.ContentType = "application/pdf";
            Response.Headers.Add("content-length", stream.Length.ToString());

            byte[] data = stream.ToArray();
            stream.Close();
            return File(data , "application/pdf", "invoice.pdf");

        }


        [HttpPost]
        public async Task<IActionResult> SendOrderInvoice(int OrderId)
        {
            PdfDocument document = await _userDashboard.MakeOrderInvice(OrderId);
            using (MemoryStream stream = new MemoryStream())
            {
                document.Save(stream);
                stream.Position = 0; 

                IFormFile pdfFile = new FormFile(stream, 0, stream.Length, "invoice", "invoice.pdf")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/pdf"
                };

                await _userDashboard.SendInvoiceFileByEmailAsync(pdfFile,OrderId);
                TempData["result"] = true;
                TempData["message"] = "Invoice has sent successfully...";
                return RedirectToAction("UserOrderDetails" , new { OrderId = OrderId });
            }
        }
        #endregion



        #region User Profile & Account Settings
        //Return User Profile
        public async Task<IActionResult> UserProfile()
        {
            var model = await _userDashboard.UserPersonalDataAsync();
            return View(model);
        }



        //Update - Remove Personal Image
        [HttpPost]
        public async Task<IActionResult> UpdateOrRemovePersonalImage(string action, IFormFile file)
        {
            switch (action)
            {
                case "Update":
                    if(file == null) return RedirectToAction("UserProfile");
                    await _userDashboard.UpdatePersonalImageAsync(file);
                    break;

                case "Delete":
                    await _userDashboard.RemovePersonalImageAsync(null);
                    break;
            }
            return RedirectToAction("UserProfile");
        }




        //Update Personal Info
        [HttpPost]
        public async Task<IActionResult> UpdatePersonalInfo(PersonalInfo model)
        {
            if (ModelState.IsValid)
                await _userDashboard.UpdatePersonalInfoAsync(model);
            return RedirectToAction("UserProfile");
        }


        //Change User His Old Password 
        [HttpPost]
        public async Task<IActionResult> ChangeUserPassword(ChangePassword changePassword)
        {
            var model = await _userDashboard.UserPersonalDataAsync();

            if (!ModelState.IsValid)
            {
                model.ChangePassword = changePassword;
                return View("UserProfile", model);
            }   
            var result = await _accountRepo.ChangePasswordAsync(changePassword);
            if(result.Succeeded)
                return RedirectToAction("UserProfile");
            foreach (var err in result.Errors)
            {
                ModelState.AddModelError("", err.Description);
            }
            model.ChangePassword = changePassword;
            return View("UserProfile", model);

        }
        #endregion









    }
}
