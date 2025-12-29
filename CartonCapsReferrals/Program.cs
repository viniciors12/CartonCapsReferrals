using CartonCapsReferrals.Api;
using CartonCapsReferrals.Api.Interfaces;
using CartonCapsReferrals.Api.Middleware;
using CartonCapsReferrals.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<IReferralService, ReferralService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IReferralStore, ReferralStore>();
builder.Services.Configure<ShortIoOptions>(builder.Configuration.GetSection("ShortIo"));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.Run();
