// Program.cs

using SuperStudentAIHub.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. HOME CONTROLLER için GEREKLİ OLAN ESKİ KAYDI GERİ GETİRME
// Bu kayıt, HomeController'ın IChatGptService bağımlılığını çözer.
builder.Services.AddSingleton<IChatGptService, ChatGptService>(); 

// 2. VISUALS CONTROLLER için SİZİN ŞEMA Servisi Kaydı (YENİ EKLEME)
// Bu kayıt, VisualsController'ın ISchematicService bağımlılığını çözer.
builder.Services.AddScoped<ISchematicService, SchematicGeneratorService>();

// 3. ARKADAŞINIZIN Image Servisi Kaydı (DEĞİŞMEDİ)
builder.Services.AddSingleton<IImageService, HuggingFaceImageService>();

// Geri kalan uygulama yapılandırması aynı kalır
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ... (Diğer tüm pipeline kodları aynı kalır)

app.UseHttpsRedirection();
app.UseRouting(); 
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();