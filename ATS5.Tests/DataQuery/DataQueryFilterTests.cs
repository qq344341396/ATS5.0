using System;
using ATS5.Application.DataQuery;
using Xunit;

namespace ATS5.Tests.DataQuery
{
    public sealed class DataQueryFilterTests
    {
        [Fact]
        public void ToLegacyCriteria_UsesFullDayLogGuidBounds()
        {
            var filter = new DataQueryFilter
            {
                StartDate = new DateTime(2026, 5, 1, 14, 30, 0),
                EndDate = new DateTime(2026, 5, 2, 8, 15, 0)
            };

            var criteria = filter.ToLegacyCriteria();

            Assert.Equal(20260501000000UL, criteria.StartLogGuid);
            Assert.Equal(20260502235959UL, criteria.EndLogGuid);
        }

        [Fact]
        public void ToLegacyCriteria_IgnoresDisabledFilters()
        {
            var filter = new DataQueryFilter
            {
                StartDate = new DateTime(2026, 5, 1),
                EndDate = new DateTime(2026, 5, 1),
                IsBarcodeFilterEnabled = false,
                Barcode = "ABC",
                IsChannelFilterEnabled = false,
                Channel = 2,
                IsFlowFilterEnabled = false,
                FlowName = "FlowA"
            };

            var criteria = filter.ToLegacyCriteria();

            Assert.Equal(string.Empty, criteria.Barcode);
            Assert.Equal((ushort)0, criteria.Channel);
            Assert.Equal(string.Empty, criteria.FlowName);
        }

        [Fact]
        public void ToLegacyCriteria_KeepsEnabledFilters()
        {
            var filter = new DataQueryFilter
            {
                StartDate = new DateTime(2026, 5, 1),
                EndDate = new DateTime(2026, 5, 1),
                IsBarcodeFilterEnabled = true,
                Barcode = "ABC",
                IsChannelFilterEnabled = true,
                Channel = 2,
                IsFlowFilterEnabled = true,
                FlowName = "FlowA"
            };

            var criteria = filter.ToLegacyCriteria();

            Assert.Equal("ABC", criteria.Barcode);
            Assert.Equal((ushort)2, criteria.Channel);
            Assert.Equal("FlowA", criteria.FlowName);
        }

        [Fact]
        public void Validate_ReturnsLegacyWarning_WhenDateSpanExceeds180Days()
        {
            var filter = new DataQueryFilter
            {
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 7, 1)
            };

            var result = filter.Validate();

            Assert.False(result.IsValid);
            Assert.Equal("日期跨度不能大于180天，请调整查询日期", result.Message);
        }
    }
}
