using System;
using System.IO;
using System.Windows.Forms;
using System.Security.Principal;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace WebsiteBlocker
{
    public partial class Form1 : Form
    {
        private const string HOSTS_FILE = @"C:\Windows\System32\drivers\etc\hosts";
        private CustomListBox blockedSitesList;
        private ModernTextBox urlInput;

        public Form1()
        {
            InitializeComponent();
            SetupForm();
            CheckAdminPrivileges();
            LoadBlockedSites();
        }

        private void SetupForm()
        {
            // Modern form settings
            this.Text = "Website Blocker";
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.Font = new Font("Segoe UI", 9F);

            // Create header panel
            var headerPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(Screen.PrimaryScreen.WorkingArea.Width, 80),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var titleLabel = new Label
            {
                Text = "Website Blocker",
                Font = new Font("Segoe UI Semibold", 24F),
                ForeColor = Color.FromArgb(51, 51, 51),
                Location = new Point(40, 20),
                AutoSize = true
            };
            headerPanel.Controls.Add(titleLabel);

            // Create main container panel
            var mainPanel = new Panel
            {
                Location = new Point(40, 100),
                Size = new Size(Screen.PrimaryScreen.WorkingArea.Width - 80, Screen.PrimaryScreen.WorkingArea.Height - 140),
                BackColor = Color.White,
                Padding = new Padding(40),
                BorderStyle = BorderStyle.FixedSingle
            };
            mainPanel.Paint += (s, e) => DrawShadowBorder(e.Graphics, mainPanel.ClientRectangle);

            // Create URL input section
            var inputLabel = new Label
            {
                Text = "Enter website to block:",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(51, 51, 51),
                Location = new Point(20, 20),
                Size = new Size(200, 20)
            };

            urlInput = new ModernTextBox
            {
                Location = new Point(20, 45),
                Size = new Size(400, 35),
                Font = new Font("Segoe UI", 10F),
                PlaceholderText = "example.com"
            };

            var addButton = new ModernButton
            {
                Text = "Block Website",
                Location = new Point(430, 45),
                Size = new Size(100, 35),
                ForeColor = Color.Black,
                BackColor = Color.FromArgb(0, 120, 212)
            };
            addButton.Click += BlockWebsite_Click;

            // Create list of blocked sites
            var listLabel = new Label
            {
                Text = "Currently Blocked Websites",
                Font = new Font("Segoe UI", 12F),
                ForeColor = Color.FromArgb(51, 51, 51),
                Location = new Point(40, 130),
                Size = new Size(300, 25)
            };

            blockedSitesList = new CustomListBox
            {
                Location = new Point(40, 160),
                Size = new Size(Screen.PrimaryScreen.WorkingArea.Width - 160, Screen.PrimaryScreen.WorkingArea.Height - 400),
                Font = new Font("Segoe UI", 11F),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 250)
            };

            var removeButton = new ModernButton
            {
                Text = "Unblock Selected",
                Location = new Point(40, Screen.PrimaryScreen.WorkingArea.Height - 200),
                Size = new Size(180, 40),
                BackColor = Color.FromArgb(209, 17, 65),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 11F)
            };
            removeButton.Click += UnblockWebsite_Click;

            // Add controls to main panel
            mainPanel.Controls.AddRange(new Control[] {
                inputLabel,
                urlInput,
                addButton,
                listLabel,
                blockedSitesList,
                removeButton
            });

            // Add panels to form
            this.Controls.AddRange(new Control[] {
                headerPanel,
                mainPanel
            });
        }

        private void DrawShadowBorder(Graphics g, Rectangle rect)
        {
            using (var path = new GraphicsPath())
            {
                path.AddRectangle(rect);
                using (var brush = new PathGradientBrush(path))
                {
                    brush.CenterColor = Color.FromArgb(0, Color.Gray);
                    brush.SurroundColors = new[] { Color.FromArgb(20, Color.Gray) };
                    g.FillPath(brush, path);
                }
            }
        }

        private void CheckAdminPrivileges()
        {
            bool isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator);

            if (!isAdmin)
            {
                MessageBox.Show("Please run as Administrator to modify the hosts file.",
                    "Admin Rights Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                Application.Exit();
            }
        }

        private void LoadBlockedSites()
        {
            try
            {
                string[] lines = File.ReadAllLines(HOSTS_FILE);
                foreach (string line in lines)
                {
                    if (line.StartsWith("127.0.0.1 ") && !line.StartsWith("#"))
                    {
                        string site = line.Replace("127.0.0.1 ", "").Trim();
                        blockedSitesList.Items.Add(site);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading hosts file: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BlockWebsite_Click(object sender, EventArgs e)
        {
            string url = urlInput.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("Please enter a website to block.",
                    "Input Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Clean up URL
            url = url.Replace("http://", "").Replace("https://", "");
            if (url.StartsWith("www."))
            {
                url = url.Substring(4);
            }

            try
            {
                // Add both www and non-www versions
                File.AppendAllText(HOSTS_FILE, $"\n127.0.0.1 {url}");
                File.AppendAllText(HOSTS_FILE, $"\n127.0.0.1 www.{url}");

                blockedSitesList.Items.Add(url);
                blockedSitesList.Items.Add($"www.{url}");
                urlInput.Clear();

                // Flush DNS cache
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "ipconfig",
                    Arguments = "/flushdns",
                    UseShellExecute = true,
                    Verb = "runas",
                    CreateNoWindow = true
                });

                MessageBox.Show($"Successfully blocked {url}",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error blocking website: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void UnblockWebsite_Click(object sender, EventArgs e)
        {
            if (blockedSitesList.SelectedItem == null)
            {
                MessageBox.Show("Please select a website to unblock.",
                    "Selection Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string selectedSite = blockedSitesList.SelectedItem.ToString();
                string[] lines = File.ReadAllLines(HOSTS_FILE);
                File.WriteAllLines(HOSTS_FILE,
                    Array.FindAll(lines, line => !line.Contains(selectedSite)));

                blockedSitesList.Items.Remove(selectedSite);

                // Flush DNS cache
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "ipconfig",
                    Arguments = "/flushdns",
                    UseShellExecute = true,
                    Verb = "runas",
                    CreateNoWindow = true
                });

                MessageBox.Show($"Successfully unblocked {selectedSite}",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error unblocking website: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }

    // Custom Controls
    public class ModernButton : Button
    {
        public ModernButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 1;
            this.Font = new Font("Segoe UI", 9F);
            this.Cursor = Cursors.Hand;
            this.TextAlign = ContentAlignment.MiddleCenter;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Draw border
            using (var pen = new Pen(Color.FromArgb(220, 220, 220)))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }
    }

    public class ModernTextBox : TextBox
    {
        public ModernTextBox()
        {
            this.BorderStyle = BorderStyle.FixedSingle;
            this.BackColor = Color.FromArgb(250, 250, 250);
            this.Padding = new Padding(5);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.DrawRectangle(
                new Pen(Color.FromArgb(220, 220, 220)),
                new Rectangle(0, 0, this.Width - 1, this.Height - 1)
            );
        }
    }

    public class CustomListBox : ListBox
    {
        public CustomListBox()
        {
            this.DrawMode = DrawMode.OwnerDrawFixed;
            this.ItemHeight = 30;
            this.BorderStyle = BorderStyle.None;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                e.Graphics.FillRectangle(
                    new SolidBrush(Color.FromArgb(229, 243, 255)),
                    e.Bounds
                );
            }

            using (var brush = new SolidBrush(Color.FromArgb(51, 51, 51)))
            {
                e.Graphics.DrawString(
                    this.Items[e.Index].ToString(),
                    this.Font,
                    brush,
                    new Point(e.Bounds.X + 5, e.Bounds.Y + 5)
                );
            }

            e.DrawFocusRectangle();
        }
    }
}