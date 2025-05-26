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

// �������
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// Program.cs �е� builder.Services.AddSwaggerGen ����
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "��ʳ�Ƽ�ϵͳ API",
        Version = "v1",
        Description = "�ṩ�û������̼Ҳ�ѯ��������ղع��ܵ� API",
        Contact = new OpenApiContact
        {
            Name = "�����Ŷ�",
            Email = "team@example.com"
        }
    });

    // ��� JWT ��֤
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT ��Ȩͷ��ʾ��: \"Authorization: Bearer {token}\"",
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

    // ���� XML ע��
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // �Զ������ ID ��������ʹ����׶�
    c.CustomOperationIds(apiDesc =>
    {
        return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
    });

    // ��ӷ����ǩ
    c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });

    // �Զ�����Ӧ
    c.DocumentFilter<SwaggerDefaultValues>();
});




// ��� MongoDB
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

    // ��Ӵ洢��
    builder.Services.AddSingleton<UserRepository>();
    builder.Services.AddSingleton<MerchantRepository>();
    builder.Services.AddSingleton<CategoryRepository>();

    // ��ӷ���
    builder.Services.AddSingleton<AuthService>();
    builder.Services.AddSingleton<FavoriteService>();
    builder.Services.AddSingleton<DataImportService>();

builder.Logging.AddFilter("MongoDB.Driver", LogLevel.Debug);

// ���� MongoDB ���л�
BsonSerializer.RegisterSerializer(new EnumSerializer<CategoryType>(BsonType.String));
// ���� Category ��ӳ��
if (!BsonClassMap.IsClassMapRegistered(typeof(Category)))
{
    BsonClassMap.RegisterClassMap<Category>(cm =>
    {
        cm.AutoMap();
        cm.SetIgnoreExtraElements(true); // �����ĵ��ж�����ֶ�
        cm.MapMember(c => c.Name).SetElementName("name");
        cm.MapMember(c => c.Type).SetElementName("type");
        cm.MapMember(c => c.Description).SetElementName("description");
        cm.MapMember(c => c.SortOrder).SetElementName("sortOrder");
    });
}

// ���� Python ·��
builder.Configuration.GetSection("PythonSettings").GetValue<string>("PythonPath");
    // ��� JWT ��֤
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            var jwtKey = builder.Configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT ��Կδ����");
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

// ��� CORS
builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });
// ���һ��Ĭ��ֵ�ĵ�������


var app = builder.Build();

    // ���� HTTP ����ܵ�
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

// ��ʼ������
await InitializeAppData(app);

    app.Run();

// ��ʼ��Ӧ������
// ��ʼ��Ӧ������
async Task InitializeAppData(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var categoryRepository = scope.ServiceProvider.GetRequiredService<CategoryRepository>();
    var dataImportService = scope.ServiceProvider.GetRequiredService<DataImportService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // ��ʼ����������
    await InitializeCategoriesAsync(categoryRepository);

    // ��ѡ���Զ������̼ұ�ǩ
    var autoImportTags = app.Configuration.GetValue<bool>("DataImport:AutoImportTags");
    if (autoImportTags)
    {
        logger.LogInformation("��ʼ�Զ������̼ұ�ǩ...");

        // �޸����ʹ�� ImportResult ��������ǽ⹹
        var result = await dataImportService.ImportMerchantTagsDirectlyAsync();

        if (result.Success)
        {
            logger.LogInformation("�Զ������̼ұ�ǩ�ɹ�: {Message}, ���� {Count} ����¼", result.Message, result.Count);
        }
        else
        {
            logger.LogWarning("�Զ������̼ұ�ǩʧ��: {Message}", result.Message);
        }
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
        new Category { Name = "������", Type = CategoryType.Cuisine, Description = "����������ϵ������ʳΪ��", SortOrder = 7 },
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
// ���һ��Ĭ��ֵ�ĵ�������
public class SwaggerDefaultValues : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Ϊ���в������Ĭ����Ӧ
        foreach (var path in swaggerDoc.Paths)
        {
            foreach (var operation in path.Value.Operations)
            {
                // ���ͨ�ô�����Ӧ
                if (!operation.Value.Responses.ContainsKey("400"))
                {
                    operation.Value.Responses.Add("400", new OpenApiResponse { Description = "������Ч" });
                }

                if (!operation.Value.Responses.ContainsKey("401"))
                {
                    operation.Value.Responses.Add("401", new OpenApiResponse { Description = "δ��Ȩ" });
                }

                if (!operation.Value.Responses.ContainsKey("500"))
                {
                    operation.Value.Responses.Add("500", new OpenApiResponse { Description = "����������" });
                }
            }
        }
    }
}
