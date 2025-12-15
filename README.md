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

- **Framework**: ASP.NET Core Web API (.NET 8.0)
- **Authentication**: JWT (JSON Web Tokens)  
- **Authorization**: Role-based authorization with ASP.NET Core Identity
- **Architecture**: Clean Architecture (N-tier)
  - **API Layer**: Controllers and endpoints
  - **Application Layer**: Services, DTOs, and business logic
  - **Domain Layer**: Entities and core business models
  - **Infrastructure Layer**: Data access, repositories, and external services
- **Database**: MS SQL Server with Entity Framework Core 8.0
- **ORM**: Entity Framework Core  
- **API Documentation**: Swagger/OpenAPI
- **Logging**: Built-in ASP.NET Core logging  
- **File Upload**: Support for image uploads with validation

## 📁 Project Structure

```
BookingApp/
├── Booking-System.API/          # Web API Layer (Controllers, Middleware)
├── Booking-System.Application/  # Application Logic (Services, DTOs, Interfaces)
├── Booking-System.Domain/       # Domain Entities
└── Booking-System.Infrastructure/ # Data Access (Repositories, DbContext)
```

## 🚦 Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- SQL Server (LocalDB or SQL Server Express)
- Visual Studio 2022 or VS Code with C# extension
- Entity Framework Core tools

### Installation

1. **Clone the repository**
   
   ```bash
   git clone https://github.com/ZiadHesham225/Booking-System.git
   cd Booking-System\BookingApp
   ```

2. **Install dependencies**
   
   ```bash
   dotnet restore
   ```

3. **Configure the database**
   - Update the connection string in `Booking-System.API/appsettings.json`:
     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Server=.;Database=BookingDb;Integrated Security=True;Trust Server Certificate=True"
     }
     ```
   - Navigate to the API project and apply migrations:
     ```bash
     cd Booking-System.API
     dotnet ef database update
     ```

4. **Configure JWT settings**
   - The JWT settings are already configured in `appsettings.json`:
     ```json
     "JWT": {
       "Secret": "59c40f6086286987cb4ef17ebbf0bf9fbf5bc9c8909ebfd979f8b30636bc4f1c",
       "ValidIssuer": "https://localhost:7189",
       "ValidAudience": "https://localhost:3000"
     }
     ```

5. **Run the application**
   ```bash
   dotnet run --project Booking-System.API/Booking-System.API.csproj
   ```
   Or from the API directory:
   ```bash
   cd Booking-System.API
   dotnet run
   ```
   
6. **Access the API**
   - Swagger UI: `http://localhost:5178/swagger/index.html`
   - API Base URL: `http://localhost:5178`
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

To enable email features (e.g., password reset), configure the following in `Booking-System.API/appsettings.json`:

```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "SenderEmail": "your-email@gmail.com",
  "SenderPassword": "your-app-specific-password"
}
```

**Note**: For Gmail, use an [App Password](https://support.google.com/accounts/answer/185833) instead of your regular password.

## 🗃️ Database Schema

The system includes the following main entities:
- **Users**: User accounts with ASP.NET Identity
- **Events**: Event details with categories and locations
- **Bookings**: User booking records
- **Coupons**: Discount coupon management
- **Categories**: Event categorization
- **TicketTypes**: Different ticket types (Standard, VIP, Student)
- **EventTicketTypes**: Junction table linking events and ticket types with pricing

## 🧪 Data Seeding

The database is automatically seeded with default data on first run:

### Default Users
- **Admin**
  - Email: `admin@booking.com`
  - Password: `Admin123!`
  - Role: Admin
- **Regular User**
  - Email: `user@booking.com`
  - Password: `User123!`
  - Role: User

### Seeded Data
- **User Roles**: `Admin`, `User`
- **Ticket Types**: `Standard`, `VIP`, `Student`
- **Event Categories**: `Concert`, `Workshop`, `Seminar`, `Tech Talk`

## 📝 API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh-token` - Refresh JWT token
- `POST /api/auth/revoke-token` - Revoke refresh token
- `POST /api/auth/forgot-password` - Request password reset
- `POST /api/auth/reset-password` - Reset password

### Events
- `GET /api/events` - Get all events (with pagination & filters)
- `GET /api/events/{id}` - Get event by ID
- `POST /api/events` - Create event (Admin only)
- `PUT /api/events/{id}` - Update event (Admin only)
- `DELETE /api/events/{id}` - Delete event (Admin only)

### Bookings
- `GET /api/bookings` - Get user bookings
- `POST /api/bookings` - Create booking
- `DELETE /api/bookings/{id}` - Cancel booking

### Coupons
- `GET /api/coupon` - Get all coupons (Admin only)
- `POST /api/coupon` - Create coupon (Admin only)
- `POST /api/coupon/validate` - Validate coupon code
- `PUT /api/coupon/{id}/toggle` - Toggle coupon status (Admin only)

### Categories & Ticket Types
- `GET /api/categories` - Get all categories
- `GET /api/tickettype` - Get all ticket types
- Category and ticket type management (Admin only)

For detailed API documentation, visit the Swagger UI at `/swagger` when running the application.

## ✉️ Email Configuration

To enable email features (e.g., password reset), configure the following in `Booking-System.API/appsettings.json`:

```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "SenderEmail": "your-email@gmail.com",
  "SenderPassword": "your-app-specific-password"
}
```

**Note**: For Gmail, use an [App Password](https://support.google.com/accounts/answer/185833) instead of your regular password.

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

- ✉️ Email notifications for bookings and confirmations
- 💳 Payment gateway integration (Stripe, PayPal)
- 🔔 Real-time availability updates with SignalR
- 📱 Mobile app support (iOS/Android)
- 📊 Advanced reporting and analytics dashboard
- 🌍 Multi-language support (i18n)
- 📧 Email verification for new registrations
- 🔍 Enhanced search with Elasticsearch
- 📅 Calendar view for events
- ⭐ Event ratings and reviews

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the project
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License.

## 👤 Author

**Ziad Hesham**
- GitHub: [@ZiadHesham225](https://github.com/ZiadHesham225)

## 📞 Support

For support, email your-email@example.com or open an issue in the repository.

---

<p align="center">Built with ❤️ using ASP.NET Core</p>
