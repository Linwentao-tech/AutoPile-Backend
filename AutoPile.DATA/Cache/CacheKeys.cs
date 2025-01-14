using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DATA.Cache
{
    public static class CacheKeys
    {
        public static string ShoppingCart(string userId) => $"ShoppingCart:{userId}";

        public static string Product(string productId) => $"Product:{productId}";

        public static string Review(string productId) => $"Product:{productId}:Reviews";

        public static string User(string applicationUserId) => $"User:{applicationUserId}";

        public static string Order(string applicationUserId) => $"Order:{applicationUserId}";
    };
}