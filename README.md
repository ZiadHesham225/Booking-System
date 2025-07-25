# Booking System API

A comprehensive event booking system built with ASP.NET Core Web API, featuring user authentication, event management, booking functionality, and coupon system.

## 🚀 Features

### 🔐 Authentication & Authorization
- User registration and login  
- JWT token-based authentication  
- Refresh token mechanism  
- Password reset functionality  
- Role-based authorization (Admin/User)  
- Token revocation support  

### 📅 Event Management
- Event creation, updating, and deletion (Admin only)  
- Event search with advanced filters  
- Pagination support for event listings  
- Image upload support for events  
- Category-based event organization  
- Ticket type management per event  

### 🎫 Booking System
- Create and manage bookings  
- User-specific booking history  
- Booking status verification  
- Prevent duplicate bookings per event  
- Secure booking deletion  

### 🏷️ Coupon Management
- Create and manage discount coupons (Admin)  
- Coupon validation and usage tracking  
- User-specific coupon history  
- Toggle coupon active/inactive status  
- Prevent multiple usage of single-use coupons  

### 📊 Admin Dashboard
- Comprehensive dashboard with system statistics  
- Admin-only access controls  
- Event and user management capabilities  

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core Web API  
- **Authentication**: JWT (JSON Web Tokens)  
- **Authorization**: Role-based authorization  
- **Architecture**: N-tier Architecture
- **Database**: MS-SQL Server
- **ORM**: Entity Framework Core  
- **Logging**: Built-in ASP.NET Core logging  
- **File Upload**: Support for image uploads  

## 🚦 Getting Started

### Prerequisites
- .NET 6.0 or later  
- SQL Server (or your preferred database)  
- Visual Studio 2022 or VS Code  

### Installation

1. **Clone the repository**
   
   ```bash
   git clone https://github.com/ZiadHesham225/Booking-System.git
   cd Booking-System\BookingApp
   ```
3. **Install dependencies**
   
   ```bash
   dotnet restore
   ```
5. **Configure the database**
   - Update the connection string in `appsettings.json`
   - Run database migrations:
     
     ```bash
     dotnet ef database update
     ```
6. **Configure JWT settings**
   - Update the `JWT` section in `appsettings.json` as follows:
     
     ```json
     "JWT": {
      "Secret": "59c40f6086286987cb4ef17ebbf0bf9fbf5bc9c8909ebfd979f8b30636bc4f1c",
      "ValidIssuer": "https://localhost:7189",
      "ValidAudience": "https://localhost:3000"
      }
     ```
7. **Run the application**
   ```bash
   cd Booking-System
   dotnet run
   ```
  Then go to : `http://localhost:5178/swagger/index.html`
## 🔑 Authentication
This API uses JWT (JSON Web Tokens) for authentication. To access protected endpoints:
1. Register a new user or login with existing credentials
2. Use the returned JWT token in the `Authorization` header:
   
   ```makefile
   Authorization: Bearer <your-jwt-token>
   ```
### User Roles
- **User**: Can view events, create bookings, use coupons
- **Admin**: Full access to all endpoints including event/coupon management

## 🛡️ Security Features

- JWT Authentication: Secure token-based authentication
- Role-based Authorization: Different access levels for users and admins
- Input Validation: Comprehensive model validation
- Error Handling: Structured error responses
- Token Refresh: Automatic token renewal mechanism
- Password Reset: Secure password recovery process

## ✉️ Email Configuration
To enable email features (e.g., password reset), configure the following in your `appsettings.json`:
```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "SenderEmail": "Your Email",
  "SenderPassword": "Your password"
}
```
## 🧪 Data Seeding
This project includes default seed data for:
- Admin and regular users
- User roles (`Admin`, `User`)
- Ticket types (`Standard`, `VIP`, `Student`)
- Event categories (`Concert`, `Workshop`, `Seminar`, `Tech Talk`)
### 🧷 Usage
Default Users:
- **Admin**
  - Email: `admin@booking.com`
  - Password: `Admin123!`
- **User**
  - Email: `user@booking.com`
  - Password: `User123!`

## 🔮 Future Enhancements

- Email notifications for bookings
- Payment gateway integration
- Real-time availability updates
- Mobile app support
- Advanced reporting features
- Multi-language support
---

<p align="center">Built with ❤️ using ASP.NET Core</p>
