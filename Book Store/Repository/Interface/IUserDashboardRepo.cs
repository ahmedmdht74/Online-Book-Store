using Book_Store.View_Models.UserDash;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PdfSharp.Pdf;

namespace Book_Store.Repository.Interface
{
    public interface IUserDashboardRepo
    {

        #region User Dashboard
        //Return the User Home page Data
        Task<UserDashboardVM> UserHomeDataAsync();

        #endregion


        #region User Favorite
        //Return the User wishlist data
        Task<UserWishlistVM> UserFavoriteDataAsync();




        //Clear All User Favorite List
        Task<bool> ClearUserFavoriteListAsync();



        //Clear All User Favorite List
        Task<dynamic> AddAllUserFavoriteListToCartAsync();



        //Send Favorite to Friend
        Task<dynamic> SendFavoriteToFriendAsync(string email);
        #endregion


        #region Orders  
        //get All User Orders ==> user Order page
        Task<List<UserOrdersVM>> UserOrdersDataAsync();
        #endregion


        #region Order Details 
        //get User Order Details  ==> user Order Details page
        Task<UserOrderdetailsVM> UserOrderDetailsDataAsync(int orderId);
        #endregion


        #region Invice
        //Make Order Invoice
        Task<PdfDocument> MakeOrderInvice(int OrderId);




        //Send Invoice By Email
        Task SendInvoiceFileByEmailAsync(IFormFile file, int OrderId);
        #endregion


        #region User Profile & Account Settings
        //Get User personal data
        Task<UserInfoVM> UserPersonalDataAsync();



        //update Personal Image
        Task UpdatePersonalImageAsync(IFormFile file);



        //Remove Personal Image
        Task RemovePersonalImageAsync(string UserId);




        //Update Personal Info Async
        Task UpdatePersonalInfoAsync(PersonalInfo model);
        #endregion
    }
}
