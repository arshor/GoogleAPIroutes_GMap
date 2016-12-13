using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Net;
using Word = Microsoft.Office.Interop.Word;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace GoogleAPIroutes_GMap
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Настройки для компонента GMap.
            gMapControl1.Bearing = 0;

            //CanDragMap - Если параметр установлен в True,
            //пользователь может перетаскивать карту 
            //с помощью правой кнопки мыши. 
            gMapControl1.CanDragMap = true;

            //Указываем, что перетаскивание карты осуществляется 
            //с использованием левой клавишей мыши.
            //По умолчанию - правая.
            gMapControl1.DragButton = MouseButtons.Left;

            gMapControl1.GrayScaleMode = true;

            //MarkersEnabled - Если параметр установлен в True,
            //любые маркеры, заданные вручную будет показаны.
            //Если нет, они не появятся.
            gMapControl1.MarkersEnabled = true;

            //Указываем значение максимального приближения.
            gMapControl1.MaxZoom = 18;

            //Указываем значение минимального приближения.
            gMapControl1.MinZoom = 2;

            //Устанавливаем центр приближения/удаления для
            //курсора мыши.
            gMapControl1.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;

            //Отказываемся от негативного режима.
            gMapControl1.NegativeMode = false;

            //Разрешаем полигоны.
            gMapControl1.PolygonsEnabled = true;

            //Разрешаем маршруты.
            gMapControl1.RoutesEnabled = true;

            //Скрываем внешнюю сетку карты
            //с заголовками.
            gMapControl1.ShowTileGridLines = false;

            //Указываем, что при загрузке карты будет использоваться 
            //2х кратное приближение.
            gMapControl1.Zoom = 2;

            //Указываем что будем использовать карты Google.
            gMapControl1.MapProvider = GMap.NET.MapProviders.GMapProviders.GoogleMap;
            GMaps.Instance.Mode = AccessMode.ServerOnly;

            //Если вы используете интернет через прокси сервер,
            //указываем свои учетные данные.
            GMap.NET.MapProviders.GMapProvider.WebProxy =
                WebRequest.GetSystemWebProxy();
            GMap.NET.MapProviders.GMapProvider.WebProxy.Credentials =
                CredentialCache.DefaultCredentials;

            //инициализируем новую таблицу,
            //для хранения данных о маршруте.
            dtRouter = new DataTable();

            //Добавляем в инициализированную таблицу,
            //новые колонки.
            dtRouter.Columns.Add("Шаг");
            dtRouter.Columns.Add("Нач. точка (latitude)");
            dtRouter.Columns.Add("Нач. точка (longitude)");
            dtRouter.Columns.Add("Кон. точка (latitude)");
            dtRouter.Columns.Add("Кон. точка (longitude)");
            dtRouter.Columns.Add("Время пути");
            dtRouter.Columns.Add("Расстояние");
            dtRouter.Columns.Add("Описание маршрута");           

            //Задаем источник данных, для объекта
            //System.Windows.Forms.DataGridView.            
            dataGridView1.DataSource = dtRouter;

            //Задаем ширину седьмого столбца.
            dataGridView1.Columns[7].Width = 250;

            //Задаем значение, указывающее, что необходимо скрыть 
            //для пользователя параметр добавления строк.
            dataGridView1.AllowUserToAddRows = false;

            //Задаем значение, указывающее, что пользователю
            //запрещено удалять строки.
            dataGridView1.AllowUserToDeleteRows = false;

            //Задаем значение, указывающее, что пользователь
            //не может изменять ячейки элемента управления.
            dataGridView1.ReadOnly = false;

            //Добавляем способы перемещения.
            comboBox1.Items.Add("Автомобильные маршруты");
            comboBox1.Items.Add("Пешеходные маршруты");
            comboBox1.Items.Add("Велосипедные маршруты");
            comboBox1.Items.Add("Маршруты общественного транспорта");

            //Выставляем по умолчанию способ перемещения:
            //Автомобильные маршруты по улично-дорожной сети.
            comboBox1.SelectedIndex = 0;
        }

        DataTable dtRouter;
        private void button1_Click(object sender, EventArgs e)
        {
            //Очищаем таблицу перед загрузкой данных.
            dtRouter.Rows.Clear();

            //Создаем список способов перемещения.
            List<string> mode = new List<string>();
            //Автомобильные маршруты по улично-дорожной сети.
            mode.Add("driving");
            //Пешеходные маршруты по прогулочным дорожкам и тротуарам.
            mode.Add("walking");
            //Велосипедные маршруты по велосипедным дорожкам и предпочитаемым улицам.
            mode.Add("bicycling");
            //Маршруты общественного транспорта.
            mode.Add("transit");

            //Фрмируем запрос к API маршрутов Google.
            string url = string.Format(
                "http://maps.googleapis.com/maps/api/directions/xml?origin={0},&destination={1}&sensor=false&language=ru&mode={2}",
                Uri.EscapeDataString(textBox1.Text), Uri.EscapeDataString(textBox2.Text), Uri.EscapeDataString(mode[comboBox1.SelectedIndex]));

            //Выполняем запрос к универсальному коду ресурса (URI).
            HttpWebRequest request =
                (HttpWebRequest)System.Net.WebRequest.Create(url);

            //Получаем ответ от интернет-ресурса.
            WebResponse response = request.GetResponse();

            //Экземпляр класса System.IO.Stream 
            //для чтения данных из интернет-ресурса.
            Stream dataStream = response.GetResponseStream();

            //Инициализируем новый экземпляр класса 
            //System.IO.StreamReader для указанного потока.
            StreamReader sreader = new System.IO.StreamReader(dataStream);

            //Считываем поток от текущего положения до конца.            
            string responsereader = sreader.ReadToEnd();

            //Закрываем поток ответа.
            response.Close();

            System.Xml.XmlDocument xmldoc = new System.Xml.XmlDocument();

            xmldoc.LoadXml(responsereader);

            if (xmldoc.GetElementsByTagName("status")[0].ChildNodes[0].InnerText == "OK")
            {   
                System.Xml.XmlNodeList nodes =
                    xmldoc.SelectNodes("//leg//step");
               
                //Формируем строку для добавления в таблицу.
                object[] dr;
                for (int i = 0; i < nodes.Count; i++)
                {
                    //Указываем что массив будет состоять из 
                    //восьми значений.
                    dr = new object[8];
                    //Номер шага.
                    dr[0] = i;
                    //Получение координат начала отрезка.
                    dr[1] = xmldoc.SelectNodes("//start_location").Item(i).SelectNodes("lat").Item(0).InnerText.ToString();
                    dr[2] = xmldoc.SelectNodes("//start_location").Item(i).SelectNodes("lng").Item(0).InnerText.ToString();
                    //Получение координат конца отрезка.
                    dr[3] = xmldoc.SelectNodes("//end_location").Item(i).SelectNodes("lat").Item(0).InnerText.ToString();
                    dr[4] = xmldoc.SelectNodes("//end_location").Item(i).SelectNodes("lng").Item(0).InnerText.ToString();
                    //Получение времени необходимого для прохождения этого отрезка.
                    dr[5] = xmldoc.SelectNodes("//duration").Item(i).SelectNodes("text").Item(0).InnerText.ToString();
                    //Получение расстояния, охватываемое этим отрезком.
                    dr[6] = xmldoc.SelectNodes("//distance").Item(i).SelectNodes("text").Item(0).InnerText.ToString();
                    //Получение инструкций для этого шага, представленные в виде текстовой строки HTML.
                    dr[7] = HtmlToPlainText(xmldoc.SelectNodes("//html_instructions").Item(i).InnerText.ToString());
                    //Добавление шага в таблицу.
                    dtRouter.Rows.Add(dr);
                }

                //Выводим в текстовое поле адрес начала пути.
                textBox1.Text = xmldoc.SelectNodes("//leg//start_address").Item(0).InnerText.ToString();
                //Выводим в текстовое поле адрес конца пути.
                textBox2.Text = xmldoc.SelectNodes("//leg//end_address").Item(0).InnerText.ToString();

                //Переменные для хранения координат начала и конца пути.
                double latStart = 0.0;
                double lngStart = 0.0;
                double latEnd = 0.0;
                double lngEnd = 0.0;

                //Получение координат начала пути.
                latStart = System.Xml.XmlConvert.ToDouble(xmldoc.GetElementsByTagName("start_location")[nodes.Count].ChildNodes[0].InnerText);
                lngStart = System.Xml.XmlConvert.ToDouble(xmldoc.GetElementsByTagName("start_location")[nodes.Count].ChildNodes[1].InnerText);
                //Получение координат конечной точки.
                latEnd = System.Xml.XmlConvert.ToDouble(xmldoc.GetElementsByTagName("end_location")[nodes.Count].ChildNodes[0].InnerText);
                lngEnd = System.Xml.XmlConvert.ToDouble(xmldoc.GetElementsByTagName("end_location")[nodes.Count].ChildNodes[1].InnerText);

                //Устанавливаем заполненную таблицу в качестве источника.
                dataGridView1.DataSource = dtRouter;

                //Устанавливаем позицию карты на начало пути.
                gMapControl1.Position = new GMap.NET.PointLatLng(latStart, lngStart);                               
                
                //Создаем новый список маркеров, с указанием компонента 
                //в котором они будут использоваться и названием списка.
                GMapOverlay markersOverlay = new GMapOverlay("marker");

                //Инициализация нового ЗЕЛЕНОГО маркера, с указанием координат начала пути.
                GMarkerGoogle markerG =
                    new GMarkerGoogle(new PointLatLng(latStart, lngStart), GMarkerGoogleType.green_pushpin);
                markerG.ToolTip =
                    new GMap.NET.WindowsForms.ToolTips.GMapRoundedToolTip(markerG);

                //Указываем, что подсказку маркера, необходимо отображать всегда.
                markerG.ToolTipMode = MarkerTooltipMode.Always;

                //Формируем подсказку для маркера.
                string[] wordsG = textBox1.Text.Split(',');
                string dataMarkerG = string.Empty;
                foreach (string word in wordsG)
                {
                    dataMarkerG += word + ";\n";
                }

                //Устанавливаем текст подсказки маркера.               
                markerG.ToolTipText = dataMarkerG;

                //Инициализация нового Красного маркера, с указанием координат конца пути.
                GMarkerGoogle markerR =
                    new GMarkerGoogle(
                    new PointLatLng(latEnd, lngEnd), GMarkerGoogleType.red_pushpin);
                markerG.ToolTip =
                    new GMap.NET.WindowsForms.ToolTips.GMapRoundedToolTip(markerG);

                //Указываем, что подсказку маркера, необходимо отображать всегда.
                markerR.ToolTipMode = MarkerTooltipMode.Always;

                //Формируем подсказку для маркера.
                string[] wordsR = textBox2.Text.Split(',');
                string dataMarkerR = string.Empty;
                foreach (string word in wordsR)
                {
                    dataMarkerR += word + ";\n";
                }

                //Текст подсказки маркера.               
                markerR.ToolTipText = dataMarkerR;

                //Добавляем маркеры в список маркеров.
                markersOverlay.Markers.Add(markerG);
                markersOverlay.Markers.Add(markerR);

                //Очищаем список маркеров компонента.
                gMapControl1.Overlays.Clear();               

                //Создаем список контрольных точек для прокладки маршрута.
                List<PointLatLng> list = new List<PointLatLng>();
             
                //Проходимся по определенным столбцам для получения
                //координат контрольных точек маршрута и занесением их
                //в список координат.
                for (int i = 0; i < dtRouter.Rows.Count; i++)
                {                    
                    double dbStartLat = double.Parse(dtRouter.Rows[i].ItemArray[1].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                    double dbStartLng = double.Parse(dtRouter.Rows[i].ItemArray[2].ToString(), System.Globalization.CultureInfo.InvariantCulture);

                    list.Add(new PointLatLng(dbStartLat, dbStartLng));

                    double dbEndLat = double.Parse(dtRouter.Rows[i].ItemArray[3].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                    double dbEndLng = double.Parse(dtRouter.Rows[i].ItemArray[4].ToString(), System.Globalization.CultureInfo.InvariantCulture);

                    list.Add(new PointLatLng(dbEndLat, dbEndLng));
                }

                //Очищаем все маршруты.
                markersOverlay.Routes.Clear();

                //Создаем маршрут на основе списка контрольных точек.
                GMapRoute r = new GMapRoute(list, "Route");

                //Указываем, что данный маршрут должен отображаться.
                r.IsVisible = true;

                //Устанавливаем цвет маршрута.
                r.Stroke.Color = Color.DarkGreen;

                //Добавляем маршрут.
                markersOverlay.Routes.Add(r);

                //Добавляем в компонент, список маркеров и маршрутов.
                gMapControl1.Overlays.Add(markersOverlay);

                //Указываем, что при загрузке карты будет использоваться 
                //9ти кратное приближение.
                gMapControl1.Zoom = 15;

                //Обновляем карту.
                gMapControl1.Refresh();

                


            }
        }

        //Удаляем HTML теги.
        public string HtmlToPlainText(string html)
       {        
            html = html.Replace("</b>","");
            return html.Replace("<b>", "");
        }

        private void button2_Click(object sender, EventArgs e)
        {
                //Делаем скриншот и отправляем его в Word
               Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(this.Location.X+20, this.Location.Y+150, 0, 0, new Size(pictureBox1.Width, pictureBox1.Height));
                }
                pictureBox1.Image = bmp;

                Word.Application application = new Word.Application();
                Object missing = Type.Missing;
                application.Documents.Add(ref missing, ref missing, ref missing, ref missing);
                Clipboard.SetImage(bmp);
                application.ActiveDocument.Paragraphs[1].Range.Paste();
                application.Visible = true;
           
        }
    }
}
