using Blog.Config;
using Blog.Repository;
using Blog.utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer Authentication with JWT Token",
        Type = SecuritySchemeType.Http
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
});

// stripe
builder.Services.Configure<StripeSetting>(
    builder.Configuration.GetSection("StripeSettings"));

builder.Services.AddScoped<CustomerService>()
        .AddScoped<ChargeService>()
        .AddScoped<TokenService>()
        .AddScoped<StripeAppService, StripeAppService>();

// connect db
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("BlogDatabase"));

// repo
builder.Services.AddSingleton<UserRepo>();
builder.Services.AddSingleton<PostRepo>();
builder.Services.AddSingleton<LikeRepo>();
builder.Services.AddSingleton<CommentRepo>();
builder.Services.AddSingleton<CategoriesRepo>();
builder.Services.AddSingleton<BookingRepo>();
builder.Services.AddSingleton<OrderRepo>();

// jwt
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization();

// thêm log
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Thêm CORS middleware vào pipeline
builder.Services.AddCors();

var app = builder.Build();

// Sử dụng CORS middleware
app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

// Configure the application
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Đặt middleware CORS trước middleware UseAuthorization()
app.UseAuthentication();

// Sử dụng middleware UseAuthorization() sau middleware UseCors()
app.UseAuthorization();

app.MapControllers();

app.Run();