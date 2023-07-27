using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using EO.WebBrowser;
namespace GamerGate
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private static string backgroundpath;
        private static string backgroundcolor = "";
        private static string buttoncolors = "";
        private static string textcolor = "";
        private void Form1_Shown(object sender, EventArgs e)
        {
            this.pictureBox1.Dock = DockStyle.Fill;
            EO.WebEngine.BrowserOptions options = new EO.WebEngine.BrowserOptions();
            options.EnableWebSecurity = false;
            EO.WebBrowser.Runtime.DefaultEngineOptions.SetDefaultBrowserOptions(options);
            EO.WebEngine.Engine.Default.Options.AllowProprietaryMediaFormats();
            EO.WebEngine.Engine.Default.Options.SetDefaultBrowserOptions(new EO.WebEngine.BrowserOptions
            {
                EnableWebSecurity = false
            });
            this.webView1.Create(pictureBox1.Handle);
            this.webView1.Engine.Options.AllowProprietaryMediaFormats();
            this.webView1.SetOptions(new EO.WebEngine.BrowserOptions
            {
                EnableWebSecurity = false
            });
            this.webView1.Engine.Options.DisableGPU = false;
            this.webView1.Engine.Options.DisableSpellChecker = true;
            this.webView1.Engine.Options.CustomUserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";
            LoadPage();
            webView1.RegisterJSExtensionFunction("saveDocument", new JSExtInvokeHandler(WebView_JSSaveDocument));
            webView1.RegisterJSExtensionFunction("execFile", new JSExtInvokeHandler(WebView_JSExecFile));
        }
        private void LoadPage()
        {
            backgroundpath = "file:///" + System.Reflection.Assembly.GetEntryAssembly().Location.Replace(@"file:\", "").Replace(Process.GetCurrentProcess().ProcessName + ".exe", "").Replace(@"\", "/").Replace(@"//", "") + "background.gif";
            backgroundcolor = "";
            buttoncolors = "";
            textcolor = "";
            using (StreamReader file = new StreamReader("colors.txt"))
            {
                file.ReadLine();
                backgroundcolor = file.ReadLine();
                file.ReadLine();
                buttoncolors = file.ReadLine();
                file.ReadLine();
                textcolor = file.ReadLine();
                file.Close();
            }
            string path = @"gamergate.txt";
            string readText = DecryptFiles(path + ".encrypted", "tybtrybrtyertu50727885");
            readText = readText.Replace("backgroundcolor", backgroundcolor).Replace("buttoncolors", buttoncolors).Replace("textcolor", textcolor).Replace("background.gif", backgroundpath);
            webView1.LoadHtml(readText);
        }
        void WebView_JSSaveDocument(object sender, JSExtInvokeArgs e)
        {
            string innerhtml = e.Arguments[0] as string;
            string newstringsearches = e.Arguments[1] as string;
            string path = @"gamergate.txt";
            int pFrom = innerhtml.IndexOf("var searches = ") + "var searches = ".Length;
            int pTo = innerhtml.LastIndexOf(@"""""];");
            string result = innerhtml.Substring(pFrom, pTo - pFrom);
            innerhtml = innerhtml.Replace(result, newstringsearches).Replace("background-image: url('" + backgroundpath + "');", "background-image: url('background.gif');").Replace("background-color: " + backgroundcolor + ";", "background-color: backgroundcolor;").Replace("background-color: " + buttoncolors + ";", "background-color: buttoncolors;").Replace("background-color: " + buttoncolors + " !important;", "background-color: buttoncolors !important;").Replace("color: " + textcolor + ";", "color: textcolor;");
            EncryptStringToFile(innerhtml, path + ".encrypted", "tybtrybrtyertu50727885");
            LoadPage();
            MessageBox.Show("The page have been saved.");
        }
        public static void EncryptStringToFile(string contents, string outputFile, string password)
        {
            byte[] salt = new byte[8];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(salt);
            using (var encryptedStream = new MemoryStream())
            {
                StreamWriter sw = new StreamWriter(encryptedStream);
                sw.Write(contents);
                sw.Flush();
                encryptedStream.Seek(0, SeekOrigin.Begin);
                using (var pbkdf = new Rfc2898DeriveBytes(password, salt))
                using (var aes = new RijndaelManaged())
                using (var encryptor = aes.CreateEncryptor(pbkdf.GetBytes(aes.KeySize / 8), pbkdf.GetBytes(aes.BlockSize / 8)))
                using (var output = File.Create(outputFile))
                {
                    output.Write(salt, 0, salt.Length);
                    using (var cs = new CryptoStream(output, encryptor, CryptoStreamMode.Write))
                        encryptedStream.CopyTo(cs);
                    encryptedStream.Flush();
                }
            }
        }
        public static string DecryptFiles(string inputFile, string password)
        {
            using (var input = File.OpenRead(inputFile))
            {
                byte[] salt = new byte[8];
                input.Read(salt, 0, salt.Length);
                using (var decryptedStream = new MemoryStream())
                using (var pbkdf = new Rfc2898DeriveBytes(password, salt))
                using (var aes = new RijndaelManaged())
                using (var decryptor = aes.CreateDecryptor(pbkdf.GetBytes(aes.KeySize / 8), pbkdf.GetBytes(aes.BlockSize / 8)))
                using (var cs = new CryptoStream(input, decryptor, CryptoStreamMode.Read))
                {
                    string contents;
                    int data;
                    while ((data = cs.ReadByte()) != -1)
                        decryptedStream.WriteByte((byte)data);
                    decryptedStream.Position = 0;
                    using (StreamReader sr = new StreamReader(decryptedStream))
                        contents = sr.ReadToEnd();
                    decryptedStream.Flush();
                    return contents;
                }
            }
        }
        void WebView_JSExecFile(object sender, JSExtInvokeArgs e)
        {
            string stringfileexec = e.Arguments[0] as string;
            Process.Start(stringfileexec);
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.webView1.Dispose();
        }
    }
}
