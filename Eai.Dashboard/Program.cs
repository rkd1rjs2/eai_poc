using Eai.Dashboard.Components;
using Eai.Infrastructure.Persistence.Repositories;
using Eai.Shared.Interfaces;
using Npgsql;
using StackExchange.Redis;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// --- 서비스 등록 영역 ---

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAntiforgery();

// 3. Redis 연결 객체 싱글톤 등록
string redisConnString = builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6380";
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
    ConnectionMultiplexer.Connect(redisConnString));

// 4. PostgreSQL 감사 로그 저장소 등록
string auditConnString = builder.Configuration.GetConnectionString("AuditDb") 
    ?? "Host=localhost;Database=eai_core_db;Username=eai_user;Password=eai_password";
builder.Services.AddScoped<IDbConnection>(sp => new NpgsqlConnection(auditConnString));
builder.Services.AddScoped<IAuditRepository, SqlAuditRepository>();

var app = builder.Build();

// --- 미들웨어 파이프라인 설정 영역 (Java의 Filter/Interceptor 설정과 비슷함) ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
//app.UseHttpsRedirection();

// 정적 파일(CSS, JS) 사용 설정
app.UseStaticFiles();

// 안티포저리 미들웨어 사용 (위에서 서비스를 등록했으므로 이제 정상 작동함)
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();