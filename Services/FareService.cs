using BLL.DTOs;
using BLL.Interfaces;
using DAL.EF.Tables;

namespace BLL.Services
{
    public class FareService : IFareService
    {
        // Assumptions (PRD Section 17), all money in integer paisa:
        //   per seat  = base 3000 paisa (Tk 30) + 1500 paisa/km * straight-line distance
        //   subtotal  = per seat * seats booked
        //   discount  = 15% per seat already occupied in the pool, capped at 45%, on the subtotal
        //   total     = subtotal - discount
        // Rounding is half-away-from-zero so results can be checked by hand.
        private const long BaseFarePaisa = 3000;
        private const long RatePerKmPaisa = 1500;
        private const decimal DiscountPerOccupiedSeat = 0.15m;
        private const decimal MaxDiscount = 0.45m;

        public FareBreakdownDto CalculateFare(Zone pickup, Zone dropoff, int seats, int occupiedSeats)
        {
            if (seats < 1) throw new ArgumentException("At least one seat is required.");
            if (occupiedSeats < 0) occupiedSeats = 0;

            var distanceKm = HaversineDistanceKm(pickup.Latitude, pickup.Longitude, dropoff.Latitude, dropoff.Longitude);
            var distanceChargePerSeat = (long)Math.Round((decimal)distanceKm * RatePerKmPaisa, MidpointRounding.AwayFromZero);

            var baseTotal = BaseFarePaisa * seats;
            var distanceTotal = distanceChargePerSeat * seats;
            var subtotal = baseTotal + distanceTotal;

            var discountRate = Math.Min(occupiedSeats * DiscountPerOccupiedSeat, MaxDiscount);
            var poolDiscount = (long)Math.Round(subtotal * discountRate, MidpointRounding.AwayFromZero);

            return new FareBreakdownDto
            {
                BaseFarePaisa = baseTotal,
                DistanceChargePaisa = distanceTotal,
                PoolDiscountPaisa = poolDiscount,
                TotalFarePaisa = subtotal - poolDiscount
            };
        }

        private static double HaversineDistanceKm(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            const double R = 6371; // Earth radius km
            var dLat = ToRad((double)(lat2 - lat1));
            var dLon = ToRad((double)(lon2 - lon1));
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRad((double)lat1)) * Math.Cos(ToRad((double)lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRad(double deg) => deg * Math.PI / 180;
    }
}
