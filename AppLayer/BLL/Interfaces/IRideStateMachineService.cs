using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Interfaces
{
    public interface IRideStateMachineService
    {
        bool CanTransition(string fromStatus, string toStatus);
        void ValidateTransition(string fromStatus, string toStatus); // throws if invalid
    }
}
