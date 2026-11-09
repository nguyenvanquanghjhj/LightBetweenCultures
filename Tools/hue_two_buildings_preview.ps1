# Preview only. Never writes Assets or production bindings.
param([string]$Output='D:\GAMEKKK\LightBetweenCultures\Validation\HueTwoBuildings-20261021')
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
public static class HueLocalPreview {
 static PointF[] Points(float[] xy){var p=new PointF[xy.Length/2];for(int i=0;i<p.Length;i++)p[i]=new PointF(xy[i*2],xy[i*2+1]);return p;}
 static void Shape(Graphics g,PointF[] p,float expand){using(var pen=new Pen(Color.White,expand)){pen.LineJoin=LineJoin.Round;g.DrawPolygon(pen,p);}g.FillPolygon(Brushes.White,p);}
 static Bitmap Layer(Bitmap input,int w,int h,RectangleF rect){var b=new Bitmap(w,h,PixelFormat.Format32bppArgb);using(var g=Graphics.FromImage(b)){g.InterpolationMode=InterpolationMode.HighQualityBicubic;g.PixelOffsetMode=PixelOffsetMode.HighQuality;g.DrawImage(input,rect);}return b;}
 static void Crop(Bitmap img,string path,Rectangle source){using(var b=new Bitmap(source.Width*3,source.Height*3)){using(var g=Graphics.FromImage(b)){g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;g.DrawImage(img,new Rectangle(0,0,b.Width,b.Height),source,GraphicsUnit.Pixel);}b.Save(path,ImageFormat.Png);}}
 public static void Run(string folder){
  using(var original=new Bitmap(Path.Combine(folder,"Hue-before.png")))
  using(var approved=new Bitmap(Path.Combine(folder,"generated-patch-A.png")))
  using(var cleared=new Bitmap(Path.Combine(folder,"B-cleared-local.png")))
  using(var final=new Bitmap(original))
  using(var maskA=new Bitmap(original.Width,original.Height,PixelFormat.Format32bppArgb))
  using(var maskB=new Bitmap(original.Width,original.Height,PixelFormat.Format32bppArgb))
  using(var layerA=Layer(approved,original.Width,original.Height,new RectangleF(51,670,202.8f,310.2f)))
  using(var layerB=Layer(cleared,original.Width,original.Height,new RectangleF(1215,110,190,210))){
   // The approved mall is shorter than the removed pagoda. Fill only the old
   // upper silhouette with nearby matching foliage; do not paste a foreign
   // staircase/roof from the generated context above the glass tower.
   using(var g=Graphics.FromImage(layerA)){
    g.InterpolationMode=InterpolationMode.HighQualityBicubic;
    g.DrawImage(original,new Rectangle(108,696,52,58),new Rectangle(56,740,40,50),GraphicsUnit.Pixel);
   }
   using(var g=Graphics.FromImage(maskA)){
    g.SmoothingMode=SmoothingMode.AntiAlias;
    // Original tower silhouette, not the whole surrounding rectangular crop.
    Shape(g,Points(new float[]{126,698,130,708,138,709,143,720,147,721,152,725,150,733,153,751,155,754,157,763,155,775,159,785,159,805,163,815,166,841,165,854,177,866,174,882,145,887,123,882,105,880,95,872,91,848,94,821,98,811,100,788,104,770,108,750,111,728,118,715}),4);
    // Exact approved mall silhouette transformed to the old tower's landing.
    var insert=Points(new float[]{377,420,494,420,527,461,529,701,632,741,658,777,669,903,635,947,575,985,511,1022,425,1048,333,1027,231,980,146,925,107,880,114,768,141,744,208,710,265,705,305,689,304,502,349,480,378,474});
    for(int i=0;i<insert.Length;i++)insert[i]=new PointF(51+insert[i].X*.2f,670+insert[i].Y*.2f);
    Shape(g,insert,3);
   }
   using(var g=Graphics.FromImage(maskB)){
    g.SmoothingMode=SmoothingMode.AntiAlias;
    Shape(g,Points(new float[]{1304,129,1308,142,1317,146,1318,152,1323,162,1324,180,1329,190,1328,208,1334,220,1337,240,1342,248,1346,261,1342,277,1326,285,1301,281,1283,274,1275,265,1280,252,1283,244,1282,229,1285,216,1287,202,1288,187,1292,179,1290,162,1295,150,1298,142}),5);
   }
   long changed=0,outside=0,aCount=0,bCount=0;
   using(var diff=new Bitmap(original.Width,original.Height,PixelFormat.Format32bppArgb)){
    for(int y=0;y<original.Height;y++)for(int x=0;x<original.Width;x++){
     var old=original.GetPixel(x,y);int a=maskA.GetPixel(x,y).A,b=maskB.GetPixel(x,y).A;
     if(a>0)aCount++;if(b>0)bCount++;
     if(a>0||b>0){var source=a>0?layerA.GetPixel(x,y):layerB.GetPixel(x,y);float t=(a>0?a:b)/255f;
      final.SetPixel(x,y,Color.FromArgb(old.A,(int)Math.Round(old.R*(1-t)+source.R*t),(int)Math.Round(old.G*(1-t)+source.G*t),(int)Math.Round(old.B*(1-t)+source.B*t)));}
     if(final.GetPixel(x,y).ToArgb()!=old.ToArgb()){changed++;if(a==0&&b==0)outside++;diff.SetPixel(x,y,Color.FromArgb(255,255,64,32));}
    }
    final.Save(Path.Combine(folder,"Hue-after-preview.png"),ImageFormat.Png);
    maskA.Save(Path.Combine(folder,"allowed-mask-A.png"),ImageFormat.Png);maskB.Save(Path.Combine(folder,"allowed-mask-B.png"),ImageFormat.Png);
    diff.Save(Path.Combine(folder,"pixel-difference.png"),ImageFormat.Png);
   }
   Crop(final,Path.Combine(folder,"A-Vincom-preview.png"),new Rectangle(50,650,230,330));
   Crop(final,Path.Combine(folder,"B-cleared-preview.png"),new Rectangle(1215,110,190,210));
   File.WriteAllText(Path.Combine(folder,"pixel-validation.json"),"{\"width\":"+original.Width+",\"height\":"+original.Height+",\"changedPixels\":"+changed+",\"maskAPixels\":"+aCount+",\"maskBPixels\":"+bCount+",\"changedOutsideAllowedMasks\":"+outside+",\"runtimeModified\":false,\"status\":\"PREVIEW_REQUIRES_VISUAL_REVIEW\"}");
   if(outside!=0)throw new Exception("Pixels changed outside permitted masks");
  }
 }
}
'@
[HueLocalPreview]::Run($Output)
Get-Content -LiteralPath (Join-Path $Output 'pixel-validation.json')
