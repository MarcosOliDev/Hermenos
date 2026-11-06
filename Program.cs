using Hermenos.Data;
using Hermenos.Services;
using Hermenos.Services.Importacao;
using Hermenos.Services.Interfaces;
using Hermenos.Services.Usuarios;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Hermenos.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<HermenosDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		options.LoginPath = "/Conta/Entrar";
		options.AccessDeniedPath = "/Conta/AcessoNegado";
		options.Cookie.Name = "Hermenos.Auth";
		options.Cookie.HttpOnly = true;
		options.Cookie.SameSite = SameSiteMode.Lax;
		options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
		options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
		options.SlidingExpiration = true;
	});
builder.Services.AddAuthorization();
builder.Services.AddScoped<GeradorNomeUsuario>();
builder.Services.AddScoped<DadosHermenos>();
builder.Services.AddScoped<MotorConciliacao>();
builder.Services.AddScoped<IImportacaoService, ImportacaoService>();
builder.Services.AddScoped<IPainelService, PainelService>();
builder.Services.AddScoped<IEmpresaService, EmpresaService>();
builder.Services.AddScoped<IConciliacaoService, ConciliacaoService>();
builder.Services.AddScoped<GeradorSenhaTemporaria>();

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//	using var escopo = app.Services.CreateScope();
//	var banco = escopo.ServiceProvider.GetRequiredService<HermenosDbContext>();
//	await banco.Database.MigrateAsync();
//	await SemeadorInicial.SemearAsync(banco);
//}

using (var escopo = app.Services.CreateScope())
{
	var servicos = escopo.ServiceProvider;
	var banco = servicos.GetRequiredService<HermenosDbContext>();

	await banco.Database.MigrateAsync();

	if (app.Environment.IsDevelopment())
	{
		await SemeadorInicial.SemearAsync(banco);
	}
	else if (app.Environment.IsProduction())
	{
		var configuracao = servicos.GetRequiredService<IConfiguration>();

		await SemeadorInicial.SemearAdminInicialAsync(
			banco,
			configuracao
		);
	}
}

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<PrimeiroAcessoMiddleware>();
app.UseAuthorization(); app.MapControllerRoute(name: "default", pattern: "{controller=Painel}/{action=Index}/{id?}");
app.Run();
