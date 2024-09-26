using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
	[Authorize(Roles = SD.Admin)]
    public class AmenityController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		public AmenityController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
		public IActionResult Index()
		{
			var listOfAmenity = _unitOfWork.amenityRepository.GetAll(null, "Villa");
			return View(listOfAmenity);
		}

		public IActionResult Create()
		{
			AmenityVM amenityVM = new AmenityVM()
			{
				VillaList = _unitOfWork.villaRepository.GetAll().ToList().Select(v => new SelectListItem()
				{
					Text = v.Name,
					Value = v.Id.ToString()
				})
			};
			return View(amenityVM);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create(AmenityVM model)
		{
			AmenityVM amenityVM = new AmenityVM()
			{
				VillaList = _unitOfWork.villaRepository.GetAll().Select(v => new SelectListItem()
				{
					Text = v.Name,
					Value = v.Id.ToString()
				})
			};
			if (ModelState.IsValid)
			{
				_unitOfWork.amenityRepository.Add(model.Amenity);
				_unitOfWork.Save();
				TempData["success"] = "The amenity has been created successfully!";
				return RedirectToAction("Index");
			}

			return View(amenityVM);
		}

		public IActionResult Update(int? amenityId)
		{
			if (amenityId == null)
			{
				return View("Error", "Home");
			}

			Amenity? amenity = _unitOfWork.amenityRepository.Get(v => v.Id == amenityId);
			if (amenity == null)
			{
				return View("Error", "Home");
			}

			AmenityVM amenityVM = new AmenityVM()
			{
				VillaList = _unitOfWork.villaRepository.GetAll().Select(v => new SelectListItem()
				{
					Text = v.Name,
					Value = v.Id.ToString()
				}),
				Amenity = amenity
			};
			return View(amenityVM);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Update(AmenityVM model)
		{
			if (ModelState.IsValid)
			{
				_unitOfWork.amenityRepository.Update(model.Amenity);
				_unitOfWork.Save();
				TempData["success"] = "The amenity has been updated successfully!";
				return RedirectToAction("Index");
			}

			AmenityVM amenityVM = new AmenityVM()
			{
				VillaList = _unitOfWork.villaRepository.GetAll().Select(v => new SelectListItem()
				{
					Text = v.Name,
					Value = v.Id.ToString()
				}),
				Amenity = model.Amenity
			};
			return View(amenityVM);
		}

		public IActionResult Delete(int? amenityId)
		{
			if (amenityId == null)
			{
				return View("Error", "Home");
			}

			Amenity? amenity = _unitOfWork.amenityRepository.Get(v => v.Id == amenityId);
			if (amenity == null)
			{
				return View("Error", "Home");
			}
			_unitOfWork.amenityRepository.Remove(amenity);
			_unitOfWork.Save();
			TempData["success"] = "The amenity has been deleted successfully!";
			return RedirectToAction("Index");
		}

	}
}
