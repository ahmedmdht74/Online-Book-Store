# 📚 Online Book Store (ASP.NET MVC 9)

A full-featured online bookstore web application built with **ASP.NET MVC 5**, supporting full admin control, user interactions, secure checkout, and dynamic dashboards for both admins and users.

---

## 🔥 Main Features

### 👤 User Side

- 📖 Browse books by category, author, or search term
- ⭐ Add/remove favorites & leave reviews
- 🛒 Manage shopping cart with quantity controls
- 💳 Complete checkout using **Cash**, **Credit**, or **Stripe online payment**
- 📬 Get invoices via email
- 🔐 Register, login, manage profile, and change password

---

### 🧑‍💼 Admin Dashboard

- 📚 Manage books (CRUD, upload images, assign genres/authors)
- 🏷️ Manage genres and authors
- 🚚 Manage orders with status tracking (Pending → Processing → Shipped → Delivered)
- 👥 User & Role Management (Assign roles, reset passwords, lock/unlock accounts)
- 🗺️ Manage states, cities, and delivery agents
- 📝 View and respond to contact messages and reports
- 📊 Download Excel reports for books, users, orders, genres, cities, deliveries, and more

---

## 🧠 Architecture

### 🔸 Controllers

- `HomeController` – Public pages (Home, Search, Book Details, Cart, etc.)
- `AccountController` – User auth & password management
- `DashboardController` – Admin panel for managing all resources
- `UserDashboardController` – User dashboard (Orders, Favorites, Profile)

### 🔸 Repositories

- `IAccountRepo` – Auth, register, login, logout, change password
- `IHomeRepo` – Book details, search, cart, checkout (incl. Stripe integration)
- `IDashboardRepo` – Book/Author/Genre/User/Order/City/State/Delivery/Report management
- `IUserDashboardRepo` – User orders, favorites, invoices, profile
- `IEmailRepo` – Email handling

---

## 🗂️ Models Overview

Includes domain models like:

- `Book`, `Author`, `Genre`, `Order`, `OrderDetails`
- `Cart`, `CartDetails`, `Favourite`, `Rating`
- `ApplicationUser`, `Contact`, `City`, `State`, `Stock`
- `OrderStatus`, `Delivery`, and more

---

## 🖥️ Views Structure

### 📄 Public/User Views

- `Login`, `Register`, `Search`, `BookDetails`, `CartDetails`, `Checkout`, `Contact`, `Landing`, `About`

### 🧑‍💼 Admin Views (`/Views/Dashboard/`)

- `HomeDash`, `BookList`, `AddOrEditBook`, `GenreList`, `AuthorList`
- `OrderList`, `OrderDetails`, `UserList`, `UserDetails`
- `CityList`, `StateList`, `DeliveryList`, `ReportList`, `ReportDetails`, `MakeAnnounce`

### 👤 User Dashboard Views (`/Views/UserDashboard/`)

- `UserDashboard`, `UserOrders`, `UserOrderDetails`, `UserFavorites`, `UserProfile`

---

## 🛠️ Tech Stack

- **ASP.NET MVC 9**
- **Entity Framework (Code-First)**
- **SQL Server**
- **Bootstrap + jQuery + AJAX**
- **Stripe API** for online payment
- **PdfSharp** for generating PDF invoices

---

## 🧾 How to Run

1. Clone the repo  
2. Update your `Web.config` with your SQL Server connection string  
3. Run DB migrations or use an initializer to seed data  
4. Set up Stripe keys in config (for payment)  
5. Run the project in Visual Studio  

---

## ✅ Highlights

- AJAX-based dynamic cart & dashboard interactions  
- Role-based authorization with full admin control  
- Export data to Excel (Books, Users, Orders, etc.)  
- Custom invoice PDF generation  
- Email notifications and report replies  
- Review system with stars and comments  
- Dynamic cities/states for shipping management  

---



## 📬 Contact

Made with ❤️ by **Ahmed Mdht**  
📧 [ahmedmdht74@gmail.com]
