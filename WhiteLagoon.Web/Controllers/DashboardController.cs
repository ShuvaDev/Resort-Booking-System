﻿using Microsoft.AspNetCore.Mvc;
using System.Linq;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Application.Services.Interfaces;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetTotalBookingRadialChartData()
        {
            return Json(await _dashboardService.GetTotalBookingRadialChartData());
        }

        public async Task<IActionResult> GetRegisteredUserRadialChartData()
        {
            return Json(await _dashboardService.GetRegisteredUserRadialChartData());
        }

        public async Task<IActionResult> GetRevenueChartData()
        {
            return Json(await _dashboardService.GetRevenueChartData());
        }

		public async Task<IActionResult> GetTotalBookingPieChartData()
		{
            return Json(await _dashboardService.GetTotalBookingPieChartData());
        }
		
		public async Task<IActionResult> GetMemberAndBookingLineChartData()
        {   
            return Json(await _dashboardService.GetMemberAndBookingLineChartData());
        }
	}
}
