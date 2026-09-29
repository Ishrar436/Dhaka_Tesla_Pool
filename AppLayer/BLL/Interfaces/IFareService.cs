using BLL.DTOs;
using DAL.EF.Tables;

namespace BLL.Interfaces
{
    public interface IFareService
    {
        // occupiedSeats = seats already taken in the pool before this booking joins.
        FareBreakdownDto CalculateFare(Zone pickup, Zone dropoff, int seats, int occupiedSeats);
    }
}
