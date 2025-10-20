using AsonDemo.Components;
using AsonDemo.Services;
using AsonDemo.State;
using Ason;
using Ason.CodeGen;
using AsonRunner;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.AI;
using System.Reflection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddSingleton<IAppDataService, InMemoryAppDataService>();
builder.Services.AddScoped<SessionState>();
builder.Services.AddScoped<DxThemesService>();

builder.Services.AddDevExpressBlazor(o => o.SizeMode = DevExpress.Blazor.SizeMode.Medium);
builder.Services.AddDevExpressAI();

var logFilePath = Path.Combine(builder.Environment.ContentRootPath, "AsonLogs.txt");

builder.Services.AddAson(
    defaultChatCompletionFactory: sp => new OpenAIChatCompletionService("gpt-4.1-mini", Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty),
    rootOperatorFactory: sp => sp.GetRequiredService<SessionState>().MainAppOperator,
    operators: new OperatorBuilder()
        .AddAssemblies(Assembly.GetExecutingAssembly(), typeof(RootOperator).Assembly)
        .AddExtractor()
        .Build(),
    configureOptions: opt => {
        opt.ExecutionMode = ExecutionMode.ExternalProcess;
        opt.MaxFixAttempts = 2;
        opt.SkipReceptionAgent = false;
        opt.LogHandler = (sender, e) => {
            try {
                // Only log if message starts with one of allowed prefixes
                var allowedPrefixes = new [] {
                    "ReceptionAgent created with instructions",
                    "ScriptAgent created with instructions",
                    "ReceptionAgent input",
                    "ReceptionAgent completed output",
                    "ScriptAgent input:",
                    "ScriptAgent outout",
                    "ScriptAgent reported impossibility",
                    "Execution error"
                };
                bool shouldLog = false;
                foreach (var prefix in allowedPrefixes) {
                    if (e.Message.StartsWith(prefix, StringComparison.Ordinal)) { shouldLog = true; break; }
                }
                if (!shouldLog) return;

                var line = $"### {e.Message}" + (e.Exception is not null ? "\n" + e.Exception : string.Empty) + "\n";
                File.AppendAllText(logFilePath, line);
            } catch { }
        };
    });

builder.Services.AddScoped<IChatClient>(sp => sp.GetService<AsonClient>()!);

var app = builder.Build();

if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();