using BLL.DTOs;
using BLL.Interfaces;
using DAL.EF.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services
{
    public class FareService : IFareService
    {
        // Assumption (documented per PRD Section 17): flat rates in paisa.
        // Base fare 3000 paisa (30 taka), 1500 paisa/km, 15% pool discount per extra rider sharing the pool.
        private const long BaseFarePaisa = 3000;
        private const long RatePerKmPaisa = 1500;
        private const decimal DiscountPerExtraPassenger = 0.15m;

        public FareBreakdownDto CalculateFare(Zone pickup, Zone dropoff, int currentPoolOccupants)
        {
            var distanceKm = HaversineDistanceKm(pickup.Latitude, pickup.Longitude, dropoff.Latitude, dropoff.Longitude);
            var distanceCharge = (long)Math.Round((decimal)distanceKm * RatePerKmPaisa);

            var subtotal = BaseFarePaisa + distanceCharge;

            // currentPoolOccupants = passengers already sharing this pool before this one joins
            var discountRate = Math.Min(currentPoolOccupants * DiscountPerExtraPassenger, 0.45m); // cap discount at 45%
            var poolDiscount = (long)Math.Round(subtotal * discountRate);

            return new FareBreakdownDto
            {
                BaseFarePaisa = BaseFarePaisa,
                DistanceChargePaisa = distanceCharge,
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
