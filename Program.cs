using ErJobPortal.Repositories;
using ErJobPortal.Data;
using ErJobPortal.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// MVC
builder.Services.AddControllersWithViews();
// shrirang 15/09/26
builder.Services.AddScoped<EmailService>();
//Database
builder.Services.AddScoped<DbConnection>();

//Repository
builder.Services.AddScoped<AccountRepository>();
builder.Services.AddScoped<CandidateProfileRepository>();

// Register HttpClientFactory
builder.Services.AddHttpClient();

//session
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Exception handling
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
// Middleware
//app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
// IMPORTANT
app.UseSession();

app.UseAuthorization();
// Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
