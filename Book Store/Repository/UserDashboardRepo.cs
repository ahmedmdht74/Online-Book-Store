using Book_Store.Data;
using Book_Store.Models;
using Book_Store.Repository.Interface;
using Book_Store.View_Models.UserDash;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Pdf;
using Stripe;
using Stripe.Climate;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Drawing;
using static System.Runtime.InteropServices.JavaScript.JSType;
using MigraDoc.DocumentObjectModel.Shapes;
using Book_Store.View_Models.Email;

namespace Book_Store.Repository
{
    public class UserDashboardRepo : IUserDashboardRepo
    {
        private readonly AppDbContext _context;
        private readonly IHomeRepo _homeRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHost;
        private readonly IEmailRepo _emailRepo;

        public UserDashboardRepo(AppDbContext context,
                                 IHomeRepo homeRepo,
                                 UserManager<ApplicationUser> userManager,
                                 IWebHostEnvironment webHost,
                                 IEmailRepo emailRepo)
        {
            _context = context;
            _userManager = userManager;
            _webHost = webHost;
            _emailRepo = emailRepo;
            _homeRepo = homeRepo;
        }


        #region User Dashboard
        //Return the User Home page Data
        public async Task<UserDashboardVM> UserHomeDataAsync()
        {
            var model = new UserDashboardVM();
            try
            {
                var userId = _homeRepo.GetUserId();
                if (userId == null) throw new Exception("User not in the system....");

                var User = await _context.Users.FindAsync(userId);
                if (User == null) throw new Exception("Wrong Credentials....");

                //Wishlist
                var userwishlist = await _context.Favourites.AsSplitQuery().AsNoTrackingWithIdentityResolution()
                                                 .Include(x => x.Book).ThenInclude(x => x.Author).Where(x => x.UserId == userId).Take(4).Select(a => new UserDashboardWishlist
                                                 {
                                                     BookId     = a.BookId,
                                                     BookName   = _context.Books.FirstOrDefault(z => z.Id == a.BookId).Name,
                                                     BookImage  = _context.Books.FirstOrDefault(z => z.Id == a.BookId).ImagePath,
                                                     AuthorName = _context.Books.FirstOrDefault(z => z.Id == a.BookId).Author.Name
                                                 }).ToListAsync();

                //Recommendations
                var Recommendations = await _context.Books.Where(x => x.Rating > 3).OrderBy(x => Guid.NewGuid()).Take(3).Select(a => new UserDashboardRecommendation
                {
                    BookId = a.Id,
                    BookName = a.Name,
                    BookImage = a.ImagePath,
                    Description = a.Description
                }).ToListAsync();

                //User Orders
                var Orders = await _context.Orders.OrderByDescending(x => x.CreatedDate).AsSplitQuery().AsNoTrackingWithIdentityResolution().Include(x => x.OrderDetails).Where(x => x.UserId == userId)
                                           .Select(a => new UserDashboardOrder
                                           {
                                               OrderId     = a.Id,
                                               OrderDate   = a.CreatedDate.ToString("dd-MM-yyyy"),
                                               OrderedBy   = a.Firstname + a.Lastname,
                                               TotalPrice  = a.OrderDetails.Sum(x => x.Quantity * x.Price) + (decimal)a.DeliveryFee,
                                               ItemCount   = a.OrderDetails.Count(),
                                               StatusNames = _context.orderStatuses.Take(4).ToList(),
                                               StatusId    = a.StatusId

                                           }).Take(4).ToListAsync();

                model.Recommendations = Recommendations;
                model.UserWishlist = userwishlist == null? null : userwishlist;
                model.Orders = Orders;

            }
            catch (Exception ex)
            {
            }
            return model;
        }
        #endregion



        #region User Favorite
        //Return the User wishlist data
        public async Task<UserWishlistVM> UserFavoriteDataAsync()
        {
            var model = new UserWishlistVM();
            try
            {
                var userId = _homeRepo.GetUserId();
                if (userId == null) throw new Exception("User not in the system....");

                var userWishList = await _context.Favourites.AsSplitQuery().Include(x => x.Book).ThenInclude(x => x.Genre).Where(x => x.UserId == userId)
                                                            .Select(a => new UserFavoriteBook
                                                            {
                                                                BookId      = a.BookId,
                                                                BookImage   = a.Book.ImagePath,
                                                                BookName     = a.Book.Name,
                                                                Genre       = a.Book.Genre.Name,
                                                                Price       = a.Book.Price,
                                                                PeopleCount = a.Book.NumOfPeople,
                                                                IsFav       = true
                                                            }).ToListAsync();

                int ItemsNum = userWishList.Count();

                model.userFavoriteBooks = userWishList;
                model.ItemsNum = ItemsNum;
                
                    return model;
            }
            catch { }
            return model;
        }




