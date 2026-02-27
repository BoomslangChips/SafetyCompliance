using SafetyCompliance.Application.Interfaces;
using SafetyCompliance.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace SafetyCompliance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IPlantService, PlantService>();
        services.AddScoped<ISectionService, SectionService>();
        services.AddScoped<IEquipmentService, EquipmentService>();
        services.AddScoped<IInspectionService, InspectionService>();
        services.AddScoped<ISetupService, SetupService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<IIssueService, IssueService>();

        return services;
    }
}
