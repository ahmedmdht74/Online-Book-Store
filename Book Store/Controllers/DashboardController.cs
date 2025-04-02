using Bogus;
using Book_Store.Models;
using Book_Store.Repository.Interface;
using Book_Store.View_Models.Dashboard;
using Book_Store.View_Models.Email;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace Book_Store.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardRepo _dashboardRepo;

        public DashboardController(IDashboardRepo dashboardRepo)
        {
            _dashboardRepo = dashboardRepo;
        }

        #region Home Dashboard
        //Home Page
        public async Task<IActionResult> HomeDash(int period)
        {
            var model = await _dashboardRepo.GetHomeDataAsync(period);
            return View(model);
        }
        #endregion


        #region Book Management
        //Book Operations (Crud)
        public async Task<IActionResult> BookList(string? searchtext,int? orderby, int currentpage=1)
        {
            var model = await _dashboardRepo.GetBookListDataAsync(searchtext ,orderby, currentpage);
            return View(model);
        }


        //Make Excel File for current books in the page
        [HttpPost]
        public async Task<IActionResult> GetExcelFile(string? searchtext, int? orderby)
        {
            var workbook = await _dashboardRepo.MakeBooksExcelFileAsync(searchtext, orderby);

            using (var stream = new MemoryStream()) 
            {
                workbook.SaveAs(stream);
                var body = stream.ToArray();
                return File(body, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Books.xlsx");
            }
        }


        //Add Book -- Edit Book
        [HttpGet]
        public async Task<IActionResult> AddOrEditBook(int BookId)
        {
            var model = await _dashboardRepo.GetDataInAddOrEditAsync();
            if (BookId == 0)
                return View(model);
            else
            {
                var BookData = await _dashboardRepo.GetBookDataAsyncById(BookId);
                BookData.Authors = model.Authors;
                BookData.Genres = model.Genres;
                return View(BookData);
            }
        }




        //Add Book -- Edit Book
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEditBook(BookDataVM model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var MainData = await _dashboardRepo.GetDataInAddOrEditAsync();
            //Add
            if (model.Id == null)
            {
                var result = await _dashboardRepo.AddBookAsync(model);
                if (result) return RedirectToAction("BookList");
            }
            //Edit
            else
            {
                var result = await _dashboardRepo.EditBookAsync(model);
                if (result) return RedirectToAction("BookList");
            }
            model.Authors = MainData.Authors;
            model.Genres = MainData.Genres;
            return View(model);

        }



        //Delete Book 
        public async Task<IActionResult> DeleteBook(int BookId)
        {
            var result =  await _dashboardRepo.DeleteBookAsync(BookId);
            if (!result) return NotFound();
            return RedirectToAction("BookList");
        }

        #endregion


        #region Genre and Authors Management
        //Genre Operations (Crud)
        //Get Genre List
        public async Task<IActionResult> GenreList(string? searchtext, int? orderby, int currentpage = 1)
        {
            var model = await _dashboardRepo.GetGenreListDataAsync(searchtext,orderby,currentpage);
            return View(model);
        }

        
        //Make Excel File for current books in the page
        [HttpPost]
        public async Task<IActionResult> GetGenreExcelFile(string? searchtext, int? orderby)
        {
            var workGenre = await _dashboardRepo.MakeGenresExcelFileAsync(searchtext, orderby);

            using (var stream = new MemoryStream())
            {
                workGenre.SaveAs(stream);
                var body = stream.ToArray();
                return File(body, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Genres.xlsx");
            }
        }


        //Add Genre
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGenre(string NewGenre)
        {
            if (NewGenre == null)
                return NotFound();
            var result = await _dashboardRepo.AddGenreAsync(NewGenre);
            if (!result) return NotFound();
            return RedirectToAction("GenreList");
        }


        //Update Genre
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGenre(int GenreId,string CurrentGenre)
        {
            if (CurrentGenre == null || GenreId == 0)
                return NotFound();
            var result = await _dashboardRepo.UpdateGenreAsync(GenreId, CurrentGenre);
            if (!result) return NotFound();
            return RedirectToAction("GenreList");
        }


        //Delte Genre
        public async Task<IActionResult> DeleteGenre(int GenreId)
        {
            if (GenreId == 0) return NotFound();
            var result = await _dashboardRepo.DeleteGenreAsync(GenreId);
            if (!result) return NotFound();
            return RedirectToAction("GenreList");
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Genre Operations (Crud)
        //Get Authors List
        public async Task<IActionResult> AuthorList(string? searchtext, int? orderby, int currentpage = 1)
        {
            var model = await _dashboardRepo.GetAuthorListDataAsync(searchtext, orderby, currentpage);
            return View(model);
        }
        
        //Make Excel File for current books in the page
        [HttpPost]
        public async Task<IActionResult> GetAuthorExcelFile(string? searchtext, int? orderby)
        {
            var workAuthor = await _dashboardRepo.MakeAutorsExcelFileAsync(searchtext, orderby);

            using (var stream = new MemoryStream())
            {
                workAuthor.SaveAs(stream);
                var body = stream.ToArray();
                return File(body, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "authors.xlsx");
            }
        }


        //Add Author
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAuthor(string NewAuthor)
        {
            if (NewAuthor == null)
                return NotFound();
            var result = await _dashboardRepo.AddAuthorAsync(NewAuthor);
            if (!result) return NotFound();
            return RedirectToAction("AuthorList");
        }


        //Update Author
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAuthor(int AuthorId, string CurrentAuthor)
        {
            if (CurrentAuthor == null || AuthorId == 0)
                return NotFound();
            var result = await _dashboardRepo.UpdateAuthorAsync(AuthorId, CurrentAuthor);
            if (!result) return NotFound();
            return RedirectToAction("AuthorList");
        }


        //Delte Author
        public async Task<IActionResult> DeleteAuthor(int AuthorId)
        {
            if (AuthorId == 0) return NotFound();
            var result = await _dashboardRepo.DeleteAuthorAsync(AuthorId);
            if (!result) return NotFound();
            return RedirectToAction("AuthorList");
        }
        #endregion


        #region Order Management
        //Order List Page
        public async Task<IActionResult> OrderList(string? searchtext, int? orderby,  string method = "All", int status = 0, int currentpage = 1)
        {
            var model = await _dashboardRepo.GetOrderListDataAsync(searchtext, orderby, currentpage,status,method);
            return View(model);
        }


        //Make Excel File for current Orders in the page
        [HttpPost]
        public async Task<IActionResult> GetOrderExcelFile(string? searchtext, int? orderby, string method = "All", int status = 0)
        {
            var workOrders = await _dashboardRepo.MakeOrdersExcelFileAsync(searchtext, orderby,status,method);

            using (var stream = new MemoryStream())
            {
                workOrders.SaveAs(stream);
                var body = stream.ToArray();
                return File(body, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "orders.xlsx");
            }
        }



        //Order Details Page 
        public async Task<IActionResult> OrderDetails(int OrderId)
        {
            if (OrderId == 0) return NotFound();
            var model = await _dashboardRepo.GetOrderDetailsDataAsync(OrderId);
            return View(model);
        }


        //Update Order Status ==> Pending --> Processing , Processing --> Shipping , Shipping --> Delivered 
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int OrderId, string DeliveyId)
        {
            if(OrderId == 0)
            {
                TempData["result"] = false;
                TempData["message"] = " something wrong happen...";
                return RedirectToAction("OrderList");
            }

            var result = await _dashboardRepo.UpdateOrderStatusAsync(OrderId, DeliveyId);
            TempData["result"] = result.result;
            TempData["message"] = result.message;
            return RedirectToAction("OrderDetails", new { OrderId = OrderId });

        }


        //Change Delivery For Order
        [HttpPost]
        public async Task<IActionResult> ChangeDelivery(int OrderId,string DeliveyId)
        {
            if (OrderId == 0 || DeliveyId == null)
            {
                TempData["result"] = false;
                TempData["message"] = " something wrong happen...";
                return RedirectToAction("OrderList");
            }

            var result = await _dashboardRepo.ChangeDeliveryAsync(OrderId, DeliveyId);
            TempData["result"] = result.result;
            TempData["message"] = result.message;
            return RedirectToAction("OrderDetails", new { OrderId = OrderId });
        }



        //Cancel Order 
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int OrderId)
        {
            if (OrderId == 0)
            {
                TempData["result"] = false;
                TempData["message"] = " something wrong happen...";
                return RedirectToAction("OrderList");
            }
            var result = await _dashboardRepo.CancelOrderAsync(OrderId);
            TempData["result"] = result.result;
            TempData["message"] = result.message;
            return RedirectToAction("OrderDetails", new { OrderId = OrderId });
        }



        //Refund Order 
        [HttpPost]
        public async Task<IActionResult> RefundOrder(int OrderId)
        {
            if (OrderId == 0)
            {
                TempData["result"] = false;
                TempData["message"] = " something wrong happen...";
                return RedirectToAction("OrderList");
            }
            var result = await _dashboardRepo.RefundOrderAsync(OrderId);
            TempData["result"] = result.result;
            TempData["message"] = result.message;
            return RedirectToAction("OrderDetails", new { OrderId = OrderId });
        }
        #endregion


        #region Role & User Management
        //User List Page
        public async Task<IActionResult> UserList(string? searchtext, int? orderby, int Role, int currentpage = 1)
        {
            var model = await _dashboardRepo.GetUserListDataAsync(searchtext, orderby, currentpage, Role);
            return View(model);
        }



        //Make Excel File for current Users in the page
        [HttpPost]
        public async Task<IActionResult> GetUsersExcelFile(string? searchtext, int? orderby, int Role = 0)
        {
            var workUsers = await _dashboardRepo.MakeUsersExcelFileAsync(searchtext, orderby, Role);

            using (var stream = new MemoryStream())
            {
                workUsers.SaveAs(stream);
                var body = stream.ToArray();
                return File(body, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Users.xlsx");
            }
        }



        //Loockout User Account 
        public async Task<IActionResult> LockUserAccount(string userId,string searchtext, int? orderby, int Role, int currentpage)
        {
            var result = await _dashboardRepo.LockUserAccountAsync(userId);
            if (result.Succeeded)
                return RedirectToAction("UserList", new { searchtext = searchtext, orderby = orderby, Role = Role, currentpage = currentpage });
            return NotFound();
        }

        //UnLoockout User Account 
        public async Task<IActionResult> UnLockUserAccount(string userId, string searchtext, int? orderby, int Role, int currentpage)
        {
            var result = await _dashboardRepo.UnLockUserAccountAsync(userId);
            if (result.Succeeded)
                return RedirectToAction("UserList", new { searchtext = searchtext, orderby = orderby, Role = Role, currentpage = currentpage });
            return NotFound();
        }



        //User Details
        public async Task<IActionResult> UserDetails(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return NotFound();

            var model = await _dashboardRepo.GetUserDetailsAsync(userId);
            return View(model);
        }




        //Change User Info Like {Email , First , Last ,Phone}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserData(string UserId, string Email , string? Firstname , string? Lastname , string? Phone)
        {
            if (Email == null || UserId == null)
                return BadRequest();
            await _dashboardRepo.ChangeUserDataAsync(UserId, Email, Firstname, Lastname, Phone);
            return RedirectToAction("UserDetails", new { userId = UserId });
        }



        //Reset User Password By His Id and get New one
        public async Task<IActionResult> ResetUserPassword(string UserId,string NewPassword , string ConfirmNewPassword)
        {
            if(UserId == null || string.IsNullOrEmpty(NewPassword))
                return BadRequest();
            await _dashboardRepo.ResetUserPasswordAsync(UserId, NewPassword);
            return RedirectToAction("UserDetails", new { userId = UserId });
        }



        //Assign Role to User
        [HttpPost]
        public async Task<IActionResult> AssignRoleToUser(string userId, string roleId, bool isAssigned) 
        {
            if(userId == null || string.IsNullOrEmpty(roleId)) return BadRequest();
            await _dashboardRepo.AssignRoleToUserAsync(userId, roleId, isAssigned);
            return Ok();
        }



        //Delete All User Fav 
        public async Task<IActionResult> ClearAllUserFavorite(string UserId)
        {
            if (UserId == null) return NotFound();
            await _dashboardRepo.ClearAllUserFavoriteAsync(UserId);
            return RedirectToAction("UserDetails", new { userId = UserId });
        }



        //Add Role 
        [HttpPost]
        public async Task<IActionResult> AddRole(string ModalRoleInput)
        {
            if(ModalRoleInput == null) return NotFound();
            await _dashboardRepo.AddRoleAsync(ModalRoleInput);
            return RedirectToAction("UserList");
        }


        //Edit Role 
        [HttpPost]
        public async Task<IActionResult> EditRole(string RoleId, string ModalRoleInput)
        {
            if (ModalRoleInput == null || RoleId == null) return NotFound();
            await _dashboardRepo.EditRoleAsync(RoleId , ModalRoleInput);
            return RedirectToAction("UserList");
        }



        //Delete Role 
        [HttpPost]
        public async Task<IActionResult> DeleteRole(string RoleId)
        {
            if ( RoleId == null) return NotFound();
            await _dashboardRepo.DeleteRoleAsync(RoleId);
            return RedirectToAction("UserList");
        }
        #endregion


        #region State - City - Deliverty Management
        //State List Page
        public async Task<IActionResult> StateList(string? searchtext, int? orderby)
        {
            var model = await _dashboardRepo.GetStateDataAsync(searchtext, orderby);
            return View(model);
        }



        //Make Excel File for current State in the page
        [HttpPost]
        public async Task<IActionResult> GetStatesExcelFile(string? searchtext, int? orderby)
        {
            var workStates = await _dashboardRepo.MakeStatesExcelFileAsync(searchtext, orderby);

            using (var stream = new MemoryStream())
            {
                workStates.SaveAs(stream);
                var body = stream.ToArray();
                return File(body, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "States.xlsx");
            }
        }



        //Add State
        public async Task<IActionResult> AddState(string ModalStateInput)
        {
            if( ModalStateInput == null) return NotFound();

            await _dashboardRepo.AddStateAsync(ModalStateInput);
            return RedirectToAction("StateList");
        }


        //Edit State
        public async Task<IActionResult> EditState(int StateId, string ModalStateInput)
        {
            if (StateId == 0 || string.IsNullOrEmpty(ModalStateInput))
                return NotFound();

            await _dashboardRepo.EditStateAsync(StateId, ModalStateInput);
            return RedirectToAction("StateList");
        }



        //Delete State
        public async Task<IActionResult> DeleteState(int StateId)
        {
            if (StateId == 0)
                return NotFound();

            await _dashboardRepo.DeleteStateAsync(StateId);
            return RedirectToAction("StateList"); 
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //City List Page
        public async Task<IActionResult> CityList(string? searchtext, int? orderby)
        {
            var model = await _dashboardRepo.GetCityDataAsync(searchtext, orderby);
            return View(model);
        }




        //Make Excel File for current Cities in the page
        [HttpPost]
        public async Task<IActionResult> GetCitiesExcelFile(string? searchtext, int? orderby)
        {
            var workCities = await _dashboardRepo.MakeCitiesExcelFileAsync(searchtext, orderby);

            using (var stream = new MemoryStream())
            {
                workCities.SaveAs(stream);
                var body = stream.ToArray();
                return File(body, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Cities.xlsx");
            }
        }



        //Add City
        public async Task<IActionResult> AddCity(int ModalStateIdInput, string ModalCityInput)
        {
            if (ModalCityInput == null || ModalStateIdInput == 0) return NotFound();

            await _dashboardRepo.AddCityAsync(ModalStateIdInput, ModalCityInput);
            return RedirectToAction("CityList");
        }


        //Edit City
        public async Task<IActionResult> EditCity(int CityId, int ModalStateIdInput, string ModalCityInput)
        {
            if (CityId == 0 || string.IsNullOrEmpty(ModalCityInput))
                return NotFound();

            await _dashboardRepo.EditCityAsync(CityId, ModalStateIdInput, ModalCityInput);
            return RedirectToAction("CityList");
        }



        //Delete City
        public async Task<IActionResult> DeleteCity(int CityId)
        {
            if (CityId == 0)
                return NotFound();

            await _dashboardRepo.DeleteCityAsync(CityId);
            return RedirectToAction("CityList");
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //Delivery List Page
        public async Task<IActionResult> DeliveryList(string? searchtext, int? orderby, int State, int currentpage = 1)
        {
            var model  = await _dashboardRepo.GetDeliveryListDataAsync(searchtext, orderby, currentpage,State);
            return View(model);
        }



        //Make Excel File for current State in the page
        [HttpPost]
        public async Task<IActionResult> GetDliveriesExcelFile(string? searchtext, int? orderby, int State)
        {
            var workDeliveries = await _dashboardRepo.MakeDeliveriesExcelFileAsync(searchtext, orderby, State);

            using (var stream = new MemoryStream())
            {
                workDeliveries.SaveAs(stream);
                var body = stream.ToArray();
                return File(body, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Delivery.xlsx");
            }
        }


        //Loockout Delivery Account 
        public async Task<IActionResult> LockDeliveryAccount(string userId, string searchtext, int? orderby, int currentpage)
        {
            var result = await _dashboardRepo.LockUserAccountAsync(userId);
            if (result.Succeeded)
                return RedirectToAction("DeliveryList", new { searchtext = searchtext, orderby = orderby, currentpage = currentpage });
            return NotFound();
        }

        //UnLoockout Delivery Account 
        public async Task<IActionResult> UnLockDeliveryAccount(string userId, string searchtext, int? orderby, int currentpage)
        {
            var result = await _dashboardRepo.UnLockUserAccountAsync(userId);
            if (result.Succeeded)
                return RedirectToAction("DeliveryList", new { searchtext = searchtext, orderby = orderby, currentpage = currentpage });
            return NotFound();
        }


        //Change Delivery his City
        [HttpPost]
        public async Task<IActionResult> ChangeDeliveryCity(string DeliveryId , int ChangedCityId)
        {
            if(string.IsNullOrEmpty(DeliveryId) || ChangedCityId == 0)
                return NotFound();

            await _dashboardRepo.ChangeDeliveryCityAsync(DeliveryId, ChangedCityId);
            return RedirectToAction("DeliveryList");
        }


        //Add New Delivery 
        [HttpPost]
        public async Task<IActionResult> AddNewDelivery(AddDeliveryVM model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            await _dashboardRepo.AddNewDeliveryAsync(model);
            return RedirectToAction("DeliveryList");
        }
        #endregion


        #region Reports ==>(Contact Page)
        //Report List Page
        public async Task<IActionResult> ReportList(string? searchtext,int status, int? orderby, int currentpage = 1)
        {
            var model = await _dashboardRepo.GetReportDataAsync(searchtext, status, orderby, currentpage);
            return View(model);
        }



        //Make Excel File for current Reports in the page
        [HttpPost]
        public async Task<IActionResult> GetReportsExcelFile(string? searchtext, int status, int? orderby)
        {
            var workRepoerts = await _dashboardRepo.MakeReportsExcelFileAsync(searchtext, orderby, status);

            using (var stream = new MemoryStream())
            {
                workRepoerts.SaveAs(stream);
                var body = stream.ToArray();
                return File(body, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Repoerts.xlsx");
            }
        }



        //Get Report Details By Report Id
        public async Task<IActionResult> ReportDetails(int ReportId)
        {
            if (ReportId == 0) return NotFound();
            var model = await _dashboardRepo.GetReportDetailsAsync(ReportId);
            return View(model);
        }



        //Make Response form Report Details Page
        [HttpPost]
        public async Task<IActionResult> MakeReportResponse(UserEmailOptions model,int ReportId)
        {
            if (ReportId == 0) return NotFound();
            await _dashboardRepo.MakeReportResponseAsync(model, ReportId);
            return RedirectToAction("ReportList");
        }



        //Make Annouce and send Email Page
        [HttpGet]
        public IActionResult MakeAnnounce()
        {
            return View();
        }


        //Make Annouce and send Email Action
        [HttpPost]
        public async Task<IActionResult> MakeAnnounce(string? RecieveEmails, string selectedRole, string Subject,string Body , IFormFile Attachment)
        {
            if (string.IsNullOrEmpty(RecieveEmails) && selectedRole == "None")
                return View();
            if (!string.IsNullOrEmpty(selectedRole) && !string.IsNullOrEmpty(Subject) && !string.IsNullOrEmpty(Body))
            {
                await _dashboardRepo.SendEmailToUserOrUsersAsync(RecieveEmails,selectedRole,Subject,Body,Attachment);
                TempData["result"] = true;
                TempData["message"] = "Email Send Successfully...";
                return View();
            }
            TempData["result"] = false;
            TempData["message"] = "something wrong happen , try again...";
            return View();
        }
        #endregion
    }
}
