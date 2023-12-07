
using FastFoodBranch.Models;
using System.Collections.Generic;

namespace FastFoodBranch.Entities
{
    internal interface IBranch
    {
        List<HoaDon> getListIncome(int year,string option="");
    }
}
