using System;

namespace PlexureExercises.CouponApi.Models
{
    public class Coupon {
        Guid Id { get; set; }
        string Title { get; set; }
        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
        int MaxCoupons { get; set; }
        int MaxCouponsPerUser { get; set; }
    }
}