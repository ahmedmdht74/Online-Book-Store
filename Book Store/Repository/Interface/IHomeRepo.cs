using Book_Store.Models;
using Book_Store.View_Models.Home;
using Microsoft.AspNetCore.Mvc;

namespace Book_Store.Repository.Interface
{
    public interface IHomeRepo
    {

        //Get User Id
        string GetUserId();
        #region Search Page
        // Get Data to Search Page Take One Parameter ==> searchtext
        Task<SearchPageVM> GetSearchPageDataAsync(string? searchtext);
        #endregion


        #region Book Details Page
        //Book Details Data
        Task<dynamic> GetBookDetailAsync(int BoodId);


        //Make Review For Special Book
        Task<dynamic> MakeReviewAsync(int productId, int stars, string? review);



        //Add or Remove Book from Favourite
        Task<bool> AddRemoveBookToFavoriteAsync(int BookId, bool isChecked);
        #endregion


        #region Cart Details

        //Get Cart Details
        Task<CartDetailsVM> GetCartDetails();


        //Get Quantity in Cart for specific user
        Task<int> GetCartCountAsync();



        //Add Book into Cart
        Task<dynamic> AddBookToCartAsync(int BookId, int Quantity);



        //Remove Book from Cart
        Task<dynamic> DeleteBookFromCartAsync(int BookId, int Quantity);


        //Remove All Quantity For specific Book From Cart
        Task<dynamic> DeleteAllBookQuantityAsync(int BookId);



        //Make Model based on the Action Type
        Task<dynamic> GetCartResultModelAsync(int BookId, string ActionType);
        #endregion


        #region Check Out   ===  Stripe Operations


        //Make VM for Chech out page 
        Task<CheckoutVM> GetCheckoutDataAsync();






        //Get Cities based on State Id
        Task<List<City>> GetCitiesAsync(int StateId);




        //Make order for ' cash - credit ' ==> payment method
        Task<dynamic> MakeOrderForCashOrCreditPaymentAsync([Bind("CityId,Firstname,Lastname,Phone,Email,Address,AdditionNotice,PaymentMethod")] CheckoutVM model);





        //Make order for ' online ' ==> payment method , After Payment
        Task<dynamic> MakeOrderForOnlinePaymentAsync([Bind("CityId,Firstname,Lastname,Phone,Email,Address,AdditionNotice,PaymentMethod")] CheckoutVM model, string SessionId);




        //Set Stripe Configration 
        Task<dynamic> SetStripeConfigrationAsync();




        #endregion


        #region Contacts
        //Save Data From Contact Page
        Task SaveContactDataAsync(Contact model);
        #endregion
    }
}
