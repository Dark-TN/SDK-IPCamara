using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityLayer.SDK.Hikvision;
using EntityLayer.SDK.Dahua;
using EntityLayer.Respuesta;

namespace EntityLayer.Negocio.RealPlay
{
    public class ORealPlay
    {
        public string Nombre { get; set; }
        public IntPtr Window { get; set; }
        public string Usuario { get; set; }
        public string Password { get; set; }
        public string IP { get; set; }
        public ushort Puerto { get; set; }
        public int Canal { get; set; }
        public List<int> Canales { get; set; }
        public int HUserID { get; set; }
        public int HRealPlayID { get; set; }
        public List<int> HStreams { get; set; }
        public IntPtr DUserID { get; set; }
        public IntPtr DRealPlayID { get; set; }
        public List<IntPtr> DStreams { get; set; }

        public ORealPlay()
        {
            this.Window = IntPtr.Zero;
            this.Usuario = String.Empty;
            this.Password = String.Empty;
            this.IP = String.Empty;
            this.Canal = 0;
            this.Canales = new List<int>();
            this.Puerto = 0;
            this.HUserID = -1;
            this.HRealPlayID = -1;
            this.HStreams = new List<int>();
            this.DUserID = IntPtr.Zero;
            this.DRealPlayID = IntPtr.Zero;
            this.DStreams = new List<IntPtr>();
        }

        public ORespuesta LoginHikvision()
        {
            ORespuesta _Respuesta = new ORespuesta();
            CHCNetSDK.NET_DVR_Init();
            CHCNetSDK.NET_DVR_SetConnectTime(2000, 1);
            CHCNetSDK.NET_DVR_SetReconnect(10000, 1);
            CHCNetSDK.NET_DVR_DEVICEINFO_V30 struDeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();
            string ip = this.IP;
            ushort port = this.Puerto;
            string user = this.Usuario;
            string password = this.Password;
            this.HUserID = CHCNetSDK.NET_DVR_Login_V30(ip, port, user, password, ref struDeviceInfo);
            if (this.HUserID < 0)
            {
                _Respuesta.Mensaje = string.Format("Error en Login");
                _Respuesta.Error = CHCNetSDK.NET_DVR_GetLastError();
                CHCNetSDK.NET_DVR_Cleanup();
                this.HUserID = 0;
                return _Respuesta;
            }
            for (int i = 1; i <= Convert.ToInt32(struDeviceInfo.byChanNum); i++) { this.Canales.Add(i); }
            _Respuesta.Exitoso = true;
            return _Respuesta;
        }

        public ORespuesta LoginDahua()
        {
            ORespuesta _Respuesta = new ORespuesta();
            fDisConnectCallBack m_DisConnectCallBack;
            fHaveReConnectCallBack m_ReConnectCallBack;
            m_DisConnectCallBack = new fDisConnectCallBack(DisConnectCallBack);
            m_ReConnectCallBack = new fHaveReConnectCallBack(ReConnectCallBack);
            bool result = NETClient.Init(m_DisConnectCallBack, IntPtr.Zero, null);
            if (!result)
            {
                _Respuesta.Mensaje = NETClient.GetLastError();
                return _Respuesta;
            }
            NETClient.SetAutoReconnect(m_ReConnectCallBack, IntPtr.Zero);
            NET_PARAM param = new NET_PARAM()
            {
                nWaittime = 10000,
                nConnectTime = 5000,
            };
            NETClient.SetNetworkParam(param);
            NET_DEVICEINFO_Ex m_DeviceInfo = new NET_DEVICEINFO_Ex();
            this.DUserID = NETClient.LoginWithHighLevelSecurity(this.IP, this.Puerto, this.Usuario, this.Password, EM_LOGIN_SPAC_CAP_TYPE.TCP, IntPtr.Zero, ref m_DeviceInfo);
            if (IntPtr.Zero == this.DUserID)
            {
                _Respuesta.Mensaje = NETClient.GetLastError();
                return _Respuesta;
            }
            for (int i = 1; i <= m_DeviceInfo.nChanNum; i++) { this.Canales.Add(i); }
            _Respuesta.Exitoso = true;
            return _Respuesta;
        }

