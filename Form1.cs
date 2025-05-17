using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp7
{
    public partial class Form1 : Form
    {
        private MyStack clipboardStack = new MyStack();
        private List<string> pinnedItems = new List<string>(); // Danh sách các mục đã ghim
        private const int MaxItems = 15;
        private TextBox txtInput;
        private ListBox lstHistory;
        private ListBox lstPinned; // ListBox mới cho các mục đã ghim
        private Button btnClearAll;
        private Button btnPaste; // Nút mới để dán vào clipboard
        private TabControl tabControl; // TabControl để chuyển đổi giữa clipboard và đã ghim
        private TabPage tabClipboard;
        private TabPage tabPinned;
        private RadioButton rbtClear;
        private System.Windows.Forms.Timer clipboardTimer;
        private string lastClipboardText = "";
        private int checkCount = 0;

        public Form1()
        {
            InitializeComponent();
            SetupControls();
            SetupEvents();
            this.Load += Form1_Load;
            this.MinimumSize = this.Size;
        
        }

        private void SetupControls()
        {
            // Khởi tạo các control
            txtInput = new TextBox();
            tabControl = new TabControl();
            tabClipboard = new TabPage("Clipboard");
            tabPinned = new TabPage("Đã Ghim");
            lstHistory = new ListBox();
            lstPinned = new ListBox();
            btnClearAll = new Button();
            btnPaste = new Button(); // Khởi tạo nút Paste
            rbtClear = new RadioButton();

            // Cấu hình TextBox
            txtInput.Location = new Point(12, 12);
            txtInput.Size = new Size(ClientSize.Width - 300, 30);
            Controls.Add(txtInput);

            // Cấu hình TabControl
            tabControl.Location = new Point(12, 50);
            tabControl.Size = new Size(ClientSize.Width - 24, ClientSize.Height - 62);
            tabControl.Dock = DockStyle.None;
            Controls.Add(tabControl);

            // Cấu hình Tab Clipboard
            tabClipboard.Text = "Clipboard";
            tabControl.TabPages.Add(tabClipboard);

            // Cấu hình Tab Đã Ghim
            tabPinned.Text = "Đã Ghim";
            tabControl.TabPages.Add(tabPinned);

            // Cấu hình ListBox Clipboard
            lstHistory.Dock = DockStyle.Fill;
            lstHistory.ScrollAlwaysVisible = true;
            lstHistory.HorizontalScrollbar = false;
            lstHistory.IntegralHeight = false;
            lstHistory.BorderStyle = BorderStyle.Fixed3D;
            lstHistory.DrawMode = DrawMode.OwnerDrawFixed; // Cho phép vẽ tùy chỉnh
            tabClipboard.Controls.Add(lstHistory);

            // Cấu hình ListBox Đã Ghim
            lstPinned.Dock = DockStyle.Fill;
            lstPinned.ScrollAlwaysVisible = true;
            lstPinned.HorizontalScrollbar = false;
            lstPinned.IntegralHeight = false;
            lstPinned.BorderStyle = BorderStyle.Fixed3D;
            lstPinned.DrawMode = DrawMode.OwnerDrawFixed; // Cho phép vẽ tùy chỉnh
            tabPinned.Controls.Add(lstPinned);

            // Cấu hình nút Paste - Đặt ở giữa để tách biệt với Clear All
            btnPaste.Location = new Point(ClientSize.Width - 100, 12);
            btnPaste.Size = new Size(80, 30);
            btnPaste.Text = "Dán";
            Controls.Add(btnPaste);

            // Cấu hình nút ClearAll - Đặt ở bên phải, cách xa nút Dán
            btnClearAll.Location = new Point(ClientSize.Width - 115, 12);
            btnClearAll.Size = new Size(100, 30);
            btnClearAll.Text = "Clear All";
            Controls.Add(btnClearAll);

            // Khởi tạo timer với interval cao hơn để tránh tràn RAM
            try
            {
                clipboardTimer = new System.Windows.Forms.Timer();
                clipboardTimer.Interval = 1500; // 1.5 giây để giảm tải hệ thống
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi tạo timer: " + ex.Message);
            }

            // Khởi tạo dữ liệu ban đầu
            InitializeData();
        }

        private void SetupEvents()
        {
            // Đăng ký các sự kiện
            KeyPreview = true;
            KeyDown += MainForm_KeyDown;
            Resize += MainForm_Resize;
            Activated += Form1_Activated;
            btnClearAll.Click += BtnClearAll_Click;
            btnPaste.Click += BtnPaste_Click; // Thêm sự kiện cho nút Paste

            // Đăng ký sự kiện cho ListBox
            lstHistory.MouseDoubleClick += LstHistory_MouseDoubleClick;
            lstHistory.DrawItem += LstHistory_DrawItem;
            lstHistory.MouseUp += LstHistory_MouseUp;

            // Đăng ký sự kiện cho ListBox Đã Ghim
            lstPinned.MouseDoubleClick += LstPinned_MouseDoubleClick;
            lstPinned.DrawItem += LstPinned_DrawItem;
            lstPinned.MouseUp += LstPinned_MouseUp;

            // Đăng ký sự kiện timer
            try
            {
                clipboardTimer.Tick += ClipboardTimer_Tick;
                clipboardTimer.Enabled = true; // Bật timer
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đăng ký sự kiện timer: " + ex.Message);
            }
        }

        // Xử lý sự kiện khi nhấn nút Paste (Dán)
        private void BtnPaste_Click(object sender, EventArgs e)
        {
            try
            {
                // Nếu có văn bản trong ô input hoặc có mục được chọn trong lstHistory
                if (!string.IsNullOrEmpty(txtInput.Text))
                {
                    // Đặt nội dung vào clipboard để người dùng có thể dán vào ứng dụng khác
                    Clipboard.SetText(txtInput.Text);
                    lastClipboardText = txtInput.Text;

                    // Kích hoạt tổ hợp phím Ctrl+V để dán nội dung
                    // Đưa ứng dụng khác lên trước
                    this.WindowState = FormWindowState.Minimized;

                    // Đợi một chút để cửa sổ khác xuất hiện
                    System.Threading.Thread.Sleep(100);

                    // Gửi phím Ctrl+V để dán
                    SendKeys.SendWait("^v");

                    // Hiển thị thông báo
                    this.Text = "Đã dán văn bản thành công!";

                    // Đặt lại tiêu đề sau 2 giây
                    System.Threading.Tasks.Task.Delay(2000).ContinueWith(t => {
                        if (!this.IsDisposed)
                        {
                            this.Invoke(new Action(() => this.Text = "Clipboard Manager"));
                        }
                    });
                }
                else if (lstHistory.SelectedIndex != -1)
                {
                    // Nếu có mục được chọn trong lstHistory
                    string selectedText = lstHistory.SelectedItem.ToString();
                    txtInput.Text = selectedText;

                    // Đặt nội dung vào clipboard
                    Clipboard.SetText(selectedText);
                    lastClipboardText = selectedText;

                    // Kích hoạt tổ hợp phím Ctrl+V để dán nội dung
                    this.WindowState = FormWindowState.Minimized;
                    System.Threading.Thread.Sleep(100);
                    SendKeys.SendWait("^v");
                }
                else if (lstPinned.SelectedIndex != -1)
                {
                    // Nếu có mục được chọn trong lstPinned
                    string selectedText = lstPinned.SelectedItem.ToString();
                    txtInput.Text = selectedText;

                    // Đặt nội dung vào clipboard
                    Clipboard.SetText(selectedText);
                    lastClipboardText = selectedText;

                    // Kích hoạt tổ hợp phím Ctrl+V để dán nội dung
                    this.WindowState = FormWindowState.Minimized;
                    System.Threading.Thread.Sleep(100);
                    SendKeys.SendWait("^v");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi dán: " + ex.Message);
            }
        }

        // Cập nhật stack từ ListBox sau khi xóa
        private void UpdateStackFromListBox()
        {
            // Xóa stack hiện tại
            while (!clipboardStack.IsEmpty())
            {
                clipboardStack.Pop();
            }

            // Tạo stack tạm
            MyStack tempStack = new MyStack();

            // Thêm các mục từ ListBox vào stack tạm
            for (int i = 0; i < lstHistory.Items.Count; i++)
            {
                tempStack.Push(lstHistory.Items[i].ToString());
            }

            // Chuyển từ stack tạm vào stack chính
            while (!tempStack.IsEmpty())
            {
                clipboardStack.Push(tempStack.Pop());
            }
        }

        // Sự kiện khi double click vào mục trong ListBox Clipboard
        private void LstHistory_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                int index = lstHistory.IndexFromPoint(e.Location);
                if (index != ListBox.NoMatches)
                {
                    // Đặt nội dung được chọn vào clipboard
                    string selectedText = lstHistory.Items[index].ToString();
                    Clipboard.SetText(selectedText);
                    lastClipboardText = selectedText;
                    txtInput.Text = selectedText;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi chọn mục: " + ex.Message);
            }
        }

        // Sự kiện khi double click vào mục trong ListBox Đã Ghim
        private void LstPinned_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                int index = lstPinned.IndexFromPoint(e.Location);
                if (index != ListBox.NoMatches)
                {
                    // Đặt nội dung được chọn vào clipboard
                    string selectedText = lstPinned.Items[index].ToString();
                    Clipboard.SetText(selectedText);
                    lastClipboardText = selectedText;
                    txtInput.Text = selectedText;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi chọn mục đã ghim: " + ex.Message);
            }
        }

        // Vẽ tùy chỉnh cho mục trong ListBox Clipboard (thêm nút xóa và ghim)
        private void LstHistory_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            // Vẽ nội dung
            string text = lstHistory.Items[e.Index].ToString();
            using (Brush brush = new SolidBrush(e.ForeColor))
            {
                e.Graphics.DrawString(text, e.Font, brush,
                    new RectangleF(e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 55, e.Bounds.Height));
            }

            // Vẽ nút Xóa
            Rectangle deleteRect = new Rectangle(e.Bounds.Right - 45, e.Bounds.Y + 2, 16, 16);
            using (Brush brush = new SolidBrush(Color.Red))
            {
                e.Graphics.DrawString("🗑️", new Font("Segoe UI Symbol", 8, FontStyle.Regular), brush, deleteRect);
            }

            // Vẽ nút Ghim
            Rectangle pinRect = new Rectangle(e.Bounds.Right - 20, e.Bounds.Y + 2, 16, 16);
            using (Brush brush = new SolidBrush(Color.Blue))
            {
                e.Graphics.DrawString("📌", new Font("Segoe UI Symbol", 8, FontStyle.Regular), brush, pinRect);
            }

            e.DrawFocusRectangle();
        }

        // Vẽ tùy chỉnh cho mục trong ListBox Đã Ghim (thêm nút xóa)
        private void LstPinned_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            // Vẽ nội dung
            string text = lstPinned.Items[e.Index].ToString();
            using (Brush brush = new SolidBrush(e.ForeColor))
            {
                e.Graphics.DrawString(text, e.Font, brush,
                    new RectangleF(e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 25, e.Bounds.Height));
            }

            // Vẽ nút xóa
            Rectangle deleteRect = new Rectangle(e.Bounds.Right - 20, e.Bounds.Y + 2, 16, 16);
            using (Brush brush = new SolidBrush(Color.Red))
            {
                e.Graphics.DrawString("🗑️", new Font("Segoe UI Symbol", 8, FontStyle.Regular), brush, deleteRect);
            }

            e.DrawFocusRectangle();
        }

        // Xử lý sự kiện click chuột trên ListBox Clipboard (kiểm tra nếu click vào nút xóa hoặc ghim)
        private void LstHistory_MouseUp(object sender, MouseEventArgs e)
        {
            int index = lstHistory.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                Rectangle itemRect = lstHistory.GetItemRectangle(index);
                Rectangle deleteRect = new Rectangle(itemRect.Right - 45, itemRect.Y + 2, 16, 16);
                Rectangle pinRect = new Rectangle(itemRect.Right - 20, itemRect.Y + 2, 16, 16);

                if (deleteRect.Contains(e.Location))
                {
                    // Xóa CHỈ 1 mục được chọn khỏi ListBox thay vì xóa tất cả
                    string itemToRemove = lstHistory.Items[index].ToString();
                    lstHistory.Items.RemoveAt(index);

                    // Cập nhật lại stack
                    UpdateStackFromListBox();
                }
                else if (pinRect.Contains(e.Location))
                {
                    // Ghim mục được chọn
                    string selectedItem = lstHistory.Items[index].ToString();

                    // Thêm vào danh sách đã ghim nếu chưa tồn tại
                    if (!pinnedItems.Contains(selectedItem))
                    {
                        pinnedItems.Add(selectedItem);
                        lstPinned.Items.Add(selectedItem);

                        // Xóa khỏi clipboard history
                        lstHistory.Items.RemoveAt(index);

                        // Cập nhật lại stack
                        UpdateStackFromListBox();

                        // Chuyển sang tab Đã Ghim để người dùng thấy kết quả
                        tabControl.SelectedTab = tabPinned;
                    }
                }
            }
        }

        // Xử lý sự kiện click chuột trên ListBox Đã Ghim (kiểm tra nếu click vào nút X)
        private void LstPinned_MouseUp(object sender, MouseEventArgs e)
        {
            int index = lstPinned.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                Rectangle itemRect = lstPinned.GetItemRectangle(index);
                Rectangle deleteRect = new Rectangle(itemRect.Right - 20, itemRect.Y + 2, 16, 16);

                if (deleteRect.Contains(e.Location))
                {
                    // Xóa CHỈ mục được chọn khỏi danh sách đã ghim
                    string itemToRemove = lstPinned.Items[index].ToString();
                    pinnedItems.Remove(itemToRemove);
                    lstPinned.Items.RemoveAt(index);
                }
            }
        }

        private void InitializeData()
        {
            try
            {
                // Kiểm tra và lấy dữ liệu clipboard khi khởi động
                if (Clipboard.ContainsText())
                {
                    string clipText = Clipboard.GetText();
                    txtInput.Text = clipText;
                    lastClipboardText = clipText;

                    // Thêm vào stack và listbox
                    clipboardStack.Push(clipText);
                    lstHistory.Items.Add(clipText);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khởi tạo dữ liệu: " + ex.Message);
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            try
            {
                int padding = 12;
                int spacing = 10;
                int buttonWidth = 100;
                int buttonHeight = 30;

                // TextBox giữ nguyên kích thước và vị trí
                txtInput.Location = new Point(padding, padding);
                // txtInput.Size = new Size(...); // KHÔNG thay đổi size ở đây

                // Nút Paste
                btnPaste.Size = new Size(buttonWidth, buttonHeight);
                btnPaste.Location = new Point(ClientSize.Width - 2 * buttonWidth - spacing - padding, padding);

                // Nút Clear All
                btnClearAll.Size = new Size(buttonWidth, buttonHeight);
                btnClearAll.Location = new Point(ClientSize.Width - buttonWidth - padding, padding);

                // TabControl nằm bên dưới, chiếm phần còn lại
                tabControl.Location = new Point(padding, txtInput.Bottom + spacing);
                tabControl.Size = new Size(ClientSize.Width - 2 * padding, ClientSize.Height - txtInput.Bottom - 2 * padding);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi resize: " + ex.Message);
            }
        }
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Control && e.KeyCode == Keys.C)
                {
                    // Triển khai chức năng copy dựa trên nội dung đã chọn
                    if (ActiveControl is TextBox textBox && textBox.SelectionLength > 0)
                    {
                        // Nếu đang có text được chọn trong TextBox
                        string selectedText = textBox.SelectedText;
                        CopySelectedText(selectedText);
                        e.SuppressKeyPress = true;
                    }
                    else
                    {
                        // Nếu không có text nào được chọn, sử dụng chức năng copy mặc định
                        CopyText();
                        e.SuppressKeyPress = true;
                    }
                }
                else if (e.Control && e.KeyCode == Keys.V)
                {
                    PasteText();
                    e.SuppressKeyPress = true;
                    Show();
                    WindowState = FormWindowState.Normal;
                    Activate();
                }
                // Thêm tổ hợp phím Ctrl+P để ghim ghi chú hiện tại
                else if (e.Control && e.KeyCode == Keys.P)
                {
                    PinCurrentText();
                    e.SuppressKeyPress = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xử lý phím tắt: " + ex.Message);
            }
        }

        // Phương thức mới để ghim văn bản hiện tại từ TextBox hoặc mục đã chọn
        private void PinCurrentText()
        {
            try
            {
                if (lstHistory.SelectedIndex != -1)
                {
                    // Nếu có mục được chọn trong lịch sử clipboard
                    string selectedItem = lstHistory.SelectedItem.ToString();

                    // Thêm vào danh sách đã ghim nếu chưa tồn tại
                    if (!pinnedItems.Contains(selectedItem))
                    {
                        pinnedItems.Add(selectedItem);
                        lstPinned.Items.Add(selectedItem);

                        // Xóa khỏi clipboard history (tùy chọn)
                        lstHistory.Items.RemoveAt(lstHistory.SelectedIndex);

                        // Cập nhật lại stack
                        UpdateStackFromListBox();

                        // Chuyển sang tab Đã Ghim để người dùng thấy kết quả
                        tabControl.SelectedTab = tabPinned;
                    }
                }
                else if (!string.IsNullOrEmpty(txtInput.Text))
                {
                    // Nếu không có mục nào được chọn, ghim nội dung hiện tại của TextBox
                    string textToPin = txtInput.Text;

                    if (!pinnedItems.Contains(textToPin))
                    {
                        pinnedItems.Add(textToPin);
                        lstPinned.Items.Add(textToPin);
                        tabControl.SelectedTab = tabPinned;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi ghim: " + ex.Message);
            }
        }

        // Phương thức mới để xử lý copy văn bản đã chọn
        private void CopySelectedText(string selectedText)
        {
            try
            {
                if (!string.IsNullOrEmpty(selectedText))
                {
                    // Thêm vào stack
                    PushToStack(selectedText);

                    // Cập nhật clipboard
                    Clipboard.SetText(selectedText);
                    lastClipboardText = selectedText;

                    // Cập nhật listbox
                    UpdateListBox(selectedText);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi copy văn bản đã chọn: " + ex.Message);
            }
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            CheckClipboard(); // Kiểm tra clipboard khi form được kích hoạt
        }

        private void ClipboardTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                CheckClipboard();

                // Định kỳ giải phóng bộ nhớ để tránh tràn RAM
                checkCount++;
                if (checkCount > 20) // Sau khoảng 20 lần kiểm tra (30 giây với interval 1500ms)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    checkCount = 0;
                }
            }
            catch (Exception ex)
            {
                // Tạm dừng timer nếu có lỗi nghiêm trọng
                clipboardTimer.Enabled = false;
                MessageBox.Show("Lỗi kiểm tra clipboard: " + ex.Message +
                               "\nTính năng tự động đã bị tắt, vui lòng khởi động lại ứng dụng.");
            }
        }

        private void CheckClipboard()
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    string clipboardText = Clipboard.GetText();

                    // Chỉ cập nhật nếu nội dung clipboard khác với lần trước
                    if (clipboardText != lastClipboardText)
                    {
                        lastClipboardText = clipboardText;

                        // Cập nhật textbox
                        if (txtInput != null && !txtInput.IsDisposed)
                        {
                            txtInput.Text = clipboardText;
                        }

                        // Thêm vào stack
                        PushToStack(clipboardText);

                        // Cập nhật listbox
                        UpdateListBox(clipboardText);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi kiểm tra clipboard: " + ex.Message);
            }
        }

        private void PushToStack(string text)
        {
            // Kiểm tra số lượng phần tử trong stack
            if (clipboardStack.count >= MaxItems)
            {
                // Sử dụng stack tạm để loại bỏ phần tử cũ nhất
                MyStack tempStack = new MyStack();

                // Giữ lại MaxItems - 1 phần tử gần nhất
                int itemsToKeep = clipboardStack.count - 1;
                for (int i = 0; i < itemsToKeep; i++)
                {
                    tempStack.Push(clipboardStack.Pop());
                }

                // Loại bỏ phần tử cũ nhất
                clipboardStack.Pop();

                // Khôi phục các phần tử vào stack chính
                while (!tempStack.IsEmpty())
                {
                    clipboardStack.Push(tempStack.Pop());
                }
            }

            // Thêm phần tử mới vào stack
            clipboardStack.Push(text);
        }

        private void UpdateListBox(string text)
        {
            try
            {
                if (lstHistory != null && !lstHistory.IsDisposed)
                {
                    // Chỉ thêm nếu không trùng với item cuối cùng
                    if (lstHistory.Items.Count == 0 ||
                        lstHistory.Items[lstHistory.Items.Count - 1].ToString() != text)
                    {
                        lstHistory.Items.Add(text);

                        // Giới hạn số lượng item
                        if (lstHistory.Items.Count > MaxItems)
                        {
                            lstHistory.Items.RemoveAt(0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi cập nhật listbox: " + ex.Message);
            }
        }

        private void CopyText()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(txtInput.Text))
                {
                    string text = txtInput.Text;

                    // Thêm vào stack
                    PushToStack(text);

                    // Cập nhật clipboard
                    Clipboard.SetText(text);
                    lastClipboardText = text;

                    // Cập nhật listbox
                    UpdateHistory();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi copy: " + ex.Message);
            }
        }

        private void PasteText()
        {
            try
            {
                if (!clipboardStack.IsEmpty())
                {
                    string text = clipboardStack.Peek().ToString();
                    txtInput.Text = text;

                    // Đặt nội dung vào clipboard để người dùng có thể dán vào ứng dụng khác
                    Clipboard.SetText(text);

                    // Thông báo cho người dùng
                    this.Text = "Đã sao chép vào Clipboard - Sẵn sàng để dán";

                    // Đặt lại tiêu đề sau 2 giây
                    System.Threading.Tasks.Task.Delay(2000).ContinueWith(t => {
                        if (!this.IsDisposed)
                        {
                            this.Invoke(new Action(() => this.Text = "Clipboard Manager"));
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi paste: " + ex.Message);
            }
        }

        private void UpdateHistory()
        {
            try
            {
                lstHistory.Items.Clear();
                MyStack tempStack = new MyStack();

                // Lưu tất cả các mục vào stack tạm thời và thêm vào ListBox
                while (!clipboardStack.IsEmpty())
                {
                    // Sao chép các mục từ clipboardStack
                    while (!clipboardStack.IsEmpty())
                    {
                        string item = clipboardStack.Pop().ToString();
                        tempStack.Push(item);
                        lstHistory.Items.Insert(0, item); // Thêm vào đầu ListBox
                    }

                    // Khôi phục lại stack
                    while (!tempStack.IsEmpty())
                    {
                        clipboardStack.Push(tempStack.Pop());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật lịch sử: " + ex.Message);
            }
        }
        private void BtnClearAll_Click(object sender, EventArgs e)
        {
            try
            {
                // Xóa tất cả các mục trong lịch sử
                clipboardStack = new MyStack();
                lstHistory.Items.Clear();
                txtInput.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa tất cả: " + ex.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckClipboard();
        }
    
    }
    // Tạo class MyStack để quản lý clipboard stack
    public class MyStack
    {
        private class Node
        {
            public object Data;
            public Node Next;

            public Node(object data)
            {
                Data = data;
                Next = null;
            }
        }

        private Node top;
        public int count;

        public MyStack()
        {
            top = null;
            count = 0;
        }

        public void Push(object data)
        {
            Node newNode = new Node(data);
            newNode.Next = top;
            top = newNode;
            count++;
        }

        public object Pop()
        {
            if (IsEmpty())
                throw new InvalidOperationException("Stack is empty");

            object data = top.Data;
            top = top.Next;
            count--;
            return data;
        }

        public object Peek()
        {
            if (IsEmpty())
                throw new InvalidOperationException("Stack is empty");

            return top.Data;
        }

        public bool IsEmpty()
        {
            return top == null;
        }
    }

    // Phần code tiếp theo - Sửa các lỗi theo yêu cầu

    // Phần này sẽ chứa các phương thức mở rộng và điều chỉnh theo yêu cầu:
    public partial class Form1
    {
        // 1. Điều chỉnh vị trí nút Dán và Clear All để chúng không bị dính nhau
        private void AdjustButtonPositions()
        {
            // Tách rời nút Paste và Clear All (tăng khoảng cách giữa chúng)
            btnPaste.Location = new Point(ClientSize.Width - 250, 12);
            btnClearAll.Location = new Point(ClientSize.Width - 115, 12);
        }

        // Ghi đè phương thức SetupControls để điều chỉnh vị trí nút ban đầu
        private void SetupControlsExtended()
        {
            // Gọi phương thức này sau khi InitializeComponent trong constructor
            AdjustButtonPositions();

            // Đảm bảo sự kiện Resize được cập nhật để duy trì khoảng cách
            Resize += (sender, e) => AdjustButtonPositions();
        }

        // 3. Sửa chức năng dán để hoạt động chính xác
        // Phương thức cải tiến cho nút Dán
        private void EnhancedPaste()
        {
            try
            {
                string textToPaste = "";

                // Xác định văn bản cần dán
                if (!string.IsNullOrEmpty(txtInput.Text))
                {
                    textToPaste = txtInput.Text;
                }
                else if (lstHistory.SelectedIndex != -1)
                {
                    textToPaste = lstHistory.SelectedItem.ToString();
                }
                else if (lstPinned.SelectedIndex != -1)
                {
                    textToPaste = lstPinned.SelectedItem.ToString();
                }
                else if (!clipboardStack.IsEmpty())
                {
                    textToPaste = clipboardStack.Peek().ToString();
                }

                if (!string.IsNullOrEmpty(textToPaste))
                {
                    // Lưu text hiện tại của clipboard
                    string originalClipboardText = "";
                    if (Clipboard.ContainsText())
                    {
                        originalClipboardText = Clipboard.GetText();
                    }

                    // Đặt nội dung vào clipboard
                    Clipboard.SetText(textToPaste);

                    // Tối giản hóa ứng dụng
                    WindowState = FormWindowState.Minimized;

                    // Đợi để cửa sổ khác có thời gian được kích hoạt
                    System.Threading.Thread.Sleep(300);

                    // Gửi lệnh dán
                    SendKeys.SendWait("^v");

                    // Khôi phục clipboard sau khi đã dán (tùy chọn)
                    System.Threading.Tasks.Task.Delay(500).ContinueWith(t => {
                        if (!string.IsNullOrEmpty(originalClipboardText))
                        {
                            Clipboard.SetText(originalClipboardText);
                        }
                    });

                    // Hiển thị thông báo
                    UpdateFormTitle("Đã dán văn bản thành công!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi dán: " + ex.Message);
            }
        }

        // Helper method để cập nhật tiêu đề form và đặt lại sau một khoảng thời gian
        private void UpdateFormTitle(string message)
        {
            Text = message;
            System.Threading.Tasks.Task.Delay(2000).ContinueWith(t => {
                if (!IsDisposed)
                {
                   Invoke(new Action(() => Text = "Clipboard Manager"));
                }
            });
        }

        // Ghi đè phương thức BtnPaste_Click để sử dụng phương thức cải tiến
        private void EnhancedBtnPaste_Click(object sender, EventArgs e)
        {
            EnhancedPaste();
        }

        // 4. Sửa chức năng xóa để chỉ xóa mục được chọn
    
        public void ApplyEnhancements()
        {
            // Áp dụng cải tiến bố cục nút
            SetupControlsExtended();

            // Ghi đè sự kiện click nút Paste với phiên bản cải tiến
            btnPaste.Click -= BtnPaste_Click; // Xóa sự kiện cũ nếu đã đăng ký
            btnPaste.Click += EnhancedBtnPaste_Click;

            // Không cần thực hiện thêm gì cho chức năng xóa
            // vì đã được xử lý trong LstHistory_MouseUp và LstPinned_MouseUp
        }

        // Bổ sung phương thức khởi tạo cho tính năng cải tiến
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ApplyEnhancements();
        }
    }
}
