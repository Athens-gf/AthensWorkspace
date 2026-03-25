using AthensWorkspace.Data;
using AthensWorkspace.MHWs.Data;
using AthensWorkspace.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 開発環境以外（本番環境）の場合、すべてのネットワークインターフェースからのHTTPアクセスを許可する
if (!builder.Environment.IsDevelopment()) builder.WebHost.UseUrls("http://0.0.0.0");

var services = builder.Services;
var configuration = builder.Configuration;

var serverVersion = new MySqlServerVersion(new Version(8, 4, 2));

AddDbContext<MyIdentityDbContext>("LocalIdentity", "RemoteIdentity");
AddDbContext<MHWsDbContext>("LocalMHWs", "RemoteMHWs");

services.AddIdentity<OAuthUser, IdentityRole<int>>().AddEntityFrameworkStores<MyIdentityDbContext>();

var confExternalAuth = configuration.GetSection("ExternalAuth");

// 認証機能の構成
services.AddAuthentication(options =>
    {
        // デフォルトの認証・チャレンジ（ログイン要求）スキームをクッキーに設定
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        // ログインが必要な際の転送先URL
        options.LoginPath = "/login";
        // クッキーのSameSite属性（クロスサイトリクエストの挙動）を設定
        options.Cookie.SameSite = SameSiteMode.Lax;
        // 常にSecure属性（HTTPSのみ）を付与する
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    })
    .AddGoogle(options =>
    {
        // Google認証の設定情報を読み込み（ClientId/Secretがないとエラーを投げる）
        var conf = confExternalAuth.GetSection("Google");
        options.ClientId = conf["ClientId"] ?? throw new InvalidOperationException();
        options.ClientSecret = conf["ClientSecret"] ?? throw new InvalidOperationException();
    });

// MVC(コントローラーとビュー)の機能を有効化
services.AddControllersWithViews();
// 外部API呼び出し等に使う HttpClient を登録
services.AddHttpClient();

// Identityの設定：ユーザー名に使用できる文字を制限しない（日本語名などを許可するため空文字にする）
services.Configure<IdentityOptions>(options => options.User.AllowedUserNameCharacters = "");

// ルーティングの設定：生成されるURLをすべて小文字にする（SEOや統一感のため）
services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

var app = builder.Build();

// HTTPリクエストパイプライン（ミドルウェア）の設定
if (!app.Environment.IsDevelopment())
{
    // 本番環境：例外ハンドラーとHSTS（セキュリティ強化）を有効化
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();

    // Azure App Serviceやプロキシ経由のアクセスで、送信元のIPやプロトコル(HTTPS)を正しく認識するための設定
    var forwardedHeadersOptions = new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
        RequireHeaderSymmetry = false
    };
    forwardedHeadersOptions.KnownNetworks.Clear();
    forwardedHeadersOptions.KnownProxies.Clear();
    app.UseForwardedHeaders(forwardedHeadersOptions);
}

// HTTPアクセスをHTTPSに自動リダイレクト
app.UseHttpsRedirection();
// wwwroot フォルダ内の静的ファイル（CSS/JS等）を使えるようにする
app.UseStaticFiles();

// ルーティングを有効化
app.UseRouting();
// クッキー利用ポリシーの適用
app.UseCookiePolicy();
// 誰がアクセスしているか特定（認証）
app.UseAuthentication();
// その操作が許可されているか判定（認可）
app.UseAuthorization();

// ルート（URL）のパターンを定義
if (builder.Environment.IsDevelopment())
{
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
}
else
{
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
}

app.Run();
return;

// DbContextを登録するためのヘルパーメソッド
void AddDbContext<TContext>(string keyForDevelopment, string keyForRelease) where TContext : DbContext
{
    // 環境に応じて接続文字列のキーを選択
    var key = builder.Environment.IsDevelopment() ? keyForDevelopment : keyForRelease;
    // 接続文字列を取得
    var connectionString = configuration.GetConnectionString(key) ??
                           throw new InvalidOperationException($"Connection string '{key}' not found.");
    // MySQLを使用するようにDbContextを設定
    services.AddDbContext<TContext>(options => options.UseMySql(connectionString, serverVersion));
}