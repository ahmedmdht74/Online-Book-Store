using System.Diagnostics;
using System.Net.Mail;
using System.Text.Json;
using Bogus;
using Book_Store.Data;
using Book_Store.Models;
using Book_Store.Repository.Interface;
using Book_Store.View_Models.Home;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Book_Store.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHomeRepo _homeRepo;

        public HomeController(IHomeRepo homeRepo)
        {
            _homeRepo = homeRepo;
        }

        #region  Landing
        //Return Main Page ===> Landing Page
        public IActionResult Landing()
        {
            return View();
        }
        #endregion



        #region About
        //Return About Page ===> About Page
        public IActionResult About()
        {
            return View();
        }
        #endregion



        #region Contact
        //Return Contact Page ===> Contact Page
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }


        //Post Contact Page Data ===> Contact Page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(Contact model)
        {
            if(!ModelState.IsValid) 
                return View(model);

            await _homeRepo.SaveContactDataAsync(model);
            TempData["result"] = true;
            TempData["message"] = "Message Saved Successfully";
            return View("Landing", "Home");
        }
        #endregion


        #region Search
        //Return Search Page
        [HttpGet]
        public async Task<IActionResult> Search(string? searchtext)
        {
            var model = await _homeRepo.GetSearchPageDataAsync(searchtext);
            return View(model);
        }
        #endregion



        #region Book Details
        //Return Book Details
        public async Task<IActionResult> BookDetails(int BookId)
        {
            if (BookId == 0) 
                return NotFound();
            var model = await _homeRepo.GetBookDetailAsync(BookId);
            return View(model);
            //return Json(model);
        }



        //Add Book  ==> Favorites
        [HttpPost]
        public async Task<IActionResult> AddRemoveFavourite(int itemId,bool isChecked)
        {
            if (itemId == 0) return BadRequest("No Id");
            var result = await _homeRepo.AddRemoveBookToFavoriteAsync(itemId,isChecked);
            return Json(result);
        }



        //Make Review For Special Book
        public async Task<IActionResult> MakeReview(int productId,int stars,string? review)
        {
            if (productId == 0 || stars == 0)
                return BadRequest("Values equal 0 ....");

            var result = await _homeRepo.MakeReviewAsync(productId, stars, review);
            if(result.result)
                return Ok(result.message);
            return BadRequest(result.message);
        }
        #endregion



        #region Cart Details
        //Return Cart Details Page
        [Authorize]
        public IActionResult CartDetails()
        {
            return View(new CartDetailsVM());
        }

        public async Task<IActionResult> CartDetail()
        {
            var model = await _homeRepo.GetCartDetails();
            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> updateQuantity(int BookId,string ActionType)
        {
            if (BookId == 0 || string.IsNullOrEmpty(ActionType)) return BadRequest();
            var model = await _homeRepo.GetCartResultModelAsync(BookId, ActionType);
            return Json(model);
        }
        #endregion



        #region Add or Delete And Redirection

        //Get Cart Count ===> Json Format
        public async Task<IActionResult> CartCount()
        {
            var model = await _homeRepo.GetCartCountAsync();
            return Json(model);
        }


        //Add Book into Cart
        public async Task<IActionResult> AddNewBook(int BookId, int Quantity)
        {
            if (BookId == 0 || Quantity == 0) return BadRequest();
            var model = await _homeRepo.AddBookToCartAsync(BookId, Quantity);
            return Json(model);
        }

        #endregion


        #region Check out
        //Return CheckOut Page
        [Authorize]
        public async Task<IActionResult> CheckOut()
        {
            var model = await _homeRepo.GetCheckoutDataAsync();
            return View(model);
        }



        //Get Cities By State Id
        public async Task<IActionResult> GetCitiesByState(int stateId)
        {
            if (stateId == 0)
                return NotFound("stateId equal 0.....");

            var model = await _homeRepo.GetCitiesAsync(stateId);
            return Json(model);
        }




        //Get make Order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeOrder([Bind("CityId,Firstname,Lastname,Phone,Email,Address,AdditionNotice,PaymentMethod,DeliveryFee")]CheckoutVM model)
        {
            if (!ModelState.IsValid)
            {
                model = await _homeRepo.GetCheckoutDataAsync();
                return View("CheckOut", model);
            }

            dynamic result =0;

            switch (model.PaymentMethod)
            {
                case "cash":
                    result = await _homeRepo.MakeOrderForCashOrCreditPaymentAsync(model);
                    break;

                case "credit":
                    result = await _homeRepo.MakeOrderForCashOrCreditPaymentAsync(model);
                    break;

                case "online":
                    result = await _homeRepo.SetStripeConfigrationAsync();
                    if ( result.result )
                    {
                        TempData["sessionId"] = result.message.Id;
                        //TempData["CheckoutData"] = model;
                        HttpContext.Session.SetString("CheckoutData", JsonConvert.SerializeObject(model));
                        return Redirect(result.message.Url);
                    }
                    break;
            }

            TempData["result"] = result.result;
            TempData["message"] = result.message;
            return View("Landing","Home");
        }



        
        //Confirm Online Payment
        public async Task<IActionResult> ConfirmOnlinePayment()
        {
            try
            {
                var model = JsonConvert.DeserializeObject<CheckoutVM>(HttpContext.Session.GetString("CheckoutData"));
                var SessionId = TempData["sessionId"].ToString();

                var result = await _homeRepo.MakeOrderForOnlinePaymentAsync(model, SessionId);

                TempData["result"] = result.result;
                TempData["message"] = result.message;
                return View("Landing", "Home");
            }
            catch
            {
                return RedirectToAction("Landing", "Home");
            }
        }

        #endregion




        [Route("Home/ErrorPage/{statusCode}")]
        public IActionResult ErrorPage(int statusCode)
        {
            var ex = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "The Request you Ask Can not be Found ";
                    break;
            }
            return View();
        }

        [Route("Home/Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
