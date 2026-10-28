using Hermenos.Data;
using Hermenos.Data.DadosFicticios;
using Hermenos.Services;
using Hermenos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<HermenosDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<DadosDemonstracao>();
builder.Services.AddScoped<IPainelService, PainelService>();
builder.Services.AddScoped<IEmpresaService, EmpresaService>();
builder.Services.AddScoped<IConciliacaoService, ConciliacaoService>();

var app = builder.Build();

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
