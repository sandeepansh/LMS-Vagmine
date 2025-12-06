using System.Text.Json.Serialization;
using TMS.Repository;
using TMS.ViewModels;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
#if DEBUG
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
#else
 builder.Services.AddControllersWithViews();
#endif
builder.Services.Configure<ZoomSettings>(
    builder.Configuration.GetSection("ZoomSettings")
);
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});
builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = int.MaxValue;
});
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = int.MaxValue;
});
builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.Limits.MaxRequestBodySize = 209715200; // 200MB
});


builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option => option.LoginPath = "/auth/LoginOptional");
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddExpressiveAnnotations();
builder.Services.AddSession();

//Additional dependencies
builder.Services.AddRepositoryDependencies();

builder.Services.Configure<TMS.ViewModels.StorageSettings>(builder.Configuration.GetSection("StorageSettings"));
builder.Services.Configure<TMS.ViewModels.RateConfiguration>(builder.Configuration.GetSection("RateConfiguration"));

builder.Services.AddScoped<TMS.Web.Services.IViewRenderService, TMS.Web.Services.ViewRenderService>();

var app = builder.Build();

var defaultDateCulture = "en-IN";
var cultureInfo = new System.Globalization.CultureInfo(defaultDateCulture);
cultureInfo.NumberFormat.CurrencySymbol = "₹";
cultureInfo.DateTimeFormat.ShortDatePattern = "dd'/'MM'/'yyyy";

// Configure the Localization middleware
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(cultureInfo),
    SupportedCultures = new List<System.Globalization.CultureInfo>
    {
        cultureInfo,
    },
    SupportedUICultures = new List<System.Globalization.CultureInfo>
    {
        cultureInfo,
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
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
app.UseSession();

app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    if (response.StatusCode == StatusCodes.Status403Forbidden)
    {
        response.Redirect("/Home/UnauthorizedAccess");
    }
    else if (response.StatusCode == StatusCodes.Status404NotFound)
    {
        response.Redirect("/Home/ItemNotFound");
    }
    await Task.CompletedTask;
});

app.MapControllerRoute(
    name: "defaultArea",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.Run(async context =>
//{
//    context.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpMaxRequestBodySizeFeature>().MaxRequestBodySize = 200000000;
//});
app.Run();
