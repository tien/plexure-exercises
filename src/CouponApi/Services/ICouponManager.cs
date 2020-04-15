using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexureExercises.CouponApi.Models;

namespace PlexureExercises.CouponApi.Services
{
    public interface ICouponManager
    {
        Task<bool> CanRedeemCoupon(Guid couponId, Guid userId, IEnumerable<Func<Coupon, Guid, bool>>? evaluators);
    }
}