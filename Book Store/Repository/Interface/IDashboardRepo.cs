using Book_Store.Models;
using Book_Store.View_Models.Dashboard;
using Book_Store.View_Models.Email;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;

namespace Book_Store.Repository.Interface
{
    public interface IDashboardRepo
    {

        #region Home Dashboard
        //Get data in Home Page
        Task<HomeDashVM> GetHomeDataAsync(int period);
        #endregion


        #region Book Management
        //Get Data in Book List Page
        Task<BookListVM> GetBookListDataAsync(string searchtext,int? orderby, int currentpage);



        //Make Excel file
        Task<XLWorkbook> MakeBooksExcelFileAsync(string searchtext, int? orderby);




        //Data In Add -- Edit Book ==> List of {Genres  -  Authors}
        Task<BookDataVM> GetDataInAddOrEditAsync();




        //Get Book Details Based on Id
        Task<BookDataVM> GetBookDataAsyncById(int BookId);



        //Add New Book
        Task<bool> AddBookAsync(BookDataVM model);


        //Edit Book
        Task<bool> EditBookAsync(BookDataVM model);


        //Delete Book
        Task<bool> DeleteBookAsync(int BookId);

        #endregion


        #region Genre Management
        //Get Data in Genre List Page
        Task<GenreListVM> GetGenreListDataAsync(string searchtext, int? orderby, int currentpage);



        //Make Excel file
        Task<XLWorkbook> MakeGenresExcelFileAsync(string searchtext, int? orderby);



        //Add Genre
        Task<bool> AddGenreAsync(string GenreName);



        //Update Genre
        Task<bool> UpdateGenreAsync(int GenreId, string GenreName);


        //Delete Genre
        Task<bool> DeleteGenreAsync(int GenreId);
        #endregion


        #region Authors Management
        //Get Data in Genre List Page
        Task<AuthorListVM> GetAuthorListDataAsync(string searchtext, int? orderby, int currentpage);



        //Make Excel file
        Task<XLWorkbook> MakeAutorsExcelFileAsync(string searchtext, int? orderby);



        //Add Genre
        Task<bool> AddAuthorAsync(string AuthorName);


        //Update Author
        Task<bool> UpdateAuthorAsync(int AuthorId, string AuthorName);


        //Delete Genre
        Task<bool> DeleteAuthorAsync(int AuthorId);
        #endregion


        #region Order Management
        //Get Order List data
        Task<OrderListVM> GetOrderListDataAsync(string? searchtext, int? orderby, int currentpage, int status, string method);


        //Make Excel file
        Task<XLWorkbook> MakeOrdersExcelFileAsync(string searchtext, int? orderby, int status, string method);



        //Get Order Details Data Based on ==> Order Id
        Task<OrderDetailsVM> GetOrderDetailsDataAsync(int OrderId);



        //Update Order Status ==> Pending --> Processing , Processing --> Shipping , Shipping --> Delivered 
        Task<dynamic> UpdateOrderStatusAsync(int OrderId, string DeliveyId);



        //Change Delivery For Order
        Task<dynamic> ChangeDeliveryAsync(int OrderId, string DeliveyId);



        //Cancel Order
        Task<dynamic> CancelOrderAsync(int OrderId);



        //Refund Oredr
        Task<dynamic> RefundOrderAsync(int OrderId);
        #endregion


        #region Role & User Management
        //Get Data In User List Page
        Task<UserListVM> GetUserListDataAsync(string? searchtext, int? orderby, int currentpage, int Role);


        //Make Excel file
        Task<XLWorkbook> MakeUsersExcelFileAsync(string searchtext, int? orderby, int Role);


        //Lock User Account 
        Task<IdentityResult> LockUserAccountAsync(string UserId);



        //UnLock User Account 
        Task<IdentityResult> UnLockUserAccountAsync(string UserId);



        //Get Data In User Details Page 
        Task<UserDetailsVM> GetUserDetailsAsync(string UserId);



        //Change User Info Like {Email , First , Last ,Phone}
        Task ChangeUserDataAsync(string UserId, string Email, string? Firstname, string? Lastname, string? Phone);




        //Reset User Password By His Id and get New one
        Task ResetUserPasswordAsync(string UserId, string NewPassword);



        //Assign Role to User 
        Task AssignRoleToUserAsync(string userId, string roleId, bool isAssigned);




        //Delete All User Fav 
        Task ClearAllUserFavoriteAsync(string UserId);







        //Add Role
        Task AddRoleAsync(string RoleName);



        //Edit Role
        Task EditRoleAsync(string RoleId, string RoleName);



        //Delete Role
        Task DeleteRoleAsync(string RoleId);
        #endregion


        #region State - City - Deliverty Management
        //State List Page
        Task<StateListVM> GetStateDataAsync(string? searchtext, int? orderby);



        //Make Excel file
        Task<XLWorkbook> MakeStatesExcelFileAsync(string searchtext, int? orderby);



        //Add State
        Task AddStateAsync(string StateName);



        //Edit State
        Task EditStateAsync(int StateId, string StateName);



        //Delete State
        Task DeleteStateAsync(int StateId);




        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //City List Page
        Task<CityListVM> GetCityDataAsync(string? searchtext, int? orderby);


        //Make Excel file
        Task<XLWorkbook> MakeCitiesExcelFileAsync(string searchtext, int? orderby);




        //Add State
        Task AddCityAsync(int StateId,string CityName);



        //Edit State
        Task EditCityAsync(int CityId, int StateId, string CityName);



        //Delete State
        Task DeleteCityAsync(int CityId);


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Get Data In Delivery List Page
        Task<DeliveryListVM> GetDeliveryListDataAsync(string? searchtext, int? orderby, int currentpage,int State);



        //Change Delivery his City
        Task ChangeDeliveryCityAsync(string DeliveryId, int ChangedCityId);
        Task<XLWorkbook> MakeDeliveriesExcelFileAsync(string searchtext, int? orderby, int State);


        //Add New Delivery 
        Task AddNewDeliveryAsync(AddDeliveryVM model);
        #endregion


        #region Reports ==>(Contact Page)
        //Report List Page
        Task<ReportListVM> GetReportDataAsync(string? searchtext, int status, int? orderby, int currentpage = 1);



        //Make Excel file
        Task<XLWorkbook> MakeReportsExcelFileAsync(string searchtext, int? orderby, int status);




        //Get Report Details By Report Id
        Task<ReportDetailsVM> GetReportDetailsAsync(int ReportId);




        //Make Response form Report Details Page
        Task MakeReportResponseAsync(UserEmailOptions model, int ReportId);




        //Send Email to Specific People 
        Task SendEmailToUserOrUsersAsync(string? RecieveEmails, string selectedRole, string Subject, string Body, IFormFile Attachment);
        #endregion
    }
}
