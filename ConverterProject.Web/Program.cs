using ConverterProject.Web.Services;
using ConverterProject.Web.Services.Providers;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]

var builder = WebApplication.CreateBuilder(args);

// TODO: storage se bude automaticky vybírat podle toho jestli jsme v release nebo debug
// Choose between "local" or "blob" storage
//builder.Configuration.GetValue<string>("StorageType");

builder.Configuration.GetConnectionString("BlobStorageAccount");
builder.Services.AddControllersWithViews();

// Using session for cookies.
builder.Services.AddDistributedMemoryCache();
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.ConsentCookieValue = "true";
    // Cookies will be available only via HTTPS
    options.Secure = CookieSecurePolicy.Always;
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Our services in right order.
//builder.Services.AddTransient<IFileService, BlobService>();
builder.Services.AddTransient<IFileService, LocalFileService>();
builder.Services.AddSingleton<PdfConverterProperties>();
builder.Services.AddTransient<PdfConverterService>();
builder.Services.AddSingleton<PdfQueueService>();
builder.Services.AddSingleton<FileManagerService>();

var app = builder.Build();

// Inicializing this manually for clearing all leftover files on startup.
app.Services.GetService<FileManagerService>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseRouting();
app.UseAuthorization();
app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();

