# ShopNet — ASP.NET Core MVC E-Commerce Sample

A working e-commerce sample built with **ASP.NET Core MVC (.NET 8)**, **Entity Framework Core**, and
**SQL Server**. Users register/log in (ASP.NET Core Identity), browse products, add real items to a
database-backed cart, and check out into an order — stock is decremented for real, per user, per product.

## Features

- Product catalog with search + category filter, stock-aware "Add to Cart"
- Real, persistent cart: one row per user/product in SQL Server (not session/cookies)
- Quantity updates, remove line items, stock validation on add and at checkout
- Checkout converts the cart into an `Order` + `OrderItem`s, decrements product stock, clears the cart
- Order history per user
- ASP.NET Core Identity for registration/login (email + password)

## Project layout

```
ECommerceApp/
  Controllers/       HomeController, CartController, OrdersController, AccountController
  Data/               ApplicationDbContext (EF Core + Identity), DbInitializer (seed data)
  Models/             Product, Cart, CartItem, Order, OrderItem, ApplicationUser
  Views/              Razor views (Home, Cart, Orders, Account, Shared/_Layout)
  wwwroot/            site.css, placeholder image
  Program.cs          App startup, DI, Identity + EF Core configuration
  appsettings.json    Connection string
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, which ships with Visual Studio, works out of the box — or point the connection
  string at any SQL Server instance, including a Docker container)

## Setup & run

1. **Restore packages**
   ```bash
   cd ECommerceApp
   dotnet restore
   ```

2. **Set your connection string** in `appsettings.json` if you're not using LocalDB:
   ```json
   "DefaultConnection": "Server=YOUR_SERVER;Database=ECommerceAppDb;User Id=...;Password=...;TrustServerCertificate=True"
   ```

3. **Create the database with EF Core migrations**
   ```bash
   dotnet tool install --global dotnet-ef   # if you don't have it
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```
   (`Program.cs` also calls `db.Database.Migrate()` on startup, so once a migration exists it will
   apply automatically each run.)

4. **Run**
   ```bash
   dotnet run
   ```
   Then open the URL shown in the console (typically `https://localhost:5001` or similar).

5. **Try it out**
   - Sign up for an account (`/Account/Register`)
   - Browse products on the home page and click **Add to Cart**
   - Go to **Cart** to adjust quantities or remove items
   - Enter a shipping address and **Checkout** — this creates a real `Order`, decrements stock, and
     empties your cart
   - View **My Orders** to see order history

The database is seeded with 8 sample products the first time it's created (see `Data/DbInitializer.cs`)
— edit that file to change or add your own catalog.

## Notes / next steps you may want to add

- Checkout currently marks orders as `Paid` immediately (no real payment gateway is wired up) — plug in
  Stripe/PayPal in `CartController.Checkout` if you need real payments.
- Product images point at a placeholder — swap `ImageUrl` values in `DbInitializer` for real image URLs
  or wire up file upload.
- No admin UI yet for managing products/orders — everything is seeded via `DbInitializer`. A simple
  `[Authorize(Roles = "Admin")]` `AdminController` with CRUD views would be the natural next addition.
