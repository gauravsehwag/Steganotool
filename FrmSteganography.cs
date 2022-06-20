
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Text2Image
{
  public class FrmSteganography : Form
  {
    private string loadedTrueImagePath;
    private string loadedFilePath;
    private string saveToImage;
    private string DLoadImagePath;
    private string DSaveFilePath;
    private int height;
    private int width;
    private long fileSize;
    private long fileNameSize;
    private Image loadedTrueImage;
    private Image DecryptedImage;
    private Image AfterEncryption;
    private Bitmap loadedTrueBitmap;
    private Bitmap DecryptedBitmap;
    private Rectangle previewImage = new Rectangle(20, 160, 490, 470);
    private bool canPaint = false;
    private bool EncriptionDone = false;
    private byte[] fileContainer;
    private IContainer components = (IContainer) null;
    private GroupBox groupBox1;
    private GroupBox groupBox2;
    private Button EnImageBrowse_btn;
    private Label label1;
    private TextBox EnImage_tbx;
    private Button EnFileBrowse_btn;
    private Label label2;
    private TextBox EnFile_tbx;
    private Button Encrypt_btn;
    private OpenFileDialog openFileDialog1;
    private OpenFileDialog openFileDialog2;
    private SaveFileDialog saveFileDialog1;
    private Label label5;
    private Button Decrypt_btn;
    private TextBox DeLoadImage_tbx;
    private TextBox DeSaveFile_tbx;
    private Button DeSaveFileBrowse_btn;
    private Label label6;
    private Button DeLoadImageBrowse_btn;
    private OpenFileDialog openFileDialog3;
    private Label label3;
    private GroupBox groupBox3;
    private Label label7;
    private Label label9;
    private Label ByteCapacity_lbl;
    private Label ImageSize_lbl;
    private FolderBrowserDialog folderBrowserDialog1;
    private Label CanSave_lbl;
    private Label ImageWidth_lbl;
    private Label ImageHeight_lbl;
    private Label label10;
    private Label label8;
    private Button Close_btn;
    private StatusStrip statusStrip1;
    private ToolStripStatusLabel toolStripStatusLabel1;
    private TabControl tabControl1;
    private TabPage tabPage1;
    private TabPage tabPage2;
    private LinkLabel linkLabel1;

    public FrmSteganography() => this.InitializeComponent();

    private void EnImageBrowse_btn_Click(object sender, EventArgs e)
    {
      if (this.openFileDialog1.ShowDialog() != DialogResult.OK)
        return;
      this.loadedTrueImagePath = this.openFileDialog1.FileName;
      this.EnImage_tbx.Text = this.loadedTrueImagePath;
      this.loadedTrueImage = Image.FromFile(this.loadedTrueImagePath);
      this.height = this.loadedTrueImage.Height;
      this.width = this.loadedTrueImage.Width;
      this.loadedTrueBitmap = new Bitmap(this.loadedTrueImage);
      this.ImageSize_lbl.Text = this.smalldecimal(((float) new FileInfo(this.loadedTrueImagePath).Length / 1024f).ToString(), 2) + " KB";
      this.ImageHeight_lbl.Text = this.loadedTrueImage.Height.ToString() + " Pixel";
      this.ImageWidth_lbl.Text = this.loadedTrueImage.Width.ToString() + " Pixel";
      this.CanSave_lbl.Text = this.smalldecimal((8.0 * (double) (this.height * (this.width / 3) * 3 / 3 - 1) / 1024.0).ToString(), 2) + " KB";
      this.canPaint = true;
      this.Invalidate();
    }

    private string smalldecimal(string inp, int dec)
    {
      int index;
      for (index = inp.Length - 1; index > 0; --index)
      {
        if (inp[index] == '.')
          break;
      }
      try
      {
        return inp.Substring(0, index + dec + 1);
      }
      catch
      {
        return inp;
      }
    }

    private void EnFileBrowse_btn_Click(object sender, EventArgs e)
    {
      if (this.openFileDialog2.ShowDialog() != DialogResult.OK)
        return;
      this.loadedFilePath = this.openFileDialog2.FileName;
      this.EnFile_tbx.Text = this.loadedFilePath;
      this.fileSize = new FileInfo(this.loadedFilePath).Length;
      this.fileNameSize = (long) this.justFName(this.loadedFilePath).Length;
    }

    private void Encrypt_btn_Click(object sender, EventArgs e)
    {
      if (this.saveFileDialog1.ShowDialog() != DialogResult.OK)
        return;
      this.saveToImage = this.saveFileDialog1.FileName;
      if (this.EnImage_tbx.Text == string.Empty || this.EnFile_tbx.Text == string.Empty)
      {
        int num1 = (int) MessageBox.Show("Encrypton information is incomplete!\nPlease complete them frist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
      }
      if ((long) (8 * (this.height * (this.width / 3) * 3 / 3 - 1)) < this.fileSize + this.fileNameSize)
      {
        int num2 = (int) MessageBox.Show("File size is too large!\nPlease use a larger image to hide this file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
      }
      else
      {
        this.fileContainer = File.ReadAllBytes(this.loadedFilePath);
        this.EncryptLayer();
      }
    }

    private void EncryptLayer()
    {
      this.toolStripStatusLabel1.Text = "Encrypting... Please wait";
      Application.DoEvents();
      long fileSize = this.fileSize;
      Bitmap inputBitmap = this.EncryptLayer(8, this.loadedTrueBitmap, 0L, (long) (this.height * (this.width / 3) * 3 / 3) - this.fileNameSize - 1L, true);
      long num = fileSize - ((long) (this.height * (this.width / 3) * 3 / 3) - this.fileNameSize - 1L);
      if (num > 0L)
      {
        for (int layer = 7; layer >= 0 && num > 0L; --layer)
        {
          inputBitmap = this.EncryptLayer(layer, inputBitmap, (long) ((8 - layer) * this.height * (this.width / 3) * 3 / 3) - this.fileNameSize - (long) (8 - layer), (long) ((9 - layer) * this.height * (this.width / 3) * 3 / 3) - this.fileNameSize - (long) (9 - layer), false);
          num -= (long) (this.height * (this.width / 3) * 3 / 3 - 1);
        }
      }
      inputBitmap.Save(this.saveToImage);
      this.toolStripStatusLabel1.Text = "Encrypted image has been successfully saved.";
      this.EncriptionDone = true;
      this.AfterEncryption = Image.FromFile(this.saveToImage);
      this.Invalidate();
    }

    private Bitmap EncryptLayer(
      int layer,
      Bitmap inputBitmap,
      long startPosition,
      long endPosition,
      bool writeFileName)
    {
      Bitmap bitmap = inputBitmap;
      --layer;
      int y1 = 0;
      int x1 = 0;
      long num1 = 0;
      bool[] outp1 = new bool[8];
      bool[] outp2 = new bool[8];
      bool[] outp3 = new bool[8];
      bool[] outp4 = new bool[8];
      Color color1 = new Color();
      if (writeFileName)
      {
        num1 = this.fileNameSize;
        string str = this.justFName(this.loadedFilePath);
        int y2;
        for (y2 = 0; y2 < this.height && (long) (y2 * (this.height / 3)) < this.fileNameSize; ++y2)
        {
          for (x1 = 0; x1 < this.width / 3 * 3 && (long) (y2 * (this.height / 3) + x1 / 3) < this.fileNameSize; ++x1)
          {
            this.byte2bool((byte) str[y2 * (this.height / 3) + x1 / 3], ref outp1);
            Color pixel = inputBitmap.GetPixel(x1, y2);
            byte r = pixel.R;
            byte g = pixel.G;
            byte b = pixel.B;
            this.byte2bool(r, ref outp2);
            this.byte2bool(g, ref outp3);
            this.byte2bool(b, ref outp4);
            if (x1 % 3 == 0)
            {
              outp2[7] = outp1[0];
              outp3[7] = outp1[1];
              outp4[7] = outp1[2];
            }
            else if (x1 % 3 == 1)
            {
              outp2[7] = outp1[3];
              outp3[7] = outp1[4];
              outp4[7] = outp1[5];
            }
            else
            {
              outp2[7] = outp1[6];
              outp3[7] = outp1[7];
            }
            Color color2 = Color.FromArgb((int) this.bool2byte(outp2), (int) this.bool2byte(outp3), (int) this.bool2byte(outp4));
            bitmap.SetPixel(x1, y2, color2);
          }
        }
        y1 = y2 - 1;
      }
      int num2 = x1;
      for (; y1 < this.height && (long) (y1 * (this.height / 3)) < endPosition - startPosition + num1 && startPosition + (long) (y1 * (this.height / 3)) < this.fileSize + num1; ++y1)
      {
        for (int x2 = 0; x2 < this.width / 3 * 3 && (long) (y1 * (this.height / 3) + x2 / 3) < endPosition - startPosition + num1 && startPosition + (long) (y1 * (this.height / 3)) + (long) (x2 / 3) < this.fileSize + num1; ++x2)
        {
          if (num2 != 0)
          {
            x2 = num2;
            num2 = 0;
          }
          this.byte2bool(this.fileContainer[startPosition + (long) (y1 * (this.height / 3)) + (long) (x2 / 3) - num1], ref outp1);
          Color pixel = inputBitmap.GetPixel(x2, y1);
          byte r = pixel.R;
          byte g = pixel.G;
          byte b = pixel.B;
          this.byte2bool(r, ref outp2);
          this.byte2bool(g, ref outp3);
          this.byte2bool(b, ref outp4);
          if (x2 % 3 == 0)
          {
            outp2[layer] = outp1[0];
            outp3[layer] = outp1[1];
            outp4[layer] = outp1[2];
          }
          else if (x2 % 3 == 1)
          {
            outp2[layer] = outp1[3];
            outp3[layer] = outp1[4];
            outp4[layer] = outp1[5];
          }
          else
          {
            outp2[layer] = outp1[6];
            outp3[layer] = outp1[7];
          }
          Color color3 = Color.FromArgb((int) this.bool2byte(outp2), (int) this.bool2byte(outp3), (int) this.bool2byte(outp4));
          bitmap.SetPixel(x2, y1, color3);
        }
      }
      long fileSize = this.fileSize;
      long fileNameSize = this.fileNameSize;
      byte red1 = (byte) ((ulong) fileSize % 100UL);
      long num3 = fileSize / 100L;
      byte green1 = (byte) ((ulong) num3 % 100UL);
      byte blue1 = (byte) ((ulong) (num3 / 100L) % 100UL);
      Color color4 = Color.FromArgb((int) red1, (int) green1, (int) blue1);
      bitmap.SetPixel(this.width - 1, this.height - 1, color4);
      byte red2 = (byte) ((ulong) fileNameSize % 100UL);
      long num4 = fileNameSize / 100L;
      byte green2 = (byte) ((ulong) num4 % 100UL);
      byte blue2 = (byte) ((ulong) (num4 / 100L) % 100UL);
      Color color5 = Color.FromArgb((int) red2, (int) green2, (int) blue2);
      bitmap.SetPixel(this.width - 2, this.height - 1, color5);
      return bitmap;
    }

    private void DecryptLayer()
    {
      this.toolStripStatusLabel1.Text = "Decrypting... Please wait";
      Application.DoEvents();
      int x1 = 0;
      bool[] inp = new bool[8];
      bool[] outp1 = new bool[8];
      bool[] outp2 = new bool[8];
      bool[] outp3 = new bool[8];
      Color color = new Color();
      Color pixel1 = this.DecryptedBitmap.GetPixel(this.width - 1, this.height - 1);
      long length = (long) ((int) pixel1.R + (int) pixel1.G * 100 + (int) pixel1.B * 10000);
      Color pixel2 = this.DecryptedBitmap.GetPixel(this.width - 2, this.height - 1);
      long num1 = (long) ((int) pixel2.R + (int) pixel2.G * 100 + (int) pixel2.B * 10000);
      byte[] bytes = new byte[length];
      string str = "";
      int y1;
      Color pixel3;
      for (y1 = 0; y1 < this.height && (long) (y1 * (this.height / 3)) < num1; ++y1)
      {
        for (x1 = 0; x1 < this.width / 3 * 3 && (long) (y1 * (this.height / 3) + x1 / 3) < num1; ++x1)
        {
          pixel3 = this.DecryptedBitmap.GetPixel(x1, y1);
          byte r = pixel3.R;
          byte g = pixel3.G;
          byte b = pixel3.B;
          this.byte2bool(r, ref outp1);
          this.byte2bool(g, ref outp2);
          this.byte2bool(b, ref outp3);
          if (x1 % 3 == 0)
          {
            inp[0] = outp1[7];
            inp[1] = outp2[7];
            inp[2] = outp3[7];
          }
          else if (x1 % 3 == 1)
          {
            inp[3] = outp1[7];
            inp[4] = outp2[7];
            inp[5] = outp3[7];
          }
          else
          {
            inp[6] = outp1[7];
            inp[7] = outp2[7];
            byte num2 = this.bool2byte(inp);
            str += (string) (object) num2;
          }
        }
      }
      int num3 = x1;
      for (int y2 = y1 - 1; y2 < this.height && (long) (y2 * (this.height / 3)) < length + num1; ++y2)
      {
        for (int x2 = 0; x2 < this.width / 3 * 3 && y2 * (this.height / 3) + x2 / 3 < this.height * (this.width / 3) * 3 / 3 - 1 && (long) (y2 * (this.height / 3) + x2 / 3) < length + num1; ++x2)
        {
          if (num3 != 0)
          {
            x2 = num3;
            num3 = 0;
          }
          pixel3 = this.DecryptedBitmap.GetPixel(x2, y2);
          byte r = pixel3.R;
          byte g = pixel3.G;
          byte b = pixel3.B;
          this.byte2bool(r, ref outp1);
          this.byte2bool(g, ref outp2);
          this.byte2bool(b, ref outp3);
          if (x2 % 3 == 0)
          {
            inp[0] = outp1[7];
            inp[1] = outp2[7];
            inp[2] = outp3[7];
          }
          else if (x2 % 3 == 1)
          {
            inp[3] = outp1[7];
            inp[4] = outp2[7];
            inp[5] = outp3[7];
          }
          else
          {
            inp[6] = outp1[7];
            inp[7] = outp2[7];
            byte num4 = this.bool2byte(inp);
            bytes[(long) (y2 * (this.height / 3) + x2 / 3) - num1] = num4;
          }
        }
      }
      long num5 = (long) (this.height * (this.width / 3) * 3 / 3) - num1 - 1L;
      for (int index = 6; index >= 0 && num5 + (long) ((6 - index) * (this.height * (this.width / 3) * 3 / 3 - 1)) < length; --index)
      {
        for (int y3 = 0; y3 < this.height && (long) (y3 * (this.height / 3)) + num5 + (long) ((6 - index) * (this.height * (this.width / 3) * 3 / 3 - 1)) < length; ++y3)
        {
          for (int x3 = 0; x3 < this.width / 3 * 3 && (long) (y3 * (this.height / 3) + x3 / 3) + num5 + (long) ((6 - index) * (this.height * (this.width / 3) * 3 / 3 - 1)) < length; ++x3)
          {
            pixel3 = this.DecryptedBitmap.GetPixel(x3, y3);
            byte r = pixel3.R;
            byte g = pixel3.G;
            byte b = pixel3.B;
            this.byte2bool(r, ref outp1);
            this.byte2bool(g, ref outp2);
            this.byte2bool(b, ref outp3);
            if (x3 % 3 == 0)
            {
              inp[0] = outp1[index];
              inp[1] = outp2[index];
              inp[2] = outp3[index];
            }
            else if (x3 % 3 == 1)
            {
              inp[3] = outp1[index];
              inp[4] = outp2[index];
              inp[5] = outp3[index];
            }
            else
            {
              inp[6] = outp1[index];
              inp[7] = outp2[index];
              byte num6 = this.bool2byte(inp);
              bytes[(long) (y3 * (this.height / 3) + x3 / 3 + (6 - index) * (this.height * (this.width / 3) * 3 / 3 - 1)) + num5] = num6;
            }
          }
        }
      }
      if (File.Exists(this.DSaveFilePath + "\\" + str))
      {
        int num7 = (int) MessageBox.Show("File \"" + str + "\" already exist please choose another path to save file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
      }
      else
      {
        File.WriteAllBytes(this.DSaveFilePath + "\\" + str, bytes);
        this.toolStripStatusLabel1.Text = "Decrypted file has been successfully saved.";
        Application.DoEvents();
      }
    }

    private void byte2bool(byte inp, ref bool[] outp)
    {
      if (inp < (byte) 0 || inp > byte.MaxValue)
        throw new Exception("Input number is illegal.");
      for (short index = 7; index >= (short) 0; --index)
      {
        outp[(int) index] = (int) inp % 2 == 1;
        inp /= (byte) 2;
      }
    }

    private byte bool2byte(bool[] inp)
    {
      byte num = 0;
      for (short index = 7; index >= (short) 0; --index)
      {
        if (inp[(int) index])
          num += (byte) Math.Pow(2.0, (double) (7 - (int) index));
      }
      return num;
    }

    private void Decrypt_btn_Click(object sender, EventArgs e)
    {
      if (this.DeSaveFile_tbx.Text == string.Empty || this.DeLoadImage_tbx.Text == string.Empty)
      {
        int num1 = (int) MessageBox.Show("Text boxes must not be empty!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
      }
      else if (!File.Exists(this.DeLoadImage_tbx.Text))
      {
        int num2 = (int) MessageBox.Show("Select image file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        this.DeLoadImage_tbx.Focus();
      }
      else
        this.DecryptLayer();
    }

    private void DeLoadImageBrowse_btn_Click(object sender, EventArgs e)
    {
      if (this.openFileDialog3.ShowDialog() != DialogResult.OK)
        return;
      this.DLoadImagePath = this.openFileDialog3.FileName;
      this.DeLoadImage_tbx.Text = this.DLoadImagePath;
      this.DecryptedImage = Image.FromFile(this.DLoadImagePath);
      this.height = this.DecryptedImage.Height;
      this.width = this.DecryptedImage.Width;
      this.DecryptedBitmap = new Bitmap(this.DecryptedImage);
      this.ImageSize_lbl.Text = this.smalldecimal(((float) new FileInfo(this.DLoadImagePath).Length / 1024f).ToString(), 2) + " KB";
      this.ImageHeight_lbl.Text = this.DecryptedImage.Height.ToString() + " Pixel";
      this.ImageWidth_lbl.Text = this.DecryptedImage.Width.ToString() + " Pixel";
      this.CanSave_lbl.Text = this.smalldecimal((8.0 * (double) (this.height * (this.width / 3) * 3 / 3 - 1) / 1024.0).ToString(), 2) + " KB";
      this.canPaint = true;
      this.Invalidate();
    }

    private void DeSaveFileBrowse_btn_Click(object sender, EventArgs e)
    {
      if (this.folderBrowserDialog1.ShowDialog() != DialogResult.OK)
        return;
      this.DSaveFilePath = this.folderBrowserDialog1.SelectedPath;
      this.DeSaveFile_tbx.Text = this.DSaveFilePath;
    }

    private void Form1_Paint(object sender, PaintEventArgs e)
    {
      if (!this.canPaint)
        return;
      try
      {
        if (!this.EncriptionDone)
          e.Graphics.DrawImage(this.loadedTrueImage, this.previewImage);
        else
          e.Graphics.DrawImage(this.AfterEncryption, this.previewImage);
      }
      catch
      {
        e.Graphics.DrawImage(this.DecryptedImage, this.previewImage);
      }
    }

    private string justFName(string path)
    {
      if (path.Length == 3)
        return path.Substring(0, 1);
      int index = path.Length - 1;
      while (index > 0 && path[index] != '\\')
        --index;
      return path.Substring(index + 1);
    }

    private string justEx(string fName)
    {
      int index = fName.Length - 1;
      while (index > 0 && fName[index] != '.')
        --index;
      return fName.Substring(index + 1);
    }

    private void Close_btn_Click(object sender, EventArgs e) => this.Close();

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => Process.Start("http:\\\\www.programmer2programmer.net");

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.groupBox1 = new GroupBox();
      this.label5 = new Label();
      this.Decrypt_btn = new Button();
      this.DeLoadImage_tbx = new TextBox();
      this.DeSaveFile_tbx = new TextBox();
      this.DeSaveFileBrowse_btn = new Button();
      this.label6 = new Label();
      this.DeLoadImageBrowse_btn = new Button();
      this.groupBox2 = new GroupBox();
      this.Encrypt_btn = new Button();
      this.EnFileBrowse_btn = new Button();
      this.EnImageBrowse_btn = new Button();
      this.label2 = new Label();
      this.EnFile_tbx = new TextBox();
      this.label1 = new Label();
      this.EnImage_tbx = new TextBox();
      this.openFileDialog1 = new OpenFileDialog();
      this.openFileDialog2 = new OpenFileDialog();
      this.saveFileDialog1 = new SaveFileDialog();
      this.openFileDialog3 = new OpenFileDialog();
      this.label3 = new Label();
      this.groupBox3 = new GroupBox();
      this.ByteCapacity_lbl = new Label();
      this.CanSave_lbl = new Label();
      this.ImageWidth_lbl = new Label();
      this.ImageHeight_lbl = new Label();
      this.ImageSize_lbl = new Label();
      this.label9 = new Label();
      this.label10 = new Label();
      this.label8 = new Label();
      this.label7 = new Label();
      this.folderBrowserDialog1 = new FolderBrowserDialog();
      this.Close_btn = new Button();
      this.statusStrip1 = new StatusStrip();
      this.toolStripStatusLabel1 = new ToolStripStatusLabel();
      this.tabControl1 = new TabControl();
      this.tabPage1 = new TabPage();
      this.tabPage2 = new TabPage();
      this.linkLabel1 = new LinkLabel();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.statusStrip1.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tabPage2.SuspendLayout();
      this.SuspendLayout();
      this.groupBox1.Controls.Add((Control) this.label5);
      this.groupBox1.Controls.Add((Control) this.Decrypt_btn);
      this.groupBox1.Controls.Add((Control) this.DeLoadImage_tbx);
      this.groupBox1.Controls.Add((Control) this.DeSaveFile_tbx);
      this.groupBox1.Controls.Add((Control) this.DeSaveFileBrowse_btn);
      this.groupBox1.Controls.Add((Control) this.label6);
      this.groupBox1.Controls.Add((Control) this.DeLoadImageBrowse_btn);
      this.groupBox1.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.groupBox1.Location = new Point(60, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new Size(320, 111);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.label5.AutoSize = true;
      this.label5.Font = new Font("Arial", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.label5.Location = new Point(7, 22);
      this.label5.Name = "label5";
      this.label5.Size = new Size(65, 14);
      this.label5.TabIndex = 1;
      this.label5.Text = "Load image:";
      this.Decrypt_btn.Font = new Font("Arial", 8.25f, FontStyle.Bold, GraphicsUnit.Point, (byte) 0);
      this.Decrypt_btn.Location = new Point(123, 82);
      this.Decrypt_btn.Name = "Decrypt_btn";
      this.Decrypt_btn.Size = new Size(75, 23);
      this.Decrypt_btn.TabIndex = 3;
      this.Decrypt_btn.Text = "Decrypt";
      this.Decrypt_btn.UseVisualStyleBackColor = true;
      this.Decrypt_btn.Click += new EventHandler(this.Decrypt_btn_Click);
      this.DeLoadImage_tbx.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.DeLoadImage_tbx.Location = new Point(78, 19);
      this.DeLoadImage_tbx.Name = "DeLoadImage_tbx";
      this.DeLoadImage_tbx.Size = new Size(155, 20);
      this.DeLoadImage_tbx.TabIndex = 0;
      this.DeSaveFile_tbx.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.DeSaveFile_tbx.Location = new Point(78, 54);
      this.DeSaveFile_tbx.Name = "DeSaveFile_tbx";
      this.DeSaveFile_tbx.Size = new Size(155, 20);
      this.DeSaveFile_tbx.TabIndex = 2;
      this.DeSaveFileBrowse_btn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.DeSaveFileBrowse_btn.Font = new Font("Arial", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.DeSaveFileBrowse_btn.Location = new Point(239, 52);
      this.DeSaveFileBrowse_btn.Name = "DeSaveFileBrowse_btn";
      this.DeSaveFileBrowse_btn.Size = new Size(75, 23);
      this.DeSaveFileBrowse_btn.TabIndex = 3;
      this.DeSaveFileBrowse_btn.Text = "Browse";
      this.DeSaveFileBrowse_btn.UseVisualStyleBackColor = true;
      this.DeSaveFileBrowse_btn.Click += new EventHandler(this.DeSaveFileBrowse_btn_Click);
      this.label6.AutoSize = true;
      this.label6.Font = new Font("Arial", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.label6.Location = new Point(7, 57);
      this.label6.Name = "label6";
      this.label6.Size = new Size(64, 14);
      this.label6.TabIndex = 1;
      this.label6.Text = "Save file to:";
      this.DeLoadImageBrowse_btn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.DeLoadImageBrowse_btn.Font = new Font("Arial", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.DeLoadImageBrowse_btn.Location = new Point(239, 17);
      this.DeLoadImageBrowse_btn.Name = "DeLoadImageBrowse_btn";
      this.DeLoadImageBrowse_btn.Size = new Size(75, 23);
      this.DeLoadImageBrowse_btn.TabIndex = 1;
      this.DeLoadImageBrowse_btn.Text = "Browse";
      this.DeLoadImageBrowse_btn.UseVisualStyleBackColor = true;
      this.DeLoadImageBrowse_btn.Click += new EventHandler(this.DeLoadImageBrowse_btn_Click);
      this.groupBox2.Controls.Add((Control) this.Encrypt_btn);
      this.groupBox2.Controls.Add((Control) this.EnFileBrowse_btn);
      this.groupBox2.Controls.Add((Control) this.EnImageBrowse_btn);
      this.groupBox2.Controls.Add((Control) this.label2);
      this.groupBox2.Controls.Add((Control) this.EnFile_tbx);
      this.groupBox2.Controls.Add((Control) this.label1);
      this.groupBox2.Controls.Add((Control) this.EnImage_tbx);
      this.groupBox2.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.groupBox2.Location = new Point(60, 0);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new Size(320, 111);
      this.groupBox2.TabIndex = 0;
      this.groupBox2.TabStop = false;
      this.Encrypt_btn.Font = new Font("Arial", 8.25f, FontStyle.Bold, GraphicsUnit.Point, (byte) 0);
      this.Encrypt_btn.Location = new Point(123, 83);
      this.Encrypt_btn.Name = "Encrypt_btn";
      this.Encrypt_btn.Size = new Size(75, 23);
      this.Encrypt_btn.TabIndex = 3;
      this.Encrypt_btn.Text = "Encrypt";
      this.Encrypt_btn.UseVisualStyleBackColor = true;
      this.Encrypt_btn.Click += new EventHandler(this.Encrypt_btn_Click);
      this.EnFileBrowse_btn.Font = new Font("Arial", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.EnFileBrowse_btn.Location = new Point(239, 52);
      this.EnFileBrowse_btn.Name = "EnFileBrowse_btn";
      this.EnFileBrowse_btn.Size = new Size(75, 23);
      this.EnFileBrowse_btn.TabIndex = 3;
      this.EnFileBrowse_btn.Text = "Browse";
      this.EnFileBrowse_btn.UseVisualStyleBackColor = true;
      this.EnFileBrowse_btn.Click += new EventHandler(this.EnFileBrowse_btn_Click);
      this.EnImageBrowse_btn.Font = new Font("Arial", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.EnImageBrowse_btn.Location = new Point(239, 17);
      this.EnImageBrowse_btn.Name = "EnImageBrowse_btn";
      this.EnImageBrowse_btn.Size = new Size(75, 23);
      this.EnImageBrowse_btn.TabIndex = 1;
      this.EnImageBrowse_btn.Text = "Browse";
      this.EnImageBrowse_btn.UseVisualStyleBackColor = true;
      this.EnImageBrowse_btn.Click += new EventHandler(this.EnImageBrowse_btn_Click);
      this.label2.AutoSize = true;
      this.label2.Font = new Font("Arial", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.label2.Location = new Point(6, 57);
      this.label2.Name = "label2";
      this.label2.Size = new Size(51, 14);
      this.label2.TabIndex = 1;
      this.label2.Text = "Load file:";
      this.EnFile_tbx.Location = new Point(77, 54);
      this.EnFile_tbx.Name = "EnFile_tbx";
      this.EnFile_tbx.Size = new Size(156, 20);
      this.EnFile_tbx.TabIndex = 2;
      this.label1.AutoSize = true;
      this.label1.Font = new Font("Arial", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.label1.Location = new Point(6, 22);
      this.label1.Name = "label1";
      this.label1.Size = new Size(65, 14);
      this.label1.TabIndex = 1;
      this.label1.Text = "Load image:";
      this.EnImage_tbx.Location = new Point(77, 20);
      this.EnImage_tbx.Name = "EnImage_tbx";
      this.EnImage_tbx.Size = new Size(156, 20);
      this.EnImage_tbx.TabIndex = 0;
      this.openFileDialog1.Filter = "Bitmap Files (*.bmp)|*.bmp|All files(*.*)|*.*";
      this.openFileDialog2.Filter = "All files (*.*)|*.*";
      this.saveFileDialog1.Filter = "Bitmap Files (*.bmp)|*.bmp";
      this.openFileDialog3.Filter = "Bitmap Files (*.bmp)|*.bmp";
      this.label3.AutoSize = true;
      this.label3.Font = new Font("Arial", 8.25f, FontStyle.Bold | FontStyle.Underline, GraphicsUnit.Point, (byte) 0);
      this.label3.ForeColor = Color.Black;
      this.label3.Location = new Point(8, 145);
      this.label3.Name = "label3";
      this.label3.Size = new Size(92, 14);
      this.label3.TabIndex = 1;
      this.label3.Text = "Image preview:";
      this.groupBox3.BackColor = Color.FromArgb(233, 240, 221);
      this.groupBox3.Controls.Add((Control) this.ByteCapacity_lbl);
      this.groupBox3.Controls.Add((Control) this.CanSave_lbl);
      this.groupBox3.Controls.Add((Control) this.ImageWidth_lbl);
      this.groupBox3.Controls.Add((Control) this.ImageHeight_lbl);
      this.groupBox3.Controls.Add((Control) this.ImageSize_lbl);
      this.groupBox3.Controls.Add((Control) this.label9);
      this.groupBox3.Controls.Add((Control) this.label10);
      this.groupBox3.Controls.Add((Control) this.label8);
      this.groupBox3.Controls.Add((Control) this.label7);
      this.groupBox3.FlatStyle = FlatStyle.Flat;
      this.groupBox3.Font = new Font("Arial", 8.25f, FontStyle.Bold, GraphicsUnit.Point, (byte) 0);
      this.groupBox3.ForeColor = Color.Black;
      this.groupBox3.Location = new Point(461, 22);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new Size(144, 122);
      this.groupBox3.TabIndex = 2;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Image information";
      this.ByteCapacity_lbl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.ByteCapacity_lbl.AutoSize = true;
      this.ByteCapacity_lbl.Location = new Point(88, 47);
      this.ByteCapacity_lbl.Name = "ByteCapacity_lbl";
      this.ByteCapacity_lbl.Size = new Size(0, 14);
      this.ByteCapacity_lbl.TabIndex = 2;
      this.CanSave_lbl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.CanSave_lbl.AutoSize = true;
      this.CanSave_lbl.Location = new Point(77, 97);
      this.CanSave_lbl.Name = "CanSave_lbl";
      this.CanSave_lbl.Size = new Size(35, 14);
      this.CanSave_lbl.TabIndex = 1;
      this.CanSave_lbl.Text = "none";
      this.ImageWidth_lbl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.ImageWidth_lbl.AutoSize = true;
      this.ImageWidth_lbl.Location = new Point(77, 72);
      this.ImageWidth_lbl.Name = "ImageWidth_lbl";
      this.ImageWidth_lbl.Size = new Size(35, 14);
      this.ImageWidth_lbl.TabIndex = 1;
      this.ImageWidth_lbl.Text = "none";
      this.ImageHeight_lbl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.ImageHeight_lbl.AutoSize = true;
      this.ImageHeight_lbl.Location = new Point(77, 47);
      this.ImageHeight_lbl.Name = "ImageHeight_lbl";
      this.ImageHeight_lbl.Size = new Size(35, 14);
      this.ImageHeight_lbl.TabIndex = 1;
      this.ImageHeight_lbl.Text = "none";
      this.ImageSize_lbl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.ImageSize_lbl.AutoSize = true;
      this.ImageSize_lbl.Location = new Point(77, 22);
      this.ImageSize_lbl.Name = "ImageSize_lbl";
      this.ImageSize_lbl.Size = new Size(35, 14);
      this.ImageSize_lbl.TabIndex = 1;
      this.ImageSize_lbl.Text = "none";
      this.label9.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.label9.AutoSize = true;
      this.label9.Location = new Point(13, 97);
      this.label9.Name = "label9";
      this.label9.Size = new Size(63, 14);
      this.label9.TabIndex = 0;
      this.label9.Text = "Can save: ";
      this.label10.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.label10.AutoSize = true;
      this.label10.Location = new Point(13, 72);
      this.label10.Name = "label10";
      this.label10.Size = new Size(44, 14);
      this.label10.TabIndex = 0;
      this.label10.Text = "Width: ";
      this.label8.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.label8.AutoSize = true;
      this.label8.Location = new Point(13, 47);
      this.label8.Name = "label8";
      this.label8.Size = new Size(48, 14);
      this.label8.TabIndex = 0;
      this.label8.Text = "Height: ";
      this.label7.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.label7.AutoSize = true;
      this.label7.Location = new Point(13, 22);
      this.label7.Name = "label7";
      this.label7.Size = new Size(36, 14);
      this.label7.TabIndex = 0;
      this.label7.Text = "Size: ";
      this.Close_btn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      this.Close_btn.DialogResult = DialogResult.Cancel;
      this.Close_btn.Location = new Point(527, 604);
      this.Close_btn.Name = "Close_btn";
      this.Close_btn.Size = new Size(75, 23);
      this.Close_btn.TabIndex = 3;
      this.Close_btn.Text = "Close";
      this.Close_btn.UseVisualStyleBackColor = true;
      this.Close_btn.Visible = false;
      this.Close_btn.Click += new EventHandler(this.Close_btn_Click);
      this.statusStrip1.BackColor = Color.FromArgb(221, 240, 192);
      this.statusStrip1.Items.AddRange(new ToolStripItem[1]
      {
        (ToolStripItem) this.toolStripStatusLabel1
      });
      this.statusStrip1.Location = new Point(0, 630);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Size = new Size(614, 22);
      this.statusStrip1.SizingGrip = false;
      this.statusStrip1.TabIndex = 4;
      this.statusStrip1.Text = "statusStrip1";
      this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
      this.toolStripStatusLabel1.Size = new Size(38, 17);
      this.toolStripStatusLabel1.Text = "Ready";
      this.tabControl1.Controls.Add((Control) this.tabPage1);
      this.tabControl1.Controls.Add((Control) this.tabPage2);
      this.tabControl1.Font = new Font("Arial", 8.25f, FontStyle.Bold, GraphicsUnit.Point, (byte) 0);
      this.tabControl1.ImeMode = ImeMode.NoControl;
      this.tabControl1.Location = new Point(5, 0);
      this.tabControl1.Multiline = true;
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new Size(449, 145);
      this.tabControl1.TabIndex = 5;
      this.tabPage1.BackColor = Color.FromArgb(233, 240, 221);
      this.tabPage1.Controls.Add((Control) this.groupBox2);
      this.tabPage1.Location = new Point(4, 23);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new Padding(3);
      this.tabPage1.Size = new Size(441, 118);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Encrypt Image";
      this.tabPage2.BackColor = Color.FromArgb(233, 240, 221);
      this.tabPage2.Controls.Add((Control) this.groupBox1);
      this.tabPage2.Location = new Point(4, 23);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new Padding(3);
      this.tabPage2.Size = new Size(441, 118);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Decrypt Image";
      this.linkLabel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
      this.linkLabel1.AutoSize = true;
      this.linkLabel1.BackColor = Color.FromArgb(221, 240, 192);
      this.linkLabel1.Location = new Point(446, 634);
      this.linkLabel1.Name = "linkLabel1";
      this.linkLabel1.Size = new Size(168, 13);
      this.linkLabel1.TabIndex = 6;
      this.linkLabel1.TabStop = true;
      this.linkLabel1.Text = "Made by Gaurav and Himanshi";
      this.linkLabel1.TextAlign = ContentAlignment.BottomRight;
      this.linkLabel1.LinkClicked += new LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.BackColor = Color.FromArgb(210, 218, 196);
      this.CancelButton = (IButtonControl) this.Close_btn;
      this.ClientSize = new Size(614, 652);
      this.Controls.Add((Control) this.linkLabel1);
      this.Controls.Add((Control) this.tabControl1);
      this.Controls.Add((Control) this.statusStrip1);
      this.Controls.Add((Control) this.Close_btn);
      this.Controls.Add((Control) this.groupBox3);
      this.Controls.Add((Control) this.label3);
      this.FormBorderStyle = FormBorderStyle.FixedSingle;
      this.MaximizeBox = false;
      this.Name = nameof (FrmSteganography);
      this.StartPosition = FormStartPosition.CenterScreen;
      this.Text = "Secure Data Hider";
      this.Paint += new PaintEventHandler(this.Form1_Paint);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage2.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
