using Microsoft.VisualBasic.ApplicationServices;
using SuperSimpleTcp;
using System.Text;

namespace TCPChat_Server
{
    public partial class Form1 : Form
    {
        SimpleTcpServer server;
        private string selectImageString;
        public Form1()
        {
            InitializeComponent();
        }


        private void btnStart_Click(object sender, EventArgs e)
        {
            server.Start();
            DisplayChat.Text += $"Server is running...{Environment.NewLine}";
            btnStart.Enabled = false;
            btnSend.Enabled = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            btnSend.Enabled = false;
            server = new SimpleTcpServer(txtIP.Text);
            server.Events.ClientConnected += Events_ClientConnected;
            server.Events.ClientDisconnected += Events_ClientDisconnected;
            server.Events.DataReceived += Events_DataReceived;
        }

        private void Events_DataReceived(object? sender, DataReceivedEventArgs e)
        {

            this.Invoke((MethodInvoker)delegate
            {
                DisplayChat.SelectionAlignment = HorizontalAlignment.Right;
                // lấy dữ liệu nhận được bỏ vào ô text
                DisplayChat.Text += $"{e.IpPort}: {Encoding.UTF8.GetString(e.Data)}{Environment.NewLine}";
            });
        }

        private void Events_ClientDisconnected(object? sender, ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                // hiện log
                DisplayChat.Text += $"{e.IpPort} disconnected.{Environment.NewLine}";

                // xóa địa chỉ ip hiển thị khi không kết nối đến đó
                lstClientConnted.Items.Remove(e.IpPort);
            });

        }

        private void Events_ClientConnected(object? sender, ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                // hiện log
                DisplayChat.Text += $"{e.IpPort} connected.{Environment.NewLine}";

                // xóa địa chỉ ip hiển thị khi không kết nối đến đó
                lstClientConnted.Items.Add(e.IpPort);
            });

        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (server.IsListening)
            {
                // kieemr tra thoong tin truocs khi gửi
                if (!string.IsNullOrEmpty(txtInput.Text) && lstClientConnted.SelectedItem != null)
                {
                    if (txtInput.Text.StartsWith("[Image]") && selectImageString != null)
                    {
                        Image image = Image.FromFile(selectImageString);
                        server.Send(lstClientConnted.SelectedItem.ToString(), selectImageString);

                        Bitmap myBitmap = new Bitmap(selectImageString);
                        AddImageToDisplayChat(myBitmap);

                        txtInput.Clear();


                        DisplayChat.ScrollToCaret();

                        // restore the editing capability of the textbox
                        txtInput.ReadOnly = false;
                        selectImageString = null;
                        return;
                    }
                    //gửi tin nhắn thông thường

                    server.Send(lstClientConnted.SelectedItem.ToString(), txtInput.Text);
                   
                    DisplayChat.SelectionAlignment = HorizontalAlignment.Right;

                    DisplayChat.Text += $"Server: {txtInput.Text}{Environment.NewLine}";
                    // gửi thành công thì xóa dữ liệu trong ô nhập
                    txtInput.Text = string.Empty;
                }
            }
        }
        private void AddImageToDisplayChat(Bitmap bitmap)
        {
            DisplayChat.Invoke((MethodInvoker)(() =>
            {
                DisplayChat.Text += $"Server: {Environment.NewLine}";
                int maxWidth = 150;
                int maxHeight = 150;

                int newWidth, newHeight;
                double aspectRatio = (double)bitmap.Width / bitmap.Height;

                if (aspectRatio > 1)
                {
                    newWidth = maxWidth;
                    newHeight = (int)(maxWidth / aspectRatio);
                }
                else
                {
                    // Portrait or square image
                    newHeight = maxHeight;
                    newWidth = (int)(maxHeight * aspectRatio);
                }

                Bitmap resized = new Bitmap(bitmap, new Size(newWidth, newHeight));
                Clipboard.SetDataObject(resized);
                DataFormats.Format myFormat = DataFormats.GetFormat(DataFormats.Bitmap);

                DisplayChat.ReadOnly = false; // enable editing to allow image pasting

                // move the caret to the end of the text
                DisplayChat.Select(DisplayChat.Text.Length, 0);

                if (DisplayChat.CanPaste(myFormat))
                {
                    DisplayChat.SelectionAlignment = HorizontalAlignment.Right;
                    /*                    DisplayChat.SelectionAlignment = HorizontalAlignment.Right : HorizontalAlignment.Left;
                    */
                    DisplayChat.Paste(myFormat);
                }

                // disable editing again
                DisplayChat.ReadOnly = true;
            }));
            DisplayChat.AppendText(Environment.NewLine);
            
        }
            private void btnChooseImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Title = "Select image";
            fileDialog.Filter = "Image Files (*.bmp;*.jpg;*.jpeg,*.png)|*.BMP;*.JPG;*.JPEG;*.PNG";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                txtInput.Text = $"[Image] {fileDialog.SafeFileName}";
                txtInput.ReadOnly = true;

                selectImageString = fileDialog.FileName;
            }
        }
    }
}
