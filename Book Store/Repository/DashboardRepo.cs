using Book_Store.Data;
using Book_Store.Models;
using Book_Store.Repository.Interface;
using Book_Store.View_Models.Dashboard;
using Book_Store.View_Models.Email;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Stripe;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using static System.Reflection.Metadata.BlobBuilder;

namespace Book_Store.Repository
{
    public class DashboardRepo : IDashboardRepo
    {
        private readonly AppDbContext _context;
        private readonly IHomeRepo _homeRepo;
        private readonly IWebHostEnvironment _webHost;
        private readonly IMemoryCache _cache;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailRepo _emailRepo;

        public DashboardRepo(AppDbContext context,
                             IHomeRepo homeRepo,
                             IWebHostEnvironment webHost,
                             IMemoryCache cache,
                             UserManager<ApplicationUser> userManager,
                             IEmailRepo emailRepo)
        {
            _context = context;
            _homeRepo = homeRepo;
            _webHost = webHost;
            _cache = cache;
            _userManager = userManager;
            _emailRepo = emailRepo;
        }

        
        #region Home Dashboard
        //Get data in Home Page
        public async Task<HomeDashVM> GetHomeDataAsync(int period)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var model = new HomeDashVM();

            var userId = _homeRepo.GetUserId();
            var User = await _context.Users.FindAsync(userId);
            model.AdminName = User.FirstName + " " + User.LastName;

            var Today = DateTime.Now.Date;
            DateTime StartDate, EndDate;

            switch (period)
            {
                case 1:
                    model.Period = "Today";
                    StartDate = Today;
                    EndDate = Today;
                    break;

                case 2:
                    model.Period = "This Week";
                    StartDate = DateTime.Now.AddDays(-6).Date;
                    EndDate = Today;
                    break;

                case 3:
                    model.Period = "This Month";
                    StartDate = DateTime.Now.AddMonths(-1).Date;
                    EndDate = Today;
                    break;

                case 4:
                    model.Period = "This Year";
                    StartDate = DateTime.Now.AddYears(-1).Date;
                    EndDate = Today;
                    break;

                default:
                    model.Period = "Today";
                    StartDate = Today;
                    EndDate = Today;
                    break;
            }

            var Last12Months = Enumerable.Range(0, 12).Select(i => DateTime.Now.AddMonths(-i).Date).ToList();

            var AllOrderDetailsQuery = _context.OrderDetails.AsNoTracking().Include(x => x.Order).Include(b => b.Book).ThenInclude(g => g.Genre);
            var AllOrdersQuery = _context.Orders.AsNoTracking().AsSplitQuery().Include(x => x.OrderDetails);
            var AllUsersQuery = _context.Users.AsNoTracking();


            var allOrderDetails = await AllOrderDetailsQuery.Where(x => x.Order.CreatedDate.Date >= StartDate.Date && x.Order.CreatedDate.Date <= EndDate).ToListAsync();
            var allOrders = await AllOrdersQuery.ToListAsync();
            var allUsers = await AllUsersQuery.ToListAsync();


            //First Section
            model.TotalSales = allOrders.Where(x => x.CreatedDate.Date >= StartDate.Date && x.CreatedDate.Date <= EndDate.Date).Sum(x => ((decimal)x.DeliveryFee.Value) + x.OrderDetails.Sum(z => z.Price * z.Quantity));
            model.Revenue = allOrderDetails.Where(x => x.Order.StatusId == 5).Sum(x => x.Quantity * x.Price);
            model.NewCustomers = allUsers.Count(x => x.CreatedDate?.Date >= StartDate.Date && x.CreatedDate?.Date <= EndDate);
            model.Orders = allOrders.Count(x => x.CreatedDate.Date >= StartDate.Date && x.CreatedDate.Date <= EndDate);


            //Pie Chart
            model.pieChartData = allOrderDetails.GroupBy(x => x.Book.Genre.Name).Select(a => new PieChartData
            {
                GenreName = a.Key,
                Quantity = a.Sum(z => z.Quantity)
            }).ToList();


            //Recent Order
            var status = await _context.orderStatuses.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Name);

            model.RecentOrders = allOrders.Where(x => x.CreatedDate.Date >= StartDate.Date && x.CreatedDate.Date <= EndDate)
                .Take(6).Select(a => new RecentOrder
                {
                    OrderId = a.Id,
                    Customer = a.Firstname + " " + a.Lastname,
                    Date = a.CreatedDate.ToString("MMM dd,yyyy"),
                    Status = status[a.StatusId],
                    Total = a.OrderDetails.Sum(x => x.Quantity * x.Price) + (decimal)a.DeliveryFee.Value
                }).ToList();


            //Top Selling Book
            var Bookdata = await _context.Books.AsNoTracking().Include(x => x.Author).ToDictionaryAsync(x => x.Id, x => new { price = x.Price, Bookname = x.Name, Authorname = x.Author.Name });
            model.TopSellingBooks = allOrderDetails.GroupBy(x => x.BookId).Select(a => new TopSellingBook
            {
                BookId = a.Key,
                Price = Bookdata[a.Key].price,
                BookName = Bookdata[a.Key].Bookname,
                Author = Bookdata[a.Key].Authorname,
                Sold = a.Sum(x => x.Quantity)
            })
                .OrderByDescending(x => x.Sold).Take(8).ToList();


            //StackedLineChartDatas
            model.StackedLineChartDatas = Last12Months.Select(a => new StackedLineChartData
            {
                Month = a.ToString("MMM,yyyy"),
                Order = allOrders.Count(x => x.CreatedDate.Date.Month == a.Month),
                User = allUsers.Count(x => x.CreatedDate?.Date.Month == a.Month)
            }).ToList();


            //Low Stock Books
            var BooksQuantity = await _context.Stocks.AsSplitQuery().AsNoTracking().Include(x => x.Book).ThenInclude(b => b.Author).Where(x => x.Quantity <= 10).ToListAsync();
            model.LowStockBooks = BooksQuantity.Select(a => new LowStockBook
            {
                BookId = a.BookId,
                Quantity = a.Quantity,
                BookName = a.Book.Name,
                Price = a.Book.Price,
                Author = a.Book.Author.Name
            }).ToList();

            stopwatch.Stop();
            Console.WriteLine("+++++++++++++++++++");
            Console.WriteLine("+++++++++++++++++++");
            Console.WriteLine("+++++++++++++++++++");
            Console.WriteLine("+++++++++++++++++++");
            Console.WriteLine("+++++++++++++++++++");
            Console.WriteLine("Stop Watch === " + stopwatch.ElapsedMilliseconds);

