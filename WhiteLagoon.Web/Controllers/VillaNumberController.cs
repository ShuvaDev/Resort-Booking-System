using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;
using WhiteLagoon.Infrastructure.Repository;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
	[Authorize(Roles = SD.Admin)]
	public class VillaNumberController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public VillaNumberController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var villaNumbers = _unitOfWork.villaNumberRepository.GetAll(null, "Villa");
            return View(villaNumbers);
        }

        public IActionResult Create()
        {
            VillaNumberVM villaNumberVM = new VillaNumberVM()
            {
                VillaList = _unitOfWork.villaRepository.GetAll().ToList().Select(v => new SelectListItem()
				{
					Text = v.Name,
					Value = v.Id.ToString()
				})
		    };
            return View(villaNumberVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(VillaNumberVM model)
        {
			// ModelState.Remove("Villa");
			VillaNumberVM villaNumberVM = new VillaNumberVM()
			{
				VillaList = _unitOfWork.villaRepository.GetAll().Select(v => new SelectListItem()
				{
					Text = v.Name,
					Value = v.Id.ToString()
				})
			};
			if (ModelState.IsValid)
            {
                if(_unitOfWork.villaNumberRepository.Get(vn => vn.Villa_Number == model.VillaNumber.Villa_Number) != null)
                {
                    TempData["error"] = "Villa Number already exists!";
					return View(villaNumberVM);
				}

                _unitOfWork.villaNumberRepository.Add(model.VillaNumber);
                _unitOfWork.Save();
                TempData["success"] = "The villa number has been created successfully!";
                return RedirectToAction("Index");
            }

            return View(villaNumberVM);
        }

        public IActionResult Update(int? villaNumberId)
        {
            if (villaNumberId == null)
            {
                return View("Error", "Home");
            }

            VillaNumber? villaNumber = _unitOfWork.villaNumberRepository.Get(v => v.Villa_Number == villaNumberId);
            if(villaNumber == null)
            {
				return View("Error", "Home");
			}

			VillaNumberVM villaNumberVM = new VillaNumberVM()
			{
				VillaList = _unitOfWork.villaRepository.GetAll().Select(v => new SelectListItem()
				{
					Text = v.Name,
					Value = v.Id.ToString()
				}),
                VillaNumber = villaNumber
			};
			return View(villaNumberVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(VillaNumberVM model)
        {
			if (ModelState.IsValid)
			{
                _unitOfWork.villaNumberRepository.Update(model.VillaNumber);
				_unitOfWork.Save();
				TempData["success"] = "The villa number has been updated successfully!";
				return RedirectToAction("Index");
			}

			VillaNumberVM villaNumberVM = new VillaNumberVM()
			{
				VillaList = _unitOfWork.villaRepository.GetAll().Select(v => new SelectListItem()
				{
					Text = v.Name,
					Value = v.Id.ToString()
				}),
				VillaNumber = model.VillaNumber
			};
			return View(villaNumberVM);
		}

        public IActionResult Delete(int? villaNumberId)
        {
			if (villaNumberId == null)
			{
				return View("Error", "Home");
			}

			VillaNumber? villaNumber = _unitOfWork.villaNumberRepository.Get(v => v.Villa_Number == villaNumberId);
			if (villaNumber == null)
			{
				return View("Error", "Home");
			}
            _unitOfWork.villaNumberRepository.Remove(villaNumber);
			_unitOfWork.Save();
			TempData["success"] = "The villa number has been deleted successfully!";
			return RedirectToAction("Index");
		}

	}
}
