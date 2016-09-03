using System.Drawing;

namespace FastScreenRecorder
{

    public enum ProgramStatus
    {
        Pending,
        Stoped,
        Active
    }

    public class SettingsModel
    {
        public string RecordLocation { get; set; }

        // Frame Per Minute
        // for exp. 5 => 60/5 = 12 frame per minute
        // 12 frame per minute 1 hour equals 60*12 = 720 pict.
        // 24 fps rendering => 1 sec. 24 frame => 1 min 60*24 frame
        // in this case 1 hour ~= 1 min 
        public int FPM { get; set; }

        public ProgramStatus ProgramStat { get; set; }

        public Brush StatusColor {
            get {
                switch (this.ProgramStat)
                {   
                    case ProgramStatus.Pending:
                        return Brushes.Yellow;
                    case ProgramStatus.Stoped:
                        return Brushes.Red;
                    case ProgramStatus.Active:
                        return Brushes.Green;
                    default:
                        return Brushes.Black;
                }
            }
        }

        public Size WxH { get; set; }
    }
}
