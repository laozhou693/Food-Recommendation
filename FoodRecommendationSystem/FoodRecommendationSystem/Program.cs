using FoodRecommendationSystem.DAL.Repositories;
using FoodRecommendationSystem.Services.Implementations;
using FoodRecommendationSystem.Services.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FoodRecommendationSystem.Services;
using Microsoft.OpenApi.Models;
using System.Reflection;
using FoodRecommendationSystem.Core.Models;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// ���MongoDB����
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDB"));

builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(builder.Configuration["MongoDB:ConnectionString"]));

builder.Services.AddSingleton<IMongoDatabase>(sp => {
var client = sp.GetRequiredService<IMongoClient>();
return client.GetDatabase(builder.Configuration["MongoDB:DatabaseName"]);
});

// ע��ִ�
builder.Services.AddScoped<MerchantRepository>();
builder.Services.AddScoped<DishRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ReviewRepository>();
builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<UserHistoryRepository>();

// ע���������
builder.Services.AddHttpClient<ICrawlerService, MeituanCrawlerProxy>();
builder.Services.AddHttpClient<ICrawlerService, ElementCrawlerProxy>();
builder.Services.AddHttpClient<ICrawlerService, DianpingCrawlerProxy>();

// ע��ҵ�����
builder.Services.AddScoped<IMerchantService, MerchantService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<CrawlerManager>();

// ע�����涨ʱ�������
builder.Services.AddHostedService<CrawlerHostedService>();

// �����֤
// ȷ�� JWT ���ô���
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Key is not configured. Please set the 'Jwt:Key' in application settings.");
}

if (string.IsNullOrEmpty(jwtIssuer))
{
    throw new InvalidOperationException("JWT Issuer is not configured. Please set the 'Jwt:Issuer' in application settings.");
}

if (string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("JWT Audience is not configured. Please set the 'Jwt:Audience' in application settings.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// ��ӿ�����
builder.Services.AddControllers()
    .AddJsonOptions(options => {
// ����JSON���л�ѡ������ڸ�ʽ�����Կ�ֵ��
options.JsonSerializerOptions.PropertyNamingPolicy = null;
options.JsonSerializerOptions.WriteIndented = true;
});

// ���CORS
builder.Services.AddCors(options =>
{
options.AddDefaultPolicy(
    policy =>
{
policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
      .AllowAnyHeader()
      .AllowAnyMethod();
});
});

// ���Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
options.SwaggerDoc("v1", new OpenApiInfo
{
Title = "�人ͬ����ʳ�Ƽ�API",
Version = "v1",
Description = "�人ͬ����ʳ�Ƽ�ƽ̨API��֧����ȡ��ƽ̨���ݺ͸��Ի��Ƽ�"
});

// ���JWT��֤����
options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
Description = "JWT��Ȩͷʹ��Bearer������ʾ��: \"Authorization: Bearer {token}\"",
Name = "Authorization",
In = ParameterLocation.Header,
Type = SecuritySchemeType.ApiKey,
Scheme = "Bearer"
});

options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });

// ʹ��XMLע��
var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
if (File.Exists(xmlPath))
{
options.IncludeXmlComments(xmlPath);
}
});

var app = builder.Build();

