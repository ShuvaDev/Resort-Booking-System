using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Interfaces
{
    public interface IVillaService
    {
        IEnumerable<Villa> GetAllVillas();
        Villa GetVillaById(int id);
        void CreateVilla(Villa villa, IFormFile? file);
        void UpdateVilla(Villa villa, IFormFile? file);
        bool DeleteVilla(int id);
    }
}
