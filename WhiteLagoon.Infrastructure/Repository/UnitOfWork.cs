using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        public IVillaRepository villaRepository { get; private set; }
        public IVillaNumberRepository villaNumberRepository { get; private set; }
        public IAmenityRepository amenityRepository { get; private set; }
        public IBookingRepository bookingRepository { get; private set; }
        public IApplicationUserRepository applicationUserRepository { get; private set; }

        private readonly ApplicationDbContext _db;
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            villaRepository = new VillaRepository(db);
            villaNumberRepository = new VillaNumberRepository(db);
            amenityRepository = new AmenityRepository(db);
            bookingRepository = new BookingRepository(db);
            applicationUserRepository = new ApplicationUserRepository(db);
        }
        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
