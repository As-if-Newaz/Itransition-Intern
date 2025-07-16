using Iforms.BLL.Services;
using Iforms.DAL;
using Iforms.DAL.Entity_Framework;
using Iforms.DAL.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Iforms.BLL.DTOs;
using Iforms.MVC.Controllers;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddControllersWithViews();
builder.Services.AddAutoMapper(typeof(Iforms.BLL.Services.TemplateProfile).Assembly);
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
        // Read JWT from cookie if present
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["JWTToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddScoped<DataAccess>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<TemplateService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<FormService>();
builder.Services.AddScoped<TagService>();
builder.Services.AddScoped<QuestionService>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<ApiTokenService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddSignalR();

builder.Services.AddHttpClient<SalesforceService>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>().GetSection("Salesforce");
    var clientId = config["ClientId"];
    var clientSecret = config["ClientSecret"];
    var username = config["Username"];
    var password = config["Password"];
    var tokenEndpoint = config["TokenEndpoint"];
    var apiBaseUrl = config["ApiBaseUrl"];
    // The SalesforceService constructor will be called by DI below
});
builder.Services.AddScoped<SalesforceService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>().GetSection("Salesforce");
    var clientId = config["ClientId"];
    var clientSecret = config["ClientSecret"];
    var username = config["Username"];
    var password = config["Password"];
    var tokenEndpoint = config["TokenEndpoint"];
    var apiBaseUrl = config["ApiBaseUrl"];
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient(nameof(SalesforceService));
    return new SalesforceService(httpClient, clientId, clientSecret, username, password, tokenEndpoint, apiBaseUrl);
});

builder.Services.AddDbContext<IApplicationDBContext, ApplicationDBContext>(options =>
    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("Iforms.DAL")));
           //.EnableSensitiveDataLogging() // Enables sensitive data logging for debugging
           //.LogTo(Console.WriteLine, LogLevel.Information)); // Logs SQL queries to the console

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add services to the container.

// Add this after builder is created, before app is built:
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDBContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseCors("AllowAllOrigins");
app.UseAuthentication();
app.UseAuthorization();
app.UsePathBase("/");
app.MapGet("/", context =>
{
    context.Response.Redirect("/Home/Index");
    return Task.CompletedTask;
});
app.MapControllerRoute(
   name: "default",
   pattern: "{controller=Auth}/{action=Login}/{id?}");
app.MapHub<QuestionHub>("/questionHub");
app.MapHub<Iforms.MVC.Controllers.TemplateHub>("/templateHub");

app.Run();
