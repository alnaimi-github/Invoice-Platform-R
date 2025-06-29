using FluentValidation;
using InvoiceProcessing.API.Mappings;
using InvoiceProcessing.API.Services.Interfaces;

namespace InvoiceProcessing.API;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();


        builder.Services.AddSwaggerGen();

        return builder;
    }

    public static WebApplicationBuilder AddDatabaseServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        return builder;
    }

    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        // AutoMapper
        builder.Services.AddAutoMapper(typeof(MappingProfile));

        // FluentValidation
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();

        // AWS S3
        builder.Services.AddAWSService<IAmazonS3>();

        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IInvoiceService, InvoiceService>();
        builder.Services.AddScoped<IFileStorageService, FileStorageService>();
        builder.Services.AddScoped<IXmlParserService, XmlParserService>();
        builder.Services.AddScoped<IExportService, ExportService>();

        return builder;
    }

    public static WebApplicationBuilder AddAuthenticationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "DefaultSecretKey123456789"))
                };
            });

        return builder;
    }

}