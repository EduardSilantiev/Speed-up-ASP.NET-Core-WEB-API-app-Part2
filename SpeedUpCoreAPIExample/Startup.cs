using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpeedUpCoreAPIExample.Contexts;
using SpeedUpCoreAPIExample.Interfaces;
using SpeedUpCoreAPIExample.Repositories;
using SpeedUpCoreAPIExample.Services;
using Microsoft.AspNetCore.Http;

namespace SpeedUpCoreAPIExample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        public IConfiguration Configuration { get; }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddHttpClient();

            services.AddDbContext<DefaultContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultDatabase")));

            //Cache
            services.AddDistributedRedisCache(options =>
            {
                options.InstanceName = Configuration.GetValue<string>("Redis:Name");
                options.Configuration = Configuration.GetValue<string>("Redis:Host");
            });

            services.AddScoped<IProductsRepository, ProductsRepository>();
            services.AddScoped<IPricesRepository, PricesRepository>();

            services.AddTransient<IProductsService, ProductsService>();
            services.AddTransient<IPricesService, PricesService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
