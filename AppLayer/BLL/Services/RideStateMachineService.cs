using BLL.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services
{
    public class RideStateMachineService : IRideStateMachineService
    {
        private static readonly Dictionary<string, string[]> AllowedTransitions = new()
        {
            ["Waiting"] = new[] { "Matched", "Cancelled" },
            ["Matched"] = new[] { "DriverArrived", "Cancelled" },
            ["DriverArrived"] = new[] { "InProgress", "Cancelled" },
            ["InProgress"] = new[] { "Completed", "Cancelled" },
            ["Completed"] = Array.Empty<string>(),
            ["Cancelled"] = Array.Empty<string>()
        };

        public bool CanTransition(string fromStatus, string toStatus) =>
            AllowedTransitions.TryGetValue(fromStatus, out var allowed) && allowed.Contains(toStatus);

        public void ValidateTransition(string fromStatus, string toStatus)
        {
            if (!CanTransition(fromStatus, toStatus))
                throw new InvalidOperationException($"Invalid ride status transition: {fromStatus} -> {toStatus}");
        }
    }
}
