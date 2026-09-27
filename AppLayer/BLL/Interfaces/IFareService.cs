using BLL.DTOs;
using DAL.EF.Tables;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Interfaces
{
    public interface IFareService
    {
        FareBreakdownDto CalculateFare(Zone pickup, Zone dropoff, int currentPoolOccupants);
    }
}
