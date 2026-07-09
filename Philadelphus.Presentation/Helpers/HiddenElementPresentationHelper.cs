namespace Philadelphus.Presentation.Helpers
{
    public static class HiddenElementPresentationHelper
    {
        public const double VisibleOpacity = 1.0;
        public const double HiddenOpacity = 0.55;

        public static double GetOpacity(bool isHidden)
        {
            return isHidden ? HiddenOpacity : VisibleOpacity;
        }
    }
}
