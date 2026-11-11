using System;
using ArchLight;
class RecoveryTests {
 static GammaController.RAMP Ramp(int step) {var r=new GammaController.RAMP{Red=new ushort[256],Green=new ushort[256],Blue=new ushort[256]};for(int i=0;i<256;i++)r.Red[i]=r.Green[i]=r.Blue[i]=(ushort)(i*step);return r;}
 static void Check(bool b,string name){if(!b)throw new Exception(name);Console.WriteLine("PASS "+name);}
 static void Main(){var desired=Ramp(180);var actual=Ramp(256);int writes=0;DateTime now=DateTime.UtcNow;
 GammaController.SetExpected(desired);
 Check(GammaController.Recover(()=>actual,r=>{writes++;actual=r;return true;},now),"overwritten ramp restored and read back");
 Check(writes==1,"exactly one recovery write");
 Check(!GammaController.Recover(()=>actual,r=>{writes++;return true;},now.AddSeconds(1)) && writes==1,"unchanged ramp is not rewritten");
 GammaController.SetExpected(desired);actual=Ramp(256);writes=0;
 Check(!GammaController.Recover(()=>actual,r=>{writes++;return false;},now),"driver rejection handled");
 GammaController.Recover(()=>actual,r=>{writes++;return false;},now.AddMilliseconds(500));Check(writes==1,"failed write backs off");
 Check(GammaController.Recover(()=>actual,r=>{actual=r;writes++;return true;},now.AddSeconds(2)),"recovery resumes after failure");
 GammaController.SetExpected(desired);writes=0;Check(!GammaController.Recover(()=>null,r=>{writes++;return true;},now)&&writes==0,"unavailable display is never blindly overwritten");
 GammaController.SetExpected(desired);actual=Ramp(256);Check(!GammaController.Recover(()=>actual,r=>true,now),"silent driver rejection detected by readback");
 var rounded=Ramp(180);rounded.Blue[255]+=255;Check(GammaController.RampsMatch(desired,rounded),"8-bit driver quantization tolerated");
 rounded.Blue[255]+=2;Check(!GammaController.RampsMatch(desired,rounded),"material mismatch detected");
 GammaController.ExpectedRamp=null;writes=0;Check(!GammaController.Recover(()=>actual,r=>{writes++;return true;},now)&&writes==0,"no target means no writes");
 var calibration=Ramp(230);GammaController.SaveOriginal(()=>calibration);GammaController.SetExpected(desired);
 GammaController.RAMP restored=default(GammaController.RAMP);writes=0;
 Check(GammaController.RestoreDefault(r=>{writes++;restored=r;return true;}),"restore succeeds with mocked display");
 Check(writes==1 && GammaController.RampsMatch(restored,calibration),"exact saved calibration restored instead of approximate 6500K");
 Check(!GammaController.ExpectedRamp.HasValue,"disabled filter clears recovery target");
 Check(!GammaController.Recover(()=>actual,r=>{writes++;return true;},now.AddMinutes(1))&&writes==1,"recovery cannot reapply disabled filter");
 GammaController.RestoreDefault(r=>{writes++;return true;});Check(writes==1,"repeated disposal does not overwrite display again");
 GammaController.SetExpected(desired);Check(!GammaController.RestoreDefault(r=>false)&&!GammaController.ExpectedRamp.HasValue,"rejected restore still stops active recovery");
 Console.WriteLine("17 tests passed");
 }
}
