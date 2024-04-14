using SuperSimpleTcp;
using System.Text;

namespace TCPChat
{
    public partial class Form1 : Form
    {
        SimpleTcpClient tcpClient;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                tcpClient.Connect();
                btnSend.Enabled = true;
                btnConnect.Enabled = false;
            }
            catch (Exception ec)
            {
                MessageBox.Show(ec.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {

            // kieemr tra xem đã kết nôi chauw thì mới gửi được tin nhắn
            if (tcpClient.IsConnected)
            {
                tcpClient.Send(txtInput.Text);
                DisplayChat.SelectionAlignment = HorizontalAlignment.Right;
                DisplayChat.Text += $"You: {txtInput.Text}{Environment.NewLine}";
                txtInput.Text = string.Empty;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tcpClient = new(txtIP.Text);
            tcpClient.Events.Connected += Events_Connected;
            tcpClient.Events.DataReceived += Events_DataReceived;
            tcpClient.Events.Disconnected += Events_Disconnected;

            //không cho gửi khi chưa kết nối
            btnSend.Enabled = false;
        }
        private void AddImageToDisplayChat(Bitmap bitmap)
        {
            DisplayChat.Invoke((MethodInvoker)(() =>
            {
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
                    DisplayChat.SelectionAlignment = HorizontalAlignment.Left;
                    /*                    DisplayChat.SelectionAlignment = HorizontalAlignment.Right : HorizontalAlignment.Left;
                    */
                    DisplayChat.Paste(myFormat);
                }

                // disable editing again
                DisplayChat.ReadOnly = true;
            }));
            DisplayChat.AppendText(Environment.NewLine);

        }
        private void Events_Disconnected(object? sender, ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                DisplayChat.Text += $"Disconnencted.{Environment.NewLine}";
            });
        }

        private void Events_DataReceived(object? sender, DataReceivedEventArgs e)
        {

            this.Invoke((MethodInvoker)delegate
            {
                string dataString = Encoding.UTF8.GetString(e.Data);

                if (dataString.StartsWith("D:\\"))
                {
                    Image image = Image.FromFile(dataString);

                    Bitmap myBitmap = new Bitmap(image);
                    AddImageToDisplayChat(myBitmap);


                    DisplayChat.ScrollToCaret();

                    return;
                }
                DisplayChat.SelectionAlignment = HorizontalAlignment.Left;
                DisplayChat.Text += $"Server: {Encoding.UTF8.GetString(e.Data)}{Environment.NewLine}";
            });
        }

        private void Events_Connected(object? sender, ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                DisplayChat.Text += $"Connect Successfully.{Environment.NewLine}";
            });
        }

        private void btnChoosseImage_Click(object sender, EventArgs e)
        {

        }
    }
}
