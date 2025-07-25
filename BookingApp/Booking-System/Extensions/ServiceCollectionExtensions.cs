using Booking_System.Business_Logic.Interfaces;
using Booking_System.Business_Logic.Services;
using Booking_System.Data;
using Booking_System.Data_Access.Interfaces;
using Booking_System.Data_Access.Repositories;
using Booking_System.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Booking_System.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Database configuration
            services.AddDatabaseServices(configuration);

            // Authentication and authorization
            services.AddAuthenticationServices(configuration);

            // Application services
            services.AddBusinessServices();

            // Repositories
            services.AddRepositories();

            // API configuration
            services.AddApiServices();

            // CORS configuration
            services.AddCorsServices();

            return services;
        }

        private static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddHttpClient();

            return services;
        }

        private static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddJwtAuthentication(configuration);

            return services;
        }

        private static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ITokenService, JwtTokenService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<ICouponService, CouponService>();
            services.AddScoped<ITicketTypeService, TicketTypeService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<ImageService, ImageService>();

            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IEventTicketTypeRepository, EventTicketTypeRepository>();
            services.AddScoped<ICouponRepository, CouponRepository>();
            services.AddScoped<IUserCouponRepository, UserCouponRepository>();
            services.AddScoped<ITicketTypeRepository, TicketTypeRepository>();

            return services;
        }

        private static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            services.AddSignalR();
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerDocumentation();

            return services;
        }

        private static IServiceCollection AddCorsServices(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://127.0.0.1:5500", "http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            return services;
        }
    }
}
