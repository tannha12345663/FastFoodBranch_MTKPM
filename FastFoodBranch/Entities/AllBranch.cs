using FastFoodBranch.Models;
using System.Collections.Generic;
using System.Linq;

namespace FastFoodBranch.Entities
{
    public class AllBranch : IBranch
    {
        public List<IBranch> _branches;
        public void AutoAddBranch(List<ChiNhanh> list)
        {
           _branches = new List<IBranch>();
           foreach (ChiNhanh ch in list)
            {
                Branch branch = new Branch();
                branch.fatoryFromChiNhanh(ch);
                _branches.Add(branch);
            }
        }
        public List<HoaDon> getListIncome(int year,string option)
        {
            if (option == BranchConstaint.ALL)
            {
                List<HoaDon> list = new List<HoaDon>();
                foreach (var branche in _branches)
                {
                    list.AddRange(branche.getListIncome(year));
                }
                return list;
            }
            else
            {
                Branch selectedBranch;
                foreach (Branch branch in _branches)
                {
                    if(branch.LocationID == option)
                    {
                        selectedBranch = branch;
                        return selectedBranch.getListIncome(year);
                    }
                }
                return new List<HoaDon>();               
            }

        }
    }
}