using Book_Store.Data;
using Book_Store.Models;
using Book_Store.Repository.Interface;
using Book_Store.View_Models.Home;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using System.Net;

namespace Book_Store.Repository
{
    public class HomeRepo : IHomeRepo
    {

        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHttpContextAccessor _httpContext;

        public HomeRepo(AppDbContext context,
                           UserManager<ApplicationUser> userManager,
                           SignInManager<ApplicationUser> signInManager,
                           IHttpContextAccessor httpContext)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _httpContext = httpContext;
        }

        //Get User Id
        public string GetUserId()
        {
            var user = _httpContext.HttpContext.User;
            var userId = _userManager.GetUserId(user);
            return userId;
        }


        #region Search Page
        // Get Data to Search Page Take One Parameter ==> searchtext
        public async Task<SearchPageVM> GetSearchPageDataAsync(string? searchtext = "public")
        {
            SearchPageVM model = new SearchPageVM();
            searchtext = searchtext == null ? "public" : searchtext.ToLower();

            if (!string.IsNullOrEmpty(searchtext))
            {

                var Authors = await _context.Authors.Where(x => x.Name.ToLower().Contains(searchtext)).Distinct().Take(6).ToListAsync();
                var Genres = await _context.Genres.Where(x => x.Name.ToLower().Contains(searchtext)).ToListAsync();

                var books = await _context.Books.AsSplitQuery().AsNoTracking().Select(a => new BookDetailsVM
                {
                    BookId = a.Id,
                    BookTitle = a.Name,
                    BookImage = a.ImagePath,
                    NumOfPeaple = a.NumOfPeople,
                    Rating = a.Rating,
                    Price = a.Price,
                    AuthorId = a.AuthorId,
                    AuthorName = _context.Authors.FirstOrDefault(x => x.Id == a.AuthorId).Name,
                    GenreId = a.GenreId,
                    GenreName = _context.Genres.FirstOrDefault(x => x.Id == a.GenreId).Name
                }).Where(x => x.BookTitle.ToLower().Contains(searchtext) || x.AuthorName.ToLower().Contains(searchtext) || x.GenreName.ToLower().Contains(searchtext))
                .OrderBy(x => Guid.NewGuid()).Take(30).ToListAsync();


                model.PotentialAuthors = Authors.Count == 0 || searchtext == "public" ?  books.Select(a => new Models.Author { Id = a.AuthorId , Name = a.AuthorName}).Distinct().Take(6).ToList() : Authors;
                model.PotentialGenres = Genres.Count == 0 || searchtext == "public" ? books.Select(a => new Models.Genre { Id = a.GenreId, Name = a.GenreName}).Distinct().Take(6).ToList() : Genres;

                model.Books = books;
                model.SearchText = searchtext;
            }

            return model;
        }
        #endregion


        #region Book Details

        //Book Details Data
        public async Task<dynamic> GetBookDetailAsync(int BoodId)
        {
            var userId = GetUserId();
            bool IsFav = false;
            if(userId != null)
            {
                var userFav = await _context.Favourites.Where(x => x.UserId == userId).Select(a => a.BookId).ToListAsync();
                IsFav = userFav.Any(x => x == BoodId);
            }
            //Book Data
            var MainBook = await _context.Books.AsSplitQuery().AsNoTrackingWithIdentityResolution().Select(a => new
            {
                Id = a.Id,
                Image = a.ImagePath,
                Title = a.Name,
                AuthorName = _context.Authors.FirstOrDefault(x => x.Id == a.AuthorId).Name,
                AuthorId = a.AuthorId,
                Price = a.Price,
                Des = a.Description,
                GenreId = a.GenreId,
                GenreName = _context.Genres.FirstOrDefault(x => x.Id == a.GenreId).Name,
                Rating = a.Rating,
                PeopleCount = a.NumOfPeople,
                IsFav = IsFav,
                Quantity = _context.Stocks.FirstOrDefault(x => x.BookId == a.Id).Quantity
            }).FirstOrDefaultAsync(a => a.Id == BoodId);

            //Related Data
            var relatedBooks = await _context.Books.Where(x => x.GenreId == MainBook.GenreId).OrderBy(x => Guid.NewGuid()).Take(6).Select(a => new
            {
                Id = a.Id,
                Image = a.ImagePath,
                Title = a.Name,
                Price = a.Price,
                Rating = a.Rating,
                PeopleCount = a.NumOfPeople
            }).ToListAsync();

            //Comments
            var comments = await _context.Ratings.Where(x => x.BookId == MainBook.Id).Select(a => new
            {
                RateId = a.Id,
                Rating = a.Ratings,
                Message = a.Message,
                UserImage = "/images/default.png",
                UserName = _context.Users.FirstOrDefault(x => x.Id == a.UserId).UserName,
                Date = a.CreatedDate.ToString("dd-mm-yyyy")
            }).ToListAsync();
            
            var model = new
            {
                MainBook = MainBook,
                relatedBooks = relatedBooks,
                Comments = comments
            };
            return model;
        }


