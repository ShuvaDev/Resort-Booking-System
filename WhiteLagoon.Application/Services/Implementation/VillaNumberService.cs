using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Services.Interfaces;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Services.Implementation
{
    public class VillaNumberService : IVillaNumberService
    {
        private readonly IUnitOfWork _unitOfWork;
        public VillaNumberService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public void CreateVillaNumber(VillaNumber villaNumber)
        {
            _unitOfWork.villaNumberRepository.Add(villaNumber);
            _unitOfWork.Save();
        }

        public bool DeleteVillaNumber(int id)
        {
            try
            {
                VillaNumber? objFromDb = _unitOfWork.villaNumberRepository.Get(v => v.Villa_Number == id);
                if (objFromDb is not null)
                {
                    _unitOfWork.villaNumberRepository.Remove(objFromDb);
                    _unitOfWork.Save();

                }
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public IEnumerable<VillaNumber> GetAllVillaNumber()
        {
            return _unitOfWork.villaNumberRepository.GetAll(includeProperties : "Villa");
        }

        public VillaNumber GetVillaNumberById(int id)
        {
            return _unitOfWork.villaNumberRepository.Get(v => v.Villa_Number == id, includeProperties : "Villa");
        }

        public void UpdateVillaNummber(VillaNumber villaNumber)
        {
            _unitOfWork.villaNumberRepository.Update(villaNumber);
            _unitOfWork.Save();
        }

    }
}
