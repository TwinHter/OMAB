using OMAB.Infrastructure.Persistence;
using OMAB.Application;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using OMAB.Application.Cores;
using Microsoft.AspNetCore.Mvc;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();

// builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    // Ngăn ASP.NET Core tự động phản hồi lỗi 400 khi Model không khớp
    options.SuppressModelStateInvalidFilter = true; 
});

builder.Services.AddSwaggerGen(static option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "OMAB API", Version = "v1" });
    option.CustomSchemaIds(type => type.ToString().Replace("+", "."));

    option.AddSecurityDefinition("UserIdHeader", new OpenApiSecurityScheme
    {
        Description = "Nhập User Id muốn giả lập (Ví dụ: 1, 5, 10...). Nếu không nhập sẽ mặc định là 1.",
        Name = "x-user-id",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Name = "x-user-id",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "UserIdHeader"
                }
            },
            new string[] { }
        }
    });
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var initializer = scope.ServiceProvider
            .GetRequiredService<AppDbContextInitialise>();

        await initializer.InitialiseAsync();
        await initializer.SeedAsync(CancellationToken.None);
    }
    app.UseSwagger();
    app.UseSwaggerUI();
}
// app.UseHttpsRedirection();
app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var response = Result<string>.Failure("An unexpected error occurred.", 500);

        await context.Response.WriteAsJsonAsync(response);
    });
});
app.UseAuthorization();
app.MapControllers();
app.Run();