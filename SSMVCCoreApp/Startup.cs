using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SSMVCCoreApp.Infrastructure.Abstract;
using SSMVCCoreApp.Infrastructure.Concrete;
using SSMVCCoreApp.Infrastructure.Services;

namespace SSMVCCoreApp
{
  public class Startup
  {
    private readonly IConfiguration _configuration;
    private readonly IHostingEnvironment _hostingEnvironment;

    public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
    {
      _configuration = configuration;
      _hostingEnvironment = hostingEnvironment;
    }
    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
      services.Configure<StorageUtility>(_configuration.GetSection("StorageAccountInformation"));

      services.AddMvc(opt =>
      {
        if (_hostingEnvironment.IsProduction() && _configuration["DisableSSL"] != "true")
        {
          opt.Filters.Add(new RequireHttpsAttribute());
        }
      });//AddMvc

      services.AddDbContext<SportsStoreDbContext>(cfg =>
      {
        cfg.UseSqlServer(_configuration.GetConnectionString("SSConnection"), sqlServerOptionsAction: sqlOption =>
        {
          //This is for the Resilient Entity Framework Core SQL connections (Similar to SqlAzureExecutionStrategy in MVC5)
          sqlOption.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
        });
      });
      services.AddScoped<IProductRepository, EfProductRepository>();
      services.AddScoped<IPhotoService, PhotoService>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      app.UseFileServer();

      app.UseMvc(ConfigureRoutes);

      using (var scope = app.ApplicationServices.CreateScope())
      {
        var context = scope.ServiceProvider.GetRequiredService<SportsStoreDbContext>();
        context.Database.Migrate();
      }

      app.Run(async (context) =>
      {
        await context.Response.WriteAsync("Hello World!");
      });
    }

    private void ConfigureRoutes(IRouteBuilder routeBuilder)
    {
      routeBuilder.MapRoute("category", "/{category}", new { controller = "Product", action = "GetByCategory" });
      routeBuilder.MapRoute("Default", "{controller}/{action}/{id?}", new { controller = "Product", action = "Index" });
    }
  }
}
