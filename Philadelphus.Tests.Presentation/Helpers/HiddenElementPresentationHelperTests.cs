using FluentAssertions;
using Philadelphus.Presentation.Helpers;

namespace Philadelphus.Tests.Presentation.Helpers;

public class HiddenElementPresentationHelperTests
{
    [Theory]
    [InlineData(false, 1.0)]
    [InlineData(true, 0.55)]
    public void GetOpacity_ReturnsUnifiedHiddenElementOpacity(bool isHidden, double expected)
        => HiddenElementPresentationHelper.GetOpacity(isHidden).Should().Be(expected);
}
