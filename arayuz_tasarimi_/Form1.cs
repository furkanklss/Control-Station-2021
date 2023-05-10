using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using GMap.NET.MapProviders;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms.DataVisualization.Charting;
using Microsoft.Office.Interop.Excel;
using excel = Microsoft.Office.Interop.Excel;
using AForge.Video;
using AForge.Video.DirectShow;
using Accord.Video.FFMPEG;
using Accord.Video.VFW;
using Rectangle = System.Drawing.Rectangle;




// G|SAT MAHPERİ MODEL UYDU TAKIMI YER İSTASYONU, Furkan KELEŞ tarafından hazırlanmıştır. İletişim: furkankls70@gmail.com
// G|SAT MAHPERI MODEL SATELLITE TEAM GROUND STATION was prepared by Furkan KELEŞ. Contact: furkankls70@gmail.com

namespace arayuz_tasarimi_

{
    public partial class Form1 : Form
    {
        int A = 0;
        int B = 1;
        double a, b;
        double x, y, z;
        Color renk1 = Color.Gray, renk2 = Color.Maroon;
        int line = 1;
        int column = 1;
        int lineNumber = 1;
        UdpClient udpClient;
        System.Threading.Thread ThreadReceive;

        //
        private FilterInfoCollection VideoCaptureDevices;
        private VideoCaptureDevice FinalVideo = null;
        private VideoCaptureDeviceForm captureDevice;
        private Bitmap video;
        private VideoFileWriter FileWriter = new VideoFileWriter();
        private SaveFileDialog saveAvi;


        public Form1()
        {
            InitializeComponent();
            chart1.Titles.Add("VOLTAJ GRAFİĞİ");
            chart2.Titles.Add("SICAKLIK GRAFİĞİ");
            chart3.Titles.Add("BASINÇ GRAFİĞİ");
            chart4.Titles.Add("YÜKSEKLİK GRAFİĞİ");
        }
       
        private void Form1_Load(object sender, EventArgs e)     // Form yüklemesi.
        {
            chart1.ChartAreas[0].AxisY.Minimum = 0;      
            chart1.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm:ss";
            //
            chart2.ChartAreas[0].AxisY.Minimum = 0;
            chart2.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart2.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm:ss";
            //
            chart3.ChartAreas[0].AxisY.Minimum = 0;
            chart3.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart3.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm:ss";
            //
            chart4.ChartAreas[0].AxisY.Minimum = 0;
            chart4.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart4.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm:ss";
            //
            GL.ClearColor(Color.Black);

            //
            gMapControl1.MapProvider = GMapProviders.GoogleMap;
            gMapControl1.DragButton = MouseButtons.Left;

            //
            VideoCaptureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            captureDevice = new VideoCaptureDeviceForm();

            //
            Rectangle cozunurluk = new Rectangle();
            cozunurluk = Screen.GetBounds(cozunurluk);
            float YWidth = ((float)cozunurluk.Width / (float)1570);
            float YHeight = ((float)cozunurluk.Height / (float)920);
            SizeF scale = new SizeF(YWidth, YHeight);
            this.Scale(scale);


        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)  // Form kapanıyor.
        {
            this.Dispose();

          
        }

        private void button1_Click(object sender, EventArgs e)   // Bağlantı sağlanıyor.
        {
            try
            {
                string IPAddress = textBox19.Text;
                string UDPPortStr = textBox20.Text;
                int UDPPortInt;
                bool parsed = Int32.TryParse(UDPPortStr, out UDPPortInt);
                udpClient = new UdpClient(UDPPortInt);
                udpClient.Connect(IPAddress, UDPPortInt);
                ThreadReceive = new System.Threading.Thread(ReceiveMessages);
                ThreadReceive.Start();
                textBox18.Text = "BAĞLANTI SAĞLANDI.";
                textBox18.ForeColor = Color.Green;
            }
            catch
            {
            }

            if (captureDevice.ShowDialog(this) == DialogResult.OK)
            {

                VideoCaptureDevice videoSource = captureDevice.VideoDevice;
                FinalVideo = captureDevice.VideoDevice;
                FinalVideo.NewFrame += new NewFrameEventHandler(FinalVideo_NewFrame);
                FinalVideo.Start();
            }


            saveAvi = new SaveFileDialog();
            saveAvi.Filter = "Avi Files (*.avi)|*.avi";
            if (saveAvi.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int h = captureDevice.VideoDevice.VideoResolution.FrameSize.Height;
                int w = captureDevice.VideoDevice.VideoResolution.FrameSize.Width;
                FileWriter.Open(saveAvi.FileName, w, h, 25, VideoCodec.Default, 5000000);
                FileWriter.WriteVideoFrame(video);

                button3.Text = "Stop Record";
            }


        }

