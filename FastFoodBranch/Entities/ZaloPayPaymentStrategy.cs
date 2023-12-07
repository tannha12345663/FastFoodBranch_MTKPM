using FastFoodBranch.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace FastFoodBranch.Entities
{
    public class ZaloPayPaymentStrategy : IPaymentStrategy
    {

        public string GeneratePaymentUrl(Cart cart,string maHD)
        {
            throw new NotImplementedException();
        }

        public string ValidatePaymentResult(NameValueCollection data)
        {
            throw new NotImplementedException();
        }
    }
}