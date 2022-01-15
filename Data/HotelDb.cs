public class HotelDb : DbContext
{
    public HotelDb(DbContextOptions<HotelDb> options) : base(options) {}
    public DbSet<Hotel> Hotels => Set<Hotel>();
}
