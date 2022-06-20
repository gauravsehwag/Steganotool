// Decompiled with JetBrains decompiler
// Type: Text2Image.Program
// Assembly: Steganography, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 30212C39-DC58-4837-BD01-43DC3D8B9157
// Assembly location: C:\Users\gaura\Desktop\Steganography_Source\bin\Debug\Steganography.exe

using System;
using System.Windows.Forms;

namespace Text2Image
{
  internal static class Program
  {
    [STAThread]
    private static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run((Form) new FrmSteganography());
    }
  }
}
