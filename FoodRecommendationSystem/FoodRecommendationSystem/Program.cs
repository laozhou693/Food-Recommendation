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

// 添加MongoDB配置
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDB"));

builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(builder.Configuration["MongoDB:ConnectionString"]));

builder.Services.AddSingleton<IMongoDatabase>(sp => {
var client = sp.GetRequiredService<IMongoClient>();
return client.GetDatabase(builder.Configuration["MongoDB:DatabaseName"]);
});

// 注册仓储
builder.Services.AddScoped<MerchantRepository>();
builder.Services.AddScoped<DishRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ReviewRepository>();
builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<UserHistoryRepository>();

// 注册爬虫服务
builder.Services.AddHttpClient<ICrawlerService, MeituanCrawlerProxy>();
builder.Services.AddHttpClient<ICrawlerService, ElementCrawlerProxy>();
builder.Services.AddHttpClient<ICrawlerService, DianpingCrawlerProxy>();

// 注册业务服务
builder.Services.AddScoped<IMerchantService, MerchantService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<CrawlerManager>();

// 注册爬虫定时任务服务
builder.Services.AddHostedService<CrawlerHostedService>();

// 添加认证
// 确保 JWT 配置存在
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

// 添加控制器
builder.Services.AddControllers()
    .AddJsonOptions(options => {
// 设置JSON序列化选项，如日期格式、忽略空值等
options.JsonSerializerOptions.PropertyNamingPolicy = null;
options.JsonSerializerOptions.WriteIndented = true;
});

// 添加CORS
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

// 添加Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
options.SwaggerDoc("v1", new OpenApiInfo
{
Title = "武汉同城美食推荐API",
Version = "v1",
Description = "武汉同城美食推荐平台API，支持爬取多平台数据和个性化推荐"
});

// 添加JWT验证配置
options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
Description = "JWT授权头使用Bearer方案。示例: \"Authorization: Bearer {token}\"",
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

// 使用XML注释
var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
if (File.Exists(xmlPath))
{
options.IncludeXmlComments(xmlPath);
}
});

var app = builder.Build();

// 配置HTTP请求管道
if (app.Environment.IsDevelopment())
{
app.UseSwagger();
app.UseSwaggerUI(c => {
c.SwaggerEndpoint("/swagger/v1/swagger.json", "武汉同城美食推荐API v1");
c.RoutePrefix = "swagger";
});
app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 初始化应用数据
InitializeAppData(app);

app.Run();

// 初始化应用数据的函数
void InitializeAppData(WebApplication app)
{
// 使用一个作用域服务来初始化数据
using var scope = app.Services.CreateScope();

try
{
// 示例：初始化基本的分类数据
var categoryRepo = scope.ServiceProvider.GetRequiredService<CategoryRepository>();
InitializeCategoriesAsync(categoryRepo).GetAwaiter().GetResult();

// 其他初始化代码...
}
catch (Exception ex)
{
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
logger.LogError(ex, "应用启动时初始化数据出错");
}
}

// 初始化分类数据
async Task InitializeCategoriesAsync(CategoryRepository categoryRepo)
{
// 检查是否已有分类数据
var existingCategories = await categoryRepo.GetAllAsync();
if (existingCategories.Count > 0)
return;

// 创建菜系分类
var cuisineCategories = new List<Category>
    {
        new Category { Name = "川菜", Type = CategoryType.Cuisine, Description = "四川传统菜系，以麻辣著称", SortOrder = 1 },
        new Category { Name = "湘菜", Type = CategoryType.Cuisine, Description = "湖南传统菜系，以香辣著称", SortOrder = 2 },
        new Category { Name = "粤菜", Type = CategoryType.Cuisine, Description = "广东传统菜系，以清淡著称", SortOrder = 3 },
        new Category { Name = "鄂菜", Type = CategoryType.Cuisine, Description = "湖北传统菜系，荤素搭配", SortOrder = 4 },
        new Category { Name = "江浙菜", Type = CategoryType.Cuisine, Description = "江浙地区菜系，精致细腻", SortOrder = 5 },
        new Category { Name = "东北菜", Type = CategoryType.Cuisine, Description = "东北地区菜系，菜量大", SortOrder = 6 },
        new Category { Name = "西北菜", Type = CategoryType.Cuisine, Description = "西北地区菜系，以面食为主", SortOrder = 7 }
    };

// 创建商家类型分类
var merchantCategories = new List<Category>
    {
        new Category { Name = "火锅店", Type = CategoryType.MerchantType, Description = "提供各种火锅", SortOrder = 1 },
        new Category { Name = "小吃店", Type = CategoryType.MerchantType, Description = "提供各种小吃", SortOrder = 2 },
        new Category { Name = "烧烤店", Type = CategoryType.MerchantType, Description = "提供各种烧烤", SortOrder = 3 },
        new Category { Name = "快餐店", Type = CategoryType.MerchantType, Description = "提供快速简餐", SortOrder = 4 },
        new Category { Name = "西餐厅", Type = CategoryType.MerchantType, Description = "提供西方菜系", SortOrder = 5 },
        new Category { Name = "甜品店", Type = CategoryType.MerchantType, Description = "提供各种甜品", SortOrder = 6 },
        new Category { Name = "饮品店", Type = CategoryType.MerchantType, Description = "提供各种饮品", SortOrder = 7 }
    };

// 创建菜品类型分类
var dishCategories = new List<Category>
    {
        new Category { Name = "主食", Type = CategoryType.DishType, Description = "米饭、面条等", SortOrder = 1 },
        new Category { Name = "凉菜", Type = CategoryType.DishType, Description = "冷盘前菜", SortOrder = 2 },
        new Category { Name = "热菜", Type = CategoryType.DishType, Description = "热食菜品", SortOrder = 3 },
        new Category { Name = "汤类", Type = CategoryType.DishType, Description = "各种汤品", SortOrder = 4 },
        new Category { Name = "小吃", Type = CategoryType.DishType, Description = "各种零食小吃", SortOrder = 5 },
        new Category { Name = "甜点", Type = CategoryType.DishType, Description = "甜品点心", SortOrder = 6 },
        new Category { Name = "饮品", Type = CategoryType.DishType, Description = "各种饮料", SortOrder = 7 }
    };

// 批量创建分类
await categoryRepo.CreateManyAsync(cuisineCategories);
await categoryRepo.CreateManyAsync(merchantCategories);
await categoryRepo.CreateManyAsync(dishCategories);
}

// MongoDB配置类
public class MongoDBSettings
{
    public required string ConnectionString { get; set; } 
    public required string DatabaseName { get; set; }
}