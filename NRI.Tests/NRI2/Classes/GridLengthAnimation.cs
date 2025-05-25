using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows;

namespace NRI.Classes
{
    public class GridLengthAnimation : AnimationTimeline
    {
        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(GridLength), typeof(GridLengthAnimation));

        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(GridLength), typeof(GridLengthAnimation));

        public GridLength From
        {
            get => (GridLength)GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }

        public GridLength To
        {
            get => (GridLength)GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }

        public override Type TargetPropertyType => typeof(GridLength);

        protected override Freezable CreateInstanceCore() => new GridLengthAnimation();

        public override object GetCurrentValue(object defaultOriginValue,
                                             object defaultDestinationValue,
                                             AnimationClock animationClock)
        {
            double fromVal = From.Value;
            double toVal = To.Value;

            if (animationClock.CurrentProgress == null)
                return new GridLength(fromVal, From.GridUnitType);

            double progress = animationClock.CurrentProgress.Value;

            double currentValue = fromVal + (toVal - fromVal) * progress;

            return new GridLength(currentValue, From.GridUnitType);
        }
    }
}
