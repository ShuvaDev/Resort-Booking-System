using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Application.Services.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;
using WhiteLagoon.Infrastructure.Repository;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
	[Authorize(Roles = SD.Admin)]
	public class VillaNumberController : Controller
    {
		private readonly IVillaNumberService _villaNumberService;
		private readonly IVillaService _villaService;
        public VillaNumberController(IVillaNumberService villaNumberService, IVillaService villaService)
        {
			_villaNumberService = villaNumberService;
			_villaService = villaService;
        }
        public IActionResult Index()
        {
            var villaNumbers = _villaNumberService.GetAllVillaNumber();
            return View(villaNumbers);
        }

        public IActionResult Create()
        {
            VillaNumberVM villaNumberVM = new VillaNumberVM()
            {
                VillaList = _villaService.GetAllVillas().ToList().Select(v => new SelectListItem()
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
				VillaList = _villaService.GetAllVillas().Select(v => new SelectListItem()
				{
					Text = v.Name,
					Value = v.Id.ToString()
				})
			};
			if (ModelState.IsValid)
            {
                if(_villaNumberService.GetVillaNumberById(model.VillaNumber.Villa_Number) != null)
                {
                    TempData["error"] = "Villa Number already exists!";
					return View(villaNumberVM);
				}

                _villaNumberService.CreateVillaNumber(model.VillaNumber);
                TempData["success"] = "The villa number has been created successfully!";
                return RedirectToAction("Index");
            }

            return View(villaNumberVM);
        }

        public IActionResult Update(int villaNumberId)
        {
            if (villaNumberId == null)
            {
                return View("Error", "Home");
            }

            VillaNumber? villaNumber = _villaNumberService.GetVillaNumberById(villaNumberId);
            if(villaNumber == null)
            {
				return View("Error", "Home");
			}

			VillaNumberVM villaNumberVM = new VillaNumberVM()
			{
				VillaList = _villaService.GetAllVillas().Select(v => new SelectListItem()
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
                _villaNumberService.UpdateVillaNummber(model.VillaNumber);
				TempData["success"] = "The villa number has been updated successfully!";
				return RedirectToAction("Index");
			}

			VillaNumberVM villaNumberVM = new VillaNumberVM()
			{
				VillaList = _villaService.GetAllVillas().Select(v => new SelectListItem()
				{
					Text = v.Name,
					Value = v.Id.ToString()
				}),
				VillaNumber = model.VillaNumber
			};
			return View(villaNumberVM);
		}

        public IActionResult Delete(int villaNumberId)
        {
			if (villaNumberId == null)
			{
				return View("Error", "Home");
			}

            bool deleted = _villaNumberService.DeleteVillaNumber(villaNumberId);

			if(deleted)
			{
				TempData["success"] = "The villa number has been deleted successfully!";
				return RedirectToAction("Index");
			}
			else
			{
				TempData["error"] = "Villa number has been failed to deleted";
				return RedirectToAction("Index");
			}
		}

	}
}
