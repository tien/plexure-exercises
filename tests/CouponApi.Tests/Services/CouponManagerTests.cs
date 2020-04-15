using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PlexureExercises.CouponApi.Models;
using PlexureExercises.CouponApi.Services;
using Xunit;

namespace PlexureExercises.CouponApi.Tests.Services
{
    public class CouponManagerTests
    {
        [Theory]
        [InlineData(new bool[] { false, false, true }, true)]
        [InlineData(new bool[] { false, true, false }, true)]
        [InlineData(new bool[] { true, false, false }, true)]
        [InlineData(new bool[] { false, false, false }, false)]
        public async Task WhenEvaluatingCouponRedeemability_ShouldGiveCorrectResult(bool[] evaluatorsResults, bool expectedRedeemability)
        {
            var couponManager = CreateCouponManager();

            var userId = Guid.NewGuid();
            var couponId = Guid.NewGuid();
            var evaluators = evaluatorsResults
                .Select<bool, Func<Coupon, Guid, bool>>(result => (_, __) => result);

            var sut = await couponManager.CanRedeemCoupon(userId, couponId, evaluators);

            sut.Should().Equals(expectedRedeemability);
        }

        [Fact]
        public async Task WhenEvaluatingCouponRedeemability_ShouldThrowError_WithNullEvaluators()
        {
            var couponManager = CreateCouponManager();

            var userId = Guid.NewGuid();
            var couponId = Guid.NewGuid();

            Func<Task<bool>> act = () => couponManager.CanRedeemCoupon(couponId, userId, null);

            await act.Should().ThrowExactlyAsync<ArgumentNullException>();
        }

        [Theory, AutoData]
        public async Task WhenEvaluatingCouponRedeemability_ShouldThrowError_WithNoCouponFound(IEnumerable<Func<Coupon, Guid, bool>> evaluators)
        {
            var couponProvider = Mock.Of<ICouponProvider>(x => x.Retrieve(It.IsAny<Guid>()) == Task.FromResult<Coupon?>(null));

            var couponManager = CreateCouponManager(couponProvider: couponProvider);

            var userId = Guid.NewGuid();
            var couponId = Guid.NewGuid();

            Func<Task<bool>> act = () => couponManager.CanRedeemCoupon(couponId, userId, evaluators);

            await act.Should().ThrowExactlyAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task WhenEvaluatingCouponRedeemability_ShouldDeemCouponRedeemable_WithNoEvaluator()
        {
            var couponManager = CreateCouponManager();

            var userId = Guid.NewGuid();
            var couponId = Guid.NewGuid();
            var evaluators = new Func<Coupon, Guid, bool>[0];

            var sut = await couponManager.CanRedeemCoupon(couponId, userId, evaluators);

            sut.Should().Be(true);
        }

        private ICouponManager CreateCouponManager(ILogger? logger = null, ICouponProvider? couponProvider = null)
        {
            var coupon = new Fixture().Create<Coupon>();

            return new CouponManager(
                logger ?? Mock.Of<ILogger>(),
                couponProvider ?? Mock.Of<ICouponProvider>(x => x.Retrieve(It.IsAny<Guid>()) == Task.FromResult(coupon)));
        }
    }
}