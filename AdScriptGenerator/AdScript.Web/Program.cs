using AdScript.Core.Excel;
using AdScript.Core.Models;
using AdScript.Core.Services.Excel;
using AdScript.Core.Services.Script;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.Configure<AdScriptOptions>(
    builder.Configuration.GetSection("AdScript"));

builder.Services.AddScoped<IScriptGenerator, PowerShellScriptGenerator>();

builder.Services.AddScoped<IExcelUserInputReader, ExcelUserInputReader>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
