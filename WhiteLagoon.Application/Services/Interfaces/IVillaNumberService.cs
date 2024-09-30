using Microsoft.AspNetCore.Http;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Interfaces
{
    public interface IVillaNumberService
    {
        IEnumerable<VillaNumber> GetAllVillaNumber();
        VillaNumber GetVillaNumberById(int id);
        void CreateVillaNumber(VillaNumber villa);
        void UpdateVillaNummber(VillaNumber villa);
        bool DeleteVillaNumber(int id);
    }
}
