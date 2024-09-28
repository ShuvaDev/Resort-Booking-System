using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Stripe.Checkout;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using System.Security.Claims;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Web.Controllers
{
	public class BookingController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IWebHostEnvironment _webHostEnvironment;
		public BookingController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
		{
			_unitOfWork = unitOfWork;
			_webHostEnvironment = webHostEnvironment;
		}

		public IActionResult Index()
		{
			return View();
		}

		[Authorize]
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

		[HttpPost]
		[Authorize]
		public IActionResult GenerateInvoice(int id, string downloadType)
		{
			string basePath = _webHostEnvironment.WebRootPath;
			WordDocument document = new WordDocument();

			// Load the template
			string dataPath = basePath + @"/export/BookingDetails.docx";
			using FileStream fileStream = new(dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			document.Open(fileStream, FormatType.Automatic);

			// Update Template 
			Booking bookingFromDb = _unitOfWork.bookingRepository.Get(u => u.Id == id, includeProperties: "Villa");

			TextSelection textSelection = document.Find("xx_customer_name", false, true);
			WTextRange textRange = textSelection.GetAsOneRange();
			textRange.Text = bookingFromDb.Name;

			textSelection = document.Find("xx_customer_phone", false, true);
			textRange = textSelection.GetAsOneRange();
			textRange.Text = bookingFromDb.Phone == null ? "null" : bookingFromDb.Phone;
			
			textSelection = document.Find("xx_customer_email", false, true);
			textRange = textSelection.GetAsOneRange();
			textRange.Text = bookingFromDb.Email == null ? "null" : bookingFromDb.Email;

			textSelection = document.Find("xx_payment_date", false, true);
			textRange = textSelection.GetAsOneRange();
			textRange.Text = bookingFromDb.PaymentDate == null ? "null" : bookingFromDb.PaymentDate.ToShortDateString();

			textSelection = document.Find("xx_checkin_date", false, true);
			textRange = textSelection.GetAsOneRange();
			textRange.Text = bookingFromDb.CheckInDate == null ? "null" : bookingFromDb.CheckInDate.ToShortDateString();

			textSelection = document.Find("xx_checkout_date", false, true);
			textRange = textSelection.GetAsOneRange();
			textRange.Text = bookingFromDb.CheckOutDate == null ? "null" : bookingFromDb.CheckOutDate.ToShortDateString();

			textSelection = document.Find("xx_booking_total", false, true);
			textRange = textSelection.GetAsOneRange();
			textRange.Text = bookingFromDb.TotalCost.ToString("c");

			textSelection = document.Find("XX_BOOKING_NUMBER", false, true);
			textRange = textSelection.GetAsOneRange();
			textRange.Text = "Booking ID - " + bookingFromDb.Id.ToString();

			textSelection = document.Find("XX_BOOKING_DATE", false, true);
			textRange = textSelection.GetAsOneRange();
			textRange.Text = "Booking Date - " + bookingFromDb.BookingDate.ToShortDateString();

			// Create Table
			WTable table = new(document);
			table.TableFormat.Borders.LineWidth = 1f;
			table.TableFormat.Borders.Color = Color.Black;
			table.TableFormat.Paddings.Top = 7f;
			table.TableFormat.Paddings.Bottom = 7f;
			table.TableFormat.Borders.Horizontal.LineWidth = 1f;

			table.ResetCells(2, 4);

			WTableRow row0 = table.Rows[0];
			row0.Cells[0].AddParagraph().AppendText("NIGHTS");
			row0.Cells[0].Width = 80;
			row0.Cells[1].AddParagraph().AppendText("VILLA");
			row0.Cells[2].Width = 220;
			row0.Cells[2].AddParagraph().AppendText("PRICE PER NIGHT");
			row0.Cells[3].AddParagraph().AppendText("TOTAL");
			row0.Cells[3].Width = 80;

			WTableRow row1 = table.Rows[1];
			row1.Cells[0].AddParagraph().AppendText(bookingFromDb.Nights.ToString());
			row1.Cells[0].Width = 80;
			row1.Cells[1].AddParagraph().AppendText(bookingFromDb.Villa.Name);
			row1.Cells[2].Width = 220;
			row1.Cells[2].AddParagraph().AppendText(bookingFromDb.Villa.Price.ToString());
			row1.Cells[3].AddParagraph().AppendText(bookingFromDb.TotalCost.ToString("c"));
			row1.Cells[3].Width = 80;

			WTableStyle tableStyle = document.AddTableStyle("CustomStyle") as WTableStyle;
			tableStyle.TableProperties.RowStripe = 1;
			tableStyle.TableProperties.ColumnStripe = 2;
			tableStyle.TableProperties.Paddings.Top = 2;
			tableStyle.TableProperties.Paddings.Bottom = 1;
			tableStyle.TableProperties.Paddings.Left = 5.4f;
			tableStyle.TableProperties.Paddings.Right = 5.4f;

			ConditionalFormattingStyle firstRowStyle = tableStyle.ConditionalFormattingStyles.Add(ConditionalFormattingType.FirstRow);
			firstRowStyle.CharacterFormat.Bold = true;
			firstRowStyle.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			firstRowStyle.CellProperties.BackColor = Color.Black;

			table.ApplyStyle("CustomStyle");

			TextBodyPart bodyPart = new(document);
			bodyPart.BodyItems.Add(table);
			document.Replace("<ADDTABLEHERE>", bodyPart, false, false);


			using DocIORenderer renderer = new();
			MemoryStream stream = new();

			if(downloadType == "Word")
			{
				document.Save(stream, FormatType.Docx);
				stream.Position = 0;
				return File(stream, "application/docx", "BookingDetails.docx");
			}
			else
			{
				PdfDocument pdfDocument = renderer.ConvertToPDF(document);
				pdfDocument.Save(stream);
				stream.Position = 0;
                return File(stream, "application/pdf", "BookingDetails.pdf");

            }
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
