using SafetyCompliance.Application;
using SafetyCompliance.Application.Interfaces;
using SafetyCompliance.Infrastructure;
using SafetyCompliance.Web.Components;
using SafetyCompliance.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddScoped<IPhotoStorageService, LocalPhotoStorageService>();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthentication()
    .AddCookie();
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(SafetyCompliance.Shared.UI._Imports).Assembly);

app.Run();
