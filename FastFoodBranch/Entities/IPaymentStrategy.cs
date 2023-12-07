using FastFoodBranch.Models;
using System.Collections.Specialized;

namespace FastFoodBranch.Entities
{
    internal interface IPaymentStrategy
    {
        string GeneratePaymentUrl(Cart cart,string mahd);
        string ValidatePaymentResult(NameValueCollection data);
    }
}
