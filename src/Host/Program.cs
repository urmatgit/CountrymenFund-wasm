using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
var allowOrigins = builder.Configuration.GetValue<string>("AllowOrigins");
builder.Services.AddCors(options =>
{
options.AddPolicy("CorsPolicy", builder =>
{
    builder.WithOrigins(allowOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowAnyOrigin()
      .AllowCredentials();

});
    
    
    options.AddPolicy("AllowHeaders", builder =>
    {
        builder.WithOrigins(allowOrigins)
                .WithHeaders(HeaderNames.ContentType, HeaderNames.Server, HeaderNames.AccessControlAllowHeaders, HeaderNames.AccessControlExposeHeaders, "x-custom-header", "x-path", "x-record-in-use", HeaderNames.ContentDisposition);
    });
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
  //  app.UseHsts();
    //app.UseForwardedHeaders(new ForwardedHeadersOptions
    //{
    //    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto 
    //});
    
}

//app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.UseCors("CorsPolicy");
app.MapFallbackToFile("index.html");

app.Run();