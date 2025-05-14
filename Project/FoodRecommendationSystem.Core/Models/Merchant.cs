using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// Merchant.cs
using System.Collections.Generic;

namespace FoodRecommendationSystem.Core.Models
{
    public class Merchant
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public GeoLocation Location { get; set; }
        public List<Dish> Dishes { get; set; } = new List<Dish>();
        public double Rating { get; set; }
    }
}