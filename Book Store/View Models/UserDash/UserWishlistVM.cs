using Stripe;

namespace Book_Store.View_Models.UserDash
{
    public class UserWishlistVM
    {
        public int ItemsNum { get; set; }
        public List<UserFavoriteBook>? userFavoriteBooks { get; set; }
    }

    public class UserFavoriteBook
    {
        public int  BookId { get; set; }
        public string  BookImage { get; set; }
        public string BookName { get; set; }
        public string Genre { get; set; }
        public decimal  Price { get; set; }
        public int  PeopleCount { get; set; }
        public bool IsFav { get; set; }
    }

   
   
   
   
   
   
   

}
