using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Pvis.Biz.EmailSenderServices;
using Pvis.Biz.Member;
using Pvis.Biz.Models;
using Pvis.Web.Helper;
using WebMarkupMin.AspNetCore3;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;

namespace Pvis.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.Secure = CookieSecurePolicy.SameAsRequest;
            });

            services
                .Configure<IISServerOptions>(options =>
                {
                    options.AutomaticAuthentication = false;
                })
                .Configure<IISOptions>(options =>
                {
                    options.ForwardClientCertificate = false;
                });

            services
                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("pvis.account"), b => b.MigrationsAssembly("Pvis.Web"))
                )
                .AddDbContext<DataDbContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("pvis.data"), b => b.MigrationsAssembly("Pvis.Web"))
                );
            services.AddDatabaseDeveloperPageExceptionFilter();
            services
                .AddIdentity<MyAppUser, IdentityRole>(options =>
                {
                    //密碼長度最少12碼
                    options.Password.RequiredLength = 12;
                    //需要數字
                    options.Password.RequireDigit = true;
                    //需要非英數字元
                    options.Password.RequireNonAlphanumeric = true;
                    //需要小寫英文
                    options.Password.RequireUppercase = true;
                    //需要大寫英文
                    options.Password.RequireLowercase = true;
                })
                .AddErrorDescriber<ZhTwIdentityErrorDescriber>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<IUserClaimsPrincipalFactory<MyAppUser>, AppClaimsPrincipalFactory>();

            //調整頁面授權失敗時預設的轉導頁面 , 記得要在 AddIdentity 在跑這段才有用 
            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/BackEnd/Login";
                options.Cookie.Name = "PvisCookie";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.LoginPath = "/BackEnd/Login";
                // ReturnUrlParameter requires 
                //using Microsoft.AspNetCore.Authentication.Cookies;
                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = true;

                //修正 R-Proxy 代理後可能引發網址轉倒錯誤問題.
                options.Events.OnRedirectToLogin = context => {
                    var _uri = new Uri(context.RedirectUri);
                    context.Response.Redirect(_uri.PathAndQuery);
                    return Task.CompletedTask;
                };
            });

            //https://blog.darkthread.net/blog/aspnetcore-cshtml-chinese-char-encoding/
            //處理 ASP.NET Core View 中文變 & # x4E2D; & # x6587;
            services.AddSingleton(
                 HtmlEncoder.Create(allowedRanges: new[] { UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs }));


            services.AddMvc(config =>
                {
                    config.Filters.Add(new ExceptionFilter());
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                })
                .AddMvcOptions(options => options.EnableEndpointRouting = false)
                //設定後台相關頁面需要登入才能使用
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizeAreaFolder("BackEnd", "/");
                    options.Conventions.AllowAnonymousToAreaPage("BackEnd", "/Login");
                });

            //services.AddControllers().AddJsonOptions(options =>
            //{
            //    options.JsonSerializerOptions.PropertyNamingPolicy = null;
            //});



            //底層引用 HttpContext 需要多加這個 services
            //https://docs.microsoft.com/zh-tw/aspnet/core/fundamentals/http-context?view=aspnetcore-2.2
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddHttpContextAccessor();
            services.AddWebMarkupMin(
                    options =>
                    {
                        options.AllowMinificationInDevelopmentEnvironment = true;
                        options.AllowCompressionInDevelopmentEnvironment = true;
                        options.DisablePoweredByHttpHeaders = true;
                    }
                ).AddHtmlMinification(
                    options =>
                    {
                        options.MinificationSettings.RemoveRedundantAttributes = true;
                        options.MinificationSettings.RemoveHttpProtocolFromAttributes = true;
                        options.MinificationSettings.RemoveHttpsProtocolFromAttributes = true;
                        options.MinificationSettings.RemoveOptionalEndTags = false;
                    }
                );

            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });

            // 將 Session 存在 ASP.NET Core 記憶體中
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            //加入 For ajax 呼叫時的 CSRF 驗證
            //  https://blog.darkthread.net/blog/razor-pages-ajax-call/
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
            });

            services.Configure<EmailSettings>(options =>
            {
                options.CfgFile(SysConfig.ConfgFilePath);
            })
                .AddSingleton<IEmailSender, EmailSender>();
            services.AddServerSideBlazor();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseResponseCompression();

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                await next();
            });

            app.UseStaticFiles();

            #region 產生自訂的靜態檔案放置區域, 同時正面表列支援類型
            var PdfProvider = new FileExtensionContentTypeProvider();
            PdfProvider.Mappings.Clear();
            PdfProvider.Mappings.Add(".pdf", "application/pdf");
            PdfProvider.Mappings.Add(".odt", "application/vnd.oasis.opendocument.text");
            
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "FormFileUpload")),
                RequestPath = new PathString("/FormFileUpload"),
                ContentTypeProvider = PdfProvider
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pubFile")),
                RequestPath = "/pubFile",
                ContentTypeProvider = PdfProvider
            });
            #endregion

            if (!env.IsDevelopment())
            {
                app.UseWebMarkupMin();
            }

            app.UseCookiePolicy();
            app.UseSession();


            app.UseAuthentication();
            app.UseMvc();
            app.UseRouting();

            // SessionMiddleware 加入 Pipeline



            AuthHelper.Init(app.ApplicationServices, TimeSpan.FromHours(1));
            app.UseEndpoints(endpoints =>
            {
                // 👇 Add this line
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToAreaPage("/_Host", "Backend");
            });
        }

        
    }
}