        //Clear All User Favorite List
        public async Task<bool> ClearUserFavoriteListAsync()
        {
            var userId = _homeRepo.GetUserId();

            var FavList = await _context.Favourites.Where(x => x.UserId == userId).ToListAsync();

            if(FavList.Count > 0 )
            {
                _context.Favourites.RemoveRange(FavList);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }


        //Add All User Favorite ook to Cart
        public async Task<dynamic> AddAllUserFavoriteListToCartAsync()
        {
            var userId = _homeRepo.GetUserId();

            var FavList = await _context.Favourites.Where(x => x.UserId == userId).ToListAsync();

            if (FavList.Count > 0)
            {
                foreach (var item in FavList)
                {
                    await _homeRepo.AddBookToCartAsync(item.BookId,1);
                }
                return new { result = true, message = "Added Successfully..." };
            }
            return new { result = false, message = "Something Wrong Happen , Try again..." };
        }


        //Send Favorite to Friend
        public async Task<dynamic> SendFavoriteToFriendAsync(string email)
        {
            var usernameformat = new EmailAddressAttribute().IsValid(email) ? new MailAddress(email).User : email;
            var User = await _userManager.FindByEmailAsync(usernameformat);
            if (User == null) return new { result = false, message = "There is no User has the same email..." };

            var userId = _homeRepo.GetUserId();
            var FavList = await _context.Favourites.Where(x => x.UserId == userId).ToListAsync();

            var models = new List<Favourite>();
            foreach (var item in FavList)
            {
                var newFav = new Favourite
                {
                    BookId = item.BookId,
                    UserId = User.Id
                };
                models.Add(newFav);
            }
            await _context.Favourites.AddRangeAsync(models);
            await _context.SaveChangesAsync();
            return new { result = true, message = $"All Books Send to {usernameformat} Successfully...." };
        }


        #endregion



        #region Order List 
        //get All User Orders ==> user Order page
        public async Task<List<UserOrdersVM>> UserOrdersDataAsync()
        {
            var model = new List<UserOrdersVM>();

            var userId = _homeRepo.GetUserId();

            if(userId != null)
                model = await _context.Orders.AsSplitQuery().AsNoTracking().Include(x => x.OrderDetails).ThenInclude(od => od.Book).Where(x => x.UserId == userId)
                                      .Select(a => new UserOrdersVM
                                      {
                                          OrderId = a.Id,
                                          TotalItems = a.OrderDetails.Count(),
                                          TotalPrice = a.DeliveryFee + (double)a.OrderDetails.Sum(x => x.Quantity * x.Price),
                                          OrderedBy = a.Firstname + " " + a.Lastname,
                                          Date = a.CreatedDate.ToString("dd-MM-yyyy"),
                                          Images = a.OrderDetails.Select(a => a.Book.ImagePath ?? "http://books.google.com/books/content?id=x9NDbpRYu50C&printsec=frontcover&img=1&zoom=1&source=gbs_api").ToList(),
                                          StatusId = a.StatusId,
                                          StatusName = _context.orderStatuses.Take(4).ToList()
                                      }).ToListAsync();


            return model;
        }
        #endregion



        #region Order Details 
        //get User Order Details  ==> user Order Details page
        public async Task<UserOrderdetailsVM> UserOrderDetailsDataAsync(int orderId)
        {
            var model = new UserOrderdetailsVM();
            model = await _context.Orders.AsSplitQuery().Include(x => x.OrderDetails)
                .Select(a => new UserOrderdetailsVM
                {
                    OrderId = a.Id,
                    CreatedDate = a.CreatedDate.ToString("MMM dd, yyyy"),
                    StatusId = a.StatusId,
                    StatusName = _context.orderStatuses.Take(4).ToList(),
                    Firstname = a.Firstname,
                    Lastname = a.Lastname,
                    Email = a.Email,
                    Phone = a.Phone,
                    Address = a.Address,
                    City = _context.Cities.FirstOrDefault(x => x.Id == a.CityId).Name,
                    State = _context.Cities.AsSplitQuery().Include(x => x.State).FirstOrDefault(x => x.Id == a.CityId).State.Name,
                    ShippingDate = a.ShippingDate == null ? "Not detemine" : a.ShippingDate.Value.ToString("MMM dd, yyyy"),
                    OrderItems = a.OrderDetails.Select(od => new UserOrderItemDetailsVM
                    {
                        BookId = od.BookId,
                        BookName = _context.Books.FirstOrDefault(x => x.Id == od.BookId).Name,
                        BookImage = _context.Books.FirstOrDefault(x => x.Id == od.BookId).ImagePath,
                        Quantity = od.Quantity,
                        Price = od.Price,
                        Total = od.Quantity * od.Price
                    }).ToList(),
                    Subtotal = a.OrderDetails.Sum(x => x.Quantity * x.Price),
                    Shipping = a.DeliveryFee.Value,
                    Total = a.OrderDetails.Sum(x => x.Quantity * x.Price) + (decimal)a.DeliveryFee
                }).FirstOrDefaultAsync(x => x.OrderId == orderId);

            return model;
        }
        #endregion



        #region Invice
        //Make Order Invoice
        public async Task<PdfDocument> MakeOrderInvice(int OrderId)
        {
            var document = new Document();

            await DocumentBody(document,OrderId);


            var PdfRenderer = new PdfDocumentRenderer();
            PdfRenderer.Document = document;

            PdfRenderer.RenderDocument();

            return PdfRenderer.PdfDocument;
        }



        //make order Invice ody
        private async Task DocumentBody(Document document, int OrderId)
        {
            var model = await UserOrderDetailsDataAsync(OrderId);

            //First Section
            Section section = document.AddSection();
            var paragraph = section.AddParagraph();
            paragraph.Format.Font.Bold = true;
            paragraph.AddText("Book Store");
            paragraph.Format.SpaceAfter = 0.5;

            paragraph = section.AddParagraph();
            paragraph.AddText("Website : https://localhost:7186/");
            paragraph.AddLineBreak();
            paragraph.AddText("Email : ahmedmdht74@gmail.com");
            paragraph.AddLineBreak();
            paragraph.AddText("phone : 012214989832");
            paragraph.Format.SpaceAfter = 20;


            //Second Section
            paragraph = section.AddParagraph();
            paragraph.Format.Font.Bold = true;
            paragraph.AddText("Customer Information");
            paragraph.Format.SpaceAfter = 0.5;

            paragraph = section.AddParagraph();
            paragraph.AddText($"First Name : {model.Firstname}");
            paragraph.AddLineBreak();
            paragraph.AddText($"Last Name : {model.Lastname}");
            paragraph.AddLineBreak();
            paragraph.AddText($"Customer Email : {model.Email}");
            paragraph.AddLineBreak();
            paragraph.AddText($"Customer Phone : {model.Phone}");
            paragraph.Format.SpaceAfter = 20;



            //Third Section
            paragraph = section.AddParagraph();
            paragraph.Format.Font.Bold = true;
            paragraph.AddText("Shipping Information");
            paragraph.Format.SpaceAfter = 0.5;

            paragraph = section.AddParagraph();
            paragraph.AddText($"Address : {model.Address}");
            paragraph.AddLineBreak();
            paragraph.AddText($"City : {model.City}");
            paragraph.AddLineBreak();
            paragraph.AddText($"State : {model.State}");
            paragraph.AddLineBreak();
            paragraph.AddText($"Shipping Date : {model.ShippingDate}");
            paragraph.Format.SpaceAfter = 20;

            var table = document.LastSection.AddTable();
            table.Borders.Width = 0.5;

            table.AddColumn("2cm"); // Id
            table.AddColumn("5cm");// Name
            table.AddColumn("2cm");// Qty
            table.AddColumn("2cm");// Price
            table.AddColumn("2cm");// Total

            //header
            Row row = table.AddRow();
            row.HeadingFormat = true;
            row.Format.Font.Bold = true;
            row.Format.Alignment = ParagraphAlignment.Center;
            row.Cells[0].AddParagraph("Id");
            row.Cells[1].AddParagraph("Name");
            row.Cells[2].AddParagraph("Quantity");
            row.Cells[3].AddParagraph("Price");
            row.Cells[4].AddParagraph("Total");

            foreach (var item in model.OrderItems)
            {
                row = table.AddRow();
                row.HeadingFormat = true;
                row.Cells[0].AddParagraph($"{item.BookId}");
                row.Cells[1].AddParagraph($"{item.BookName}");
                row.Cells[2].AddParagraph($"{item.Quantity}");
                row.Cells[3].AddParagraph($"{item.Price}");
                row.Cells[4].AddParagraph($"{item.Total}");
            }


            // Add an invisible row as a space line to the table
            row = table.AddRow();
            row.Borders.Visible = false;


            row = table.AddRow();
            row.Cells[0].Borders.Visible = false;
            row.Cells[0].AddParagraph($"Sub Total : {model.Subtotal}");
            row.Cells[0].Format.Font.Bold = true;
            row.Cells[0].Format.Alignment = ParagraphAlignment.Right;
            row.Cells[0].MergeRight = 4;
            row.Cells[4].AddParagraph($"{model.Subtotal}");

            row = table.AddRow();
            row.Cells[0].Borders.Visible = false;
            row.Cells[0].AddParagraph($"Shipping Fee : {model.Shipping}");
            row.Cells[0].Format.Font.Bold = true;
            row.Cells[0].Format.Alignment = ParagraphAlignment.Right;
            row.Cells[0].MergeRight = 4;

            row = table.AddRow();
            row.Cells[0].Borders.Visible = false;
            row.Cells[0].AddParagraph($"Total : {model.Total}");
            row.Cells[0].Format.Font.Bold = true;
            row.Cells[0].Format.Alignment = ParagraphAlignment.Right;
            row.Cells[0].MergeRight = 4;
            row.Cells[4].AddParagraph(model.Total + " €");



            // Put a logo in the header
            var image = section.Headers.Primary.AddImage("E:/Book Store/Book Store/Book Store/wwwroot/images/land4.jpg");
            image.Height = "2.5cm";
            image.RelativeVertical = RelativeVertical.Line;
            image.RelativeHorizontal = RelativeHorizontal.Margin;
            image.Top = ShapePosition.Top;
            image.Left = ShapePosition.Right;
            image.WrapFormat.Style = WrapStyle.None;


            // Create footer
            paragraph = section.Footers.Primary.AddParagraph();
            paragraph.AddText("Book Store · Sample Street 42 · 56789 Alex · Egypt");
            paragraph.Format.Font.Size = 9;
            paragraph.Format.Alignment = ParagraphAlignment.Center;
        }



        //Send Invoice By Email
        public async Task SendInvoiceFileByEmailAsync(IFormFile file,int OrderId)
        {
            var userId = _homeRepo.GetUserId();
            var user = await _userManager.FindByIdAsync(userId);
            var order = await _context.Orders.FindAsync(OrderId);
            UserEmailOptions model = new UserEmailOptions
            {
                Attachment = file,
                Body = $"Hi {user.UserName}, this an invoice for your order number {order.Id} ",
                RecieveEmails = new List<string> { user.Email + "@gmail.com" },
                Subject = "Online Book Store Invoice..."
            };
            await _emailRepo.SendEmail(model);
        }
        #endregion




        #region User Profile & Account Settings

        //Get User personal data
        public async Task<UserInfoVM> UserPersonalDataAsync()
        {
            var userId = _homeRepo.GetUserId();
            var User = await _userManager.FindByIdAsync(userId);

            var PersonalInfo = new PersonalInfo
            {
                Image       = User.Image,
                Firstname   = User.FirstName,
                Lastname    = User.LastName,
                Email       = User.Email+"@gmail.com", 
                Phone       = User.PhoneNumber,
            };

            var ChangePassword = new ChangePassword();

            var model = new UserInfoVM
            {
                PersonalInfo = PersonalInfo,
                ChangePassword = ChangePassword
            };

            return model;
        }



        //update Personal Image
        public async Task UpdatePersonalImageAsync(IFormFile file)
        {
            var UserId = _homeRepo.GetUserId();
            await RemovePersonalImageAsync(UserId);
            var User = await _userManager.FindByIdAsync(UserId);
            var folderName = Path.Combine(_webHost.WebRootPath, "images");
            var FileName = Guid.NewGuid().ToString() + file.FileName;
            var FullPath = Path.Combine(folderName, FileName);

            using(var filestream = new FileStream(FullPath, FileMode.Create))
            {
                await file.CopyToAsync(filestream);
            };
            User.Image = "/images/"+FileName;
            await _context.SaveChangesAsync();

        }



        //Remove Personal Image
        public async Task RemovePersonalImageAsync(string? UserId)
        {
            if (UserId == null)
                UserId = _homeRepo.GetUserId();
            var User = await _userManager.FindByIdAsync(UserId);
            if(User.Image != null) {  
            var FileName = "E:/Book Store/Book Store/Book Store/wwwroot" + User.Image;

            if (System.IO.File.Exists(FileName))
            {
                System.IO.File.Delete(FileName);
                User.Image = null;
                await _context.SaveChangesAsync();
            }
            }

        }



        //Update Personal Info Async
        public async Task UpdatePersonalInfoAsync(PersonalInfo model)
        {
            var UserId = _homeRepo.GetUserId();
            var User = await _userManager.FindByIdAsync(UserId);

            User.PhoneNumber = model.Phone;
            User.Email =  new MailAddress(model.Email).User;
            User.NormalizedEmail = new MailAddress(model.Email).User.ToUpper();
            User.UserName =  new MailAddress(model.Email).User;
            User.NormalizedUserName = new MailAddress(model.Email).User.ToUpper();
            User.FirstName= model.Firstname;
            User.LastName = model.Lastname;
            await _context.SaveChangesAsync();
        }
        #endregion
    }
}
