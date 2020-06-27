using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EntityLayer.Negocio.RealPlay;
using EntityLayer.Respuesta;
using AForge.Video;
using AForge.Video.DirectShow;
using EntityLayer.Negocio.Webcam;

namespace GUI
{
    public partial class Form1 : Form
    {
        List<ORealPlay> _Devices;
        ORespuesta _Respuesta;
        OWebcam _Webcam;
        int _Camaras;
        int _XLocation;
        int _YLocation;
        FilterInfoCollection _FilterInfoCollection;

        public Form1()
        {
            InitializeComponent();
            _Devices = new List<ORealPlay>();
            _Respuesta = new ORespuesta();
            _Camaras = 0;
            _XLocation = 0;
            _YLocation = 0;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBoxDispositivos.DataSource = _Devices;
            comboBoxDispositivos.DisplayMember = "Nombre";
            _FilterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo filterInfo in _FilterInfoCollection) { comboBoxWebCam.Items.Add(filterInfo.Name); }
            comboBoxWebCam.SelectedIndex = 0;
            _Webcam = new OWebcam();
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            ORealPlay _Data = new ORealPlay();
            _Data.IP = textBoxIP.Text;
            _Data.Puerto = ushort.Parse(textBoxPuerto.Text);
            _Data.Usuario = textBoxUsuario.Text;
            _Data.Password = textBoxPassword.Text;
            if (comboBoxMarca.SelectedIndex == 0)
            {
                _Respuesta = _Data.LoginHikvision();
                if (!_Respuesta.Exitoso) 
                { 
                    MessageBox.Show(_Respuesta.Mensaje);
                    return;
                }
                _Data.Nombre = "Hikvision " + _Data.IP;
                _Devices.Add(_Data);
            }
            else if (comboBoxMarca.SelectedIndex == 1)
            {
                _Respuesta = _Data.LoginDahua();
                if (!_Respuesta.Exitoso)
                {
                    MessageBox.Show(_Respuesta.Mensaje);
                    return;
                }
                _Data.Nombre = "Dahua " + _Data.IP;
                _Devices.Add(_Data);                
            }
            comboBoxDispositivos.DataSource = null;
            comboBoxDispositivos.DataSource = _Devices;
            comboBoxDispositivos.DisplayMember = "Nombre";
        }

        private void comboBoxDispositivos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxDispositivos.SelectedItem != null)
            {
                Console.WriteLine("pasa");
                comboBoxCanales.DataSource = null;
                comboBoxCanales.DataSource = ((ORealPlay)comboBoxDispositivos.SelectedItem).Canales;
            }
        }

        private void buttonConectar_Click(object sender, EventArgs e)
        {
            ORealPlay _Device = _Devices[comboBoxDispositivos.SelectedIndex];
            _Device.Canal = (int)comboBoxCanales.SelectedItem;
            if (_Device.HUserID >= 0)
            {
               var picture = new PictureBox
               {
                   Name = "canal" + _Camaras.ToString(),
                   Size = new Size(300, 200),
                   Location = new Point(_XLocation, _YLocation),
               };
               _XLocation += 300;
               if(_XLocation > 1500)
               {
                   _XLocation = 0;
                   _YLocation += 200;
               }
               panel1.Controls.Add(picture);
               _Device.Window = picture.Handle;
               _Respuesta = _Device.StartRealPlayHikvision();
               if (!_Respuesta.Exitoso)
               {
                   MessageBox.Show(_Respuesta.Mensaje);
                   return;
               }
            }
            else if(_Device.DUserID != IntPtr.Zero)
            {
                var picture = new PictureBox
                {
                    Name = "canal" + _Camaras.ToString(),
                    Size = new Size(300, 200),
                    Location = new Point(_XLocation, _YLocation),
                };
                _XLocation += 300;
                if (_XLocation > 1500)
                {
                    _XLocation = 0;
                    _YLocation += 200;
                }
                panel1.Controls.Add(picture);
                _Device.Window = picture.Handle;
                _Device.StartRealPlayDahua();
                if (!_Respuesta.Exitoso)
                {
                    MessageBox.Show(_Respuesta.Mensaje);
                    return;
                }
            }
        }

        private void buttonConectarWebCam_Click(object sender, EventArgs e)
        {
            var picture = new PictureBox
            {
                Name = "canal" + _Camaras.ToString(),
                Size = new Size(300, 200),
                Location = new Point(_XLocation, _YLocation),
            };
            _XLocation += 300;
            if (_XLocation > 1500)
            {
                _XLocation = 0;
                _YLocation += 200;
            }
            panel1.Controls.Add(picture);
            _Webcam.Image = picture;
            _Webcam.Device = new VideoCaptureDevice(_FilterInfoCollection[comboBoxWebCam.SelectedIndex].MonikerString);
            _Webcam.Device.NewFrame += VideoCaptureDevice_NewFrame;
            _Webcam.Device.Start();
        }

        private void VideoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            _Webcam.Image.Image = (Bitmap)eventArgs.Frame.Clone();
        }
    }
}