// ����HTTP����ܵ�
if (app.Environment.IsDevelopment())
{
app.UseSwagger();
app.UseSwaggerUI(c => {
c.SwaggerEndpoint("/swagger/v1/swagger.json", "�人ͬ����ʳ�Ƽ�API v1");
c.RoutePrefix = "swagger";
});
app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ��ʼ��Ӧ������
InitializeAppData(app);

app.Run();

// ��ʼ��Ӧ�����ݵĺ���
void InitializeAppData(WebApplication app)
{
// ʹ��һ���������������ʼ������
using var scope = app.Services.CreateScope();

try
{
// ʾ������ʼ�������ķ�������
var categoryRepo = scope.ServiceProvider.GetRequiredService<CategoryRepository>();
InitializeCategoriesAsync(categoryRepo).GetAwaiter().GetResult();

// ������ʼ������...
}
catch (Exception ex)
{
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
logger.LogError(ex, "Ӧ������ʱ��ʼ�����ݳ���");
}
}

// ��ʼ����������
async Task InitializeCategoriesAsync(CategoryRepository categoryRepo)
{
// ����Ƿ����з�������
var existingCategories = await categoryRepo.GetAllAsync();
if (existingCategories.Count > 0)
return;

// ������ϵ����
var cuisineCategories = new List<Category>
    {
        new Category { Name = "����", Type = CategoryType.Cuisine, Description = "�Ĵ���ͳ��ϵ������������", SortOrder = 1 },
        new Category { Name = "���", Type = CategoryType.Cuisine, Description = "���ϴ�ͳ��ϵ������������", SortOrder = 2 },
        new Category { Name = "����", Type = CategoryType.Cuisine, Description = "�㶫��ͳ��ϵ�����嵭����", SortOrder = 3 },
        new Category { Name = "����", Type = CategoryType.Cuisine, Description = "������ͳ��ϵ�����ش���", SortOrder = 4 },
        new Category { Name = "�����", Type = CategoryType.Cuisine, Description = "���������ϵ������ϸ��", SortOrder = 5 },
        new Category { Name = "������", Type = CategoryType.Cuisine, Description = "����������ϵ��������", SortOrder = 6 },
        new Category { Name = "������", Type = CategoryType.Cuisine, Description = "����������ϵ������ʳΪ��", SortOrder = 7 }
    };

// �����̼����ͷ���
var merchantCategories = new List<Category>
    {
        new Category { Name = "�����", Type = CategoryType.MerchantType, Description = "�ṩ���ֻ��", SortOrder = 1 },
        new Category { Name = "С�Ե�", Type = CategoryType.MerchantType, Description = "�ṩ����С��", SortOrder = 2 },
        new Category { Name = "�տ���", Type = CategoryType.MerchantType, Description = "�ṩ�����տ�", SortOrder = 3 },
        new Category { Name = "��͵�", Type = CategoryType.MerchantType, Description = "�ṩ���ټ��", SortOrder = 4 },
        new Category { Name = "������", Type = CategoryType.MerchantType, Description = "�ṩ������ϵ", SortOrder = 5 },
        new Category { Name = "��Ʒ��", Type = CategoryType.MerchantType, Description = "�ṩ������Ʒ", SortOrder = 6 },
        new Category { Name = "��Ʒ��", Type = CategoryType.MerchantType, Description = "�ṩ������Ʒ", SortOrder = 7 }
    };

// ������Ʒ���ͷ���
var dishCategories = new List<Category>
    {
        new Category { Name = "��ʳ", Type = CategoryType.DishType, Description = "�׷���������", SortOrder = 1 },
        new Category { Name = "����", Type = CategoryType.DishType, Description = "����ǰ��", SortOrder = 2 },
        new Category { Name = "�Ȳ�", Type = CategoryType.DishType, Description = "��ʳ��Ʒ", SortOrder = 3 },
        new Category { Name = "����", Type = CategoryType.DishType, Description = "������Ʒ", SortOrder = 4 },
        new Category { Name = "С��", Type = CategoryType.DishType, Description = "������ʳС��", SortOrder = 5 },
        new Category { Name = "���", Type = CategoryType.DishType, Description = "��Ʒ����", SortOrder = 6 },
        new Category { Name = "��Ʒ", Type = CategoryType.DishType, Description = "��������", SortOrder = 7 }
    };

// ������������
await categoryRepo.CreateManyAsync(cuisineCategories);
await categoryRepo.CreateManyAsync(merchantCategories);
await categoryRepo.CreateManyAsync(dishCategories);
}

// MongoDB������
public class MongoDBSettings
{
    public required string ConnectionString { get; set; } 
    public required string DatabaseName { get; set; }
}