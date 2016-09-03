using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastScreenRecorder
{
    public class PiAction
    {
        private ProjectInfo pi;

        public PiAction()
        {

        }
        public PiAction(ProjectInfo pi)
        {
            // get projectInfo.json file

            if (!File.Exists(pi.ProjectLocation + "/ProjectInfo.json"))
            {
                // if not exist pi file
                string json = JsonConvert.SerializeObject(pi);
                File.WriteAllText(pi.ProjectLocation + "/ProjectInfo.json", json);
            }
        }

        public ProjectInfo GetPi(string location)
        {
            if(!File.Exists(location + "/ProjectInfo.json"))
            {
                System.Windows.Forms.MessageBox.Show("Proje dosyası bulunamadı!");
            }
            else
            {
                using (StreamReader r = new StreamReader(location + "/ProjectInfo.json"))
                {
                    string json = r.ReadToEnd();
                    this.pi = JsonConvert.DeserializeObject<ProjectInfo>(json);
                }
                return this.pi;
            }
            return null;
            
        }

        public void writePi(string location, ProjectInfo newPi)
        {
            string json = JsonConvert.SerializeObject(newPi);
            File.WriteAllText(location + "/ProjectInfo.json", json);
        }
    }
}