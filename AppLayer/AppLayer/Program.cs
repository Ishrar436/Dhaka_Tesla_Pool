// AppLayer/Program.cs
using BLL.Interfaces;
using BLL.Services;
using BLL.Settings;
using DAL.EF;
using DAL.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi;
using AppLayer.Middleware;
using BLL.Seeding;

var builder = WebApplication.CreateBuilder(args);

// --- Configuration ---
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;

// --- DbContext ---
builder.Services.AddDbContext<TeslaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Dbconn")));

// --- DAL ---
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// --- BLL ---
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IZoneService, ZoneService>();
builder.Services.AddScoped<IFareService, FareService>();
builder.Services.AddScoped<IRideStateMachineService, RideStateMachineService>();
builder.Services.AddScoped<IPoolingService, PoolingService>();
builder.Services.AddScoped<IRideRequestService, RideRequestService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IPoolLifecycleService, PoolLifecycleService>();
builder.Services.AddScoped<DbSeeder>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// --- Auth ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
    });

builder.Services.AddAuthorization();

// --- API essentials ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Dhaka Tesla Pool API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste only the JWT token. Swagger adds the 'Bearer ' prefix itself."
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var app = builder.Build();

app.UseExceptionHandler();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<DbSeeder>().SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();