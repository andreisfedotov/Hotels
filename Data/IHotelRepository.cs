public interface IHotelRepository : IDisposable
{
    Task<List<Hotel>> GetHotelsAsync();
    Task<List<Hotel>> GetHotelsAsync(string name);
    Task<Hotel> GetHotelAsync(int hotelId);
    Task InsertHotelAsync(Hotel hotel);
    Task UpdateHotelAsync(Hotel hotel);
    Task DeleteHotelAsync(int hotelId);
    Task SaveAsync();
}