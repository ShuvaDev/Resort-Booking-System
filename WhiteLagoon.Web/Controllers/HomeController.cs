using Microsoft.AspNetCore.Mvc;
using Syncfusion.Presentation;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            HomeVM homeVM = new HomeVM()
            {
                VillaList = _unitOfWork.villaRepository.GetAll(includeProperties: "VillaAmenities"),
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
                int totalBookedVilla = _unitOfWork.bookingRepository.GetAll(b => b.VillaId == villa.Id && b.Status != SD.StatusCancelled && ((searchCheckInDate >= b.CheckInDate && searchCheckInDate <= b.CheckOutDate) || (searchCheckOutDate >= b.CheckInDate && searchCheckOutDate <= b.CheckOutDate))).Count();

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
        public IActionResult GeneratePPTExport(int id)
        {
            var villa = _unitOfWork.villaRepository.Get(v => v.Id == id, includeProperties: "VillaAmenities");
            if (villa == null)
            {
                return RedirectToAction(nameof(Error));
            }

            string basePath = _webHostEnvironment.WebRootPath;
            string filePath = basePath + @"/export/ExportVillaDetails.pptx";

            using IPresentation presentation = Presentation.Open(filePath);
            ISlide slide = presentation.Slides[0];

            IShape shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaName") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = villa.Name;
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaDescription") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = villa.Description;
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtOccupancy") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("Max Occupancy : {0} adults", villa.Occupancy);
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaSize") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("Villa Size : {0} sqft", villa.Sqft);
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtPricePerNight") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("USD {0}/night", villa.Price);
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaAmenitiesHeading") as IShape;
            if (shape is not null)
            {
                List<string> listItems = villa.VillaAmenities.Select(v => v.Name).ToList();
                shape.TextBody.Text = "";
                foreach (var item in listItems)
                {
                    IParagraph paragraph = shape.TextBody.AddParagraph();
                    ITextPart textPart = paragraph.AddTextPart(item);

                    paragraph.ListFormat.Type = ListType.Bulleted;
                    paragraph.ListFormat.BulletCharacter = '\u2022';
                    textPart.Font.FontName = "Bahnschrift";
                    textPart.Font.FontSize = 18;
                    textPart.Font.Color = ColorObject.FromArgb(144, 148, 152);
                }
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "imgVilla") as IShape;
            if (shape is not null)
            {
                byte[] imageData;
                string imageUrl;
                try
                {
                    imageUrl = string.Format("{0}{1}", basePath, villa.ImageUrl);
                    imageData = System.IO.File.ReadAllBytes(imageUrl);
                }
                catch (Exception)
                {
                    imageUrl = string.Format("{0}{1}", basePath, "/images/placeholder.png");
                    imageData = System.IO.File.ReadAllBytes(imageUrl);
                }
                slide.Shapes.Remove(shape);
                using MemoryStream imageStream = new(imageData);
                IPicture newPicture = slide.Pictures.AddPicture(imageStream, 60, 120, 300, 200);

            }
            MemoryStream stream = new();
            presentation.Save(stream);
            stream.Position = 0;
            return File(stream, "application/pptx", "villa.pptx");
        }
        public IActionResult Error()
        {
            return View();
        }
    }
}