        public ORespuesta StartRealPlayHikvision()
        {
            ORespuesta _Respuesta = new ORespuesta();
            CHCNetSDK.NET_DVR_SetExceptionCallBack_V30(0, IntPtr.Zero, g_ExceptionCallBack, IntPtr.Zero);
            CHCNetSDK.NET_DVR_PREVIEWINFO struPlayInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
            struPlayInfo.hPlayWnd = this.Window;
            struPlayInfo.lChannel = this.Canal; 
            struPlayInfo.dwStreamType = 0;
            struPlayInfo.dwLinkMode = 0;
            this.HRealPlayID = CHCNetSDK.NET_DVR_RealPlay_V40(this.HUserID, ref struPlayInfo, null, IntPtr.Zero);
            if (this.HRealPlayID < 0)
            {
                _Respuesta.Mensaje = string.Format("Error en RealPlay");
                _Respuesta.Error = CHCNetSDK.NET_DVR_GetLastError();
                CHCNetSDK.NET_DVR_Cleanup();
                this.HRealPlayID = 0;
                return _Respuesta;
            }
            this.HStreams.Add(this.HRealPlayID);
            _Respuesta.Exitoso = true;
            return _Respuesta;
        }

        public ORespuesta StartRealPlayDahua()
        {
            ORespuesta _Respuesta = new ORespuesta();
            this.DRealPlayID = NETClient.RealPlay(this.DUserID, 0, this.Window, EM_RealPlayType.Realplay);
            if (IntPtr.Zero == this.DRealPlayID)
            {
                _Respuesta.Mensaje = NETClient.GetLastError();
                return _Respuesta;
            }
            this.DStreams.Add(this.DRealPlayID);
            _Respuesta.Exitoso = true;
            return _Respuesta;
        }

        public ORespuesta StopRealPlayHikvision()
        {
            ORespuesta _Respuesta = new ORespuesta();
            if (!CHCNetSDK.NET_DVR_StopRealPlay(this.HRealPlayID))
            {
                _Respuesta.Mensaje = string.Format("Error en RealPlay");
                _Respuesta.Error = CHCNetSDK.NET_DVR_GetLastError();
                CHCNetSDK.NET_DVR_Cleanup();
                return _Respuesta;
            }
            CHCNetSDK.NET_DVR_Logout(this.HUserID);
            CHCNetSDK.NET_DVR_Cleanup();
            this.HUserID = 0;
            this.HRealPlayID = 0;
            _Respuesta.Exitoso = true;
            return _Respuesta;
        }

        public ORespuesta StopRealPlayDahua()
        {
            ORespuesta _Respuesta = new ORespuesta();
            bool ret = NETClient.StopRealPlay(this.DRealPlayID);
            if (!ret)
            {
                _Respuesta.Mensaje = NETClient.GetLastError();
                return _Respuesta;
            }
            this.DRealPlayID = IntPtr.Zero;
            if (IntPtr.Zero != this.DUserID)
            {
                bool result = NETClient.Logout(this.DUserID);
                if (!result)
                {
                    _Respuesta.Mensaje = NETClient.GetLastError();
                    return _Respuesta;
                }
                this.DUserID = IntPtr.Zero;
            }
            NETClient.Cleanup();
            _Respuesta.Exitoso = true;
            return _Respuesta;
        }

        public void g_ExceptionCallBack(uint dwType, int lUserID, int lHandle, IntPtr pUser)
        {
            Console.WriteLine("algo");
        }

        private void DisConnectCallBack(IntPtr lLoginID, IntPtr pchDVRIP, int nDVRPort, IntPtr dwUser)
        {
            Console.WriteLine("algo");
        }

        private void ReConnectCallBack(IntPtr lLoginID, IntPtr pchDVRIP, int nDVRPort, IntPtr dwUser)
        {
            Console.WriteLine("algo");
        }
    }
}
