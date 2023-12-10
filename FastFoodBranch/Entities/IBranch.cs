
using FastFoodBranch.Models;
using System.Collections.Generic;

namespace FastFoodBranch.Entities
{
    public interface IBranch
    {
        List<HoaDon> getListIncome(int year,string option="");
    }
}
