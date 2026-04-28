namespace Mesty
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            webView21.Source = new Uri("https://www.google.com");

            webView21.SourceChanged += UpdateAddressBar;


            // Теперь главный браузер будет сам писать свой адрес в твою "штуку"
            webView21.SourceChanged += (s, ev) => { textBox1.Text = webView21.Source.ToString(); };

            // 1. Сначала ОБЯЗАТЕЛЬНО инициализируем движок
            await webView21.EnsureCoreWebView2Async();

            // 2. Подписываемся на обновление адреса
            webView21.SourceChanged += UpdateAddressBar;

            // 3. Проверяем файл с сохраненными вкладками
            if (System.IO.File.Exists("tabs.txt"))
            {
                string[] savedTabs = System.IO.File.ReadAllLines("tabs.txt");

                if (savedTabs.Length > 0)
                {
                    // ИСПРАВЛЕНО: берем только ПЕРВУЮ строку из файла [0]
                    webView21.Source = new Uri(savedTabs[0]);

                    // Для всех остальных строк создаем новые вкладки
                    for (int i = 1; i < savedTabs.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(savedTabs[i]))
                        {
                            await CreateTabWithUrl(savedTabs[i]);
                        }
                    }
                }
            }
            else
            {
                // Если файла нет — просто открываем гугл по дефолту
                webView21.Source = new Uri("https://www.google.com");
            }

        }



        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void webView21_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Ищем, какой браузер сейчас виден
            var activeWeb = this.Controls.OfType<Microsoft.Web.WebView2.WinForms.WebView2>()
                                 .FirstOrDefault(w => w.Visible);

            if (activeWeb != null && activeWeb.CanGoBack)
            {
                activeWeb.GoBack();
            }
        }



        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            var activeWeb = this.Controls.OfType<Microsoft.Web.WebView2.WinForms.WebView2>()
                         .FirstOrDefault(w => w.Visible);

            if (activeWeb != null && activeWeb.CanGoForward)
            {
                activeWeb.GoForward();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private async void button2_Click_1(object sender, EventArgs e)
        {
            await CreateTabWithUrl("https://google.com");
        }

        // А это универсальный метод создания вкладки
        private async Task CreateTabWithUrl(string url)
        {
            var extraWeb = new Microsoft.Web.WebView2.WinForms.WebView2();
            extraWeb.Dock = DockStyle.Fill;
            extraWeb.Visible = false;
            this.Controls.Add(extraWeb);
            extraWeb.BringToFront();

            await extraWeb.EnsureCoreWebView2Async();
            extraWeb.Source = new Uri(url);
            extraWeb.SourceChanged += UpdateAddressBar;

            Button newBtn = new Button { Width = 120, Dock = DockStyle.Left, FlatStyle = FlatStyle.Flat, Tag = extraWeb };

            extraWeb.NavigationCompleted += (s, ev) => {
                newBtn.Text = extraWeb.CoreWebView2.DocumentTitle;
            };

            newBtn.Click += (s, ev) => {
                webView21.Visible = false;
                foreach (Control c in this.Controls) if (c is Microsoft.Web.WebView2.WinForms.WebView2) c.Visible = false;
                var current = (Microsoft.Web.WebView2.WinForms.WebView2)((Button)s).Tag;
                current.Visible = true;
                current.BringToFront();
                textBox1.Text = current.Source.ToString();

                // Подсветка активной вкладки
                foreach (Control c in pnlTabs.Controls) c.BackColor = Color.LightGray;
                newBtn.BackColor = Color.White;
            };

            // --- ДОБАВЛЕНО: ЗАКРЫТИЕ ВКЛАДКИ ПРАВОЙ КНОПКОЙ ---
            newBtn.MouseDown += (s, ev) => {
                if (ev.Button == MouseButtons.Right)
                {
                    this.Controls.Remove(extraWeb);
                    extraWeb.Dispose();
                    pnlTabs.Controls.Remove(newBtn);
                    newBtn.Dispose();

                    // Возвращаемся на главную, если закрыли текущую
                    webView21.Visible = true;
                    webView21.BringToFront();
                    textBox1.Text = webView21.Source.ToString();
                }
            };
            // ------------------------------------------------

            pnlTabs.Controls.Add(newBtn);
            newBtn.PerformClick(); // Сразу переключаемся на новую вкладку
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Прячем все созданные вкладки
            foreach (Control c in this.Controls)
            {
                if (c is Microsoft.Web.WebView2.WinForms.WebView2) c.Visible = false;
            }
            // Показываем твой самый первый браузер
            webView21.Visible = true;
            webView21.BringToFront();
            textBox1.Text = webView21.Source.ToString();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            // Проверяем, что нажат именно Enter
            if (e.KeyCode == Keys.Enter)
            {
                string input = textBox1.Text.Trim(); // Берем текст и убираем лишние пробелы

                if (!string.IsNullOrWhiteSpace(input))
                {
                    // 1. Формируем правильную ссылку
                    string url = input;

                    // Если ввели просто "google.com", добавим https автоматически
                    if (!url.StartsWith("http") && url.Contains("."))
                    {
                        url = "https://" + url;
                    }
                    // Если ввели фигню без точек (просто слово), отправим в поиск Google
                    else if (!url.Contains("."))
                    {
                        url = "" + Uri.EscapeDataString(input);
                    }

                    // 2. Ищем активный браузер (тот, что сейчас на экране)
                    Microsoft.Web.WebView2.WinForms.WebView2 activeWeb = null;

                    if (webView21.Visible)
                    {
                        activeWeb = webView21;
                    }
                    else
                    {
                        // Ищем среди созданных вкладок ту, которая видна
                        activeWeb = this.Controls.OfType<Microsoft.Web.WebView2.WinForms.WebView2>()
                                                 .FirstOrDefault(w => w.Visible);
                    }

                    // 3. Переходим!
                    if (activeWeb != null)
                    {
                        activeWeb.Source = new Uri(url);
                    }
                }

                // Это чтобы винда не издавала противный звук "пик" при нажатии Enter
                e.SuppressKeyPress = true;
            }
        }

        // Создаем один метод для всех браузеров сразу
        private void UpdateAddressBar(object sender, Microsoft.Web.WebView2.Core.CoreWebView2SourceChangedEventArgs e)
        {
            var web = (Microsoft.Web.WebView2.WinForms.WebView2)sender;

            // Обновляем текст, только если этот браузер сейчас виден юзеру
            if (web.Visible)
            {
                textBox1.Text = web.Source.ToString();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            List<string> sites = new List<string>();

            // Сохраняем адрес главного браузера
            sites.Add(webView21.Source.ToString());

            // Сохраняем адреса всех созданных вкладок из кнопок
            foreach (Control c in pnlTabs.Controls)
            {
                if (c is Button && c.Tag is Microsoft.Web.WebView2.WinForms.WebView2 web)
                {
                    sites.Add(web.Source.ToString());
                }
            }

            // Записываем всё в файл "tabs.txt" рядом с программой
            System.IO.File.WriteAllLines("tabs.txt", sites);
        }
    }
}
