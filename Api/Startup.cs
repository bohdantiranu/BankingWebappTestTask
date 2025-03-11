using BankingWebApp.Api.Configurations;
using BankingWebApp.Api.Data;
using BankingWebApp.Api.Data.Repository;
using BankingWebApp.Api.Data.Repository.Impl;
using BankingWebApp.Api.Dto.Requests;
using BankingWebApp.Api.HostedServices;
using BankingWebApp.Api.Mapping;
using BankingWebApp.Api.Middleware;
using BankingWebApp.Api.Services;
using BankingWebApp.Api.Services.Impl;
using BankingWebApp.Api.Validators;
using BankingWebApp.Auth.Extensions;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.OpenApi.Models;
using Serilog;


namespace BankingWebApp.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(mc => mc.AddProfile(new MappingProfile()));
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();

            services.AddAuth(Configuration);
            //
            services.Configure<MongoDbSettings>(options => Configuration.GetSection("MongoDB").Bind(options));
            services.AddSingleton<IDbContext, MongoDbContext>();

            services.AddHostedService<DbIndexCreationService>();

            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();

            services.AddScoped<IAccountsService, AccountsService>();
            services.AddScoped<ITransactionService, TransactionService>();

            services.AddFluentValidationAutoValidation();
            services.AddScoped<IValidator<CreateAccountRequest>, CreateAccountRequestValidator>();
            services.AddScoped<IValidator<TransactionRequest>, TransactionRequestValidator>();
            services.AddScoped<IValidator<TransferTransactionRequest>, TransferTransactiRequestValidator>();

            services.AddControllers();

            // Swagger/OpenAPI
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
             app.UseSwagger();
             app.UseSwaggerUI();

            app.UseMiddleware<ExceptionMiddleware>();
            //app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