        //Make Review For Special Book
        public async Task<dynamic> MakeReviewAsync(int productId, int stars, string? review)
        {
            var userId = GetUserId();
            if (userId == null) return new {result = false , message = "Log in to meke review ,Try again...." };


            var AlreadyBuyResult = await _context.OrderDetails.AsSplitQuery().AsNoTrackingWithIdentityResolution()
                                                              .Where(x => x.Order.UserId == userId).AnyAsync(a => a.BookId == productId);
            if (!AlreadyBuyResult)
                return new { result = false, message = "You must buy this book to be able making review..." };


            var AlreadyRatingResult = await _context.Ratings.FirstOrDefaultAsync(x => x.UserId == userId && x.BookId == productId);
            if (AlreadyRatingResult != null)
                return new { result = false, message = "You Already make previous review for this book , thank you..." };


            Rating model = new Rating
            {
                BookId = productId,
                Message = review == null? null : review,
                Ratings = stars,
                UserId = userId
            };
            await _context.Ratings.AddAsync(model);
            await _context.SaveChangesAsync();

            var Book = await _context.Books.FindAsync(productId);
            var ratings = await _context.Ratings.Where(x => x.BookId == productId).ToListAsync();
            Book.Rating = ratings.Sum(x => x.Ratings) / ratings.Count;
            Book.NumOfPeople = ratings.Count();
            await _context.SaveChangesAsync();
            return new { result = true, message = "Review Added Successfully..." };
        }