            return model;
        }
        #endregion


        #region Book Management
        //Get Data in Book List Page
        public async Task<BookListVM> GetBookListDataAsync(string searchtext, int? orderby, int currentpage)
        {
            var model = new BookListVM();
            Stopwatch getquery = new Stopwatch();
            getquery.Start();
            var BookSold = await _context.OrderDetails.AsNoTracking().Include(x => x.Order).Where(x => x.Order.StatusId < 5)
                                                  .GroupBy(x => x.BookId).ToDictionaryAsync(x => x.Key, x => x.Sum(z => z.Quantity));

            var Books = _context.Books.AsNoTracking().Include(g => g.Genre).Include(b => b.Author).Include(s => s.Stock).Select(a => new BookDetailInBookList
            {
                BookId = a.Id,
                BookName = a.Name,
                Author = a.Author.Name,
                Genre = a.Genre.Name,
                Price = a.Price,
                Stock = a.Stock.Quantity,
                Sold = BookSold.ContainsKey(a.Id) ? BookSold[a.Id] : 0,
                CoverImageUrl = a.ImagePath,
                Description = a.Description
            });
            getquery.Stop();

            Stopwatch search = new Stopwatch();
            search.Start();
            //Search
            if (!string.IsNullOrEmpty(searchtext))
            {
                searchtext = searchtext.Trim().ToLower();
                Books = Books.Where(x => EF.Functions.Like(x.BookName, $"%{searchtext}%") || EF.Functions.Like(x.Author, $"%{searchtext}%") || EF.Functions.Like(x.Genre, $"%{searchtext}%"));
            }
            model.searchtext = searchtext;
            search.Stop();

            Stopwatch order = new Stopwatch();
            order.Start();
            //Order by
            switch (orderby)
            {
                case 1:
                    Books = Books.OrderBy(x => x.BookName);
                    model.orderby = 1;
                    break;
                case 2:
                    Books = Books.OrderBy(x => x.Author);
                    model.orderby = 2;
                    break;
                case 3:
                    Books = Books.OrderBy(x => x.Genre);
                    model.orderby = 3;
                    break;
                case 4:
                    Books = Books.OrderBy(x => x.Price);
                    model.orderby = 4;
                    break;
                case 5:
                    Books = Books.OrderBy(x => x.Stock);
                    model.orderby = 5;
                    break;
                case 6:
                    Books = Books.OrderByDescending(x => x.Sold);
                    model.orderby = 6;
                    break;
            }
            order.Stop();

            Stopwatch pagination = new Stopwatch();
            pagination.Start();
            //Pagination
            model.CurrentPage = currentpage;
            model.PageSize = 20;
            model.BookCount = Books.Count();
            model.TotalPages = (int)Math.Ceiling((decimal)model.BookCount / model.PageSize);
            model.bookDetailInBookLists = Books.Skip((model.CurrentPage - 1) * model.PageSize).Take(model.PageSize).ToList();
            pagination.Stop();

            Console.WriteLine("Get Data =====>" + getquery.ElapsedMilliseconds);
            Console.WriteLine("Search =====>" + search.ElapsedMilliseconds);
            Console.WriteLine("Order =====>" + order.ElapsedMilliseconds);
            Console.WriteLine("Pagination =====>" + pagination.ElapsedMilliseconds);

            return model;
        }


        //Private ==> Make Book Style for Excel 
        private async Task<List<BookDetailInBookList>> makebooks(string searchtext, int? orderby)
        {
            if (!_cache.TryGetValue("BookList", out List<BookDetailInBookList> Books))
            {
                Books = await _context.Books.AsNoTracking().Include(b => b.Genre).Select(a => new BookDetailInBookList
                {
                    BookId = a.Id,
                    BookName = a.Name,
                    Author = _context.Authors.FirstOrDefault(x => x.Id == a.AuthorId).Name,
                    Genre = a.Genre.Name,
                    Price = a.Price,
                    Stock = _context.Stocks.FirstOrDefault(x => x.BookId == a.Id).Quantity,
                    Sold = _context.OrderDetails.AsNoTracking().Where(x => x.BookId == a.Id).Sum(x => x.Quantity),
                    CoverImageUrl = a.ImagePath,
                    Description = a.Description
                }).ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(40), SlidingExpiration = TimeSpan.FromMinutes(20) };
                _cache.Set("BookList", Books, cacheOptions);
            }


            //Search
            if (searchtext != null)
            {
                searchtext = searchtext.ToLower();
                Books = Books.Where(x => x.BookName.ToLower().Contains(searchtext) || x.Author.ToLower().Contains(searchtext) || x.Genre.ToLower().Contains(searchtext)).ToList();
            }

            //Order by
            switch (orderby)
            {
                case 1:
                    Books = Books.OrderBy(x => x.BookName).ToList();
                    break;
                case 2:
                    Books = Books.OrderBy(x => x.Author).ToList();
                    break;
                case 3:
                    Books = Books.OrderBy(x => x.Genre).ToList();
                    break;
                case 4:
                    Books = Books.OrderBy(x => x.Price).ToList();
                    break;
                case 5:
                    Books = Books.OrderBy(x => x.Stock).ToList();
                    break;
                case 6:
                    Books = Books.OrderBy(x => x.Sold).ToList();
                    break;
            }

            return Books.ToList();
        }


        //Make Excel file
        public async Task<XLWorkbook> MakeBooksExcelFileAsync(string searchtext, int? orderby)
        {
            var Books = await makebooks(searchtext, orderby);
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Books");
            var currentRow = 1;

            worksheet.Cell(currentRow, 1).Value = "BookId";
            worksheet.Cell(currentRow, 2).Value = "Title";
            worksheet.Cell(currentRow, 3).Value = "Description";
            worksheet.Cell(currentRow, 4).Value = "Author ";
            worksheet.Cell(currentRow, 5).Value = "Genre";
            worksheet.Cell(currentRow, 6).Value = "Price";
            worksheet.Cell(currentRow, 7).Value = "Stock";
            worksheet.Cell(currentRow, 8).Value = "ImageCoverURL";

            foreach (var book in Books)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = book.BookId;
                worksheet.Cell(currentRow, 2).Value = book.BookName;
                worksheet.Cell(currentRow, 3).Value = book.Description;
                worksheet.Cell(currentRow, 4).Value = book.Author;
                worksheet.Cell(currentRow, 5).Value = book.Genre;
                worksheet.Cell(currentRow, 6).Value = book.Price;
                worksheet.Cell(currentRow, 7).Value = book.Stock;
                worksheet.Cell(currentRow, 8).Value = book.CoverImageUrl;
            }

            return workbook;
        }


        //Data In Add -- Edit Book ==> List of {Genres  -  Authors}
        public async Task<BookDataVM> GetDataInAddOrEditAsync()
        {
            var model = new BookDataVM();
            model.Authors = await _context.Authors.OrderBy(x => x.Name).ToListAsync();
            model.Genres = await _context.Genres.OrderBy(x => x.Name).ToListAsync();
            return model;
        }


        //Get Book Details Based on Id
        public async Task<BookDataVM> GetBookDataAsyncById(int BookId)
        {
            var Book = await _context.Books.Select(a => new BookDataVM
            {
                Id = a.Id,
                Description = a.Description,
                AuthorId = a.AuthorId,
                BookName = a.Name,
                GenreId = a.GenreId,
                ImagePath = a.ImagePath,
                Price = (double)a.Price,
                Quantity = _context.Stocks.FirstOrDefault(x => x.BookId == a.Id).Quantity
            }).FirstOrDefaultAsync(x => x.Id == BookId);
            return Book;
        }


        //Add New Book
        public async Task<bool> AddBookAsync(BookDataVM model)
        {
            try
            {
                var book = new Book
                {
                    AuthorId = model.AuthorId,
                    Description = model.Description ?? "",
                    GenreId = model.GenreId,
                    IsDeleted = false,
                    Name = model.BookName,
                    Price = (decimal)model.Price,
                    ImagePath = model.BookImageFile == null ? "" : await SaveBookImageAsync(model.BookImageFile),
                };
                await _context.Books.AddAsync(book);
                await _context.SaveChangesAsync();

                var BookStock = new Stock
                {
                    BookId = book.Id,
                    Quantity = model.Quantity.Value
                };
                await _context.Stocks.AddAsync(BookStock);
                await _context.SaveChangesAsync();
                _cache.Remove("BookList");
                return true;
            }
            catch
            {
                return false;
            }
        }


        //Edit Book
        public async Task<bool> EditBookAsync(BookDataVM model)
        {
            try
            {
                var Book = await _context.Books.FindAsync(model.Id);
                if (Book == null) throw new Exception("Not Found");
                if (model.BookImageFile != null)
                {
                    await RemoveBookImageAsync(Book.Id);
                    Book.ImagePath = await SaveBookImageAsync(model.BookImageFile);
                }
                Book.AuthorId = model.AuthorId;
                Book.GenreId = model.GenreId;
                Book.Description = model.Description;
                Book.Price = (decimal)model.Price;
                Book.Name = model.BookName;

                if (model.Quantity.HasValue)
                {
                    var BookStock = await _context.Stocks.FirstOrDefaultAsync(x => x.BookId == Book.Id);
                    BookStock.Quantity = model.Quantity.Value;
                }
                await _context.SaveChangesAsync();
                _cache.Remove("BookList");
                return true;
            }
            catch
            {
                return false;
            }
        }


        //Delete Book
        public async Task<bool> DeleteBookAsync(int BookId)
        {
            try
            {
                var Book = await _context.Books.FindAsync(BookId) ?? throw new Exception("Null Book Data...");
                Book.IsDeleted = true;
                await _context.SaveChangesAsync();
                _cache.Remove("BookList");
                return true;
            }
            catch { return false; }
        }


        //Save Image and return ImageUrl
        public async Task<string> SaveBookImageAsync(IFormFile file)
        {
            var folderName =System.IO.Path.Combine(_webHost.WebRootPath, "images");
            var FileName = Guid.NewGuid().ToString() + file.FileName;
            var FullPath = System.IO.Path.Combine(folderName, FileName);

            using (var filestream = new FileStream(FullPath, FileMode.Create))
            {
                await file.CopyToAsync(filestream);
            };
            return "/images/" + FileName;
        }


        //Remove Personal Image
        public async Task RemoveBookImageAsync(int BookId)
        {
            var Book = await _context.Books.FindAsync(BookId);
            if (Book.ImagePath != null)
            {
                var FileName = "E:/Book Store/Book Store/Book Store/wwwroot" + Book.ImagePath;

                if (System.IO.File.Exists(FileName))
                {
                    System.IO.File.Delete(FileName);
                    Book.ImagePath = null;
                    await _context.SaveChangesAsync();
                }
            }

        }
        #endregion


        #region Genre Management

        //Get Data in Genre List Page
        public async Task<GenreListVM> GetGenreListDataAsync(string searchtext, int? orderby, int currentpage)
        {
            var model = new GenreListVM();

            if (!_cache.TryGetValue("GenreList", out List<GenreDetailInGenreList> Genres))
            {
                var GenreSalesList = await _context.OrderDetails.AsNoTracking().Include(x => x.Book)
                                               .GroupBy(x => x.Book.GenreId).ToDictionaryAsync(a => a.Key, a => (a.Sum(x => x.Quantity * x.Price), a.Sum(x => x.Quantity)));

                GenreSalesList.Add(0, (0, 0));

                var GenreList = await _context.Genres.AsNoTracking().Include(x => x.Books).ToListAsync();
                Genres = GenreList
                     .Select(a => new GenreDetailInGenreList
                     {
                         GenreId = a.Id,
                         GenreName = a.Name.ToLower(),
                         NumOfBooks = a.Books.Count(),
                         Totalsales = GenreSalesList.ContainsKey(a.Id) ? GenreSalesList[a.Id].Item1 : 0,
                         SoldBook = GenreSalesList.ContainsKey(a.Id) ? GenreSalesList[a.Id].Item2 : 0,
                     }).ToList();

                var cacheOptions = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(40), SlidingExpiration = TimeSpan.FromMinutes(20) };
                _cache.Set("GenreList", Genres, cacheOptions);
            }


            //Search
            if (!string.IsNullOrEmpty(searchtext))
            {
                searchtext = searchtext.Trim().ToLower();
                Genres = Genres.Where(x => x.GenreName.Contains(searchtext)).ToList();
            }
            model.searchtext = searchtext;

            //Order by
            switch (orderby)
            {
                case 1:
                    Genres = Genres.OrderBy(x => x.GenreName).ToList();
                    model.orderby = 1;
                    break;
                case 2:
                    Genres = Genres.OrderBy(x => x.NumOfBooks).ToList();
                    model.orderby = 2;
                    break;
                case 3:
                    Genres = Genres.OrderBy(x => x.Totalsales).ToList();
                    model.orderby = 3;
                    break;
                case 4:
                    Genres = Genres.OrderBy(x => x.GenreId).ToList();
                    model.orderby = 4;
                    break;
            }




            //Pagination
            model.CurrentPage = currentpage;
            model.PageSize = 20;
            model.GenreCount = Genres.Count();
            model.TotalPages = (int)Math.Ceiling((decimal)model.GenreCount / model.PageSize);
            model.GenreDetailInGenreLists = Genres.Skip((model.CurrentPage - 1) * model.PageSize).Take(model.PageSize).ToList();

            return model;
        }



        //Private ==> Make Genre Style for Excel 
        private async Task<List<GenreDetailInGenreList>> makeGenres(string searchtext, int? orderby)
        {
            if (!_cache.TryGetValue("GenreList", out List<GenreDetailInGenreList> Genres))
            {
                var GenreSalesList = await _context.OrderDetails.AsNoTracking().Include(x => x.Book).ThenInclude(b => b.Genre)
                                               .GroupBy(x => x.Book.GenreId).ToDictionaryAsync(a => a.Key, a => (a.Sum(x => x.Quantity * x.Price), a.Sum(x => x.Quantity)));

                Genres = await _context.Genres.AsNoTracking().Include(x => x.Books)
                     .Select(a => new GenreDetailInGenreList
                     {
                         GenreId = a.Id,
                         GenreName = a.Name.ToLower(),
                         NumOfBooks = a.Books.Count(),
                         SoldBook = GenreSalesList.ContainsKey(a.Id) ? GenreSalesList[a.Id].Item2 : 0,
                         Totalsales = GenreSalesList.ContainsKey(a.Id) ? GenreSalesList[a.Id].Item1 : 0,
                     }).ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(40), SlidingExpiration = TimeSpan.FromMinutes(20) };
                _cache.Set("GenreList", Genres, cacheOptions);
            }

            //Search
            if (!string.IsNullOrEmpty(searchtext))
            {
                searchtext = searchtext.Trim().ToLower();
                Genres = Genres.Where(x => x.GenreName.Contains(searchtext)).ToList();
            }


            //Order by
            Genres = orderby switch
            {
                1 => Genres.OrderBy(x => x.GenreName).ToList(),
                2 => Genres.OrderBy(x => x.NumOfBooks).ToList(),
                3 => Genres.OrderBy(x => x.Totalsales).ToList(),
                4 => Genres.OrderBy(x => x.GenreId).ToList(),
            };
            return Genres.ToList();
        }



        //Make Excel file
        public async Task<XLWorkbook> MakeGenresExcelFileAsync(string searchtext, int? orderby)
        {
            var Genres = await makeGenres(searchtext, orderby);
            var workGenre = new XLWorkbook();
            var worksheet = workGenre.Worksheets.Add("Genres");
            var currentRow = 1;

            worksheet.Cell(currentRow, 1).Value = "GenreId";
            worksheet.Cell(currentRow, 2).Value = "Name";
            worksheet.Cell(currentRow, 3).Value = "Num of Books";
            worksheet.Cell(currentRow, 4).Value = "Total sales ";
            worksheet.Cell(currentRow, 5).Value = "Sold Book";

            foreach (var book in Genres)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = book.GenreId;
                worksheet.Cell(currentRow, 2).Value = book.GenreName;
                worksheet.Cell(currentRow, 3).Value = book.NumOfBooks;
                worksheet.Cell(currentRow, 4).Value = book.Totalsales;
                worksheet.Cell(currentRow, 5).Value = book.SoldBook;
            }

            return workGenre;
        }



        //Add Genre
        public async Task<bool> AddGenreAsync(string GenreName)
        {
            try
            {
                var Genre = new Genre
                {
                    Name = GenreName,
                };
                await _context.Genres.AddAsync(Genre);
                await _context.SaveChangesAsync();
                _cache.Remove("GenreList");
                return true;
            }
            catch { return false; }
        }



        //Update Genre
        public async Task<bool> UpdateGenreAsync(int GenreId, string GenreName)
        {
            try
            {
                var Genre = await _context.Genres.FindAsync(GenreId) ?? throw new Exception("Genre Not Found");
                Genre.Name = GenreName;
                _context.Genres.Update(Genre);
                await _context.SaveChangesAsync();
                _cache.Remove("GenreList");
                return true;
            }
            catch { return false; }
        }



        //Delete Genre
        public async Task<bool> DeleteGenreAsync(int GenreId)
        {
            try
            {
                var Genre = await _context.Genres.FindAsync(GenreId) ?? throw new Exception("Genre Not Exist...");

                Genre.IsDeleted = true;
                await _context.SaveChangesAsync();
                _cache.Remove("GenreList");
                return true;
            }
            catch { return false; }
        }

        #endregion


        #region Authors Management
        //Get Data in Author List Page
        public async Task<AuthorListVM> GetAuthorListDataAsync(string searchtext, int? orderby, int currentpage)
        {
            var model = new AuthorListVM();
            if (!_cache.TryGetValue("AuthorList", out List<AuthorDetailInAuthorList> Authors))
            {
                var AuthorSalesList = await _context.OrderDetails.AsNoTracking()
                                               .GroupBy(x => x.Book.AuthorId).ToDictionaryAsync(a => a.Key, a => a.Sum(x => x.Quantity * x.Price));

                Authors = await _context.Authors.AsNoTracking().Include(x => x.Books)
                     .Select(a => new AuthorDetailInAuthorList
                     {
                         AuthorId = a.Id,
                         AuthorName = a.Name.ToLower(),
                         BookPublish = a.Books.Count(),
                         Totalsales = AuthorSalesList.ContainsKey(a.Id) ? AuthorSalesList[a.Id] : 0,
                     }).ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(40), SlidingExpiration = TimeSpan.FromMinutes(20) };
                _cache.Set("AuthorList", Authors, cacheOptions);
            }

            //Search
            if (!string.IsNullOrEmpty(searchtext))
            {
                searchtext = searchtext.Trim().ToLower();
                Authors = Authors.Where(x => x.AuthorName.Contains(searchtext)).ToList();
            }
            model.searchtext = searchtext;

            //Order by
            switch (orderby)
            {
                case 1:
                    Authors = Authors.OrderBy(x => x.AuthorId).ToList();
                    model.orderby = 1;
                    break;
                case 2:
                    Authors = Authors.OrderBy(x => x.AuthorName).ToList();
                    model.orderby = 2;
                    break;
                case 3:
                    Authors = Authors.OrderBy(x => x.BookPublish).ToList();
                    model.orderby = 3;
                    break;
                case 4:
                    Authors = Authors.OrderBy(x => x.Totalsales).ToList();
                    model.orderby = 4;
                    break;
            }

            //Pagination
            model.CurrentPage = currentpage;
            model.PageSize = 20;
            model.AuthorCount = Authors.Count();
            model.TotalPages = (int)Math.Ceiling((decimal)model.AuthorCount / model.PageSize);
            model.AuthorDetailInAuthorList = Authors.Skip((model.CurrentPage - 1) * model.PageSize).Take(model.PageSize).ToList();

            return model;
        }


        //Private ==> Make Author Style for Excel 
        private async Task<List<AuthorDetailInAuthorList>> makeAuthors(string searchtext, int? orderby)
        {
            if (!_cache.TryGetValue("AuthorList", out List<AuthorDetailInAuthorList> Authors))
            {
                var AuthorSalesList = await _context.OrderDetails.AsNoTracking()
                                               .GroupBy(x => x.Book.AuthorId).ToDictionaryAsync(a => a.Key, a => a.Sum(x => x.Quantity * x.Price));

                Authors = await _context.Authors.AsNoTracking().Include(x => x.Books)
                     .Select(a => new AuthorDetailInAuthorList
                     {
                         AuthorId = a.Id,
                         AuthorName = a.Name.ToLower(),
                         BookPublish = a.Books.Count(),
                         Totalsales = AuthorSalesList.ContainsKey(a.Id) ? AuthorSalesList[a.Id] : 0,
                     }).ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(40), SlidingExpiration = TimeSpan.FromMinutes(20) };
                _cache.Set("AuthorList", Authors, cacheOptions);
            }

            //Search
            if (!string.IsNullOrEmpty(searchtext))
            {
                searchtext = searchtext.Trim().ToLower();
                Authors = Authors.Where(x => x.AuthorName.Contains(searchtext)).ToList();
            }



            //Order by
            Authors = orderby switch
            {
                1 => Authors.OrderBy(x => x.AuthorId).ToList(),
                2 => Authors.OrderBy(x => x.AuthorName).ToList(),
                3 => Authors.OrderBy(x => x.BookPublish).ToList(),
                4 => Authors.OrderBy(x => x.Totalsales).ToList(),
            };

            return Authors.ToList();
        }


        //Make Excel file
        public async Task<XLWorkbook> MakeAutorsExcelFileAsync(string searchtext, int? orderby)
        {
            var Authors = await makeAuthors(searchtext, orderby);
            var workAuthor = new XLWorkbook();
            var worksheet = workAuthor.Worksheets.Add("Authors");
            var currentRow = 1;

            worksheet.Cell(currentRow, 1).Value = "AuthorId";
            worksheet.Cell(currentRow, 2).Value = "Name";
            worksheet.Cell(currentRow, 3).Value = "Books Published";
            worksheet.Cell(currentRow, 4).Value = "Total sales";

            foreach (var book in Authors)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = book.AuthorId;
                worksheet.Cell(currentRow, 2).Value = book.AuthorName;
                worksheet.Cell(currentRow, 3).Value = book.BookPublish;
                worksheet.Cell(currentRow, 4).Value = book.Totalsales;
            }

            return workAuthor;
        }


        //Add Author
        public async Task<bool> AddAuthorAsync(string AuthorName)
        {
            try
            {
                var Author = new Models.Author
                {
                    Name = AuthorName,
                };
                await _context.Authors.AddAsync(Author);
                await _context.SaveChangesAsync();
                _cache.Remove("AuthorList");
                return true;
            }
            catch { return false; }
        }



        //Update Author
        public async Task<bool> UpdateAuthorAsync(int AuthorId, string AuthorName)
        {
            try
            {
                var Author = await _context.Authors.FindAsync(AuthorId) ?? throw new Exception("Author Not Found");
                Author.Name = AuthorName;
                _context.Authors.Update(Author);
                await _context.SaveChangesAsync();
                _cache.Remove("AuthorList");
                return true;
            }
            catch { return false; }
        }


        //Delete Author
        public async Task<bool> DeleteAuthorAsync(int AuthorId)
        {
            try
            {
                var Author = await _context.Authors.FindAsync(AuthorId) ?? throw new Exception("Author Not Exist...");

                Author.IsDeleted = true;
                await _context.SaveChangesAsync();
                _cache.Remove("AuthorList");
                return true;
            }
            catch { return false; }
        }
        #endregion


        #region Order Management
        //Get Order List data
        public async Task<OrderListVM> GetOrderListDataAsync(string? searchtext, int? orderby, int currentpage, int status, string method)
        {
            var model = new OrderListVM();
            if(!_cache.TryGetValue("OrderList",out List<OrderDetailInOrderList> Orders))
            {
                var OrderList = await _context.Orders.AsNoTracking().Include(x => x.OrderDetails).Include(x => x.OrderStatus).OrderByDescending(x => x.CreatedDate).ToListAsync();
                Orders = OrderList.Select(a => new OrderDetailInOrderList
                {
                    OrderId = a.Id,
                    Customer = a.Firstname.ToLower() + " " +a.Lastname.ToLower(),
                    PaymentMethod = a.PaymentMethod.ToLower(),
                    TotalAmount = a.OrderDetails.Sum(x => x.Quantity * x.Price) + (decimal)a.DeliveryFee.Value,
                    OrderDate = a.CreatedDate.ToString("MMM dd, yyyy"),
                    OrderStatus = a.OrderStatus.Name,
                    OrderStatusId = a.StatusId
                }).ToList();

                var CacheOptions = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60),SlidingExpiration =  TimeSpan.FromMinutes(30) };
                _cache.Set("OrderList", Orders, CacheOptions);
            }

            //filter => status
            switch (status)
            {
                case 0:
                    Orders = Orders.ToList();
                    model.status = "0";
                    break;
                case 1:
                    Orders = Orders.Where(x => x.OrderStatusId == 1).ToList();
                    model.status = "1";
                    break;
                case 2:
                    Orders = Orders.Where(x => x.OrderStatusId == 2).ToList();
                    model.status = "2";
                    break;
                case 3:
                    Orders = Orders.Where(x => x.OrderStatusId == 3).ToList();
                    model.status = "3";
                    break;
                case 4:
                    Orders = Orders.Where(x => x.OrderStatusId == 4).ToList();
                    model.status = "4";
                    break;
                case 5:
                    Orders = Orders.Where(x => x.OrderStatusId == 5).ToList();
                    model.status = "5";
                    break;
                case 6:
                    Orders = Orders.Where(x => x.OrderStatusId == 6).ToList();
                    model.status = "6";
                    break;
            }


            //filter => method
            switch (method)
            {
                case "All":
                    Orders = Orders.ToList();
                    model.method = "All";
                    break;
                case "online":
                    Orders = Orders.Where(x => x.PaymentMethod == "online").ToList();
                    model.method = "online";
                    break;
                case "cash":
                    Orders = Orders.Where(x => x.PaymentMethod == "cash").ToList();
                    model.method = "cash";
                    break;
                case "credit":
                    Orders = Orders.Where(x => x.PaymentMethod == "credit").ToList();
                    model.method = "credit";
                    break;
            }


            //Search
            if (!string.IsNullOrEmpty(searchtext))
            {
                searchtext = searchtext.Trim().ToLower();
                Orders = Orders.Where(x => x.Customer.Contains(searchtext) || x.PaymentMethod.Contains(searchtext) || x.OrderStatus.Contains(searchtext)).ToList();
            }
            model.searchtext = searchtext;


            //Order by
            switch (orderby)
            {
                case 1:
                    Orders = Orders.OrderByDescending(x => x.OrderId).ToList();
                    model.orderby = 1;
                    break;
                case 2:
                    Orders = Orders.OrderByDescending(x => x.Customer).ToList();
                    model.orderby = 2;
                    break;
                case 3:
                    Orders = Orders.OrderByDescending(x => x.TotalAmount).ToList();
                    model.orderby = 3;
                    break;
                case 4:
                    Orders = Orders.OrderByDescending(x => x.PaymentMethod).ToList();
                    model.orderby = 4;
                    break;
                case 5:
                    Orders = Orders.OrderByDescending(x => x.OrderStatus).ToList();
                    model.orderby = 5;
                    break;
                case 6:
                    Orders = Orders.OrderByDescending(x => x.OrderDate).ToList();
                    model.orderby = 6;
                    break;
            }


            //Pagination
            model.CurrentPage = currentpage;
            model.PageSize = 20;
            model.OrderCount = Orders.Count();
            model.TotalPages = (int)Math.Ceiling((decimal)model.OrderCount / model.PageSize);
            model.OrderDetailInOrderList = Orders.Skip((model.CurrentPage - 1) * model.PageSize).Take(model.PageSize).ToList();

            return model;
        }



        //Private ==> Make Orders Style for Excel 
        private async Task<List<OrderDetailInOrderList>> makeOrders(string? searchtext, int? orderby, int status, string method)
        {
            if (!_cache.TryGetValue("OrderList", out List<OrderDetailInOrderList> Orders))
            {
                var StatusList = await _context.orderStatuses.ToDictionaryAsync(a => a.Id, a => a.Name);
                Orders = await _context.Orders.AsNoTracking().Include(x => x.OrderDetails).OrderByDescending(x => x.CreatedDate).Select(a => new OrderDetailInOrderList
                {
                    OrderId = a.Id,
                    Customer = a.Firstname.ToLower() + " " + a.Lastname.ToLower(),
                    PaymentMethod = a.PaymentMethod.ToLower(),
                    TotalAmount = a.OrderDetails.Sum(x => x.Quantity * x.Price) + (decimal)a.DeliveryFee.Value,
                    OrderDate = a.CreatedDate.ToString("MMM dd, yyyy"),
                    OrderStatus = StatusList[a.StatusId].ToLower(),
                    OrderStatusId = a.StatusId
                }).ToListAsync();

                var CacheOptions = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60), SlidingExpiration = TimeSpan.FromMinutes(30) };
                _cache.Set("OrderList", Orders, CacheOptions);
            }

            //filter => status
            Orders = status switch
            {
                0 => Orders = Orders.ToList(),
                1 => Orders = Orders.Where(x => x.OrderStatusId == 1).ToList(),
                2 => Orders = Orders.Where(x => x.OrderStatusId == 2).ToList(),
                3 => Orders = Orders.Where(x => x.OrderStatusId == 3).ToList(),
                4 => Orders = Orders.Where(x => x.OrderStatusId == 4).ToList(),
                5 => Orders = Orders.Where(x => x.OrderStatusId == 5).ToList(),
                6 => Orders = Orders.Where(x => x.OrderStatusId == 6).ToList(),
            };


            //filter => method
            Orders = method switch
            {
                "All" => Orders = Orders.ToList(),
                "online" => Orders = Orders.Where(x => x.PaymentMethod == "online").ToList(),
                "cash" => Orders = Orders.Where(x => x.PaymentMethod == "cash").ToList(),
                "credit" => Orders = Orders.Where(x => x.PaymentMethod == "credit").ToList(),
            };


            //Search
            if (!string.IsNullOrEmpty(searchtext))
            {
                searchtext = searchtext.Trim().ToLower();
                Orders = Orders.Where(x => x.Customer.Contains(searchtext) || x.PaymentMethod.Contains(searchtext) || x.OrderStatus.Contains(searchtext)).ToList();
            }


            //Order by
            Orders = orderby switch
            {
                1 =>  Orders.OrderByDescending(x => x.OrderId).ToList(),
                2 =>  Orders.OrderByDescending(x => x.Customer).ToList(),
                3 =>  Orders.OrderByDescending(x => x.TotalAmount).ToList(),
                4 =>  Orders.OrderByDescending(x => x.PaymentMethod).ToList(),
                5 =>  Orders.OrderByDescending(x => x.OrderStatus).ToList(),
                6 =>  Orders.OrderByDescending(x => x.OrderDate).ToList(),
                _ =>  Orders.ToList()
            };
            return Orders.ToList();
        }



        //Make Excel file
        public async Task<XLWorkbook> MakeOrdersExcelFileAsync(string searchtext, int? orderby, int status, string method)
        {
            var Orders = await makeOrders(searchtext, orderby,status,method);
            var workOrder = new XLWorkbook();
            var worksheet = workOrder.Worksheets.Add("Orders");
            var currentRow = 1;

            worksheet.Cell(currentRow, 1).Value = "Order no";
            worksheet.Cell(currentRow, 2).Value = "Customer";
            worksheet.Cell(currentRow, 3).Value = "Total Amount";
            worksheet.Cell(currentRow, 4).Value = "Payment Method";
            worksheet.Cell(currentRow, 5).Value = "Order Status";
            worksheet.Cell(currentRow, 6).Value = "Order Date";

            foreach (var book in Orders)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = book.OrderId;
                worksheet.Cell(currentRow, 2).Value = book.Customer;
                worksheet.Cell(currentRow, 3).Value = book.TotalAmount;
                worksheet.Cell(currentRow, 4).Value = book.PaymentMethod;
                worksheet.Cell(currentRow, 5).Value = book.OrderStatus;
                worksheet.Cell(currentRow, 6).Value = book.OrderDate;
            }

            return workOrder;
        }



        //Get Order Details Data Based on ==> Order Id
        public async Task<OrderDetailsVM> GetOrderDetailsDataAsync(int OrderId)
        {
            var model = new OrderDetailsVM();
            var Order = await _context.Orders.Include(x => x.OrderDetails).Include(s => s.OrderStatus).FirstOrDefaultAsync(x => x.Id == OrderId);

            model.OrderId = Order.Id;
            model.StatusStage = Order.StatusId switch { 1 => 25, 2 => 50, 3 => 75, 4 => 100, _ => 100 };
            model.StatusId = Order.StatusId;
            model.CustomerName = Order.Firstname + " " + Order.Lastname;
            model.Phone = Order.Phone;
            model.Email = Order.Email;
            var City = await _context.Cities.Include(x => x.State).FirstOrDefaultAsync(x => x.Id == Order.CityId);
            model.ShippingAddress = Order.Address + "  /  " + City.Name + "  /  " + City.State.Name;
            model.PaymentMethod = Order.PaymentMethod;
            model.TotalAmount = Order.OrderDetails.Count;
            model.OrderDate = Order.CreatedDate.ToString("MMM dd,yyyy");
            model.ShippingDate = Order.ShippingDate == null ? "Not Determine" : Order.ShippingDate?.ToString("MMM dd,yyyy");
            model.OrderStatus = Order.OrderStatus.Name;
            model.SubTotal = Order.OrderDetails.Sum(x => x.Quantity * x.Price);
            model.DeliveryFee = Order.DeliveryFee == null ? 0 : Order.DeliveryFee;
            model.Total = model.SubTotal + (decimal)model.DeliveryFee;
            model.DeliveyId = Order.CityDeliveryId ?? string.Empty;

            model.BookDetailsInOrder = Order.OrderDetails.Select(a => new BookDetailsInOrderVM
            {
                BookId = a.BookId,
                Image = _context.Books.FirstOrDefault(x => x.Id == a.BookId).ImagePath ?? "http://books.google.com/books/content?id=x9NDbpRYu50C&printsec=frontcover&img=1&zoom=1&source=gbs_api",
                Price = a.Price,
                Quantity = a.Quantity,
                Title = _context.Books.FirstOrDefault(x => x.Id == a.BookId).Name,
                TotalAmount = a.Quantity * a.Price
            }).ToList();
            model.CityDeliveries = await _context.Users.Where(x => x.CityId == Order.CityId).ToListAsync(); 
            return model;
        }



        //Update Order Status ==> Pending --> Processing , Processing --> Shipping , Shipping --> Delivered 
        public async Task<dynamic> UpdateOrderStatusAsync(int OrderId ,string DeliveyId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(OrderId) ?? throw new Exception("Order not Found, try again...");
                if (order.StatusId == 2 && DeliveyId == null) throw new Exception("Pick a delivery pls...");

                switch (order.StatusId)
                {
                    case 1:
                        order.StatusId = 2;
                        break;
                    case 2:
                        order.StatusId = 3;
                        order.CityDeliveryId = DeliveyId;
                        order.ShippingDate = DateTime.Now;
                        break;
                    case 3:
                        order.StatusId = 4;
                        break;
                    default:
                        break;
                }
                await _context.SaveChangesAsync();
                _cache.Remove("OrderList");
                _cache.Remove("BookList");
                return new { result = true, message = "Status updated successfully..." };
            }
            catch (Exception ex)
            {
                return new { result = false, message = ex.Message };
            }
        }



        //Change Delivery For Order
        public async Task<dynamic> ChangeDeliveryAsync(int OrderId, string DeliveyId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(OrderId) ?? throw new Exception("Order not Found, try again...");

                order.CityDeliveryId = DeliveyId;
                await _context.SaveChangesAsync();
                _cache.Remove("OrderList");
                _cache.Remove("BookList");
                return new { result = true, message = "Delivey changed successfully..." };
            }
            catch (Exception ex)
            {
                return new { result = false, message = ex.Message };
            }
        }




        //Refund Online Method
        private async Task<bool> MakeOnlineRefundOrderAsync(int OrderId)
        {
            try
            {
                var Order = await _context.Orders.FindAsync(OrderId) ?? throw new Exception("Order not Found...");
                if(Order.PaymentMethod == "online")
                {
                    if (!string.IsNullOrEmpty(Order.StripeSessionId) || !string.IsNullOrEmpty(Order.StripePaymentIntentId))
                    {
                        var options = new RefundCreateOptions
                        {
                            Reason = RefundReasons.RequestedByCustomer,
                            PaymentIntent = Order.StripePaymentIntentId
                        };

                        var services = new RefundService();
                        Refund refund = services.Create(options);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }



        //Return Order Items to Stock
        private async Task<bool> ReturnOrderItemsToStock(int OrderId)
        {
            try
            {
                var Order = await _context.Orders.Include(x => x.OrderDetails).FirstOrDefaultAsync(x => x.Id == OrderId) ?? throw new Exception("Order not Found....");
                foreach (var item in Order.OrderDetails)
                {
                    var BookStock = await _context.Stocks.FirstOrDefaultAsync(x => x.BookId == item.BookId);
                    BookStock.Quantity += item.Quantity;
                    _context.Stocks.Update(BookStock);
                }
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }



        //Cancel Order
        public async Task<dynamic> CancelOrderAsync(int OrderId)
        {
            try
            {
                var Order = await _context.Orders.FindAsync(OrderId) ?? throw new Exception("Order not found");

                if(Order.PaymentMethod == "online")
                {
                    var result1 = await MakeOnlineRefundOrderAsync(OrderId);
                    if (!result1) throw new Exception("something wrong happen...");
                }

                Order.StatusId = 5;
                var result2 = await ReturnOrderItemsToStock(OrderId);
                if (!result2) throw new Exception("something wrong happen...");
                _cache.Remove("OrderList");
                _cache.Remove("BookList");
                return new { result = true, message = "Order Canceled successfully..." };
            }
            catch (Exception ex)
            {
                return new { result = false, message = ex.Message };
            }
        }

        //Refund Oredr
        public async Task<dynamic> RefundOrderAsync(int OrderId)
        {
            try
            {
                var Order = await _context.Orders.FindAsync(OrderId) ?? throw new Exception("Order not found");

                if (Order.PaymentMethod == "online")
                {
                    var result1 = await MakeOnlineRefundOrderAsync(OrderId);
                    if (!result1) throw new Exception("something wrong happen...");
                }

                Order.StatusId = 6;
                var result2 = await ReturnOrderItemsToStock(OrderId);
                if (!result2) throw new Exception("something wrong happen...");
                _cache.Remove("OrderList");
                _cache.Remove("BookList");
                return new { result = true, message = "Order Refunded successfully..." };
            }
            catch (Exception ex)
            {
                return new { result = false, message = ex.Message };
            }
        }
        #endregion


        #region Role & User Management
        //Get Data In User List Page
        public async Task<UserListVM> GetUserListDataAsync(string? searchtext, int? orderby, int currentpage, int Role)
        {
            var model = new UserListVM();
            if (!_cache.TryGetValue("UserList", out List<UserDetailsInUserList> Users))
            {
                var UsersOrders = await _context.Orders.GroupBy(x => x.UserId).ToDictionaryAsync(x => x.Key, x => x.Count());
                Users = await _context.Users.AsNoTracking().Select(a => new UserDetailsInUserList
                {
                    Id = a.Id,
                    Email = a.UserName+ "@gmail.com",
                    Phone = a.PhoneNumber ?? "not detemined",
                    JoinedDate = a.CreatedDate ?? DateTime.Now,
                    OrdersNum = UsersOrders.ContainsKey(a.Id) ? UsersOrders[a.Id] : 0,
                    Role = _context.UserRoles.Where(x => x.UserId == a.Id).Join(_context.Roles,ur => ur.RoleId , r => r.Id , (ur , r) => r.Name).ToList(),
                    IsLocked = a.LockoutEnd.HasValue ? true : false 
                }).OrderByDescending(x => x.JoinedDate).ToListAsync();

                var CacheOptions = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60), SlidingExpiration = TimeSpan.FromMinutes(30) };
                _cache.Set("UserList", Users, CacheOptions);
            }

            if (!_cache.TryGetValue("RoleList", out List<RoleDetailsInUserList> Roles))
            {
                var RolesNumber = await _context.UserRoles.GroupBy(x => x.RoleId).ToDictionaryAsync(x => x.Key , x => x.Count());
                Roles = await _context.Roles.Select(a => new RoleDetailsInUserList
                {
                    RoleId = a.Id,
                    Name = a.Name,
                    UserNumber = RolesNumber.ContainsKey(a.Id) ? RolesNumber[a.Id] : 0
                }).ToListAsync();

                var CacheOptions = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60), SlidingExpiration = TimeSpan.FromMinutes(30) };
                _cache.Set("RoleList", Users, CacheOptions);
            }

            model.RoleDetailsInUserList = Roles;

            model.Roles = await _context.Roles.ToListAsync();

            //filter => Role
            for (int i = 1; i <= model.Roles.Count ; i++)
            {
                if(Role == i)
                {
                    Users = Users.Where(x => x.Role.Any(x => x == model.Roles[i-1].Name)).ToList();
                    model.RoleFilter = model.Roles[i - 1].Name;
                    model.RoleFilterId = i;
                }
            }

            //Search
            if (!string.IsNullOrEmpty(searchtext))
            {
                searchtext = searchtext.Trim().ToLower();
                Users = Users.Where(x => x.Email.ToLower().Contains(searchtext) || x.Phone.Contains(searchtext)).ToList();
            }
            model.searchtext = searchtext;


            //Order by
            switch (orderby)
            {
                case 1:
                    Users = Users.OrderByDescending(x => x.Email).ToList();
                    model.orderby = 1;
                    break;
                case 2:
                    Users = Users.OrderByDescending(x => x.Phone).ToList();
                    model.orderby = 2;
                    break;
                case 3:
                    Users = Users.OrderByDescending(x => x.JoinedDate).ToList();
                    model.orderby = 3;
                    break;
                case 4:
                    Users = Users.OrderByDescending(x => x.OrdersNum).ToList();
                    model.orderby = 4;
                    break;
            }

            //Pagination
            model.CurrentPage = currentpage;
            model.PageSize = 20;
            model.UserCount = Users.Count();
            model.TotalPages = (int)Math.Ceiling((decimal)model.UserCount / model.PageSize);
            model.UserDetailsInUserLists = Users.Skip((model.CurrentPage - 1) * model.PageSize).Take(model.PageSize).ToList();

            return model;
        }



        //Private ==> Make Users Style for Excel 
        private async Task<List<UserDetailsInUserList>> makeUsers(string? searchtext, int? orderby, int Role)
        {
            if (!_cache.TryGetValue("UserList", out List<UserDetailsInUserList> Users))
            {
                var UsersOrders = await _context.Orders.GroupBy(x => x.UserId).ToDictionaryAsync(x => x.Key, x => x.Count());
                Users = await _context.Users.AsNoTracking().Select(a => new UserDetailsInUserList
                {
                    Id = a.Id.Substring(0, 8),
                    Email = a.UserName + "@gmail.com",
                    Phone = a.PhoneNumber ?? "not detemined",
                    JoinedDate = a.CreatedDate ?? DateTime.Now,
                    OrdersNum = UsersOrders.ContainsKey(a.Id) ? UsersOrders[a.Id] : 0,
                    Role = _context.UserRoles.Where(x => x.UserId == a.Id).Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name).ToList()
                }).OrderByDescending(x => x.JoinedDate).ToListAsync();

                var CacheOptions = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60), SlidingExpiration = TimeSpan.FromMinutes(30) };
                _cache.Set("UserList", Users, CacheOptions);
            }


            //filter => Role
            var Roles = await _context.Roles.ToListAsync();
            for (int i = 1; i <= Roles.Count; i++)
            {
                if (Role == i)
                {
                    Users = Users.Where(x => x.Role.Any(x => x == Roles[i - 1].Name)).ToList();
                }
            }


            //Search
            if (!string.IsNullOrEmpty(searchtext))
            {
                searchtext = searchtext.Trim();
                Users = Users.Where(x => x.Email.ToLower().Contains(searchtext) || x.Phone.Contains(searchtext)).ToList();
            }

            //Order by
            Users = orderby switch
            {
                1 => Users.OrderByDescending(x => x.Email).ToList(),
                2 => Users.OrderByDescending(x => x.Phone).ToList(),
                3 => Users.OrderByDescending(x => x.JoinedDate).ToList(),
                4 => Users.OrderByDescending(x => x.OrdersNum).ToList(),
                _ => Users.ToList()
            };
            return Users.ToList();
        }


        //Make Excel file
        public async Task<XLWorkbook> MakeUsersExcelFileAsync(string searchtext, int? orderby, int Role)
        {
            var Users = await makeUsers(searchtext, orderby,Role);
            var workUser = new XLWorkbook();
            var worksheet = workUser.Worksheets.Add("Users");
            var currentRow = 1;

            worksheet.Cell(currentRow, 1).Value = "User no";
            worksheet.Cell(currentRow, 2).Value = "Email";
            worksheet.Cell(currentRow, 3).Value = "Phone";
            worksheet.Cell(currentRow, 4).Value = "Role";
            worksheet.Cell(currentRow, 5).Value = "Joined Date";
            worksheet.Cell(currentRow, 6).Value = "Order Count";

            foreach (var user in Users)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = user.Id;
                worksheet.Cell(currentRow, 2).Value = user.Email;
                worksheet.Cell(currentRow, 3).Value = user.Phone;
                worksheet.Cell(currentRow, 4).Value = string.Join(",", user.Role);
                worksheet.Cell(currentRow, 5).Value = user.JoinedDate.ToString("MMM dd,yyyy");
                worksheet.Cell(currentRow, 6).Value = user.OrdersNum;
            }

            return workUser;
        }



        //Lock User Account 
        public async Task<IdentityResult> LockUserAccountAsync(string UserId)
        {
            var User = await _userManager.FindByIdAsync(UserId);
            _cache.Remove("UserList");
            _cache.Remove("DeliveryList");
            return await _userManager.SetLockoutEndDateAsync(User, DateTimeOffset.MaxValue);
        }



        //UnLock User Account 
        public async Task<IdentityResult> UnLockUserAccountAsync(string UserId)
        {
            var User = await _userManager.FindByIdAsync(UserId);
            _cache.Remove("UserList"); 
            _cache.Remove("DeliveryList"); 
            return await _userManager.SetLockoutEndDateAsync(User, null);
        }



        //Get Data In User Details Page 
        public async Task<UserDetailsVM> GetUserDetailsAsync(string UserId)
        {
            var model = new UserDetailsVM();
            var user = await _userManager.FindByIdAsync(UserId);

            model.UserId = UserId;
            model.Image = user.Image ?? "/images/default.png";
            model.FullName = (user.FirstName != null && user.LastName != null)? user.FirstName +" " + user.LastName : user.Email;
            model.Email = user.Email + "@gmail.com";
            model.Firstname = user.FirstName;
            model.Lastname = user.LastName;
            model.Phone = user.PhoneNumber;
            var userRoles = await _userManager.GetRolesAsync(user);
            model.UserRoles = string.Join(" - ", userRoles.ToList());

            model.UserOrders = await _context.Orders.AsNoTracking().Include(x => x.OrderDetails).Include(z => z.OrderStatus)
                                                    .Where(x => x.UserId == UserId).Select(a => new OrderDetailInOrderList
            {
                OrderId = a.Id,
                OrderDate = a.CreatedDate.ToString("MMM dd,yyyy"),
                Customer = a.Firstname + " " + a.Lastname,
                OrderStatus = a.OrderStatus.Name,
                OrderStatusId = a.StatusId,
                PaymentMethod = a.PaymentMethod,
                TotalAmount = (decimal)a.DeliveryFee + a.OrderDetails.Sum(x => x.Price * x.Quantity)
            }).ToListAsync();

            model.UserFav = await (from UF in _context.Favourites
                                   join B in _context.Books on UF.BookId equals B.Id
                                   where UF.UserId == user.Id
                                   select new UserFavInUserDetailsVM
                                   {
                                       Image = B.ImagePath ?? "http://books.google.com/books/content?id=x9NDbpRYu50C&printsec=frontcover&img=1&zoom=1&source=gbs_api",
                                       Name = B.Name,
                                       Id = B.Id
                                   }).ToListAsync();

            var AllRoles = await _context.Roles.ToListAsync();

            model.UserRoleInUserDetails = AllRoles.Select(a => new UserRoleInUserDetailsVM
            {
                RoleId = a.Id,
                RoleName = a.Name, 
                IsExisted = userRoles.Any(x => x == a.Name) ? true : false
            }).ToList();


            return model;
        }


        //Change User Info Like {Email , First , Last ,Phone}
        public async Task ChangeUserDataAsync(string UserId, string Email, string? Firstname, string? Lastname, string? Phone)
        {
            var User = await _userManager.FindByIdAsync(UserId);
            User.PhoneNumber = Phone;
            User.Email = new MailAddress(Email).User;
            User.NormalizedEmail = new MailAddress(Email).User.ToUpper();
            User.UserName = new MailAddress(Email).User;
            User.NormalizedUserName = new MailAddress(Email).User.ToUpper();
            User.FirstName = Firstname;
            User.LastName =  Lastname;
            await _context.SaveChangesAsync();
            _cache.Remove("DeliveryList");
            _cache.Remove("UserList");
        }



        //Reset User Password By His Id and get New one
        public async Task ResetUserPasswordAsync(string UserId, string NewPassword)
        {
            var user = await _userManager.FindByIdAsync(UserId);

            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (hasPassword)
            {
                var RemovePasswordResult = await _userManager.RemovePasswordAsync(user);
                if (RemovePasswordResult.Succeeded)
                {
                    await _userManager.AddPasswordAsync(user, NewPassword);
                }
            }
            await _context.SaveChangesAsync();
            _cache.Remove("DeliveryList");
            _cache.Remove("UserList");
        }



        //Assign Role to User 
        public async Task AssignRoleToUserAsync(string userId, string roleId, bool isAssigned)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var UserRoles = await _context.UserRoles.Where(x => x.UserId == userId).Join(_context.Roles , ur => ur.RoleId , r => r.Id , (Uri ,r) => new IdentityRole
            {
                Id = r.Id,
                Name = r.Name
            }).ToListAsync();


            //Remove
            if(UserRoles.Any(x => x.Id == roleId) && !isAssigned)
            {
                var removed = await _context.UserRoles.FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId);
                _context.UserRoles.Remove(removed);
                await _context.SaveChangesAsync();
            }

            //Add
            if (!UserRoles.Any(x => x.Id == roleId) && isAssigned)
            {
                var rolename = await _context.Roles.FirstOrDefaultAsync(x => x.Id == roleId);
                await _userManager.AddToRoleAsync(user, rolename.Name);
                await _context.SaveChangesAsync();
            }
            _cache.Remove("DeliveryList");
            _cache.Remove("UserList");
        }



        //Delete All User Fav 
        public async Task ClearAllUserFavoriteAsync(string UserId)
        {
            var userFavList = await _context.Favourites.Where(x => x.UserId == UserId).ToListAsync();
            if(userFavList.Count() > 0)
            {
                _context.Favourites.RemoveRange(userFavList);
                await _context.SaveChangesAsync();

            }
        }



        //Add Role
        public async Task AddRoleAsync(string RoleName)
        {
            var newrole = new IdentityRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = RoleName.ToLower(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                NormalizedName = RoleName.ToUpper()
            };
            await _context.Roles.AddAsync(newrole);
            await _context.SaveChangesAsync();
            _cache.Remove("RoleList");
        }



        //Edit Role
        public async Task EditRoleAsync(string RoleId, string RoleName)
        {
            var Role = await _context.Roles.FindAsync(RoleId);
            Role.Name = RoleName.ToLower();
            Role.NormalizedName = RoleName.ToUpper();
            _context.Roles.Update(Role);
            await _context.SaveChangesAsync();
            _cache.Remove("RoleList");
        }


        //Delete Role
        public async Task DeleteRoleAsync(string RoleId)
        {
            var Role = await _context.Roles.FindAsync(RoleId);
            _context.Roles.Remove(Role);
            await _context.SaveChangesAsync();
            _cache.Remove("RoleList");
        }
        #endregion


        #region State - City - Deliverty Management
        //State List Page
        public async Task<StateListVM> GetStateDataAsync(string? searchtext, int? orderby)
        {
            var model = new StateListVM();

            var states = await _context.States.AsNoTracking().Include(x => x.Cities).ThenInclude(c => c.CityDeliveries).Select(a => new StateDetailInStateListVM
            {
                ID = a.Id,
                Name = a.Name,
                CityNumbers = a.Cities.Count(),
                AllDeliveryNumber = _context.Users.AsNoTracking().Include(x => x.City).Where(x => x.City.StateId == a.Id).Count(),
                Cities = _context.Cities.Where(x => x.StateId == a.Id).ToList()
            }).ToListAsync();


            //Search
            if (!string.IsNullOrEmpty(searchtext))
            {
                searchtext = searchtext.Trim().ToLower();
                states = states.Where(x => x.Name.ToLower().Contains(searchtext)).ToList();
            }
            model.searchtext = searchtext;


            //Order by
            switch (orderby)
            {
                case 1:
                    states = states.OrderByDescending(x => x.ID).ToList();
                    model.orderby = 1;
                    break;
                case 2:
                    states = states.OrderBy(x => x.Name).ToList();
                    model.orderby = 2;
                    break;
                case 3:
                    states = states.OrderByDescending(x => x.CityNumbers).ToList();
                    model.orderby = 3;
                    break;
                case 4:
                    states = states.OrderByDescending(x => x.AllDeliveryNumber).ToList();
                    model.orderby = 4;
                    break;
                default:
                    states = states.ToList();
                    break;
            }

            model.StateCount = states.Count();
            model.StateDetailInStateLists = states;
            return model;
        }



        //Private ==> Make Orders Style for Excel 
        private async Task<List<StateDetailInStateListVM>> makeStates(string? searchtext, int? orderby)
        {
            var states = await _context.States.AsNoTracking().Include(x => x.Cities).ThenInclude(c => c.CityDeliveries).Select(a => new StateDetailInStateListVM
            {
                ID = a.Id,
                Name = a.Name,
                CityNumbers = a.Cities.Count(),
                AllDeliveryNumber = _context.Users.AsNoTracking().Include(x => x.City).Where(x => x.City.StateId == a.Id).Count()
            }).ToListAsync();


            //Search
            if (!string.IsNullOrEmpty(searchtext))
            {
                searchtext = searchtext.Trim().ToLower();
                states = states.Where(x => x.Name.ToLower().Contains(searchtext)).ToList();
            }

            //Order by
            states = orderby switch
            {
                1 => states.OrderByDescending(x => x.ID).ToList(),
                2 => states.OrderBy(x => x.Name).ToList(),
                3 => states.OrderByDescending(x => x.CityNumbers).ToList(),
                4 => states.OrderByDescending(x => x.AllDeliveryNumber).ToList(),
                _ => states.ToList()
            };


            return states.ToList();
        }



        //Make Excel file
        public async Task<XLWorkbook> MakeStatesExcelFileAsync(string searchtext, int? orderby)
        {
            var States = await makeStates(searchtext, orderby);
            var workState = new XLWorkbook();
            var worksheet = workState.Worksheets.Add("States");
            var currentRow = 1;

            worksheet.Cell(currentRow, 1).Value = "State no";
            worksheet.Cell(currentRow, 2).Value = "Name";
            worksheet.Cell(currentRow, 3).Value = "Num of Cities";
            worksheet.Cell(currentRow, 4).Value = "Num of Deliveries";

            foreach (var book in States)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = book.ID;
                worksheet.Cell(currentRow, 2).Value = book.Name;
                worksheet.Cell(currentRow, 3).Value = book.CityNumbers;
                worksheet.Cell(currentRow, 4).Value = book.AllDeliveryNumber;
            }

            return workState;
        }




        //Add State
        public async Task AddStateAsync(string StateName)
        {
            var newState = new State
            {
                Name = StateName,
                IsDeleted = false
            };
            await _context.States.AddAsync(newState);
            await _context.SaveChangesAsync();
        }



        //Edit State
        public async Task EditStateAsync(int StateId,string StateName)
        {
            var State = await _context.States.FindAsync(StateId);
            State.Name = StateName;

            _context.States.Update(State);
            await _context.SaveChangesAsync();
        }



        //Delete State
        public async Task DeleteStateAsync(int StateId)
        {
            var State = await _context.States.FindAsync(StateId);
            State.IsDeleted = true;
            _context.States.Update(State);
            await _context.SaveChangesAsync();
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //City List Page
        public async Task<CityListVM> GetCityDataAsync(string? searchtext, int? orderby)
        {
            var model = new CityListVM();

            var Cities = await _context.Cities.AsNoTracking().Include(x => x.State).Include(c => c.CityDeliveries).Select(a => new CityDetailInCityListVM
            {
                ID = a.Id,
                Name = a.Name,
                StateId = a.StateId,
                StateName = a.State.Name,
                DeliveryNumber = a.CityDeliveries.Count()
            }).ToListAsync();

            model.StateList = await _context.States.ToListAsync();
            //Search
            if (!string.IsNullOrEmpty(searchtext))
            {
                searchtext = searchtext.Trim().ToLower();
                Cities = Cities.Where(x => x.Name.ToLower().Contains(searchtext) || x.StateName.ToLower().Contains(searchtext)).ToList();
            }
            model.searchtext = searchtext;


            //Order by
            switch (orderby)
            {
                case 1:
                    Cities = Cities.OrderByDescending(x => x.ID).ToList();
                    model.orderby = 1;
                    break;
                case 2:
                    Cities = Cities.OrderBy(x => x.Name).ToList();
                    model.orderby = 2;
                    break;
                case 3:
                    Cities = Cities.OrderByDescending(x => x.DeliveryNumber).ToList();
                    model.orderby = 3;
                    break;
                case 4:
                    Cities = Cities.OrderByDescending(x => x.StateName).ToList();
                    model.orderby = 3;
                    break;
                default:
                    Cities = Cities.ToList();
                    break;
            }

            model.CityCount = Cities.Count();
            model.CityDetailInCityLists = Cities;
            return model;
        }




        //Private ==> Make Orders Style for Excel 
        private async Task<List<CityDetailInCityListVM>> makeCities(string? searchtext, int? orderby)
        {
            var Cities = await _context.Cities.AsNoTracking().Include(x => x.State).Include(c => c.CityDeliveries).Select(a => new CityDetailInCityListVM
            {
                ID = a.Id,
                Name = a.Name,
                StateId = a.StateId,
                StateName = a.State.Name,
                DeliveryNumber = a.CityDeliveries.Count()
            }).ToListAsync();


            //Search
            if (!string.IsNullOrEmpty(searchtext))
            {
                searchtext = searchtext.Trim().ToLower();
                Cities = Cities.Where(x => x.Name.ToLower().Contains(searchtext) || x.StateName.ToLower().Contains(searchtext)).ToList();
            }

            //Order by
            Cities = orderby switch
            {
                1 => Cities.OrderByDescending(x => x.ID).ToList(),
                2 => Cities.OrderBy(x => x.Name).ToList(),
                3 => Cities.OrderByDescending(x => x.DeliveryNumber).ToList(),
                4 => Cities.OrderBy(x => x.StateName).ToList(),
                _ => Cities.ToList()
            };


            return Cities.ToList();
        }



        //Make Excel file
        public async Task<XLWorkbook> MakeCitiesExcelFileAsync(string searchtext, int? orderby)
        {
            var Cities = await makeCities(searchtext, orderby);
            var workState = new XLWorkbook();
            var worksheet = workState.Worksheets.Add("Cities");
            var currentRow = 1;

            worksheet.Cell(currentRow, 1).Value = "City no";
            worksheet.Cell(currentRow, 2).Value = "Name";
            worksheet.Cell(currentRow, 3).Value = "Num of Deliveries";
            worksheet.Cell(currentRow, 4).Value = "State";

            foreach (var city in Cities)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = city.ID;
                worksheet.Cell(currentRow, 2).Value = city.Name;
                worksheet.Cell(currentRow, 3).Value = city.DeliveryNumber;
                worksheet.Cell(currentRow, 4).Value = city.StateName;
            }

            return workState;
        }




        //Add City
        public async Task AddCityAsync(int StateId, string CityName)
        {
            var newCity = new City
            {
                Name = CityName,
                IsDeleted = false,
                StateId = StateId
            };
            await _context.Cities.AddAsync(newCity);
            await _context.SaveChangesAsync();
        }



        //Edit City
        public async Task EditCityAsync(int CityId, int StateId, string CityName)
        {
            var City = await _context.Cities.FindAsync(CityId);
            City.Name = CityName;
            City.StateId = StateId;
            _context.Cities.Update(City);
            await _context.SaveChangesAsync();
        }



        //Delete City
        public async Task DeleteCityAsync(int CityId)
        {
            var City = await _context.Cities.FindAsync(CityId);
            City.IsDeleted = true;
            _context.Cities.Update(City);
            await _context.SaveChangesAsync();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //Get Data In Delivery List Page
        public async Task<DeliveryListVM> GetDeliveryListDataAsync(string? searchtext, int? orderby, int currentpage, int State)
        {
            var model = new DeliveryListVM();
            if (!_cache.TryGetValue("DeliveryList", out List<UserDetailsInUserList> Deliveries))
            {
                var DeliveryRole = await _context.Roles.FirstOrDefaultAsync(x => x.Name == "delivery");
                var Cities = await _context.Cities.Include(x => x.State).ToDictionaryAsync(x => x.Id, x => new {Name = x.Name , State = x.State.Name});
                Cities.Add(0 , new { Name= "Not Determine", State= "Not Determine" });

                Deliveries = await _context.UserRoles.Where(x => x.RoleId == DeliveryRole.Id).Join(_context.Users, ur => ur.UserId, u => u.Id, (ur, u) => new UserDetailsInUserList
                {
                    Id = u.Id,
                    Email = u.FirstName +""+ u.LastName,
                    Phone = u.PhoneNumber,
                    JoinedDate = u.CreatedDate ?? DateTime.MaxValue,
                    AssignedCity = Cities.ContainsKey(u.CityId ?? 0) ? Cities[u.CityId ?? 0].Name : "Not Detemine",
                    AssignedState = Cities.ContainsKey(u.CityId ?? 0) ? Cities[u.CityId ?? 0].State : "Not Detemine",
                    IsLocked = u.LockoutEnd.HasValue ? true : false
                }).ToListAsync();

                var CacheOptions = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60), SlidingExpiration = TimeSpan.FromMinutes(30) };
                _cache.Set("DeliveryList", Deliveries, CacheOptions);
            }
            model.States = await _context.States.ToListAsync();

            //filter => State
            for (int i = 1; i <= model.States.Count; i++)
            {
                if (State == i)
                {
                    Deliveries = Deliveries.Where(x => x.AssignedState == model.States[i - 1].Name).ToList();
                    model.ChosenState = model.States[i - 1].Name;
                    model.ChosenStateId = i;
                }
            }

            //Search
            if (!string.IsNullOrEmpty(searchtext))
            {
                searchtext = searchtext.Trim().ToLower();
                Deliveries = Deliveries.Where(x => x.Email.ToLower().Contains(searchtext) || x.Phone.ToLower().Contains(searchtext) || x.AssignedCity.ToLower().Contains(searchtext) || x.AssignedState.ToLower().Contains(searchtext)).ToList();
            }
            model.searchtext = searchtext;


            //Order by
            switch (orderby)
            {
                case 1:
                    Deliveries = Deliveries.OrderBy(x => x.Email).ToList();
                    model.orderby = 1;
                    break;
                case 2:
                    Deliveries = Deliveries.OrderBy(x => x.Phone).ToList();
                    model.orderby = 2;
                    break;
                case 3:
                    Deliveries = Deliveries.OrderByDescending(x => x.JoinedDate).ToList();
                    model.orderby = 3;
                    break;
                case 4:
                    Deliveries = Deliveries.OrderBy(x => x.AssignedCity).ToList();
                    model.orderby = 4;
                    break;
            }

            //Pagination
            model.CurrentPage = currentpage;
            model.PageSize = 20;
            model.DeliveryCount = Deliveries.Count();
            model.TotalPages = (int)Math.Ceiling((decimal)model.DeliveryCount / model.PageSize);
            model.UserDetailsInUserLists = Deliveries.Skip((model.CurrentPage - 1) * model.PageSize).Take(model.PageSize).ToList();

            return model;
        }




        //Private ==> Make Users Style for Excel 
        private async Task<List<UserDetailsInUserList>> makeDeliveries(string? searchtext, int? orderby, int State)
        {
            if (!_cache.TryGetValue("DeliveryList", out List<UserDetailsInUserList> Deliveries))
            {
                var DeliveryRole = await _context.Roles.FirstOrDefaultAsync(x => x.Name == "delivery");
                var Cities = await _context.Cities.Include(x => x.State).ToDictionaryAsync(x => x.Id, x => new { Name = x.Name, State = x.State.Name });
                Cities.Add(0, new { Name = "Not Determine", State = "Not Determine" });

                Deliveries = await _context.UserRoles.Where(x => x.RoleId == DeliveryRole.Id).Join(_context.Users, ur => ur.UserId, u => u.Id, (ur, u) => new UserDetailsInUserList
                {
                    Id = u.Id,
                    Email = u.FirstName + "" + u.LastName,
                    Phone = u.PhoneNumber,
                    JoinedDate = u.CreatedDate ?? DateTime.MaxValue,
                    AssignedCity = Cities.ContainsKey(u.CityId ?? 0) ? Cities[u.CityId ?? 0].Name : "Not Detemine",
                    AssignedState = Cities.ContainsKey(u.CityId ?? 0) ? Cities[u.CityId ?? 0].State : "Not Detemine",
                    IsLocked = u.LockoutEnd.HasValue ? true : false
                }).ToListAsync();

                var CacheOptions = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60), SlidingExpiration = TimeSpan.FromMinutes(30) };
                _cache.Set("DeliveryList", Deliveries, CacheOptions);
            }
            var States = await _context.States.ToListAsync();

            //filter => State
            for (int i = 1; i <= States.Count; i++)
            {
                if (State == i)
                {
                    Deliveries = Deliveries.Where(x => x.AssignedState == States[i - 1].Name).ToList();
                }
            }

            //Search
            if (!string.IsNullOrEmpty(searchtext))
            {
                searchtext = searchtext.Trim().ToLower();
                Deliveries = Deliveries.Where(x => x.Email.ToLower().Contains(searchtext) || x.Phone.ToLower().Contains(searchtext) || x.AssignedCity.ToLower().Contains(searchtext) || x.AssignedState.ToLower().Contains(searchtext)).ToList();
            }

            //Order by
            Deliveries = orderby switch
            {
                1 => Deliveries.OrderBy(x => x.Email).ToList(),
                2 => Deliveries.OrderBy(x => x.Phone).ToList(),
                3 => Deliveries.OrderByDescending(x => x.JoinedDate).ToList(),
                4 => Deliveries.OrderBy(x => x.AssignedCity).ToList(),
                _ => Deliveries.ToList()
            };
            return Deliveries.ToList();
        }


        //Make Excel file
        public async Task<XLWorkbook> MakeDeliveriesExcelFileAsync(string searchtext, int? orderby, int State)
        {
            var Users = await makeDeliveries(searchtext, orderby, State);
            var workUser = new XLWorkbook();
            var worksheet = workUser.Worksheets.Add("Users");
            var currentRow = 1;

            worksheet.Cell(currentRow, 1).Value = "User no";
            worksheet.Cell(currentRow, 2).Value = "Email";
            worksheet.Cell(currentRow, 3).Value = "Phone";
            worksheet.Cell(currentRow, 4).Value = "AssignedCity";
            worksheet.Cell(currentRow, 5).Value = "Joined Date";

            foreach (var user in Users)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = user.Id;
                worksheet.Cell(currentRow, 2).Value = user.Email;
                worksheet.Cell(currentRow, 3).Value = user.Phone;
                worksheet.Cell(currentRow, 4).Value = user.AssignedCity;
                worksheet.Cell(currentRow, 5).Value = user.JoinedDate.ToString("MMM dd,yyyy");
            }

            return workUser;
        }




        //Change Delivery his City
        public async Task ChangeDeliveryCityAsync(string DeliveryId, int ChangedCityId)
        {
            var Delivery = await _userManager.FindByIdAsync(DeliveryId);
            Delivery.CityId = ChangedCityId;
            await _context.SaveChangesAsync();
            _cache.Remove("DeliveryList");
        }



        //Add New Delivery 
        public async Task AddNewDeliveryAsync(AddDeliveryVM model)
        {
            var Delivery = new ApplicationUser
            {
                Email = new MailAddress(model.Email).User,
                CityId = model.CityId,
                UserName = new MailAddress(model.Email).User,
                FirstName = model.Firstname,
                LastName = model.Lastname,
                PhoneNumber = model.Phone,
                CreatedDate = DateTime.Now
            };
            var result = await _userManager.CreateAsync(Delivery, model.Password);
            if (result.Succeeded)
                await _userManager.AddToRoleAsync(Delivery, "delivery");
            _cache.Remove("DeliveryList");
            _cache.Remove("UserList");
        }
        #endregion


        #region Reports ==>(Contact Page)
        //Report List Page
        public async Task<ReportListVM> GetReportDataAsync(string? searchtext, int status, int? orderby, int currentpage = 1)
        {
            var model = new ReportListVM();

            var Reports = await _context.Contacts.AsNoTracking().Select(a => new ReportDetailsInReportListVM
            {
                Id = a.Id,
                Name = a.Name,
                Email = a.Email,
                Date = a.CreatedAt.Value,
                Message = a.Message,
                Subject = a.Subject ?? "Not Determined",
                Status = a.Status
            }).ToListAsync();

            //filter => Status
            switch (status)
            {
                case 0:
                    Reports = Reports.ToList();
                    model.status = "0";
                    break;
                case 1:
                    Reports = Reports.Where(x => x.Status == ContactStatus.New.ToString()).ToList();
                    model.status = "1";
                    break;
                case 2:
                    Reports = Reports.Where(x => x.Status == ContactStatus.InProcess.ToString()).ToList();
                    model.status = "2";
                    break;
                case 3:
                    Reports = Reports.Where(x => x.Status == ContactStatus.Replied.ToString()).ToList();
                    model.status = "3";
                    break;
            }

            //Search
            if (!string.IsNullOrEmpty(searchtext))
            {
                searchtext = searchtext.Trim().ToLower();
                Reports = Reports.Where(x => x.Email.ToLower().Contains(searchtext) || x.Subject.ToLower().Contains(searchtext) || x.Name.ToLower().Contains(searchtext) || x.Status.ToLower().Contains(searchtext)).ToList();
            }
            model.searchtext = searchtext;


            //Order by
            switch (orderby)
            {
                case 1:
                    Reports = Reports.OrderBy(x => x.Id).ToList();
                    model.orderby = 1;
                    break;
                case 2:
                    Reports = Reports.OrderBy(x => x.Name).ToList();
                    model.orderby = 2;
                    break;
                case 3:
                    Reports = Reports.OrderBy(x => x.Email).ToList();
                    model.orderby = 3;
                    break;
                case 4:
                    Reports = Reports.OrderBy(x => x.Subject).ToList();
                    model.orderby = 4;
                    break;
                case 5:
                    Reports = Reports.OrderByDescending(x => x.Date).ToList();
                    model.orderby = 4;
                    break;
                case 6:
                    Reports = Reports.OrderBy(x => x.Status).ToList();
                    model.orderby = 4;
                    break;
            }

            //Pagination
            model.CurrentPage = currentpage;
            model.PageSize = 20;
            model.ReportCount = Reports.Count();
            model.TotalPages = (int)Math.Ceiling((decimal)model.ReportCount / model.PageSize);
            model.ReportDetailsInReportLists = Reports.Skip((model.CurrentPage - 1) * model.PageSize).Take(model.PageSize).ToList();

            return model;
        }



        //Private ==> Make Orders Style for Excel 
        private async Task<List<ReportDetailsInReportListVM>> makeReports(string? searchtext, int status, int? orderby)
        {
            var Reports = await _context.Contacts.AsNoTracking().Select(a => new ReportDetailsInReportListVM
            {
                Id = a.Id,
                Name = a.Name,
                Email = a.Email,
                Date = a.CreatedAt.Value,
                Message = a.Message,
                Subject = a.Subject ?? "Not Determined",
                Status = a.Status
            }).ToListAsync();

            //filter => Status
            Reports = status switch
            {
                0 => Reports.ToList(),
                1 => Reports.Where(x => x.Status == ContactStatus.New.ToString()).ToList(),
                2 => Reports.Where(x => x.Status == ContactStatus.InProcess.ToString()).ToList(),
                3 => Reports.Where(x => x.Status == ContactStatus.Replied.ToString()).ToList(),
                _ => Reports.ToList()
            };

            //Search
            if (!string.IsNullOrEmpty(searchtext))
            {
                searchtext = searchtext.Trim().ToLower();
                Reports = Reports.Where(x => x.Email.ToLower().Contains(searchtext) || x.Subject.ToLower().Contains(searchtext) || x.Name.ToLower().Contains(searchtext) || x.Status.ToLower().Contains(searchtext)).ToList();
            }


            //Order by


            Reports = orderby switch
            {
                1 => Reports.OrderBy(x => x.Id).ToList(),
                2 => Reports.OrderBy(x => x.Name).ToList(),
                3 => Reports.OrderBy(x => x.Email).ToList(),
                4 => Reports.OrderBy(x => x.Subject).ToList(),
                5 => Reports.OrderByDescending(x => x.Date).ToList(),
                6 => Reports.OrderBy(x => x.Status).ToList(),
                _ => Reports.ToList()
            };
            
            return Reports.ToList();
        }



        //Make Excel file
        public async Task<XLWorkbook> MakeReportsExcelFileAsync(string searchtext, int? orderby, int status)
        {
            var Reports = await makeReports(searchtext, status, orderby);
            var workReport = new XLWorkbook();
            var worksheet = workReport.Worksheets.Add("Report");
            var currentRow = 1;

            worksheet.Cell(currentRow, 1).Value = "Report no";
            worksheet.Cell(currentRow, 2).Value = "Name";
            worksheet.Cell(currentRow, 3).Value = "Email";
            worksheet.Cell(currentRow, 4).Value = "Subject";
            worksheet.Cell(currentRow, 5).Value = "Message";
            worksheet.Cell(currentRow, 6).Value = "Date";
            worksheet.Cell(currentRow, 7).Value = "Status";

            foreach (var report in Reports)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = report.Id;
                worksheet.Cell(currentRow, 2).Value = report.Name;
                worksheet.Cell(currentRow, 3).Value = report.Email;
                worksheet.Cell(currentRow, 4).Value = report.Subject;
                worksheet.Cell(currentRow, 5).Value = report.Message;
                worksheet.Cell(currentRow, 6).Value = report.Date.ToString("MMM dd ,yyyy");
                worksheet.Cell(currentRow, 7).Value = report.Status;
            }

            return workReport;
        }



        //Get Report Details By Report Id
        public async Task<ReportDetailsVM> GetReportDetailsAsync(int ReportId)
        {
            var report = await _context.Contacts.FindAsync(ReportId);

            var model = new ReportDetailsVM
            {
                ReportId = report.Id,
                Date = report.CreatedAt ?? DateTime.MinValue,
                Email = report.Email,
                Name = report.Name,
                Subject = report.Subject ?? "Not Determined",
                Message = report.Message,
                Status = report.Status
            };

            if(report.Status == ContactStatus.New.ToString())
            {
                report.Status = ContactStatus.InProcess.ToString();
                _context.Contacts.Update(report);
                await _context.SaveChangesAsync();
            }

            return model;
        }



        //Make Response form Report Details Page
        public async Task MakeReportResponseAsync(UserEmailOptions model, int ReportId)
        {
            await _emailRepo.SendEmail(model);
            var report = await _context.Contacts.FindAsync(ReportId);

            if (report.Status == ContactStatus.InProcess.ToString())
            {
                report.Status = ContactStatus.Replied.ToString();
                _context.Contacts.Update(report);
                await _context.SaveChangesAsync();
            }

        }



        //Send Email to Specific People 
        public async Task SendEmailToUserOrUsersAsync(string? RecieveEmails, string selectedRole, string Subject, string Body, IFormFile Attachment)
        {
            List<string> ToUsers = new List<string>();
            List<string> Emails;
            IdentityRole UserRole;
            switch (selectedRole)
            {
                case "None":
                    ToUsers.Add(RecieveEmails);
                    break;
                case "All":
                    Emails = await _context.Users.Select(a => a.Email + "@gmail.com").ToListAsync();
                    ToUsers.AddRange(Emails);
                    break;
                case "User":
                    UserRole = _context.Roles.FirstOrDefault(x => x.Name == "user");
                    Emails = _context.UserRoles.Where(x => x.RoleId == UserRole.Id).Join(_context.Users, ur => ur.UserId , u => u.Id ,(ur,u) => u.Email+ "@gmail.com").ToList();
                    ToUsers.AddRange(Emails);
                    break;
                case "Admin":
                    UserRole = _context.Roles.FirstOrDefault(x => x.Name == "admin");
                    Emails = _context.UserRoles.Where(x => x.RoleId == UserRole.Id).Join(_context.Users, ur => ur.UserId, u => u.Id, (ur, u) => u.Email + "@gmail.com").ToList();
                    ToUsers.AddRange(Emails);
                    break;
                case "Delivery":
                    UserRole = _context.Roles.FirstOrDefault(x => x.Name == "delivery");
                    Emails = _context.UserRoles.Where(x => x.RoleId == UserRole.Id).Join(_context.Users, ur => ur.UserId, u => u.Id, (ur, u) => u.Email + "@gmail.com").ToList();
                    ToUsers.AddRange(Emails);
                    break;
            }

            UserEmailOptions model = new UserEmailOptions
            {
                RecieveEmails = ToUsers,
                Attachment = Attachment,
                Body = Body,
                Subject = Subject
            };
            await _emailRepo.SendEmail(model);
        }

        #endregion
    }
}
