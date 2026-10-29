using Hermenos.Data;
using Hermenos.Services;
using Hermenos.Services.Importacao;
using Hermenos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<HermenosDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<DadosHermenos>();
builder.Services.AddScoped<MotorConciliacao>();
builder.Services.AddScoped<IImportacaoService, ImportacaoService>();
builder.Services.AddScoped<IPainelService, PainelService>();
builder.Services.AddScoped<IEmpresaService, EmpresaService>();
builder.Services.AddScoped<IConciliacaoService, ConciliacaoService>();

var app = builder.Build();

// Em desenvolvimento, aplica as migrations pendentes e cadastra empresas/usuários iniciais.
if (app.Environment.IsDevelopment())
{
	using var escopo = app.Services.CreateScope();
	var banco = escopo.ServiceProvider.GetRequiredService<HermenosDbContext>();
	await banco.Database.MigrateAsync();
	await SemeadorInicial.SemearAsync(banco);
}

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Painel}/{action=Index}/{id?}")
	.WithStaticAssets();

app.Run();