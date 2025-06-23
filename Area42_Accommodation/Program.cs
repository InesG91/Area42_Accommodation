using Infrastructure.DataAccess;
using Core.Domain.Services;
using Infrastructure.DataAccess.Repositories;
using Core.Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();



builder.Services.AddScoped<IAccommodationRepository, AccommodationRepository>();
builder.Services.AddScoped<IGuestRepository, GuestRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IAvailabilityRepository, AvailabilityRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Concrete services 
builder.Services.AddScoped<PaymentService>();           
builder.Services.AddScoped<PricingService>();           
builder.Services.AddScoped<BookingService>();           
builder.Services.AddScoped<GuestService>();             
builder.Services.AddScoped<AccommodationService>();
builder.Services.AddScoped<AvailabilityService>();

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
