using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;
using ProgrammersBlog.Services.Extensions;
using ProgrammersBlog.Data.Concrete.EntitiyFramework.Contexts;
using ProgrammersBlog.Services.Abstract;
using ProgrammersBlog.Services.Concrete;
using ProgrammersBlog.Data.Concrete;
using ProgrammersBlog.Data.Abstract;
using ProgrammersBlog.Services.AutoMapper.Profiles;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.CookiePolicy;
using ProgrammersBlog.Mvc.AutoMapper.Profiles;
using Microsoft.AspNetCore.Authentication.Cookies;
using ProgrammersBlog.Entities.Concrete;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
});
builder.Services.AddSession();
builder.Services.AddAutoMapper(typeof(CategoryProfile), typeof(ArticleProfile), typeof(UserProfile));
builder.Services.LoadMyServices();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});





//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {

//        options.LoginPath = new PathString("/Admin/User/Login");
//        options.LogoutPath = new PathString("/Admin/User/Logout");
//        options.Cookie = new CookieBuilder
//        {
//            Name = "ProgrammersBlog",
//            HttpOnly = true,
//            SameSite = SameSiteMode.Strict,
//            SecurePolicy = CookieSecurePolicy.SameAsRequest // http üzerinden request geliýse responce https oolablýr


//        };

//        options.SlidingExpiration = true;
//        options.ExpireTimeSpan = System.TimeSpan.FromDays(7);// cooki bilgileri 7 gun býyunca kalabilr
//        options.AccessDeniedPath = new PathString("/Admin/User/AccessDenied");

//    });




builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();



builder.Services.ConfigureApplicationCookie(opt =>
{
    var cookieBuilder = new CookieBuilder();
    opt.Cookie = new CookieBuilder
    {
        Name = "ProgrammersBlog",
        HttpOnly = true,
        SameSite = SameSiteMode.Strict,
        SecurePolicy = CookieSecurePolicy.SameAsRequest // http üzerinden request geliýse responce https oolablýr


    };


    opt.LoginPath = new PathString("/Admin/User/Login");
    opt.Cookie = cookieBuilder;
    opt.ExpireTimeSpan = TimeSpan.FromDays(60);
    opt.SlidingExpiration = true;
    opt.LogoutPath = new PathString("/Admin/User/Logout");

  


});

var app = builder.Build();

app.UseSession();
app.UseStaticFiles();


app.UseHttpsRedirection();


app.UseRouting();// we must 2 method between rout and useendpoýnt

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.UseEndpoints(endpoints =>
//{


//    endpoints.MapAreaControllerRoute(
//        name: "Admin",
//        areaName: "Admin",
//        pattern: "Admin/{controller=Home}/{action=Index}/{id?}"
//        );

//    endpoints.MapDefaultControllerRoute();



//}
//);





//app.MapControllers();

app.Run();
