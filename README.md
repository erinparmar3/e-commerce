# ShopNet — Multi-Vendor E-Commerce Marketplace

ShopNet is a full-featured, production-ready **Multi-Vendor E-Commerce Marketplace** built with **ASP.NET Core (.NET 10)**, **C#**, **PostgreSQL**, **Entity Framework Core**, **Bootstrap 5.3**, and **Razor Views (MVC)** alongside a complete **RESTful Web API** documented with **Swagger/OpenAPI**.

The platform is designed to emulate modern e-commerce marketplaces like Amazon or Flipkart: independent merchants can apply for shop accounts, upload products with device pictures, and manage inventory and order fulfillment, while buyers enjoy a responsive shopping experience with filtering, instant wishlist toggling, multi-vendor cart handling, checkout, and order tracking.

---

## 🌟 Key Features

### 🛍️ Storefront & Buyer Experience
* **Modern Commercial UI**: Designed with a Slate Navy (`#0F172A`) and Sapphire Blue (`#2563EB`) palette with clean typography (`Inter`), customer utility strips, and trust badges.
* **Catalog Browsing & Search**: Real-time keyword search, department filtering, price range controls, and sorting (price low-to-high, high-to-low, newest).
* **Instant Wishlist**: In-place AJAX heart toggling with visual state updates without full page reloads.
* **Multi-Vendor Shopping Cart**: Automatic subtotal recalculation, item quantity adjustments, standard 10% tax computation, and stock limitation checks.
* **Secure Checkout Flow**: Shipping address capture, simulated payment options (Credit/Debit Card, Instant UPI / QR, Net Banking), and order confirmation.
* **Order Tracking & Printable Invoices**: Visual 4-step status timeline (`Pending` ➔ `Confirmed` ➔ `Shipped` ➔ `Delivered`) and native printable invoice receipts.
* **Verified Purchase Reviews**: Only buyers who ordered and received a product can submit verified star ratings and feedback.

### 🏪 Seller Portal
* **Vendor Application Workflow**: Prospective sellers apply through `/seller/apply`; shops remain pending until vetted by an administrator.
* **Merchant Dashboard**: Real-time business KPIs (listed products count, gross sales volume, pending orders count).
* **Local Device Image Upload**: Direct image file upload (`.jpg`, `.jpeg`, `.png`, `.webp` up to 5 MB) from desktop/mobile with instant client-side preview and server-side GUID isolation.
* **Independent Inventory Control**: Vendors can only view and edit their own products; cross-vendor modification is blocked at the service level (HTTP 403).
* **Order Fulfillment**: Sellers update the shipment lifecycle of customer orders containing their items.

### 🛡️ Admin Panel & Security
* **Role-Based Access Control (RBAC)**: Strict separation of privileges across `ADMIN`, `SELLER`, and `BUYER`.
* **Registration Protection**: Public registration is locked to the `BUYER` role to prevent unauthorized privilege escalation.
* **Vendor Approval Queue**: 1-click review, approval, or rejection of pending merchant applications.
* **Category Taxonomy Manager**: Add, edit, or deactivate product categories across the marketplace.
* **User Management**: Activate or suspend buyer and merchant accounts platform-wide.
* **Password Security**: Salted adaptive password hashing using **BCrypt** (`workFactor: 11`).
* **CSRF & XSS Safeguards**: Every POST form is fortified with `@Html.AntiForgeryToken()` and validated with `[ValidateAntiForgeryToken]`.

---

## 🏛️ Architecture & Tech Stack

```
┌─────────────────────────────────────────────────────────────┐
│                       SHOPNET PLATFORM                      │
├──────────────────────────────┬──────────────────────────────┤
│       Razor MVC Views        │     REST Web API (/api/*)    │
│  (HTML5 / Bootstrap 5.3 SSR) │    (JSON Payloads / Swagger) │
├──────────────────────────────┴──────────────────────────────┤
│           Dual Authentication Scheme (JWT + Cookie)         │
├─────────────────────────────────────────────────────────────┤
│         Controllers Layer (ViewControllers & ApiControllers)│
├─────────────────────────────────────────────────────────────┤
│       Business Services Layer (Cart, Order, Checkout, etc.) │
├─────────────────────────────────────────────────────────────┤
│      Data Access Layer (Entity Framework Core 10 / Npgsql)  │
├─────────────────────────────────────────────────────────────┤
│                 PostgreSQL Relational Database              │
└─────────────────────────────────────────────────────────────┘
```

* **Framework:** ASP.NET Core (.NET 10)
* **Language:** C# 13
* **Database:** PostgreSQL
* **ORM:** Entity Framework Core (Code-First Migrations)
* **Authentication:** Dual Scheme (`CookieAuthentication` for browser sessions + `JwtBearer` for REST API clients)
* **API Documentation:** Swagger / OpenAPI with Bearer Token integration
* **Testing:** xUnit Automated Unit Test Suite (`ECommerceApp.Tests`)

---

## 📂 Project Structure

