var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<HotelDb>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
});
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddSingleton<ITokenService>(new TokenService());
builder.Services.AddSingleton<IUserRepository>(new UserRepository());
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HotelDb>();
    db.Database.EnsureCreated();
}

app.MapGet("/login", [AllowAnonymous] async (HttpContext context,
    ITokenService tokenService, IUserRepository userRepository) => {
        UserModel userModel = new()
        {
            UserName = context.Request.Query["username"],
            Password = context.Request.Query["password"]
        };
        var userDto = userRepository.GetUser(userModel);
        if (userDto == null) return Results.Unauthorized();
        var token = tokenService.BuildToken(builder.Configuration["Jwt:Key"],
            builder.Configuration["Jwt:Issuer"], userDto);
        return Results.Ok(token);
    });

app.MapGet("/hotels", [Authorize] async (IHotelRepository repository) =>
    Results.Extensions.Xml(await repository.GetHotelsAsync()))
    .Produces<List<Hotel>>(StatusCodes.Status200OK)
    .WithName("GetAllHotels")
    .WithTags("Getters");

app.MapGet("/hotels/{id}", [Authorize] async (int id, IHotelRepository repository) =>
    await repository.GetHotelAsync(id) is Hotel hotel
    ? Results.Ok(hotel)
    : Results.NotFound())
    .Produces<Hotel>(StatusCodes.Status200OK)
    .WithName("GetHotel")
    .WithTags("Getters");

app.MapPost("/hotels", [Authorize] async ([FromBody] Hotel hotel, IHotelRepository repository) =>
    {
        await repository.InsertHotelAsync(hotel);
        await repository.SaveAsync();
        return Results.Created($"/hotels/{hotel.Id}", hotel);
    })
    .Accepts<Hotel>("application/json")
    .Produces<Hotel>(StatusCodes.Status201Created)
    .WithName("CreateHotel")
    .WithTags("Creators");

app.MapPut("/hotels", [Authorize] async ([FromBody] Hotel hotel, IHotelRepository repository) =>
    {
        await repository.UpdateHotelAsync(hotel);
        await repository.SaveAsync();
        return Results.NoContent();
    })
    .Accepts<Hotel>("application/json")
    .WithName("UpdateHotel")
    .WithTags("Updaters");

app.MapDelete("hotels/{id}", [Authorize] async (int id, IHotelRepository repository) =>
    {
        await repository.DeleteHotelAsync(id);
        await repository.SaveAsync();
        return Results.NoContent();
    })
    .WithName("DeleteHotel")
    .WithTags("Deleters");

app.MapGet("/hotels/search/name/{query}",
    [Authorize] async (string query, IHotelRepository repository) =>
        await repository.GetHotelsAsync(query) is IEnumerable<Hotel> hotels
            ? Results.Ok(hotels)
            : Results.NotFound(Array.Empty<Hotel>()))
    .Produces<List<Hotel>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("SearchHotels")
    .WithTags("Getters")
    .ExcludeFromDescription();

app.MapGet("/hotels/search/location/{coordinate}",
    [Authorize] async (Coordinate coordinate, IHotelRepository repository) =>
        await repository.GetHotelsAsync(coordinate) is IEnumerable<Hotel> hotels
            ? Results.Ok(hotels)
            : Results.NotFound(Array.Empty<Hotel>()))
    .ExcludeFromDescription();

app.UseHttpsRedirection();

app.Run();
