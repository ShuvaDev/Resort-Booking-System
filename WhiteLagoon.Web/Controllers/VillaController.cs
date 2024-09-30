using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Application.Services.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Web.Controllers
{
    [Authorize(Roles = SD.Admin)]
    public class VillaController : Controller
    {
        private readonly IVillaService _villaService;

		public VillaController(IVillaService villaService)
        {
            _villaService = villaService;
        }
        public IActionResult Index()
        {
            var villas = _villaService.GetAllVillas();
            return View(villas);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Villa villa, IFormFile? file)
        {
			if (ModelState.IsValid)
            {
                _villaService.CreateVilla(villa, file);

                TempData["success"] = "The villa has been created successfully!";
                return RedirectToAction("Index");
            }
            return View(villa);
        }

        public IActionResult Update(int villaId)
        {
            if (villaId == null)
            {
                return View("Error", "Home");
            }

            Villa? villa = _villaService.GetVillaById(villaId);
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
                _villaService.UpdateVilla(villa, file);
                TempData["success"] = "The villa has been updated successfully!";
                return RedirectToAction("Index");
            }
            return View(villa);
        }

        public IActionResult Delete(int villaId)
        {
			if (villaId == null)
			{
				return View("Error", "Home");
			}

            Villa? villa = _villaService.GetVillaById(villaId);
			if (villa == null)
			{
				return View("Error", "Home");
			}
			return View(villa);
		}

        [HttpPost]
		public IActionResult Delete(Villa villa)
		{
            bool deleted = _villaService.DeleteVilla(villa.Id);
            if (deleted) 
            { 
                TempData["success"] = "The villa has been deleted successfully!";
			    return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Failed to delete the villa!";
            }
			return View();
		}
	}
}
