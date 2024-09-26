using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            HomeVM homeVM = new HomeVM()
            {
                VillaList = _unitOfWork.villaRepository.GetAll(includeProperties : "VillaAmenities"),
                CheckInDate = DateOnly.FromDateTime(DateTime.Now),
                Nights = 1
            };

            return View(homeVM);
        }


        [HttpPost]
        public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
        {
			var VillaList = _unitOfWork.villaRepository.GetAll(includeProperties: "VillaAmenities");
			foreach (var villa in VillaList)
			{
                DateOnly searchCheckInDate = checkInDate;
                DateOnly searchCheckOutDate = checkInDate.AddDays(nights);


                // Find out total number of villa from VillaNumber table
                int totalVilla = _unitOfWork.villaNumberRepository.GetAll(vn => vn.VillaId == villa.Id).Count();

                // check number of booked villa based on checkInDate and nights
                int totalBookedVilla = _unitOfWork.bookingRepository.GetAll(b => b.VillaId == villa.Id && b.Status != SD.StatusCancelled  && ((searchCheckInDate >= b.CheckInDate && searchCheckInDate <= b.CheckOutDate) || (searchCheckOutDate >= b.CheckInDate && searchCheckOutDate <= b.CheckOutDate))).Count();
                
                if (totalVilla > totalBookedVilla)
                {
                    villa.IsAvailable = true;
                }
            }

            HomeVM homeVM = new()
            {
                Nights = nights,
                CheckInDate = checkInDate,
                VillaList = VillaList
            };

            return PartialView("_VillaList", homeVM);
		}

        public IActionResult Error()
        {
            return View();
        }
    }
}
