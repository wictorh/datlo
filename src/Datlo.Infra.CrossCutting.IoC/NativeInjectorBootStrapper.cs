using Datlo.Application.Interfaces;
using Datlo.Application.Services;
using Datlo.Domain.Interfaces;
using Datlo.Domain.Notifications;
using Datlo.Infra.Data.Context;
using Datlo.Infra.Data.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Datlo.Infra.CrossCutting.IoC
{
    public class NativeInjectorBootStrapper
    {
        public static void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<AppDbContext>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped<IDataTypeRepository, DataTypeRepository>();

            services.AddScoped<INotifier, Notifier>();

            services.AddScoped<ICsvUploaderService, CsvUploaderService>();
            services.AddScoped<IDataTypeService, DataTypeService>();
        }
    }
}
