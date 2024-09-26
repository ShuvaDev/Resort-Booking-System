using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Stripe.Checkout;
using System.Security.Claims;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Web.Controllers
{
	public class BookingController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		public BookingController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult BookingDetails(int bookingId)
		{
			Booking bookingFromDb = _unitOfWork.bookingRepository.Get(b => b.Id == bookingId, includeProperties : "Villa");

			List<int> allVillaNumbers = _unitOfWork.villaNumberRepository.GetAll(vn => vn.VillaId == bookingFromDb.VillaId).Select(vn => vn.Villa_Number).ToList();
			List<int> bookedVillaNumbers = _unitOfWork.bookingRepository.GetAll(b => b.VillaId == bookingFromDb.VillaId && b.VillaNumber != 0 && b.Status != SD.StatusCompleted).Select(b => b.VillaNumber).ToList();

			
			foreach(var vn in allVillaNumbers)
			{
				if(!bookedVillaNumbers.Contains(vn))
				{
					bookingFromDb.AvailableVillaNumber.Add(vn);
                }
			}

			return View(bookingFromDb);
		}

		[Authorize]
		public IActionResult GetBookingList(string? status)
		{
			IEnumerable<Booking> bookingList;
			if (User.IsInRole(SD.Admin))
			{
				bookingList = _unitOfWork.bookingRepository.GetAll();
			}
			else
			{
				var claimsIdentity = (ClaimsIdentity)User.Identity;
				var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

				bookingList = _unitOfWork.bookingRepository.GetAll(b => b.UserId ==  userId);
			}

			if(!string.IsNullOrEmpty(status))
			{
				bookingList = bookingList.Where(b => b.Status.ToLower().Equals(status.ToLower()));
			}
			return Json(new
			{
				data = bookingList
			});
		}

		[Authorize]
		public IActionResult FinalizeBooking(int villaId, DateOnly checkInDate, int nights)
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			ApplicationUser user = _unitOfWork.applicationUserRepository.Get(u => u.Id == userId);


			Booking booking = new Booking()
			{
				VillaId = villaId,
				Villa = _unitOfWork.villaRepository.Get(v => v.Id == villaId, includeProperties : "VillaAmenities"),
				CheckInDate = checkInDate,
				Nights = nights,
				CheckOutDate = checkInDate.AddDays(nights),
				UserId = userId,
				Name = user.Name,
				Email = user.Email,
				Phone = user.PhoneNumber,
			};
			booking.Villa.IsAvailable = true;
			booking.TotalCost = booking.Villa.Price * nights;

			return View(booking);
		}
        [Authorize]
		[HttpPost]
        public IActionResult FinalizeBooking(Booking booking)
		{
			var villa = _unitOfWork.villaRepository.Get(v => v.Id ==  booking.VillaId);
			booking.TotalCost = villa.Price * booking.Nights;
			booking.BookingDate = DateTime.Now;

			booking.Status = SD.StatusPending;

			_unitOfWork.bookingRepository.Add(booking);
			_unitOfWork.Save();

            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"Booking/BookingConfirmation?bookingId={booking.Id}",
                CancelUrl = domain + $"Booking/FinalizeBooking/villaId={booking.VillaId}&checkInDate={booking.CheckInDate}&nights={booking.Nights}",
            };

			options.LineItems.Add(new SessionLineItemOptions()
			{
				PriceData = new SessionLineItemPriceDataOptions()
				{
					UnitAmount = (long)(booking.TotalCost * 100),
					Currency = "usd",
					ProductData = new SessionLineItemPriceDataProductDataOptions()
					{
						Name = villa.Name
					}
				},
				Quantity = 1
			});

            var service = new SessionService();
            Session session = service.Create(options);

			_unitOfWork.bookingRepository.UpdateStripePaymentId(booking.Id, session.Id, session.PaymentIntentId);
			_unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
			return new StatusCodeResult(303);
		}

		[Authorize]
		public IActionResult BookingConfirmation(int bookingId)
		{
			Booking bookingFromDb = _unitOfWork.bookingRepository.Get(b => b.Id == bookingId);

			if(bookingFromDb != null)
			{
                var service = new SessionService();
				Session session = service.Get(bookingFromDb.StripeSessionId);

				if(session.PaymentStatus == "paid")
				{
					_unitOfWork.bookingRepository.UpdateStatus(bookingFromDb.Id, SD.StatusApproved);
					_unitOfWork.bookingRepository.UpdateStripePaymentId(bookingFromDb.Id, session.Id, session.PaymentIntentId);
					_unitOfWork.Save();
				}

            }
			return View(bookingId);
		}

		[HttpPost]
		[Authorize(Roles = SD.Admin)]
		public IActionResult CheckIn(Booking booking)
		{
			_unitOfWork.bookingRepository.UpdateStatus(booking.Id, SD.StatusCheckedIn, booking.VillaNumber);
			_unitOfWork.Save();
			TempData["success"] = "Booking Updated Successfully";

			return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
		}

        [HttpPost]
        [Authorize(Roles = SD.Admin)]
        public IActionResult CheckOut(Booking booking)
        {
            _unitOfWork.bookingRepository.UpdateStatus(booking.Id, SD.StatusCompleted, booking.VillaNumber);
            _unitOfWork.Save();
            TempData["success"] = "Booking completed Successfully";

            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Admin)]
        public IActionResult CancelBooking(Booking booking)
        {
            _unitOfWork.bookingRepository.UpdateStatus(booking.Id, SD.StatusCancelled, 0);
            _unitOfWork.Save();
            TempData["success"] = "Booking cancelled Successfully";

            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

    }
}
