using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UserManagement.BLL.Services;
using UserManagement.DAL;
using UserManagement.DAL.EntityFramework;


var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("UserManagement_Task-4");
// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<DataAccess>();
builder.Services.AddScoped<UserServices>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddDistributedMemoryCache(); // Adds a default in-memory implementation of IDistributedCache
builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

builder.Services.AddDbContext<IApplicationDBContext,ApplicationDBContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
 
    if (!dbContext.Database.CanConnect())
    {
        throw new NotImplementedException("Cannot connect to the db");
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


app.UseAuthorization();

// Add the following line before app.Run() to resolve the issue.  
app.UsePathBase("/");
app.MapGet("/", context =>
{
    context.Response.Redirect("/login");
    return Task.CompletedTask;
});
app.MapControllerRoute(
   name: "default",
   pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
