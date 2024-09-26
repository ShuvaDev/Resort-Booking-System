using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Web.Controllers
{
    [Authorize(Roles = SD.Admin)]
    public class VillaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
		public VillaController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var villas = _unitOfWork.villaRepository.GetAll();
            return View(villas);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Villa model, IFormFile? file)
        {
            
			if (ModelState.IsValid)
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

					model.ImageUrl = @"\images\villa\" + fileName;
				}
                else
                {
                    model.ImageUrl = "https://placehold.co/600x400";
				}
				_unitOfWork.villaRepository.Add(model);
                _unitOfWork.Save();
                TempData["success"] = "The villa has been created successfully!";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public IActionResult Update(int? villaId)
        {
            if (villaId == null)
            {
                return View("Error", "Home");
            }

            Villa? villa = _unitOfWork.villaRepository.Get(v => v.Id == villaId);
            if(villa == null)
            {
				return View("Error", "Home");
			}
            return View(villa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(Villa villa, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                if(file != null)
                {
					string wwwRootPath = _webHostEnvironment.WebRootPath;

                    if(!villa.ImageUrl.Contains("placehold"))
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
                TempData["success"] = "The villa has been updated successfully!";
                return RedirectToAction("Index");
            }
            return View(villa);
        }

        public IActionResult Delete(int? villaId)
        {
			if (villaId == null)
			{
				return View("Error", "Home");
			}

			Villa? villa = _unitOfWork.villaRepository.Get(v => v.Id == villaId);
			if (villa == null)
			{
				return View("Error", "Home");
			}
			return View(villa);
		}

        [HttpPost]
		public IActionResult Delete(Villa villa)
		{
            Villa? objFromDb = _unitOfWork.villaRepository.Get(v => v.Id == villa.Id);
            if (objFromDb is not null)
            {
                if(!objFromDb.ImageUrl.Contains("placehold"))
                {
					string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, objFromDb.ImageUrl.Remove(0, 1));
					System.IO.File.Delete(oldImagePath);
				}

                _unitOfWork.villaRepository.Remove(objFromDb);
                _unitOfWork.Save();
                TempData["success"] = "The villa has been deleted successfully!";
			    return RedirectToAction("Index");
            }
			return View("Error", "Home");
		}
	}
}
