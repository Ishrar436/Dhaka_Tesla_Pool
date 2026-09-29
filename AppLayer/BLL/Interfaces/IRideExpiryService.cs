namespace BLL.Interfaces
{
    public interface IRideExpiryService
    {
        // Cancels rides still "Waiting" after maxWait and frees pools left empty. Returns how many expired.
        Task<int> ExpireStaleAsync(TimeSpan maxWait);
    }
}
