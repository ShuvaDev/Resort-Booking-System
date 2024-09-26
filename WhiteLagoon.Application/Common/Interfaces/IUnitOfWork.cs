using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteLagoon.Application.Common.Interfaces
{
    public interface IUnitOfWork
    {
        IVillaRepository villaRepository { get; }
        IVillaNumberRepository villaNumberRepository { get; }
        IAmenityRepository amenityRepository { get; }
        IBookingRepository bookingRepository { get; }
        IApplicationUserRepository applicationUserRepository { get; }
        void Save();
    }
}
