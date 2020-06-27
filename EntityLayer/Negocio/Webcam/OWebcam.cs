using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video.DirectShow;

namespace EntityLayer.Negocio.Webcam
{
    public class OWebcam
    {
        public PictureBox Image { get; set; }
        public VideoCaptureDevice Device { get; set; }
        public OWebcam()
        {
            this.Image = new PictureBox();
            this.Device = new VideoCaptureDevice();
        }
    }
}
