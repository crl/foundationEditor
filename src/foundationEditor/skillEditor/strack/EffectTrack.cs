using foundation;
using UnityEngine;

namespace foundationEditor
{
    public class AnimatorTrack : BaseTrack
    {
        public override Color stackColor
        {
            get
            {
                return Color.gray;
            }
        }
    }
    public class EffectTrack:BaseTrack
    {
        public override Color stackColor
        {
            get
            {
                return ColorUtils.RGBToColor(0x019ADD);
            }
        }
    }

    public class SoundTrack : BaseTrack
    {
        public override Color stackColor
        {
            get { return ColorUtils.RGBToColor(0xFEA527); }
        }
    }

    public class CameraTrack : BaseTrack
    {
        public override Color stackColor
        {
            get { return ColorUtils.RGBToColor(0x014544); }
        }

        public override Color defaultPointColor {
            get { return stackColor; }
        }
    }


    public class EventTrack : BaseTrack
    {
        public override Color stackColor
        {
            get { return ColorUtils.RGBToColor(0x990000); }
        }

        public override Color defaultPointColor
        {
            get { return stackColor; }
        }
    }

    public class TimelineTrack : BaseTrack
    {
        public override Color stackColor
        {
            get
            {
                return ColorUtils.RGBToColor(0x321914);
            }
        }
    }



    
}