﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Application.Services.Interfaces;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Application.Services.Implementation
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        static int previousMonth = DateTime.Now.Month == 1 ? 12 : DateTime.Now.Month - 1;
        static int previousYear = DateTime.Now.Month == 1 ? DateTime.Now.Year - 1 : DateTime.Now.Year;

        readonly DateTime previousMonthStartDate = new(previousYear, previousMonth, 1);
        readonly DateTime currentMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);
        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public class JoinResult
        {
            public DateTime DateTime { get; set; }
            public int NewBookingCount { get; set; }
            public int NewCustomerCount { get; set; }
        }
        public async Task<LineChartDTO> GetMemberAndBookingLineChartData()
        {
            var bookingData = _unitOfWork.bookingRepository.GetAll(b => b.BookingDate >= DateTime.Now.AddDays(-30) && b.BookingDate.Date <= DateTime.Now)
                .GroupBy(b => b.BookingDate.Date)
                .Select(u => new {
                    DateTime = u.Key,
                    NewBookingCount = u.Count()
                });

            var customerData = _unitOfWork.applicationUserRepository.GetAll(u => u.CreatedAt >= DateTime.Now.AddDays(-30) && u.CreatedAt <= DateTime.Now)
                .GroupBy(u => Convert.ToDateTime(u.CreatedAt).Date)
                .Select(g => new {
                    DateTime = g.Key,
                    NewCustomerCount = g.Count()
                });

            var leftJoin = bookingData.GroupJoin(customerData, b => b.DateTime, c => c.DateTime, (booking, customer) => new JoinResult
            {
                DateTime = booking.DateTime,
                NewBookingCount = booking.NewBookingCount,
                NewCustomerCount = customer.Select(c => c.NewCustomerCount).FirstOrDefault()
            });

            var rightJoin = customerData.GroupJoin(bookingData, c => c.DateTime, b => b.DateTime, (customer, booking) => new JoinResult
            {
                DateTime = customer.DateTime,
                NewCustomerCount = customer.NewCustomerCount,
                NewBookingCount = booking.Select(c => c.NewBookingCount).FirstOrDefault()
            });

            var mergedData = leftJoin.Union(rightJoin).OrderBy(x => x.DateTime)
            .GroupBy(x => x.DateTime)
                .Select(g => new JoinResult
                {
                    DateTime = g.Key,
                    NewBookingCount = g.First().NewBookingCount,
                    NewCustomerCount = g.First().NewCustomerCount
                });

            var newBookingData = mergedData.Select(x => x.NewBookingCount).ToArray();
            var newCustomerData = mergedData.Select(x => x.NewCustomerCount).ToArray();
            var categories = mergedData.Select(x => x.DateTime.ToString("dd/MM/yyyy")).ToArray();

            List<ChartData> chartDataList = new()
            {
                new()
                {
                    Name = "New Bookings",
                    Data = newBookingData
                },
                new()
                {
                    Name = "New Members",
                    Data = newCustomerData
                }
            };

            LineChartDTO lineChartDTO = new LineChartDTO()
            {
                Categories = categories,
                Series = chartDataList
            };
            return lineChartDTO;
        }

        public async Task<RadialBarChartDTO> GetRegisteredUserRadialChartData()
        {
            var totalUsers = _unitOfWork.applicationUserRepository.GetAll();

            var countByCurrentMonth = totalUsers.Where(u => u.CreatedAt >= currentMonthStartDate && u.CreatedAt <= DateTime.Now).Count();
            var countByPreviousMonth = totalUsers.Where(u => u.CreatedAt >= previousMonthStartDate && u.CreatedAt < currentMonthStartDate).Count();

            int increaseDecreaseRatio = 100;

            if (countByPreviousMonth != 0)
            {
                increaseDecreaseRatio = Convert.ToInt32((countByCurrentMonth - countByPreviousMonth) / countByPreviousMonth * 100);
            }

            RadialBarChartDTO radialBarChartDTO = new()
            {
                TotalCount = totalUsers.Count(),
                CountInCurrentMonth = countByCurrentMonth,
                HasRatioIncreased = countByCurrentMonth > countByPreviousMonth,
                Series = new int[] { increaseDecreaseRatio }
            };
            return radialBarChartDTO;
        }

        public async Task<RadialBarChartDTO> GetRevenueChartData()
        {
            var totalBookings = _unitOfWork.bookingRepository.GetAll(b => b.Status != SD.StatusPending && b.Status != SD.StatusCancelled);

            var totalRevenue = Convert.ToDecimal(totalBookings.Sum(b => b.TotalCost));

            var countByCurrentMonth = Convert.ToDecimal(totalBookings.Where(b => b.BookingDate >= currentMonthStartDate && b.BookingDate <= DateTime.Now).Sum(b => b.TotalCost));
            var countByPreviousMonth = Convert.ToDecimal(totalBookings.Where(b => b.BookingDate >= previousMonthStartDate && b.BookingDate < currentMonthStartDate).Sum(b => b.TotalCost));

            int increaseDecreaseRatio = 100;

            if (countByPreviousMonth != 0)
            {
                increaseDecreaseRatio = Convert.ToInt32((countByCurrentMonth - countByPreviousMonth) / countByPreviousMonth * 100);
            }

            RadialBarChartDTO radialBarChartDTO = new()
            {
                TotalCount = totalRevenue,
                CountInCurrentMonth = countByCurrentMonth,
                HasRatioIncreased = countByCurrentMonth > countByPreviousMonth,
                Series = new int[] { increaseDecreaseRatio }
            };
            return radialBarChartDTO;
        }

        public async Task<PieChartDTO> GetTotalBookingPieChartData()
        {
            var totalBookings = _unitOfWork.bookingRepository.GetAll(b => b.BookingDate >= DateTime.Now.AddDays(-30) && (b.Status != SD.StatusPending && b.Status != SD.StatusCancelled));

            var customerWithOneBookings = totalBookings.GroupBy(b => b.UserId).Where(x => x.Count() == 1).Select(x => x.Key).ToList();

            int bookingsByNewCustomer = customerWithOneBookings.Count();
            int bookingsByReturningCustomer = totalBookings.Count() - bookingsByNewCustomer;

            PieChartDTO pieChartDTO = new()
            {
                Labels = new string[] { "New Customer Bookings", "Returning Customer Bookings" },
                Series = new decimal[] { bookingsByNewCustomer, bookingsByReturningCustomer }
            };
            return pieChartDTO;
        }

        public async Task<RadialBarChartDTO> GetTotalBookingRadialChartData()
        {
            var totalBookings = _unitOfWork.bookingRepository.GetAll(b => b.Status != SD.StatusPending && b.Status != SD.StatusCancelled);

            var countByCurrentMonth = totalBookings.Where(b => b.BookingDate >= currentMonthStartDate && b.BookingDate <= DateTime.Now).Count();
            var countByPreviousMonth = totalBookings.Where(b => b.BookingDate >= previousMonthStartDate && b.BookingDate < currentMonthStartDate).Count();

            int increaseDecreaseRatio = 100;

            if (countByPreviousMonth != 0)
            {
                increaseDecreaseRatio = Convert.ToInt32((countByCurrentMonth - countByPreviousMonth) / countByPreviousMonth * 100);
            }

            RadialBarChartDTO radialBarChartDTO = new()
            {
                TotalCount = totalBookings.Count(),
                CountInCurrentMonth = countByCurrentMonth,
                HasRatioIncreased = countByCurrentMonth > countByPreviousMonth,
                Series = new int[] { increaseDecreaseRatio }
            };
            return radialBarChartDTO;
        }
    }
}
