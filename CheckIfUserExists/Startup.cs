using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Extensions.Options;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Add services to the container.
        services.Configure<AzureFaceRecognitionSettings>(Configuration.GetSection("AzureFaceRecognitionService"));

        services.AddSingleton<IFaceClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<AzureFaceRecognitionSettings>>().Value;
            return new FaceClient(new ApiKeyServiceClientCredentials(settings.API_KEY)) { Endpoint = settings.API_URL };
        });

        services.AddLogging();
        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Configure the HTTP request pipeline.
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}