        void FinalVideo_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (button3.Text == "Stop Record")
            {
                video = (Bitmap)eventArgs.Frame.Clone();
                pictureBox3.Image = (Bitmap)eventArgs.Frame.Clone();
                FileWriter.WriteVideoFrame(video);
            }
            else
            {
                video = (Bitmap)eventArgs.Frame.Clone();
                pictureBox3.Image = (Bitmap)eventArgs.Frame.Clone();
            }
        }

        public void ReceiveMessages()
        {
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
            string returnData = Encoding.ASCII.GetString(receiveBytes);
            this.Invoke(new MethodInvoker(delegate ()
            {
                string[] datas = returnData.Split('*');
                textBox1.Text = datas[0];       // Takım No
                textBox2.Text = datas[1];       // Paket Numarası
                textBox3.Text = datas[2];       // Gönderme Saati
                textBox4.Text = datas[3];       // Basınç 
                textBox5.Text = datas[4];       // Yükseklik
                textBox6.Text = datas[5];       // İniş Hızı
                textBox7.Text = datas[6];       // Sıcaklık
                textBox8.Text = datas[7];       // Pil Gerilimi
                textBox9.Text = datas[8];       // Gps Latitude
                textBox10.Text = datas[9];      // Gps Longitude
                textBox11.Text = datas[10];     // Gps Altitude
                textBox12.Text = datas[11];     // Uydu Statüsü
                textBox13.Text = datas[12];     // Pitch
                textBox14.Text = datas[13];     // Roll
                textBox15.Text = datas[14];     // Yaw
                textBox16.Text = datas[15];     // Dönüş Sayısı
                textBox17.Text = datas[16];     // Video Aktarım Bilgisi

                //
                this.chart1.Series[0].Points.AddXY(datas[2], datas[7]);   // Voltaj grafiğine değer ataması yapıldı.
                this.chart3.Series[0].Points.AddXY(datas[2], datas[3]);   // Basınç grafiğine değer ataması yapıldı.
                this.chart2.Series[0].Points.AddXY(datas[2], datas[6]);   // Sıcaklık grafiğine değer ataması yapıldı.
                this.chart4.Series[0].Points.AddXY(datas[2], datas[4]);   // Yükseklik grafiğine değer ataması yapıldı.

                //
                line = dataGridView1.Rows.Add();                          // Verileri excele aktarmak için atama yapılıyor.
                dataGridView1.Rows[line].Cells[0].Value = lineNumber;
                dataGridView1.Rows[line].Cells[1].Value = datas[0];
                dataGridView1.Rows[line].Cells[2].Value = datas[1];
                dataGridView1.Rows[line].Cells[3].Value = datas[2];
                dataGridView1.Rows[line].Cells[4].Value = datas[3];
                dataGridView1.Rows[line].Cells[5].Value = datas[4];
                dataGridView1.Rows[line].Cells[6].Value = datas[5];
                dataGridView1.Rows[line].Cells[7].Value = datas[6];
                dataGridView1.Rows[line].Cells[8].Value = datas[7];
                dataGridView1.Rows[line].Cells[9].Value = datas[8];
                dataGridView1.Rows[line].Cells[10].Value = datas[9];
                dataGridView1.Rows[line].Cells[11].Value = datas[10];
                dataGridView1.Rows[line].Cells[12].Value = datas[11];
                dataGridView1.Rows[line].Cells[13].Value = datas[12];
                dataGridView1.Rows[line].Cells[14].Value = datas[13];
                dataGridView1.Rows[line].Cells[15].Value = datas[14];
                dataGridView1.Rows[line].Cells[16].Value = datas[15];
                dataGridView1.Rows[line].Cells[17].Value = datas[16];
                line++;
                lineNumber++;

                //
                x = Convert.ToDouble(datas[12]);   // Eksen duruş modellemesi için veriler atanıp dönüştürülüyor.
                y = Convert.ToDouble(datas[13]);   // Eksen duruş modellemesi için veriler atanıp dönüştürülüyor.
                z = Convert.ToDouble(datas[14]);   // Eksen duruş modellemesi için veriler atanıp dönüştürülüyor.
                glControl1.Invalidate();

                //
                double a = Convert.ToDouble(datas[8]);
                double b = Convert.ToDouble(datas[9]);
                gMapControl1.Position = new GMap.NET.PointLatLng(a, b);
                gMapControl1.MinZoom = 10;
                gMapControl1.MaxZoom = 1000;
                gMapControl1.Zoom = 15;

            }));
            NewInitialize();
        }

