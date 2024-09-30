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
    public class VillaService : IVillaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public VillaService(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public void CreateVilla(Villa villa, IFormFile? file)
        {
            if (file != null)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string filePath = Path.Combine(wwwRootPath, @"images\villa");

                using (var fileStream = new FileStream(Path.Combine(filePath, fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                villa.ImageUrl = @"\images\villa\" + fileName;
            }
            else
            {
                villa.ImageUrl = "https://placehold.co/600x400";
            }
            _unitOfWork.villaRepository.Add(villa);
            _unitOfWork.Save();
        }

        public bool DeleteVilla(int id)
        {
            try
            {
                Villa? objFromDb = _unitOfWork.villaRepository.Get(v => v.Id == id);
                if (objFromDb is not null)
                {
                    if (!objFromDb.ImageUrl.Contains("placehold"))
                    {
                        string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, objFromDb.ImageUrl.Remove(0, 1));
                        System.IO.File.Delete(oldImagePath);
                    }

                    _unitOfWork.villaRepository.Remove(objFromDb);
                    _unitOfWork.Save();

                }
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public IEnumerable<Villa> GetAllVillas()
        {
            return _unitOfWork.villaRepository.GetAll(includeProperties : "VillaAmenities");
        }

        public Villa GetVillaById(int id)
        {
            return _unitOfWork.villaRepository.Get(v => v.Id == id, includeProperties: "VillaAmenities");
        }

        public void UpdateVilla(Villa villa, IFormFile? file)
        {
            if (file != null)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;

                if (!villa.ImageUrl.Contains("placehold"))
                {
                    string oldImagePath = Path.Combine(wwwRootPath, villa.ImageUrl.Remove(0, 1));
                    System.IO.File.Delete(oldImagePath);
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string filePath = Path.Combine(wwwRootPath, @"images\villa");

                using (var fileStream = new FileStream(Path.Combine(filePath, fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                villa.ImageUrl = @"\images\villa\" + fileName;
            }

            _unitOfWork.villaRepository.Update(villa);
            _unitOfWork.Save();
        }
    }
}
