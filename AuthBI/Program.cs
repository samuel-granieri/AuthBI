using AuthBI.Infrastructure.WsManager;
using AuthBI.Models.Domain;
using AuthBI.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

//using var connection = new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"));
//await connection.OpenAsync();
//Console.WriteLine("Conectou!");

// --------------------------------------------------
// Configuration
// --------------------------------------------------
builder.Configuration.AddEnvironmentVariables();
var mongoSettings = builder.Configuration.GetSection("MongoDbSettings");

// --------------------------------------------------
// Services
// --------------------------------------------------
builder.Services.AddSingleton<DreCsvService>();
builder.Services.AddSingleton<WorkerConnectionManager>();
builder.Services.AddScoped<QueryService>();             // ? adicionar

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.User.RequireUniqueEmail = true;
})
.AddErrorDescriber<PtBrIdentityErrorDescriber>()
.AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>(
    mongoSettings["ConnectionString"],
    mongoSettings["DatabaseName"])
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "AuthBI.Cookie";
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddControllersWithViews();

// Configuração do Data Protection para persistir chaves em disco (importante para múltiplas instâncias)
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/dataprotection-keys"))
    .SetApplicationName("AuthBI");

// --------------------------------------------------
// App
// --------------------------------------------------
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets(new WebSocketOptions        // ? adicionar, antes do MapControllerRoute
{
    KeepAliveInterval = TimeSpan.FromSeconds(20)
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"
);

app.Run();