        public void NewInitialize()
        {
            ThreadReceive = new System.Threading.Thread(ReceiveMessages);
            ThreadReceive.Start();
        }

        private void button6_Click(object sender, EventArgs e)    // Bağlantı kesiliyor.
        {
            try
            {
                udpClient.Close();                        //APPLICATION EXIT' E DÖNÜŞTÜRÜLECEK.       
                textBox18.Text = "BAĞLANTI KESİLDİ.";
                textBox18.ForeColor = Color.DarkRed;
            }
            catch
            {

            }
        }

        private void button4_Click(object sender, EventArgs e)    // Manuel Ayrılma için komut gönderiliyor.
        {
            UdpClient udpClient = new UdpClient();
            udpClient.Connect(textBox19.Text, Convert.ToInt16(textBox20.Text));
            Byte[] senddata = Encoding.ASCII.GetBytes("02");
            udpClient.Send(senddata, senddata.Length);
        }

        private void button2_Click(object sender, EventArgs e)    // Manuel Tahrik için komut gönderiliyor.
        {
            UdpClient udpClient = new UdpClient();
            udpClient.Connect(textBox19.Text, Convert.ToInt16(textBox20.Text));
            Byte[] senddata = Encoding.ASCII.GetBytes("0");
            udpClient.Send(senddata, senddata.Length);
        }
        private void button3_Click(object sender, EventArgs e)
        {
            Microsoft.Office.Interop.Excel.Application uyg = new Microsoft.Office.Interop.Excel.Application();
            uyg.Visible = true;
            Microsoft.Office.Interop.Excel.Workbook kitap = uyg.Workbooks.Add(System.Reflection.Missing.Value);
            Microsoft.Office.Interop.Excel.Worksheet sheet1 = (Microsoft.Office.Interop.Excel.Worksheet)kitap.Sheets[1];
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                Microsoft.Office.Interop.Excel.Range myRange = (Microsoft.Office.Interop.Excel.Range)sheet1.Cells[1, i + 1];
                myRange.Value2 = dataGridView1.Columns[i].HeaderText;
            }

            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                for (int j = 0; j < dataGridView1.Rows.Count; j++)
                {
                    Microsoft.Office.Interop.Excel.Range myRange = (Microsoft.Office.Interop.Excel.Range)sheet1.Cells[j + 2, i + 1];
                    myRange.Value2 = dataGridView1[i, j].Value;
                }
            }
        }


        // Eksen duruş modellemesi kodlarının başlıyor.

        private void glControl1_Load(object sender, EventArgs e)         // GLControl yükleniyor.
        {
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.Enable(EnableCap.DepthTest);
        }



        private void glControl1_Paint(object sender, PaintEventArgs e)   // GLControl model uydu çizimi yapılıyor.
        {
            float step = 1.0f;//Adım genişliği çözünürlük
            float topla = step;//Tanpon 
            float radius = 4.0f;//Yarıçağ Modle Uydunun
            GL.Clear(ClearBufferMask.ColorBufferBit);//Buffer temizlenmez ise görüntüler üst üste bine o yüzden temizliyoruz.
            GL.Clear(ClearBufferMask.DepthBufferBit);

            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(1.04f, 4 / 3, 1, 10000);
            Matrix4 lookat = Matrix4.LookAt(25, 0, 0, 0, 0, 0, 0, 1, 0);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.LoadMatrix(ref perspective);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.LoadMatrix(ref lookat);
            GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            GL.Rotate(y+2, 1.0, 0.0, 0.0);
            GL.Rotate(x-28, 0.0, 1.0, 0.0);
            GL.Rotate(z+90, 0.0, 0.0, 1.0);

            silindir(step, topla, radius, 3, -5);
            koni(0.01f, 0.01f, radius, 3.0f, 3, 4);
            koni(0.01f, 0.01f, radius, 2.0f, -5.0f, -7.0f);
            silindir(0.01f, topla, 0.07f, 9, 3);      
            silindir(0.01f, topla, 0.2f, 7, 7.3f);

            silindir(0.01f, topla, 0.2f, 7.3f, 7f);
            Pervane(7.0f, 7.0f, 0.3f, 0.3f);
            GL.Begin(BeginMode.Lines);

            GL.Color3(Color.FromArgb(250, 0, 0));
            GL.Vertex3(-1000, 0, 0);
            GL.Vertex3(1000, 0, 0);

            GL.Color3(Color.FromArgb(25, 150, 100));
            GL.Vertex3(0, 0, -1000);
            GL.Vertex3(0, 0, 1000);

            GL.Color3(Color.FromArgb(0, 0, 0));
            GL.Vertex3(0, 1000, 0);
            GL.Vertex3(0, -1000, 0);

            GL.End();
            //GraphicsContext.CurrentContext.VSync = true;
            glControl1.SwapBuffers();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            UdpClient udpClient = new UdpClient();
            udpClient.Connect(textBox19.Text, Convert.ToInt16(textBox20.Text));
            Byte[] senddata = Encoding.ASCII.GetBytes("04");
            udpClient.Send(senddata, senddata.Length);
        }

      

        private void silindir(float step, float topla, float radius, float dikey1, float dikey2)
        {
            float eski_step = 0.1f;
            GL.Begin(BeginMode.Quads);//Y EKSEN CIZIM DAİRENİN
            while (step <= 360)
            {
                renk_ataması(step);
                float ciz1_x = (float)(radius * Math.Cos(step * Math.PI / 180F));
                float ciz1_y = (float)(radius * Math.Sin(step * Math.PI / 180F));
                GL.Vertex3(ciz1_x, dikey1, ciz1_y);

                float ciz2_x = (float)(radius * Math.Cos((step + 2) * Math.PI / 180F));
                float ciz2_y = (float)(radius * Math.Sin((step + 2) * Math.PI / 180F));
                GL.Vertex3(ciz2_x, dikey1, ciz2_y);

                GL.Vertex3(ciz1_x, dikey2, ciz1_y);
                GL.Vertex3(ciz2_x, dikey2, ciz2_y);
                step += topla;
            }
            GL.End();
            GL.Begin(BeginMode.Lines);
            step = eski_step;
            topla = step;
            while (step <= 180)// UST KAPAK
            {
                renk_ataması(step);
                float ciz1_x = (float)(radius * Math.Cos(step * Math.PI / 180F));
                float ciz1_y = (float)(radius * Math.Sin(step * Math.PI / 180F));
                GL.Vertex3(ciz1_x, dikey1, ciz1_y);

                float ciz2_x = (float)(radius * Math.Cos((step + 180) * Math.PI / 180F));
                float ciz2_y = (float)(radius * Math.Sin((step + 180) * Math.PI / 180F));
                GL.Vertex3(ciz2_x, dikey1, ciz2_y);

                GL.Vertex3(ciz1_x, dikey1, ciz1_y);
                GL.Vertex3(ciz2_x, dikey1, ciz2_y);
                step += topla;
            }
            step = eski_step;
            topla = step;
            while (step <= 180)//ALT KAPAK
            {
                renk_ataması(step);

                float ciz1_x = (float)(radius * Math.Cos(step * Math.PI / 180F));
                float ciz1_y = (float)(radius * Math.Sin(step * Math.PI / 180F));
                GL.Vertex3(ciz1_x, dikey2, ciz1_y);

                float ciz2_x = (float)(radius * Math.Cos((step + 180) * Math.PI / 180F));
                float ciz2_y = (float)(radius * Math.Sin((step + 180) * Math.PI / 180F));
                GL.Vertex3(ciz2_x, dikey2, ciz2_y);

                GL.Vertex3(ciz1_x, dikey2, ciz1_y);
                GL.Vertex3(ciz2_x, dikey2, ciz2_y);
                step += topla;
            }
            GL.End();
        }
        private void koni(float step, float topla, float radius1, float radius2, float dikey1, float dikey2)
        {
            float eski_step = 0.1f;
            GL.Begin(BeginMode.Lines);//Y EKSEN CIZIM DAİRENİN
            while (step <= 360)
            {
                renk_ataması(step);
                float ciz1_x = (float)(radius1 * Math.Cos(step * Math.PI / 180F));
                float ciz1_y = (float)(radius1 * Math.Sin(step * Math.PI / 180F));
                GL.Vertex3(ciz1_x, dikey1, ciz1_y);

                float ciz2_x = (float)(radius2 * Math.Cos(step * Math.PI / 180F));
                float ciz2_y = (float)(radius2 * Math.Sin(step * Math.PI / 180F));
                GL.Vertex3(ciz2_x, dikey2, ciz2_y);
                step += topla;
            }
            GL.End();

            GL.Begin(BeginMode.Lines);
            step = eski_step;
            topla = step;
            while (step <= 180)// UST KAPAK
            {
                renk_ataması(step);
                float ciz1_x = (float)(radius2 * Math.Cos(step * Math.PI / 180F));
                float ciz1_y = (float)(radius2 * Math.Sin(step * Math.PI / 180F));
                GL.Vertex3(ciz1_x, dikey2, ciz1_y);

                float ciz2_x = (float)(radius2 * Math.Cos((step + 180) * Math.PI / 180F));
                float ciz2_y = (float)(radius2 * Math.Sin((step + 180) * Math.PI / 180F));
                GL.Vertex3(ciz2_x, dikey2, ciz2_y);

                GL.Vertex3(ciz1_x, dikey2, ciz1_y);
                GL.Vertex3(ciz2_x, dikey2, ciz2_y);
                step += topla;
            }
            step = eski_step;
            topla = step;
            GL.End();
        }
        private void Pervane(float yukseklik, float uzunluk, float kalinlik, float egiklik)
        {
            float radius = 10, angle = 45.0f;
            GL.Begin(BeginMode.Quads);

            GL.Color3(renk2);
            GL.Vertex3(uzunluk, yukseklik, kalinlik);
            GL.Vertex3(uzunluk, yukseklik + egiklik, -kalinlik);
            GL.Vertex3(0, yukseklik + egiklik, -kalinlik);
            GL.Vertex3(0, yukseklik, kalinlik);

            GL.Color3(renk2);
            GL.Vertex3(-uzunluk, yukseklik + egiklik, kalinlik);
            GL.Vertex3(-uzunluk, yukseklik, -kalinlik);
            GL.Vertex3(0, yukseklik, -kalinlik);
            GL.Vertex3(0, yukseklik + egiklik, kalinlik);

            GL.Color3(renk1);
            GL.Vertex3(kalinlik, yukseklik, -uzunluk);
            GL.Vertex3(-kalinlik, yukseklik + egiklik, -uzunluk);
            GL.Vertex3(-kalinlik, yukseklik + egiklik, 0.0);//+
            GL.Vertex3(kalinlik, yukseklik, 0.0);//-

            GL.Color3(renk1);
            GL.Vertex3(kalinlik, yukseklik + egiklik, +uzunluk);
            GL.Vertex3(-kalinlik, yukseklik, +uzunluk);
            GL.Vertex3(-kalinlik, yukseklik, 0.0);
            GL.Vertex3(kalinlik, yukseklik + egiklik, 0.0);
            GL.End();

        }
        private void renk_ataması(float step)
        {
            if (step < 45)
                GL.Color3(renk2);
            else if (step < 90)
                GL.Color3(renk1);
            else if (step < 135)
                GL.Color3(renk2);
            else if (step < 180)
                GL.Color3(renk1);
            else if (step < 225)
                GL.Color3(renk2);
            else if (step < 270)
                GL.Color3(renk1);
            else if (step < 315)
                GL.Color3(renk2);
            else if (step < 360)
                GL.Color3(renk1);
        }
    }
}
