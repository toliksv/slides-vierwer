using SlidesViewer.Host.Contracts.Categories;
using SlidesViewer.Host.Repositories;
using SlidesViewer.Host.Rules;
using SlidesViewer.Host.Rules.Categories;
using SlidesViewer.Host.Services;

var builder = WebApplication.CreateBuilder(args);

// CORS for Web frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebFrontend", policy =>
    {
        policy.WithOrigins(
            builder.Configuration.GetValue<string>("WebFrontend:BaseUrl") ?? "http://localhost:5014",
            "http://localhost:5014",
            "https://localhost:5014"
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Kafka settings
builder.Services.Configure<KafkaSettings>(
    builder.Configuration.GetSection(KafkaSettings.SectionName));

// Analysis message publisher
builder.Services.AddSingleton<IAnalysisMessagePublisher, AnalysisMessagePublisher>();

// Repository
builder.Services.AddTransient<ISlideRepository, SlideRepository>();
builder.Services.AddTransient<IFolderRepository, FolderRepository>();

// Service
builder.Services.AddTransient<ISlideService, SlideService>();
builder.Services.AddTransient<IFolderService, FolderService>();

// Приоритетные правила (PCategoryRule, KCategoryRule, ObereskiCategoryRule, KSubstringCategoryRule)
builder.Services.AddSingleton<IRule, PCategoryRule>();
builder.Services.AddSingleton<IRule, KCategoryRule>();
builder.Services.AddSingleton<IRule, ObereskiCategoryRule>();
builder.Services.AddSingleton<IRule, KSubstringCategoryRule>();

// Правило по умолчанию — применяется после приоритетных, порядок регистрации не важен
builder.Services.AddSingleton<IMustApplyRule, DefaultCategoryRule>();

// Fallback-правило — всегда применяется последним, порядок регистрации не важен
builder.Services.AddSingleton<IFallbackRule, NoCategoryRule>();

// Keyed service для KCategoryRule
builder.Services.AddKeyedSingleton<IRule, DefaultCategoryRule>("DefaultCategory");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowWebFrontend");
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
