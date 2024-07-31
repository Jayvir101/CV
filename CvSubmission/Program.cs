//using CvSubmission.Configurations;
//using CvSubmission.Services;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Net.Http;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddControllersWithViews();

//// Configure HttpClient with BaseAddress
//builder.Services.AddHttpClient("UserApiClient", client =>
//{
//    client.BaseAddress = new Uri("http://localhost:5217/"); // Set this to the base address of your API
//});

//var openAiApiKey = builder.Configuration["OpenAI:ApiKey"];
//builder.Services.AddScoped<IOpenAIService, OpenAIService>();
//builder.Services.AddHttpClient("OpenAI", client =>
//{
//    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", openAiApiKey);
//});

//// Add logging services
//builder.Services.AddLogging(config =>
//{
//    config.AddConsole();
//    config.AddDebug();
//});

//builder.Services.AddHttpClient("UserApiClient")
//    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
//    {
//        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
//    });

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//}
//else
//{
//    app.UseExceptionHandler("/Home/Error");
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();

//app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=FormEntry}/{id?}");

//// Log that the application has started
//var logger = app.Services.GetRequiredService<ILogger<Program>>();
//logger.LogInformation("Application started.");

//app.Run();


using CvSubmission.Configurations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
            var settings = config.Build();
            GlobalSettings.Bind(settings);
        })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
