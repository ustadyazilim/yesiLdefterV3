using Aspose.Words.Drawing.Ole;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YesiLdefter
{
    public partial class ms_TileControl : Form
    {
        private TileControl tileControl;
        private TileGroup defaultGroup;
        private TileGroup customGroup;

        private TileGroup dataGroup;
        private TileGroup reportGroup;
        private TileGroup settingsGroup;

        public ms_TileControl()
        {
            InitializeComponent();
            InitializeTileControl();
            SetupTileControl();
            AddTileGroups();
            AddTileItems();
            ConfigureLayout();
        }


        private void InitializeTileControl()
        {
            // TileControl oluşturma
            tileControl = new TileControl
            {
                Name = "runtimeTileControl",
                Dock = DockStyle.Fill,
                Location = new Point(10, 10),
                Size = new Size(1180, 780),
                BorderStyle = BorderStyles.Default,// NoBorder,
                AllowDrag = true,
                AllowItemHover = true,
                IndentBetweenGroups = 15,
                IndentBetweenItems = 8,
                //ItemSize = 100,//170,
                Margin = new Padding(10),
                Padding = new Padding(5)
            };

            // TileControl stil ayarları
            tileControl.AppearanceGroupText.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            tileControl.AppearanceGroupText.ForeColor = Color.DarkSlateBlue;

            tileControl.ShowGroupText = true;
            tileControl.ShowText = true;

            this.Controls.Add(tileControl);
        }


        private void SetupTileControl()
        {
            /*
            // Item görünüm ayarları
            tileControl.AppearanceItem.Normal.Font = new Font("Segoe UI", 9);
            tileControl.AppearanceItem.Normal.BackColor = Color.White;
            tileControl.AppearanceItem.Normal.BorderColor = Color.Silver;
            //tileControl.AppearanceItem.Normal.BorderWidth = 1;

            // Hover görünümü
            tileControl.AppearanceItem.Hovered.BackColor = Color.FromArgb(240, 240, 240);
            tileControl.AppearanceItem.Hovered.BorderColor = Color.DodgerBlue;
            //tileControl.AppearanceItem.Hovered.BorderWidth = 2;

            // Seçili görünüm
            tileControl.AppearanceItem.Selected.BackColor = Color.FromArgb(220, 230, 255);
            tileControl.AppearanceItem.Selected.BorderColor = Color.RoyalBlue;
            //tileControl.AppearanceItem.Selected.BorderWidth = 2;
            */
            // Event handlers
            tileControl.ItemClick += TileControl_ItemClick;
            tileControl.ItemDoubleClick += TileControl_ItemDoubleClick;
        }


        private void AddTileGroups()
        {
            // Gruplar oluştur
            defaultGroup = new TileGroup
            {
                Name = "grpDefault",
                Text = "Temel İşlemler",
                Tag = "Grup1",
                Visible = true
            };

            customGroup = new TileGroup
            {
                Name = "grpCustom",
                Text = "Özel İşlemler",
                Tag = "Grup2",
                Visible = true
            };

            dataGroup = new TileGroup
            {
                Name = "grpData",
                Text = "Veri Yönetimi",
                Tag = "Grup3",
                Visible = true
            };

            reportGroup = new TileGroup
            {
                Name = "grpReport",
                Text = "Raporlar",
                Tag = "Grup4",
                Visible = true
            };

            settingsGroup = new TileGroup
            {
                Name = "grpSettings",
                Text = "Sistem Ayarları",
                Tag = "Grup5",
                Visible = true
            };

            tileControl.Groups.Add(defaultGroup);
            tileControl.Groups.Add(customGroup);
            tileControl.Groups.Add(dataGroup);
            tileControl.Groups.Add(reportGroup);
            tileControl.Groups.Add(settingsGroup);
                        
        }


        private void AddTileItems()
        {
            // 1. Temel İşlemler Grubu
            CreateAndAddTileItem(defaultGroup,
                "itemNew",
                "Yeni Kayıt",
                "Yeni müşteri kaydı oluştur",
                TileItemSize.Wide,
                Color.FromArgb(46, 204, 113), // Açık yeşil
                GetIcon("New"));

            CreateAndAddTileItem(defaultGroup,
                "itemEdit",
                "Düzenle",
                "Mevcut kaydı düzenle",
                TileItemSize.Medium,
                Color.FromArgb(52, 152, 219), // Açık mavi
                GetIcon("Edit"));

            CreateAndAddTileItem(defaultGroup,
                "itemDelete",
                "Sil",
                "Kaydı sil",
                TileItemSize.Medium,
                Color.FromArgb(231, 76, 60), // Açık kırmızı
                GetIcon("Delete"));

            CreateAndAddTileItem(defaultGroup,
                "itemSearch",
                "Ara",
                "Kayıtlarda arama yap",
                TileItemSize.Wide,
                Color.FromArgb(155, 89, 182), // Mor
                GetIcon("Search"));

            // 2. Özel İşlemler Grubu
            CreateAndAddCheckableTileItem(customGroup,
                "itemCheckData",
                "Veri Doğrulama",
                "Verileri kontrol et ve doğrula",
                TileItemSize.Medium,
                Color.FromArgb(241, 196, 15), // Sarı
                GetIcon("Check"));

            CreateAndAddTileItem(customGroup,
                "itemImport",
                "Veri Al",
                "Dış kaynaktan veri al",
                TileItemSize.Wide,
                Color.FromArgb(230, 126, 34), // Turuncu
                GetIcon("Import"));

            CreateAndAddTileItem(customGroup,
                "itemExport",
                "Dışa Aktar",
                "Verileri dışa aktar",
                TileItemSize.Medium,
                Color.FromArgb(22, 160, 133), // Yeşil
                GetIcon("Export"));

            CreateAndAddTileItemWithBadge(customGroup,
                "itemNotification",
                "Bildirimler",
                "Sistem bildirimleri",
                TileItemSize.Small,
                Color.FromArgb(142, 68, 173), // Koyu mor
                GetIcon("Notification"),
                5); // Badge sayısı

            // 3. Veri Yönetimi Grubu
            CreateAndAddTileItem(dataGroup,
                "itemCustomers",
                "Müşteriler",
                "Müşteri yönetimi",
                TileItemSize.Wide,
                Color.FromArgb(41, 128, 185),
                GetIcon("Customer"));

            CreateAndAddTileItem(dataGroup,
                "itemProducts",
                "Ürünler",
                "Ürün yönetimi",
                TileItemSize.Medium,
                Color.FromArgb(39, 174, 96),
                GetIcon("Product"));

            CreateAndAddTileItem(dataGroup,
                "itemOrders",
                "Siparişler",
                "Sipariş yönetimi",
                TileItemSize.Medium,
                Color.FromArgb(211, 84, 0),
                GetIcon("Order"));

            CreateAndAddTileItem(dataGroup,
                "itemInventory",
                "Stok",
                "Stok yönetimi",
                TileItemSize.Wide,
                Color.FromArgb(192, 57, 43),
                GetIcon("Inventory"));

            // 4. Raporlar Grubu
            CreateAndAddTileItem(reportGroup,
                "itemSalesReport",
                "Satış Raporu",
                "Günlük satış raporları",
                TileItemSize.Wide,
                Color.FromArgb(44, 62, 80),
                GetIcon("Report"));

            CreateAndAddTileItem(reportGroup,
                "itemFinancial",
                "Finansal Rapor",
                "Mali durum raporları",
                TileItemSize.Medium,
                Color.FromArgb(127, 140, 141),
                GetIcon("Finance"));

            CreateAndAddTileItem(reportGroup,
                "itemPerformance",
                "Performans",
                "Sistem performans raporları",
                TileItemSize.Medium,
                Color.FromArgb(52, 73, 94),
                GetIcon("Performance"));

            // 5. Sistem Ayarları Grubu
            CreateAndAddTileItem(settingsGroup,
                "itemSettings",
                "Ayarlar",
                "Sistem ayarlarını yönet",
                TileItemSize.Wide,
                Color.FromArgb(149, 165, 166),
                GetIcon("Settings"));

            CreateAndAddTileItem(settingsGroup,
                "itemUsers",
                "Kullanıcılar",
                "Kullanıcı yönetimi",
                TileItemSize.Medium,
                Color.FromArgb(189, 195, 199),
                GetIcon("User"));

            CreateAndAddTileItem(settingsGroup,
                "itemBackup",
                "Yedekleme",
                "Sistem yedekleme",
                TileItemSize.Medium,
                Color.FromArgb(127, 140, 141),
                GetIcon("Backup"));

            CreateAndAddTileItem(settingsGroup,
                "itemHelp",
                "Yardım",
                "Yardım dokümanları",
                TileItemSize.Small,
                Color.FromArgb(52, 152, 219),
                GetIcon("Help"));
        }

        private void CreateAndAddTileItem(TileGroup group, string name, string text,
           string description, TileItemSize size, Color backColor, Image image = null)
        {
            TileItem item = new TileItem
            {
                Name = name,
                ItemSize = size,
                Tag = name // Ek bilgi için tag kullanımı
            };

            // TileItemElement oluştur
            TileItemElement element = new TileItemElement();
            element.Text = text;

            if (!string.IsNullOrEmpty(description))
            {
                element.Text += Environment.NewLine + description;
            }

            if (image != null)
            {
                element.Image = image;
                element.ImageAlignment = TileItemContentAlignment.TopCenter;
                element.ImageScaleMode = TileItemImageScaleMode.Squeeze;
            }

            element.TextAlignment = TileItemContentAlignment.BottomCenter;
            element.Appearance.Normal.Font = new Font("Segoe UI", size == TileItemSize.Wide ? 10 : 9);

            item.Elements.Add(element);

            // Görünüm ayarları
            item.AppearanceItem.Normal.BackColor = backColor;
            item.AppearanceItem.Normal.BackColor2 = Lighten(backColor, 0.3f);
            item.AppearanceItem.Normal.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            item.AppearanceItem.Normal.BorderColor = Darken(backColor, 0.3f);
            //item.AppearanceItem.Normal.BorderWidth = 1;

            item.AppearanceItem.Hovered.BackColor = Lighten(backColor, 0.1f);
            item.AppearanceItem.Hovered.BackColor2 = Lighten(backColor, 0.4f);
            item.AppearanceItem.Hovered.BorderColor = Color.DodgerBlue;
            //item.AppearanceItem.Hovered.BorderWidth = 2;

            item.AppearanceItem.Pressed.BackColor = Darken(backColor, 0.1f);
            item.AppearanceItem.Pressed.BackColor2 = Darken(backColor, 0.2f);

            // Event handler ekle
            item.ItemClick += TileItem_ItemClick;

            // Gruba ekle
            group.Items.Add(item);
        }


        private void CreateAndAddCheckableTileItem(TileGroup group, string name, string text,
            string description, TileItemSize size, Color backColor, Image image = null)
        {
            TileItem item = new TileItem
            {
                Name = name,
                ItemSize = size,
                Checked = false,
                
                //CheckMarkVisible = true,
                //CheckMarkColor = Color.White
            };

            TileItemElement element = new TileItemElement();
            element.Text = text;

            if (!string.IsNullOrEmpty(description))
            {
                element.Text += Environment.NewLine + description;
            }

            if (image != null)
            {
                element.Image = image;
                element.ImageAlignment = TileItemContentAlignment.TopCenter;
            }

            element.TextAlignment = TileItemContentAlignment.BottomCenter;
            item.Elements.Add(element);

            // Checkable item görünümü
            item.AppearanceItem.Normal.BackColor = backColor;
            item.AppearanceItem.Normal.BackColor2 = Lighten(backColor, 0.3f);
            item.AppearanceItem.Normal.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;

            item.AppearanceItem.Selected.BackColor = Darken(backColor, 0.2f);
            item.AppearanceItem.Selected.BackColor2 = Darken(backColor, 0.4f);
            item.AppearanceItem.Selected.FontStyleDelta = FontStyle.Bold;

            // Check değişikliği event'i
            item.ItemClick += (sender, e) =>
            {
                TileItem tile = sender as TileItem;
                if (tile != null)
                {
                    tile.Checked = !tile.Checked;
                    ShowNotification($"{tile.Name} durumu: {(tile.Checked ? "Aktif" : "Pasif")}");
                }
            };

            group.Items.Add(item);
        }

        private void CreateAndAddTileItemWithBadge(TileGroup group, string name, string text,
            string description, TileItemSize size, Color backColor, Image image = null, int badgeCount = 0)
        {
            TileItem item = new TileItem
            {
                Name = name,
                ItemSize = size
            };

            TileItemElement element = new TileItemElement();
            element.Text = text;

            if (!string.IsNullOrEmpty(description))
            {
                element.Text += Environment.NewLine + description;
            }

            if (image != null)
            {
                element.Image = image;
                element.ImageAlignment = TileItemContentAlignment.TopCenter;
            }

            element.TextAlignment = TileItemContentAlignment.BottomCenter;
            item.Elements.Add(element);

            // Badge ekleme
            if (badgeCount > 0)
            {
                TileItemElement badgeElement = new TileItemElement();
                badgeElement.Text = badgeCount.ToString();
                badgeElement.Appearance.Normal.BackColor = Color.Red;
                badgeElement.Appearance.Normal.ForeColor = Color.White;
                badgeElement.Appearance.Normal.Font = new Font("Segoe UI", 8, FontStyle.Bold);
                badgeElement.TextAlignment = TileItemContentAlignment.TopRight;
                badgeElement.ColumnIndex = 1;
                badgeElement.RowIndex = 0;
                item.Elements.Add(badgeElement);
            }

            // Görünüm ayarları
            item.AppearanceItem.Normal.BackColor = backColor;
            item.AppearanceItem.Normal.BackColor2 = Lighten(backColor, 0.3f);

            group.Items.Add(item);
        }

        private void ConfigureLayout()
        {
            // TileControl düzeni
            tileControl.LayoutMode = TileControlLayoutMode.Standard;// List;
            tileControl.Orientation = System.Windows.Forms.Orientation.Vertical;
            tileControl.ColumnCount = 4; // Maksimum kolon sayısı

            // Grup ayarları
            foreach (TileGroup group in tileControl.Groups)
            {
                //group.ColumnCount = 2; // Her grup için kolon sayısı
                //group.ItemSize = TileItemSize.Wide;
            }
        }

        private Image GetIcon(string iconName)
        {
            // Basit ikon oluşturma (gerçek projede resim dosyaları kullanın)
            Bitmap bmp = new Bitmap(32, 32);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);

                switch (iconName)
                {
                    case "New":
                        g.FillRectangle(Brushes.Green, 8, 8, 16, 16);
                        g.DrawString("N", new Font("Arial", 10), Brushes.White, 11, 10);
                        break;
                    case "Edit":
                        g.FillRectangle(Brushes.Blue, 8, 8, 16, 16);
                        g.DrawString("E", new Font("Arial", 10), Brushes.White, 11, 10);
                        break;
                    case "Delete":
                        g.FillRectangle(Brushes.Red, 8, 8, 16, 16);
                        g.DrawString("D", new Font("Arial", 10), Brushes.White, 11, 10);
                        break;
                    case "Search":
                        g.FillEllipse(Brushes.Purple, 8, 8, 16, 16);
                        g.DrawString("S", new Font("Arial", 10), Brushes.White, 11, 10);
                        break;
                    default:
                        g.FillRectangle(Brushes.Gray, 8, 8, 16, 16);
                        g.DrawString("I", new Font("Arial", 10), Brushes.White, 11, 10);
                        break;
                }
            }
            return bmp;
        }


        // Yardımcı renk fonksiyonları
        private Color Lighten(Color color, float factor)
        {
            return Color.FromArgb(
                color.A,
                Math.Min(255, (int)(color.R + (255 - color.R) * factor)),
                Math.Min(255, (int)(color.G + (255 - color.G) * factor)),
                Math.Min(255, (int)(color.B + (255 - color.B) * factor))
            );
        }

        private Color Darken(Color color, float factor)
        {
            return Color.FromArgb(
                color.A,
                Math.Max(0, (int)(color.R * (1 - factor))),
                Math.Max(0, (int)(color.G * (1 - factor))),
                Math.Max(0, (int)(color.B * (1 - factor)))
            );
        }

        // Event Handlers
        private void TileControl_ItemClick(object sender, TileItemEventArgs e)
        {
            // Tile tıklama işlemi
            ProcessTileClick(e.Item);
        }

        private void TileControl_ItemDoubleClick(object sender, TileItemEventArgs e)
        {
            // Tile çift tıklama işlemi
            ProcessTileDoubleClick(e.Item);
        }

        private void TileItem_ItemClick(object sender, TileItemEventArgs e)
        {
            // TileItem tıklama işlemi
            TileItem item = e.Item;

            string message = $"{item.Name} tıklandı!";
            ShowNotification(message);

            // Özel işlemler
            switch (item.Name)
            {
                case "itemNew":
                    ShowNewRecordForm();
                    break;
                case "itemEdit":
                    ShowEditForm();
                    break;
                case "itemDelete":
                    ShowDeleteConfirmation();
                    break;
                case "itemSearch":
                    ShowSearchDialog();
                    break;
                case "itemSettings":
                    ShowSettingsForm();
                    break;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UpdateStatus($"TileControl hazır. Toplam {GetTotalTileCount()} tile oluşturuldu.");

            // Dinamik olarak yeni grup ekleme butonu
            SimpleButton btnAddGroup = new SimpleButton
            {
                Text = "Yeni Grup Ekle",
                Location = new Point(10, 10),
                Size = new Size(120, 30)
            };
            btnAddGroup.Click += BtnAddGroup_Click;

            SimpleButton btnRefresh = new SimpleButton
            {
                Text = "Yenile",
                Location = new Point(140, 10),
                Size = new Size(80, 30)
            };
            btnRefresh.Click += BtnRefresh_Click;

            this.Controls.Add(btnAddGroup);
            this.Controls.Add(btnRefresh);
            btnAddGroup.BringToFront();
            btnRefresh.BringToFront();
        }

        private void BtnAddGroup_Click(object sender, EventArgs e)
        {
            AddDynamicGroup();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            tileControl.Refresh();
            ShowNotification("TileControl yenilendi");
        }

        // Yardımcı metodlar
        private int GetTotalTileCount()
        {
            int count = 0;
            foreach (TileGroup group in tileControl.Groups)
            {
                count += group.Items.Count;
            }
            return count;
        }

        private void UpdateStatus(string message)
        {
            this.Text = $"DevExpress v19.2 TileControl - {message}";
        }

        private void ShowNotification(string message)
        {
            // Basit bildirim (gerçek projede XtraMessageBox kullanın)
            Console.WriteLine($"Bildirim: {message}");

            // Status bar veya label güncellemesi yapılabilir
        }

        // Tile işlem metodları
        private void ProcessTileClick(TileItem item)
        {
            // Tile tıklama işlemi
            Console.WriteLine($"Tile tıklandı: {item.Name}");
        }

        private void ProcessTileDoubleClick(TileItem item)
        {
            // Tile çift tıklama işlemi
            Console.WriteLine($"Tile çift tıklandı: {item.Name}");
            ShowNotification($"{item.Name} çift tıklandı!");
        }

        private void AddDynamicGroup()
        {
            TileGroup dynamicGroup = new TileGroup
            {
                Name = $"grpDynamic_{tileControl.Groups.Count + 1}",
                Text = $"Dinamik Grup {tileControl.Groups.Count + 1}",
                //HeaderVisible = true
            };

            tileControl.Groups.Add(dynamicGroup);

            // Dinamik tile'lar ekle
            for (int i = 1; i <= 4; i++)
            {
                TileItem item = new TileItem
                {
                    Name = $"dynamicItem_{i}",
                    ItemSize = i % 2 == 0 ? TileItemSize.Wide : TileItemSize.Medium
                };

                TileItemElement element = new TileItemElement();
                element.Text = $"Dinamik İşlem {i}";
                element.Text += Environment.NewLine + $"Açıklama {i}";
                element.TextAlignment = TileItemContentAlignment.MiddleCenter;

                // Rastgele renk
                Random rnd = new Random();
                Color randomColor = Color.FromArgb(
                    rnd.Next(100, 200),
                    rnd.Next(100, 200),
                    rnd.Next(100, 200)
                );

                item.AppearanceItem.Normal.BackColor = randomColor;
                item.AppearanceItem.Normal.BackColor2 = Lighten(randomColor, 0.3f);
                item.AppearanceItem.Normal.GradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;

                item.Elements.Add(element);
                dynamicGroup.Items.Add(item);

                item.ItemClick += (sender, e) =>
                {
                    ShowNotification($"Dinamik tile tıklandı: {item.Name}");
                };
            }

            tileControl.Refresh();
            UpdateStatus($"Yeni grup eklendi. Toplam grup: {tileControl.Groups.Count}");
        }

        // Form işlem metodları (örnek)
        private void ShowNewRecordForm()
        {
            XtraForm form = new XtraForm
            {
                Text = "Yeni Kayıt",
                Size = new Size(400, 300),
                StartPosition = FormStartPosition.CenterParent
            };

            Label label = new Label
            {
                Text = "Yeni kayıt formu örneği",
                Location = new Point(50, 50),
                AutoSize = true
            };

            form.Controls.Add(label);
            form.ShowDialog(this);
        }

        private void ShowEditForm()
        {
            ShowNotification("Düzenleme formu açılıyor...");
        }

        private void ShowDeleteConfirmation()
        {
            if (XtraMessageBox.Show("Silmek istediğinize emin misiniz?",
                "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ShowNotification("Kayıt silindi");
            }
        }

        private void ShowSearchDialog()
        {
            ShowNotification("Arama dialogu açılıyor...");
        }

        private void ShowSettingsForm()
        {
            ShowNotification("Ayarlar formu açılıyor...");
        }



    } ///********


     

}