        //Add or Remove Book from Favourite
        public async Task<bool> AddRemoveBookToFavoriteAsync(int BookId, bool isChecked)
        {
            var userId = GetUserId();
            if(userId == null) return false;
            var userFav = await _context.Favourites.Where(x => x.UserId == userId).Select(a => a.BookId).ToListAsync();
            //Remove
            if(userFav.Any(x => x == BookId)  && !isChecked)
            {
                var deletedone = await _context.Favourites.FirstOrDefaultAsync(x => x.UserId == userId && x.BookId == BookId);
                _context.Favourites.Remove(deletedone);
            };

            //Add
            if (!userFav.Any(x => x == BookId) && isChecked)
            {
                var newdone = new Favourite
                {
                    BookId = BookId,
                    UserId = userId
                };
                await _context.Favourites.AddAsync(newdone);
            };
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion


        #region Cart Details       ==== Add , Remove From Cart Operations

        //Get Cart Details
        public async Task<CartDetailsVM> GetCartDetails()
        {
            var UserId = GetUserId();
            CartDetailsVM model = new CartDetailsVM();
            try
            {
                if(UserId == null) throw new Exception("No User Exists");

                var UserCart = await _context.Carts.FirstOrDefaultAsync(x => x.UserId == UserId);
                if (UserCart == null) throw new Exception("This User has no Cart");

                var CartDetaisl = await _context.CartDetails.Where(x => x.CartId == UserCart.Id).ToListAsync();

                List<BookDetailsVM> CartBooks = new();
                double SubTotal = 0;
                double Discount = 0;

                foreach (var item in CartDetaisl)
                {
                    var book = await _context.Books.FindAsync(item.BookId);
                    var Author = await _context.Authors.FindAsync(book.AuthorId);
                    CartBooks.Add(new BookDetailsVM
                    {
                        AuthorName = Author.Name,
                        BookImage = book.ImagePath,
                        BookTitle = book.Name,
                        BookId = book.Id,
                        AuthorId = Author.Id,
                        Quantity = item.Quantity,
                        Price = item.Quantity * book.Price
                    });

                    SubTotal += (double)item.Price * item.Quantity;
                    Discount = 0;
                }

                model.booksDetails = CartBooks;
                model.SubTotal = SubTotal;
                model.Discount = Discount;
                model.Total = model.SubTotal - model.Discount;

                return model;
            }
            catch (Exception ex) { }
            return model;
        }



        //Get Quantity in Cart for specific user
        public async Task<int> GetCartCountAsync()
        {
            int Qty = 0;
            var userId = GetUserId();
            if (userId != null)
            {
                Qty = await _context.CartDetails.AsSplitQuery().AsNoTrackingWithIdentityResolution()
                                                .Include(x => x.Cart).Where(x => x.Cart.UserId == userId).CountAsync();
            }
            return Qty;
        }



        //Add Book into Cart
        public async Task<dynamic> AddBookToCartAsync(int BookId,int Quantity)
        {
            using var Transiant = _context.Database.BeginTransaction();
            try
            {
                var userId = GetUserId();
                if (userId == null) throw new Exception("Login To Add This Book...");

                var Book = await _context.Books.FindAsync(BookId);
                if (Book == null) throw new Exception("Wrong Book Id");

                var BookStock = await _context.Stocks.FirstAsync(x => x.BookId == Book.Id);


                Cart UserCart = await _context.Carts.FirstOrDefaultAsync(x => x.UserId == userId);
                if(UserCart == null) 
                {
                    UserCart = new Cart
                    {
                        UserId = userId,
                        IsDeleted = false,
                    };
                    await _context.Carts.AddAsync(UserCart);
                    await _context.SaveChangesAsync();
                }

                CartDetails cartDetails = await _context.CartDetails.FirstOrDefaultAsync(x => x.CartId == UserCart.Id && x.BookId == BookId);
                if (cartDetails == null) 
                {
                    cartDetails = new CartDetails
                    {
                        CartId = UserCart.Id,
                        BookId = BookId,
                        Quantity = Quantity,
                        IsDeleted = false,
                        Price = Book.Price
                    };
                    await _context.CartDetails.AddAsync(cartDetails);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    if (cartDetails.Quantity + Quantity > BookStock.Quantity) throw new Exception("Over Load Quantity");

                    if (cartDetails.Quantity < BookStock.Quantity && (cartDetails.Quantity + Quantity) <= BookStock.Quantity)
                        cartDetails.Quantity += Quantity;
                }



                await _context.SaveChangesAsync();
                Transiant.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new {result = false, message = ex.Message};
                //return 0;
            }

            return new { result = true, message = "Book added to cart successfully!" };
        }



        //Remove Book from Cart
        public async Task<dynamic> DeleteBookFromCartAsync(int BookId, int Quantity)
        {
            using var Transiant = _context.Database.BeginTransaction();
            try
            {
                var userId = GetUserId();
                if (userId == null) throw new Exception("Login To Complete");

                var Book = await _context.Books.FindAsync(BookId);
                if (Book == null) throw new Exception("Wrong Book Id");


                Cart UserCart = await _context.Carts.FirstOrDefaultAsync(x => x.UserId == userId);
                if (UserCart == null)
                    throw new Exception("User has no Cart....");


                CartDetails cartDetails = await _context.CartDetails.FirstOrDefaultAsync(x => x.CartId == UserCart.Id && x.BookId == BookId);
                if (cartDetails == null)
                    throw new Exception("User has no Book into the cart....");



                if (cartDetails.Quantity == 1)
                    _context.CartDetails.Remove(cartDetails);

                else
                    cartDetails.Quantity -= Quantity;



                await _context.SaveChangesAsync();
                Transiant.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new { result = false, message = ex.Message };
            }

            return new { result = true, message = "Book Removed from cart successfully!" };
        }


        //Remove All Quantity For specific Book From Cart
        public async Task<dynamic> DeleteAllBookQuantityAsync(int BookId)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null) throw new Exception("Login To Complete");

                var Book = await _context.Books.FindAsync(BookId);
                if (Book == null) throw new Exception("Wrong Book Id");


                Cart UserCart = await _context.Carts.FirstOrDefaultAsync(x => x.UserId == userId);
                if (UserCart == null)
                    throw new Exception("User has no Cart....");



