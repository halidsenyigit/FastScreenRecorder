using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastScreenRecorder
{
    public class ProjectInfo
    {
        public string ProjectName { get; set; }

        public string ProjectLocation { get; set; }
        public List<Images> Images { get; set; }

        public SettingsModel SettingsModel { get; set; }

        public int LastImageID { get; set; }
    }

    
    public class Images
    {
        public int imageId { get; set; }
        public DateTime Date { get; set; }
        public string ImageName { get; set; }
    }
}
