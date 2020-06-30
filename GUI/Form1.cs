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
        List<ORealPlay> _Devices; //Lista con todas las camaras IP conectadas
        ORespuesta _Respuesta; //Respuesta de los metodos de los SDK, aqui se manejan los mensajes de error y podrian manejarse mensajes de login
        OWebcam _Webcam; //Objeto tipo webcam para conectar la webcam
        int _Camaras; //Numero de camaras conectadas
        int _XLocation; //Posicion horizontal para colocar un nuevo picturebox donde se visualizara el video
        int _YLocation; //Posicion vertical para colocar un nuevo picturebox donde se visualizara el video
        FilterInfoCollection _FilterInfoCollection; //Esta clase pertenece a la biblioteca AForge. Enlista las webcam disponibles

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
            comboBoxDispositivos.DataSource = _Devices; //Se le asigna la lista con los dispositivos conectados al comobobox de dispositivos
            comboBoxDispositivos.DisplayMember = "Nombre";
            _FilterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice); //Se agregan las webcam disponibles al combobox de webcam
            foreach (FilterInfo filterInfo in _FilterInfoCollection) { comboBoxWebCam.Items.Add(filterInfo.Name); }
            comboBoxWebCam.SelectedIndex = 0;
            _Webcam = new OWebcam();
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            ORealPlay _Data = new ORealPlay(); //Esto se ejecuta al darle click al boton de login. Primero de crea un nuevo objeto para la camara ip
            _Data.IP = textBoxIP.Text; //Se le asignan los atributos al objeto de la camara ip como la ip, el puerto, el usuario y la password
            _Data.Puerto = ushort.Parse(textBoxPuerto.Text);
            _Data.Usuario = textBoxUsuario.Text;
            _Data.Password = textBoxPassword.Text;
            if (comboBoxMarca.SelectedIndex == 0) //Si se selecciona la marca Hikvision se ejecutan los métodos para Hikvision
            {
                _Respuesta = _Data.LoginHikvision(); //Se logea
                if (!_Respuesta.Exitoso) //Si hay un error al logearse
                { 
                    MessageBox.Show(_Respuesta.Mensaje); //Muestra un error y sale del metodo 
                    return;
                }
                _Data.Nombre = "Hikvision " + _Data.IP; //Se agrega el dispositivo al combobox de dispositivos conectados
                _Devices.Add(_Data);
            }
            else if (comboBoxMarca.SelectedIndex == 1)//Si se selecciona la marca Dahua se ejecutan los métodos para Dahua
            {
                _Respuesta = _Data.LoginDahua();//Se logea
                if (!_Respuesta.Exitoso)//Si hay un error al logearse
                {
                    MessageBox.Show(_Respuesta.Mensaje);//Muestra un error y sale del metodo 
                    return;
                }
                _Data.Nombre = "Dahua " + _Data.IP; //Se agrega el dispositivo al combobox de dispositivos conectados
                _Devices.Add(_Data);                
            }
            comboBoxDispositivos.DataSource = null; //Se refresca la lista de dispositivos conectados
            comboBoxDispositivos.DataSource = _Devices;
            comboBoxDispositivos.DisplayMember = "Nombre";
        }

        private void comboBoxDispositivos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxDispositivos.SelectedItem != null)
            {
                comboBoxCanales.DataSource = null;
                comboBoxCanales.DataSource = ((ORealPlay)comboBoxDispositivos.SelectedItem).Canales;
            }
        }

        private void buttonConectar_Click(object sender, EventArgs e)
        {
            ORealPlay _Device = _Devices[comboBoxDispositivos.SelectedIndex]; //Obtene el objeto de l acamara ip del combobox para iniciar la transmision
            _Device.Canal = (int)comboBoxCanales.SelectedItem; //Obtiene el canal seleccionado
            if (_Device.HUserID >= 0) //Si al usuario es de una camara Hikvision
            {
               var picture = new PictureBox //Se agrega un uevo pictirebox para visualizar el video
               {
                   Name = "canal" + _Camaras.ToString(),
                   Size = new Size(300, 200), //El tamaño de la imagen, este se puede modificar
                   Location = new Point(_XLocation, _YLocation),
               };
               _XLocation += 300; //Aumenta el valor de la poscicion horizontal de los picturebox. Si cambia el ancho de los picturebox se debe cambiar el aumento.
               if(_XLocation > 1500) //Si hay 5 picturebox horizontales cambia a la siguiente linea
               {
                   _XLocation = 0;
                   _YLocation += 200; //Si cambia el largo de los picturebox debe cambiarse este aumento
               }
               panel1.Controls.Add(picture);
               _Device.Window = picture.Handle; //Se hace la referencia al picturebox creado para iniciar la transmision de video
               _Respuesta = _Device.StartRealPlayHikvision(); //Inicia la transmision
               if (!_Respuesta.Exitoso)
               {
                   MessageBox.Show(_Respuesta.Mensaje); //Si hubo error al iniciar la transmision manda un mensaje de error y sale del metodo
                   return;
               }
            }
            else if(_Device.DUserID != IntPtr.Zero) //Si el usuario es de una camara Dahua
            {
                var picture = new PictureBox //Se agrega un uevo pictirebox para visualizar el video
                {
                    Name = "canal" + _Camaras.ToString(),
                    Size = new Size(300, 200), //El tamaño de la imagen, este se puede modificar
                    Location = new Point(_XLocation, _YLocation),
                };
                _XLocation += 300; //Aumenta el valor de la poscicion horizontal de los picturebox. Si cambia el ancho de los picturebox se debe cambiar el aumento.
                if (_XLocation > 1500) //Si hay 5 picturebox horizontales cambia a la siguiente linea
                {
                    _XLocation = 0;
                    _YLocation += 200; //Si cambia el largo de los picturebox debe cambiarse este aumento
                }
                panel1.Controls.Add(picture);
                _Device.Window = picture.Handle; //Se hace la referencia al picturebox creado para iniciar la transmision de video
                _Device.StartRealPlayDahua(); //Inicia la transmision
                if (!_Respuesta.Exitoso)
                {
                    MessageBox.Show(_Respuesta.Mensaje); //Si hubo error al iniciar la transmision manda un mensaje de error y sale del metodo
                    return;
                }
            }
        }

        private void buttonConectarWebCam_Click(object sender, EventArgs e)
        {
            var picture = new PictureBox //Crea un picturebox para visualizar la webcam
            {
                Name = "canal" + _Camaras.ToString(),
                Size = new Size(300, 200), //El tamaño se puede modificar
                Location = new Point(_XLocation, _YLocation),
            };
            _XLocation += 300;
            if (_XLocation > 1500)
            {
                _XLocation = 0;
                _YLocation += 200;
            }
            panel1.Controls.Add(picture);
            _Webcam.Image = picture; //Se le asigna el picturebox al objeto webcam
            _Webcam.Device = new VideoCaptureDevice(_FilterInfoCollection[comboBoxWebCam.SelectedIndex].MonikerString); //Se inicializa la webcam
            _Webcam.Device.NewFrame += VideoCaptureDevice_NewFrame; //Evento que muestra el video en el pictiurebox
            _Webcam.Device.Start(); //inicia el video
        }

        private void VideoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            _Webcam.Image.Image = (Bitmap)eventArgs.Frame.Clone(); //Evento que muestra el video de la webcam
        }
    }
}
