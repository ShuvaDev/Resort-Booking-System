using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Application.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<RadialBarChartDTO> GetTotalBookingRadialChartData();
        Task<RadialBarChartDTO> GetRegisteredUserRadialChartData();
        Task<RadialBarChartDTO> GetRevenueChartData();
        Task<PieChartDTO> GetTotalBookingPieChartData();
        Task<LineChartDTO> GetMemberAndBookingLineChartData();
    }
}
