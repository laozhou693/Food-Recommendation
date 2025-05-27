using Food.Models;
using Food.Repository;
using Food.Service;
using MongoDB.Driver;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.IO;
using FoodRecommendationSystem.Controllers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

var builder = WebApplication.CreateBuilder(args);

// 添加配置
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// Program.cs 中的 builder.Services.AddSwaggerGen 部分
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "美食推荐系统 API",
        Version = "v1",
        Description = "提供用户管理、商家查询、分类和收藏功能的 API",
        Contact = new OpenApiContact
        {
            Name = "开发团队",
            Email = "team@example.com"
        }
    });

    // 添加 JWT 认证
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT 授权头。示例: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
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

    // 包含 XML 注释
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // 自定义操作 ID 生成器，使其更易读
    c.CustomOperationIds(apiDesc =>
    {
        return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
    });

    // 添加分组标签
    c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });

    // 自定义响应
    c.DocumentFilter<SwaggerDefaultValues>();
});




// 添加 MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("MongoDB");
        return new MongoClient(connectionString);
    });

    builder.Services.AddSingleton<IMongoDatabase>(sp =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        var client = sp.GetRequiredService<IMongoClient>();
        var databaseName = configuration.GetConnectionString("MongoDBName");
        return client.GetDatabase(databaseName);
    });

    // 添加存储库
    builder.Services.AddSingleton<UserRepository>();
    builder.Services.AddSingleton<MerchantRepository>();
    builder.Services.AddSingleton<CategoryRepository>();

    // 添加服务
    builder.Services.AddSingleton<AuthService>();
    builder.Services.AddSingleton<FavoriteService>();
    builder.Services.AddSingleton<DataImportService>();

builder.Logging.AddFilter("MongoDB.Driver", LogLevel.Debug);

// 配置 MongoDB 序列化
BsonSerializer.RegisterSerializer(new EnumSerializer<CategoryType>(BsonType.String));
// 配置 Category 类映射
if (!BsonClassMap.IsClassMapRegistered(typeof(Category)))
{
    BsonClassMap.RegisterClassMap<Category>(cm =>
    {
        cm.AutoMap();
        cm.SetIgnoreExtraElements(true); // 忽略文档中多余的字段
        cm.MapMember(c => c.Name).SetElementName("name");
        cm.MapMember(c => c.Type).SetElementName("type");
        cm.MapMember(c => c.Description).SetElementName("description");
        cm.MapMember(c => c.SortOrder).SetElementName("sortOrder");
    });
}

// 配置 Python 路径
builder.Configuration.GetSection("PythonSettings").GetValue<string>("PythonPath");
    // 添加 JWT 认证
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            var jwtKey = builder.Configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT 密钥未配置");
            }

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });

// 添加 CORS
builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });
// 添加一个默认值文档过滤器


var app = builder.Build();

    // 配置 HTTP 请求管道
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 初始化数据
await InitializeAppData(app);

    app.Run();

// 初始化应用数据
// 初始化应用数据
async Task InitializeAppData(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var categoryRepository = scope.ServiceProvider.GetRequiredService<CategoryRepository>();
    var dataImportService = scope.ServiceProvider.GetRequiredService<DataImportService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // 初始化分类数据
    await InitializeCategoriesAsync(categoryRepository);

    // 可选：自动导入商家标签
    var autoImportTags = app.Configuration.GetValue<bool>("DataImport:AutoImportTags");
    if (autoImportTags)
    {
        logger.LogInformation("开始自动导入商家标签...");

        // 修改这里：使用 ImportResult 对象而不是解构
        var result = await dataImportService.ImportMerchantTagsDirectlyAsync();

        if (result.Success)
        {
            logger.LogInformation("自动导入商家标签成功: {Message}, 新增 {Count} 条记录", result.Message, result.Count);
        }
        else
        {
            logger.LogWarning("自动导入商家标签失败: {Message}", result.Message);
        }
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
        new Category { Name = "西北菜", Type = CategoryType.Cuisine, Description = "西北地区菜系，以面食为主", SortOrder = 7 },
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
// 添加一个默认值文档过滤器
public class SwaggerDefaultValues : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // 为所有操作添加默认响应
        foreach (var path in swaggerDoc.Paths)
        {
            foreach (var operation in path.Value.Operations)
            {
                // 添加通用错误响应
                if (!operation.Value.Responses.ContainsKey("400"))
                {
                    operation.Value.Responses.Add("400", new OpenApiResponse { Description = "请求无效" });
                }

                if (!operation.Value.Responses.ContainsKey("401"))
                {
                    operation.Value.Responses.Add("401", new OpenApiResponse { Description = "未授权" });
                }

                if (!operation.Value.Responses.ContainsKey("500"))
                {
                    operation.Value.Responses.Add("500", new OpenApiResponse { Description = "服务器错误" });
                }
            }
        }
    }
}
