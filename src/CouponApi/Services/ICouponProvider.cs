using System;
using System.Threading.Tasks;
using PlexureExercises.CouponApi.Models;

namespace PlexureExercises.CouponApi.Services
{
    public interface ICouponProvider
    {
        Task<Coupon?> Retrieve(Guid id);
    }
}