﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Collections;
using Tesseract;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
namespace TesseractForNetDemo
{
  public partial class Main : Form
  {
    private string strSelectImgFile = "";
    private Bitmap bmpNew=null;
    private Bitmap bmpScan = null;
    public Main()
    {
      InitializeComponent();
    }

    private void btn_1_Click(object sender, EventArgs e)
    {
      OpenFileDialog openFileDialog1 = new OpenFileDialog();
      openFileDialog1.InitialDirectory = Application.StartupPath;
      openFileDialog1.Filter = "TIF files(*.tif)|*.tif|PNG files(*.png)|*.png";
      openFileDialog1.RestoreDirectory = true;
      if (openFileDialog1.ShowDialog() == DialogResult.OK)
      {
        strSelectImgFile = openFileDialog1.FileName;
        RemoveWhite(strSelectImgFile);
        pictureBox1.Image = bmpNew;
        pictureBox1.Size = pictureBox1.Image.Size;
        //Bitmap img = new Bitmap(openFileDialog1.FileName);
        //img.MakeTransparent(Color.White);
        //pictureBox1.Image = img;
      }
    }

    private void btn_2_Click(object sender, EventArgs e)
    {
      if (strSelectImgFile.Length < 1) return;
      try
      {
        using (var engine = new TesseractEngine(@"./tessdata", "normal", EngineMode.Default))
        {
          using (var img = Pix.LoadFromFile(strSelectImgFile))
          {
            using (var page = engine.Process(img))
            {
              string text = page.GetText();
              ArrayList strlist = splittext(text);                
              ArrayList strlistNew = new ArrayList();
              for (int I = 0; I < strlist.Count; I++)
              {
                  string strTmp = strlist[I].ToString().Substring(strlist[I].ToString().IndexOf(":") + 1);
                  strlistNew.Add(strTmp);
              }
              cbo_1.Text = strlistNew[2].ToString();
              dtp_2.Text = strlistNew[3].ToString();
              txt_3.Text = strlistNew[18].ToString();
              cbo_4.Text = strlistNew[4].ToString();
              cbo_5.Text = strlistNew[5].ToString();
              cbo_6.Text = strlistNew[19].ToString();
              cbo_7.Text = strlistNew[6].ToString();
              cbo_8.Text = strlistNew[17].ToString();
              //dtp_9.Text = strlistNew[20].ToString();
              cbo_10.Text = strlistNew[7].ToString();
              cbo_11.Text = strlistNew[8].ToString();
              cbo_12.Text = strlistNew[22].ToString();
              cbo_13.Text = strlistNew[9].ToString();
              cbo_14.Text = strlistNew[10].ToString();
            }
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    private ArrayList splittext(string text)
    {
      ArrayList allstr = new ArrayList();
      string[] strnn = Regex.Split(text, "\n\n", RegexOptions.IgnoreCase);
      foreach (string strs in strnn)
      {
        string temp = strs.Replace(": ", ":").Replace(" :",":").Trim();
        string[] strn = Regex.Split(temp, "\n", RegexOptions.IgnoreCase);
        if (strn.Length > 1)
        {
          foreach (string str in strn)
          {
            Regex rgx = new Regex(@"[\u4e00-\u9fa5]*[:]\S*", RegexOptions.IgnoreCase);
            MatchCollection mc = rgx.Matches(str);
            if (mc.Count>0)
            {
              foreach (Match ma in mc)
              {
                if(ma.Success)
                {
                  allstr.Add(ma.Value.ToString());
                }
              }
            }
            else
            {
              allstr.Add(str);
            }
          }
        }
        else if (temp.Length > 2)
        {
          allstr.Add(temp);
        }
      }
      return allstr;
    }

    private void Main_Load(object sender, EventArgs e)
    {
      // TODO: 这行代码将数据加载到表“db_Simple_Imgr2DataSet.Imgr2”中。您可以根据需要移动或删除它。
        this.pictureBox1.MouseWheel += new MouseEventHandler(pictureBox1_MouseWheel);
        this.pictureBox1.MouseEnter += new EventHandler(pictureBox1_MouseEnter);
      this.imgr2TableAdapter.Fill(this.db_Simple_Imgr2DataSet.Imgr2);

    }
    private void pictureBox1_MouseEnter(object sender, EventArgs e)
    {
        this.pictureBox1.Focus();
    }

    private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
    {
        double dbScale = 1;
        if (this.pictureBox1.Height > 0)
        {
            dbScale = (double)this.pictureBox1.Width / (double)this.pictureBox1.Height;
        }
        pictureBox1.Width += (int)(e.Delta * dbScale);
        pictureBox1.Height += e.Delta;
    }

    private void RemoveWhite(string strFileName)
    {
        bmpScan = new Bitmap(Image.FromFile(strFileName), Image.FromFile(strFileName).Size);
        bmpNew = bmpScan;
        BitmapData data = bmpNew.LockBits(new Rectangle(0, 0, bmpNew.Width, bmpNew.Height), ImageLockMode.ReadWrite, bmpNew.PixelFormat);
        int length = data.Stride * data.Height;
        IntPtr ptr = data.Scan0;
        byte[] buff = new byte[length];
        Marshal.Copy(ptr, buff, 0, length);
        int intRemove = 0;
        for (int i = length - 1; i > 2; i -= 4)
        {
            if (i >= 3)
            {
                if (buff[i - 1] >= 230 && buff[i - 2] >= 230 && buff[i - 3] >= 230)
                {
                    buff[i] = 0;
                    if ((i / data.Stride) > 500)  //用于预防图片顶部为白色。   
                    {
                        intRemove = i;
                    }
                }               
            }
        }
        Marshal.Copy(buff, 0, ptr, length);
        bmpNew.UnlockBits(data);
        int intHeight=(int)(intRemove / data.Stride);
        bmpNew = GetPicThumbnail(bmpNew, (int)((panel2.Width - 30) * (double)((double)intHeight / (double)bmpNew.Width)), (panel2.Width - 30),  System.Drawing.Imaging.PixelFormat.Format32bppArgb, intHeight);
    }

    public Bitmap GetPicThumbnail(Bitmap iSource, int dHeight, int dWidth, System.Drawing.Imaging.PixelFormat flag, int initHeight)
    {
        System.Drawing.Image initImage = iSource;

        //原图宽高均小于模版，不作处理，直接保存
        if (initImage.Width <= dWidth && initImage.Height <= dHeight)
        {
            return iSource;
        }
        else
        {
            //原始图片的宽 
            int initWidth = initImage.Width;
            //截图对象
                System.Drawing.Image pickedImage = null;
                System.Drawing.Graphics pickedG = null;

                pickedImage = new System.Drawing.Bitmap(initWidth, initHeight);
                pickedG = System.Drawing.Graphics.FromImage(pickedImage);
                //设置质量
                pickedG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                pickedG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                //定位
                Rectangle fromR = new Rectangle(0, 0, initWidth, initHeight);
                Rectangle toR = new Rectangle(0, 0, initWidth, initHeight);
                //画图
                pickedG.DrawImage(initImage, toR, fromR, System.Drawing.GraphicsUnit.Pixel);
           
                //将截图对象赋给原图
                initImage = (System.Drawing.Image)pickedImage.Clone();
                //释放截图资源
                pickedG.Dispose();
                pickedImage.Dispose();

            //缩略图对象
            System.Drawing.Image resultImage = new System.Drawing.Bitmap(dWidth, dHeight);
            System.Drawing.Graphics resultG = System.Drawing.Graphics.FromImage(resultImage);
            //设置质量
            resultG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            resultG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //用指定背景色清空画布
            resultG.Clear(Color.White);
            //绘制缩略图
            resultG.DrawImage(initImage, new System.Drawing.Rectangle(0, 0, dWidth, dHeight), new System.Drawing.Rectangle(0, 0, initWidth, initHeight), System.Drawing.GraphicsUnit.Pixel);
            //关键质量控制
            //获取系统编码类型数组,包含了jpeg,bmp,png,gif,tiff
            ImageCodecInfo[] icis = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo ici = null;
            foreach (ImageCodecInfo i in icis)
            {
                if (i.MimeType == "image/jpeg" || i.MimeType == "image/bmp" || i.MimeType == "image/png" || i.MimeType == "image/gif")
                {
                    ici = i;
                }
            }
            EncoderParameters ep = new EncoderParameters(1);
            ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)flag);
           return (Bitmap) resultImage ;
        }        
    }
  }
}