                CartDetails cartDetails = await _context.CartDetails.FirstOrDefaultAsync(x => x.CartId == UserCart.Id && x.BookId == BookId); 
                if (cartDetails == null)
                    throw new Exception("This Book Already has no Qantity , Not Existing...");
                _context.CartDetails.Remove(cartDetails);
                await _context.SaveChangesAsync();
            
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new { result = false, message = ex.Message };
            }

            return new { result = true, message = "All Book has benn removed successfully!" };
        }



        //Make Model based on the Action Type
        public async Task<dynamic> GetCartResultModelAsync(int BookId, string ActionType)
        {
            switch (ActionType)
            {
                case "add":
                    await AddBookToCartAsync(BookId, 1);
                    break;

                case "removeOne":
                    await DeleteBookFromCartAsync(BookId, 1);
                    break;

                case "removeAll":
                    await DeleteAllBookQuantityAsync(BookId);
                    break;
            }


            var model = await GetCartDetails();
            var myBook = model.booksDetails.FirstOrDefault(x => x.BookId == BookId);
            var obj = new { success = true, newQuantity = myBook == null ? 0 : myBook.Quantity, subTotal = model.SubTotal, total = model.Total };
            return obj;
        }
        #endregion



        #region Check Out   ===  Stripe Operations

        //Make VM for Chech out page 
        public async Task<CheckoutVM> GetCheckoutDataAsync()
        {
            CheckoutVM model = new();
            try
            {
                var userId = GetUserId();
                if (userId == null) throw new Exception("There is no user in the system , login in to get this page");

                var user = await _context.Users.FindAsync(userId);
                if (user == null) throw new Exception("There is no user in the system , Wrong Id....");

                model.Email = user.Email+"@gmail.com";

                model.States = await _context.States.ToListAsync();

                var cartDetails = await _context.CartDetails.AsSplitQuery().Include(x => x.Cart).Where(x => x.Cart.UserId == userId).ToListAsync();
                model.CartItems = cartDetails.Count();

                model.CartBookDetails = cartDetails.Select(a => new
                {
                    Bookid = a.BookId,
                    BookImage = _context.Books.FirstOrDefault(x => x.Id == a.BookId).ImagePath,
                    BookName = _context.Books.FirstOrDefault(x => x.Id == a.BookId).Name,
                    Quantity = a.Quantity,
                    Price = a.Price
                }).ToList();

                model.Subtotal = (double)cartDetails.Sum(a => a.Quantity * a.Price);
                model.Discount = 0 ;
                model.DeliveryFee = 30;
                model.TotalAmountDue = model.Subtotal + model.DeliveryFee - model.Discount;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "<=====================");
            }

            return model;
        }



        //Get Cities based on State Id
        public async Task<List<City>> GetCitiesAsync(int StateId)
        {
            var model = await _context.Cities.Where(x => x.StateId == StateId).ToListAsync();
            return model;
        }



        //Make order for ' cash - credit ' ==> payment method
        public async Task<dynamic> MakeOrderForCashOrCreditPaymentAsync([Bind("CityId,Firstname,Lastname,Phone,Email,Address,AdditionNotice,PaymentMethod,DeliveryFee")] CheckoutVM model)
        {
            using var transiant = _context.Database.BeginTransaction();
            try
            {
                var userId = GetUserId();
                if (userId == null) throw new Exception("User Not Existing... Login and try again.....");

                var CartDetails = await GetCartDetailsByUserIdAsync(userId);

                //Create Order
                Order NewOrder = new Order
                {
                    AdditionNotice = model.AdditionNotice == null? null : model.AdditionNotice,
                    UserId = userId,
                    Address = model.Address,
                    CityId = model.CityId,
                    CreatedDate = DateTime.Now,
                    Email = model.Email,
                    Firstname = model.Firstname,
                    IsDeleted = false,
                    Lastname = model.Lastname,
                    PaymentMethod = model.PaymentMethod,
                    Phone = model.Phone,
                    StatusId = 1,
                    DeliveryFee = model.DeliveryFee
                };
                await _context.Orders.AddAsync(NewOrder);
                await _context.SaveChangesAsync();


                //Create Order Details  --- Clear Items from cart

                await MakeOrderDetailsAndClearCartDetailsAsync(CartDetails, NewOrder.Id);


                transiant.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new { result = false, message = ex.Message };
            }
            return new { result = true, message = "Your order has completed successfully" };
        }



        //Set Stripe Configration 
        public async Task<dynamic> SetStripeConfigrationAsync()
        {
            try
            {
                var userId = GetUserId();
                if (userId == null) throw new Exception("User Not Existing... Login and try again.....");

                var CartDetails = await _context.CartDetails.AsSplitQuery().Include(x => x.Cart).Where(x => x.Cart.UserId == userId).ToListAsync();
                if (CartDetails == null) throw new Exception("there is no items in the cart , can't make a order.....");

                var Domain = "https://localhost:7186/";
                var MonsterDomain = "http://onlinebookstore.runasp.net/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = MonsterDomain + "Home/ConfirmOnlinePayment",
                    CancelUrl = MonsterDomain + "Home/CheckOut",
                    Mode = "payment",
                    LineItems = new List<SessionLineItemOptions>()
                };

                foreach (var item in CartDetails)
                {
                    var LineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = _context.Books.FirstOrDefault(x => x.Id == item.BookId).Name,
                                Description = _context.Books.FirstOrDefault(x => x.Id == item.BookId).Description,
                                Images = new List<string> { _context.Books.FirstOrDefault(x => x.Id == item.BookId).ImagePath }
                            },
                            Currency = "egp",
                            UnitAmount = (long)( item.Price * 100),
                        },
                        Quantity = item.Quantity,
                    };

                    options.LineItems.Add(LineItem);
                }

                // Delivery Fee
                var DeliveryFee = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Delivery Fee"
                        },
                        Currency = "egp",
                        UnitAmount = (long)(30 * 100) 
                    },
                    Quantity = 1
                };
                options.LineItems.Add(DeliveryFee);

                var Service = new SessionService();
                Session session = Service.Create(options);
                
                return new { result = true, message = session };
            }
            catch (Exception ex) { return new { result = false, message = ex.Message+"..." }; }
        }



        //Make order for ' online ' ==> payment method , After Payment
        public async Task<dynamic> MakeOrderForOnlinePaymentAsync([Bind("CityId,Firstname,Lastname,Phone,Email,Address,AdditionNotice,PaymentMethod,DeliveryFee")] CheckoutVM model,string SessionId)
        {
            var Service = new SessionService();
            var Session = Service.Get(SessionId);

            if(Session.PaymentStatus == "unpaid") throw new Exception("Online payment has someThing wrong happen.....");


            using var transiant = _context.Database.BeginTransaction();
            try
            {
                var userId = GetUserId();
                if (userId == null) throw new Exception("User Not Existing... Login and try again.....");

                var CartDetails = await GetCartDetailsByUserIdAsync(userId);

                //Create Order
                Order NewOrder = new Order
                {
                    AdditionNotice = model.AdditionNotice == null ? null : model.AdditionNotice,
                    UserId = userId,
                    Address = model.Address,
                    CityId = model.CityId,
                    CreatedDate = DateTime.Now,
                    Email = model.Email,
                    Firstname = model.Firstname,
                    IsDeleted = false,
                    Lastname = model.Lastname,
                    PaymentMethod = model.PaymentMethod,
                    Phone = model.Phone,
                    StatusId = 1,
                    StripeSessionId = Session.Id,
                    StripePaymentIntentId = Session.PaymentIntentId,
                    DeliveryFee= model.DeliveryFee
                };
                await _context.Orders.AddAsync(NewOrder);
                await _context.SaveChangesAsync();


                //Create Order Details  --- Clear Items from cart

                await MakeOrderDetailsAndClearCartDetailsAsync(CartDetails,NewOrder.Id);

                transiant.Commit();

            }
            catch (Exception ex) {return new { result = false, message = ex.Message }; }
            return new { result = true, message = "Your order has completed successfully" };
        }



        //Get Cart Details By UserId     ===> Private <===
        private async Task<List<CartDetails>> GetCartDetailsByUserIdAsync(string userId)
        {
            var CartDetails = await _context.CartDetails.AsSplitQuery().Include(x => x.Cart).Where(x => x.Cart.UserId == userId).ToListAsync();
            if (CartDetails == null) throw new Exception("there is no items in the cart , can't make a order.....");

            return CartDetails;
        }


        //Make Order Details Based On Cart Details And Clear It
        private async Task MakeOrderDetailsAndClearCartDetailsAsync(List<CartDetails> CartDetails , int OrderId)
        {
            //Create Order Details
            foreach (var item in CartDetails)
            {
                OrderDetails NewOrderDetails = new OrderDetails
                {
                    BookId = item.BookId,
                    IsDeleted = false,
                    OrderId = OrderId,
                    Price = item.Price,
                    Quantity = item.Quantity,
                };
                await _context.OrderDetails.AddAsync(NewOrderDetails);
                await _context.SaveChangesAsync();
                await ModifyBookQuantityAsync(item.BookId, item.Quantity);
            }

            //Clear Items from cart
            _context.CartDetails.RemoveRange(CartDetails);
            await _context.SaveChangesAsync();
        }


        //Modify Quantity for Specific Book Based on Id
        public async Task ModifyBookQuantityAsync(int BookId,int OutQuantity)
        {
            var book = await _context.Books.FindAsync(BookId);
            var Stock = await _context.Stocks.FirstOrDefaultAsync(x => x.BookId == BookId);

            Stock.Quantity -= OutQuantity;
            await _context.SaveChangesAsync();
        }
        #endregion



        #region Contact
        //Save Data From Contact Page
        public async Task SaveContactDataAsync(Contact model)
        {
            var newcontact = new Contact
            {
                Email = model.Email,
                IsDeleted = false,
                Message = model.Message,
                Name = model.Name,
                UserId = GetUserId(),
                Subject = model.Subject,
                CreatedAt = DateTime.Now,
                Status = ContactStatus.New.ToString()
            };
            await _context.Contacts.AddAsync(newcontact);
            await _context.SaveChangesAsync();
        }
        #endregion
    }
}