```
├── Common/                  # Cross-cutting utilities, exceptions, PasswordHasher (BCrypt), Claim extensions
├── Controllers/             # MVC View controllers and RESTful Web API controllers
├── Data/                    # ApplicationDbContext (EF Core) and DbInitializer (seed data)
├── DTOs/                    # Data Transfer Objects for requests and responses
├── Interfaces/              # Contracts for business logic services (Dependency Injection)
├── Middleware/              # Global ExceptionHandlingMiddleware (HTML error pages & JSON API errors)
├── Migrations/              # EF Core Code-First PostgreSQL database migrations
├── Models/                  # Domain entities (User, SellerProfile, Product, Category, Order, Cart, etc.)
├── Services/                # Business logic implementation (Cart, Checkout, Order, Product, Seller, etc.)
├── Views/                   # Razor MVC views organized by feature area
│   ├── Account/             # Login, Register, Profile, Access Denied
│   ├── Admin/               # Dashboard, Seller Approvals, Categories, Users, Orders
│   ├── Cart/                # Shopping Cart view
│   ├── Checkout/            # Checkout address/payment form and Confirmation receipt
│   ├── Home/                # Storefront catalog and Product Details with Reviews
│   ├── Notifications/       # Order updates and notifications
│   ├── Orders/              # Order history, timeline tracking, and invoice view
│   ├── Seller/              # Vendor Dashboard and Product upload/edit forms
│   └── Shared/              # Master layout (_Layout.cshtml) with announcement bar & footer
├── wwwroot/                 # Static web assets (CSS, JS, Bootstrap Icons, uploads/products)
└── ECommerceApp.Tests/      # Automated unit tests covering core domain business rules
```

---

## 👥 Seed Accounts & Demo Credentials

On first run, the database automatically provisions the following test accounts:

| Role | Email | Password | Shop / Details |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@marketplace.com` | `AdminPassword123!` | System Administrator (Full platform access) |
| **Seller** | `seller1@techhub.com` | `SellerPassword123!` | **TechHub Official** (Electronics merchant) |
| **Seller** | `seller2@fashiontrend.com`| `SellerPassword123!` | **Urban Threads Store** (Fashion merchant) |
| **Seller** | `seller3@homekitchen.com` | `SellerPassword123!` | **KitchenPrime** (Home & Kitchen merchant) |
| **Buyer** | `buyer1@gmail.com` | `BuyerPassword123!` | Aarav Sharma (Verified customer) |
| **Buyer** | `buyer2@gmail.com` | `BuyerPassword123!` | Priya Patel (Verified customer) |

---

## 🚀 Getting Started

### 1. Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/)
* [PostgreSQL](https://www.postgresql.org/) (Running locally or hosted)

### 2. Configure Database Connection
Update the connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=ecommerce-DB;Username=postgres;Password=your_password"
}
```

### 3. Run Migrations & Launch Application
Open your terminal in the project directory:

```bash
# Build the project
dotnet build

# Run unit tests
dotnet test ECommerceApp.Tests/ECommerceApp.Tests.csproj

# Run the marketplace web application
dotnet run --urls "http://localhost:5005;http://localhost:5000"
```

### 4. Access the Platform
* **Web Storefront:** Open [http://localhost:5005](http://localhost:5005) or [http://localhost:5000](http://localhost:5000)
* **Swagger API Documentation:** Open [http://localhost:5005/swagger](http://localhost:5005/swagger)

---

## 📡 REST API Summary

| Area | Method | Endpoint | Description | Auth |
| :--- | :--- | :--- | :--- | :--- |
| **Auth** | `POST` | `/api/auth/register` | Register new buyer account | Public |
| **Auth** | `POST` | `/api/auth/login` | Authenticate and get JWT token | Public |
| **Auth** | `GET` | `/api/auth/me` | Fetch authenticated user profile | Bearer |
| **Products**| `GET` | `/api/products` | Browse catalog with filters & pagination | Public |
| **Products**| `GET` | `/api/products/{id}` | Retrieve product details | Public |
| **Cart** | `GET` | `/api/cart` | View current user's shopping cart | Bearer |
| **Cart** | `POST` | `/api/cart/items` | Add product to cart | Bearer |
| **Cart** | `DELETE`| `/api/cart/items/{id}`| Remove item from cart | Bearer |
| **Checkout**| `POST` | `/api/checkout` | Process checkout and place order | Bearer |
| **Orders** | `GET` | `/api/orders` | View user order history | Bearer |
| **Orders** | `POST` | `/api/orders/{id}/cancel`| Cancel pending order & restore stock | Bearer |
| **Seller** | `POST` | `/api/seller/products` | List new product | Seller |
| **Seller** | `GET` | `/api/seller/orders` | View incoming orders for vendor | Seller |
| **Admin** | `GET` | `/api/admin/dashboard`| Retrieve executive marketplace KPIs | Admin |
| **Admin** | `PUT` | `/api/admin/sellers/{id}/approve` | Approve merchant shop application | Admin |

---

## 📄 License

This project is developed for educational and portfolio demonstration purposes. All rights reserved.
