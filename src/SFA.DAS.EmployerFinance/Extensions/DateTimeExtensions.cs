﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Extensions
{
    public static class DateTimeExtensions
    {
        private const int FinancialYearStartDay = 20;
        private const int FinancialYearStartMonth = 4;

        public static string ToFinancialYearString(this DateTime dateTime)
        {
            DateTime FinancialYearStartDate = new DateTime(DateTime.UtcNow.Year, FinancialYearStartMonth, FinancialYearStartDay);
            if(dateTime < FinancialYearStartDate)
            {
                return $"{dateTime.Year - 1}/{dateTime.ToString("yy")}";
            }
            else
            {
                return $"{dateTime.Year}/{dateTime.AddYears(1).ToString("yy")}";
            }
        }

        public static string ToNextFinancialYearString(this DateTime dateTime)
        {
            DateTime FinancialYearStartDate = new DateTime(DateTime.UtcNow.Year, FinancialYearStartMonth, FinancialYearStartDay);
            if (dateTime < FinancialYearStartDate)
            {
                return $"{dateTime.Year}/{dateTime.AddYears(1).ToString("yy")}";
            }
            else
            {
                return $"{dateTime.AddYears(1).Year}/{dateTime.AddYears(2).ToString("yy")}";
            }
        }

        public static string ToYearAfterNextFinancialYearString(this DateTime dateTime)
        {
            DateTime FinancialYearStartDate = new DateTime(DateTime.UtcNow.Year, FinancialYearStartMonth, FinancialYearStartDay);
            if (dateTime < FinancialYearStartDate)
            {
                return $"{dateTime.AddYears(1).Year}/{dateTime.AddYears(2).ToString("yy")}";
            }
            else
            {
                return $"{dateTime.AddYears(2).Year}/{dateTime.AddYears(3).ToString("yy")}";
            }
        }
    }
}
