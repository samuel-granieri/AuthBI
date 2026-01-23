using AuthBI.Models.Domain;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

var mongoSettings = builder.Configuration.GetSection("MongoDbSettings");

//Configure Identity to use MongoDB
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddErrorDescriber<PtBrIdentityErrorDescriber>()
    .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>(
        mongoSettings["ConnectionString"],
        mongoSettings["DatabaseName"])
    .AddDefaultTokenProviders();

//configurar cookie de autenticação
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "AuthBI.Cookie";
    options.Cookie.HttpOnly = true;

    // Login / Logout
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";

    //Tempo de vida do cookie
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);

    //Renovar automaticamente enquanto o usuário navega
    options.SlidingExpiration = true;

    // HTTPS (produção)
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});


//Customize password settings
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 4;

    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers = true;
});


// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
