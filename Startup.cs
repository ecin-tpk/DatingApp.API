using System;
using AutoMapper;
using DatingApp.API.Helpers;
using DatingApp.API.Hubs;
using DatingApp.API.Middleware;
using DatingApp.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace DatingApp.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options => options.SuppressAsyncSuffixInActionNames = false)
                .AddNewtonsoftJson(opt => { opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; });
            // For development, remember to set mode in launchSettings
            //services.AddDbContext<DataContext>(x => x.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));
            // For production, remember to set mode in launchSettings
            services.AddDbContext<DataContext>(x => x.UseMySql("server=localhost;port=3306;user=datingappuser;password=2tZSN[F&L)23y=Aw;database=datingapp"));
            services.AddCors();
            services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings"));
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddScoped<IFacebookService, FacebookService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<ILikeService, LikeService>();
            services.AddScoped<IInterestService, InterestService>();
            services.AddScoped<IChartService, ChartService>();

            services.AddSignalR();
            services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseCors(x => x
            .SetIsOriginAllowed(origin => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
            app.UseMiddleware<ErrorHandlerMiddleware>();
            app.UseMiddleware<JwtMiddleware>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<MessagesHub>("/hubs/messages");
                endpoints.MapHub<NotificationHub>("/hubs/notification");
            });
        }
    }
}
