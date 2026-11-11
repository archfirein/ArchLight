using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ArchLight
{
    internal static class Shapes
    {
        internal static GraphicsPath Round(Rectangle r, int radius)
        {
            int d = Math.Max(1, Math.Min(radius * 2, Math.Min(r.Width, r.Height)));
            GraphicsPath p = new GraphicsPath();
            p.AddArc(r.Left, r.Top, d, d, 180, 90); p.AddArc(r.Right-d, r.Top, d, d, 270, 90);
            p.AddArc(r.Right-d, r.Bottom-d, d, d, 0, 90); p.AddArc(r.Left, r.Bottom-d, d, d, 90, 90); p.CloseFigure(); return p;
        }
    }
    internal class RoundPanel : Panel
    {
        public RoundPanel() { DoubleBuffered = true; }
        protected override void OnPaintBackground(PaintEventArgs e) {e.Graphics.Clear(Parent==null?BackColor:Parent.BackColor);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;using(var p=Shapes.Round(ClientRectangle,18))using(var b=new SolidBrush(BackColor))e.Graphics.FillPath(b,p);}
        protected override void OnResize(EventArgs e) { base.OnResize(e); if(Width<1||Height<1)return; using(var p=Shapes.Round(ClientRectangle,18)){var old=Region;Region=new Region(p);if(old!=null)old.Dispose();} }
    }
    internal class ModernButton : Button
    {
        bool mouseFocus;
        protected bool KeyboardFocus { get { return Focused && ShowFocusCues && !mouseFocus; } }
        public ModernButton() { DoubleBuffered=true; }
        protected override void OnGotFocus(EventArgs e) { mouseFocus=false; base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }
        protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); mouseFocus=true; Invalidate(); }
        protected override void OnKeyDown(KeyEventArgs e) { mouseFocus=false; base.OnKeyDown(e); Invalidate(); }
        protected override void OnChangeUICues(UICuesEventArgs e) { base.OnChangeUICues(e); Invalidate(); }
        protected override void OnPaint(PaintEventArgs e) {e.Graphics.Clear(Parent.BackColor);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;using(var p=Shapes.Round(new Rectangle(0,0,Width-1,Height-1),10))using(var b=new SolidBrush(BackColor))e.Graphics.FillPath(b,p);TextRenderer.DrawText(e.Graphics,Text,Font,new Rectangle(6,2,Width-12,Height-4),ForeColor,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.WordBreak);if(KeyboardFocus)using(var focus=Shapes.Round(Rectangle.Inflate(ClientRectangle,-4,-4),7))using(var pen=new Pen(ForeColor,1))e.Graphics.DrawPath(pen,focus);}
        protected override void OnResize(EventArgs e) {base.OnResize(e);if(Width<1||Height<1)return;using(var p=Shapes.Round(ClientRectangle,10)){var old=Region;Region=new Region(p);if(old!=null)old.Dispose();}}
    }
    internal class ChoiceButton : ModernButton
    {
        readonly ComboBox source;
        readonly ContextMenuStrip menu = new ContextMenuStrip { ShowImageMargin = false };
        public Color Accent {get;set;}

        public ChoiceButton(ComboBox source) {this.source=source;AccessibleRole=AccessibleRole.ButtonDropDown;source.SelectedIndexChanged+=delegate{Invalidate();};menu.Opened+=delegate{Invalidate();};menu.Closed+=delegate{Invalidate();};}
        string Caption {get{string text=source.SelectedItem==null?"":source.SelectedItem.ToString();return text.Length>0&&char.IsSurrogate(text[0])&&text.IndexOf(' ')>=0?text.Substring(text.IndexOf(' ')+1):text;}}
        protected override void OnPaint(PaintEventArgs e)
        {
            var g=e.Graphics;g.Clear(Parent.BackColor);g.SmoothingMode=SmoothingMode.AntiAlias;
            using(var p=Shapes.Round(new Rectangle(1,1,Width-3,Height-3),11))using(var b=new SolidBrush(BackColor))using(var pen=new Pen(KeyboardFocus||menu.Visible?Accent:ControlPaint.Light(BackColor,0.15f),1)){g.FillPath(b,p);g.DrawPath(pen,p);}
            TextRenderer.DrawText(g,Caption,Font,new Rectangle(14,0,Width-42,Height),ForeColor,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
            using(var pen=new Pen(Accent,2)){g.DrawLines(pen,new[]{new Point(Width-25,Height/2-2),new Point(Width-20,Height/2+3),new Point(Width-15,Height/2-2)});}
        }
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            // ToolStrip closes before completing item activation. Keep the menu alive
            // through that event sequence and dispose it with its owning control.
            if(menu.Visible)menu.Close();
            while(menu.Items.Count>0){var old=menu.Items[0];menu.Items.RemoveAt(0);old.Dispose();}
            menu.BackColor=BackColor;menu.ForeColor=ForeColor;
            for(int i=0;i<source.Items.Count;i++){int index=i;string text=source.Items[i].ToString();if(text.Length>0&&char.IsSurrogate(text[0])&&text.IndexOf(' ')>=0)text=text.Substring(text.IndexOf(' ')+1);var item=new ToolStripMenuItem(text){Checked=i==source.SelectedIndex,BackColor=BackColor,ForeColor=ForeColor};item.Click+=delegate{source.SelectedIndex=index;};menu.Items.Add(item);}
            menu.Show(this,new Point(0,Height));
        }
        protected override void Dispose(bool disposing)
        {
            if(disposing)menu.Dispose();
            base.Dispose(disposing);
        }
    }
    internal class ModernSlider : Control
    {
        int minimum, maximum=100, value; bool dragging;
        public int Minimum {get{return minimum;}set{minimum=value;}}
        public int Maximum {get{return maximum;}set{maximum=value;}}
        public int SmallChange {get;set;} public int LargeChange {get;set;} public int TickFrequency {get;set;}
        public Color Accent {get;set;}
        public int Value {get{return value;}set{int next=Math.Max(Minimum,Math.Min(Maximum,value));if(this.value==next)return;this.value=next;Invalidate();if(ValueChanged!=null)ValueChanged(this,EventArgs.Empty);}}
        public event EventHandler ValueChanged; public event EventHandler Scroll;
        public ModernSlider(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.Selectable,true);TabStop=true;SmallChange=1;LargeChange=10;Accent=Color.FromArgb(247,69,105);AccessibleRole=AccessibleRole.Slider;}
        protected override bool IsInputKey(Keys k){return k==Keys.Left||k==Keys.Right||k==Keys.Up||k==Keys.Down||base.IsInputKey(k);}
        void MoveTo(int x){Value=Minimum+(int)Math.Round((Maximum-Minimum)*Math.Max(0,Math.Min(1,(x-12.0)/Math.Max(1,Width-24))));if(Scroll!=null)Scroll(this,EventArgs.Empty);}
        protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);if(e.Button!=MouseButtons.Left)return;Focus();dragging=true;Capture=true;MoveTo(e.X);}
        protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);if(dragging)MoveTo(e.X);}
        protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);dragging=false;Capture=false;}
        protected override void OnMouseCaptureChanged(EventArgs e){base.OnMouseCaptureChanged(e);if(!Capture)dragging=false;}
        protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);switch(e.KeyCode){case Keys.Left:case Keys.Down:Value-=SmallChange;break;case Keys.Right:case Keys.Up:Value+=SmallChange;break;case Keys.PageDown:Value-=LargeChange;break;case Keys.PageUp:Value+=LargeChange;break;case Keys.Home:Value=Minimum;break;case Keys.End:Value=Maximum;break;default:return;}e.Handled=true;}
        protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;float y=Height/2f,x=12+(Width-24)*(Value-Minimum)/(float)Math.Max(1,Maximum-Minimum);using(var track=new Pen(Color.FromArgb(65,74,89),4))using(var active=new Pen(Accent,4)){track.StartCap=track.EndCap=active.StartCap=active.EndCap=LineCap.Round;g.DrawLine(track,12,y,Width-12,y);g.DrawLine(active,12,y,x,y);}using(var b=new SolidBrush(Accent))g.FillEllipse(b,x-7,y-7,14,14);using(var b=new SolidBrush(BackColor))g.FillEllipse(b,x-3,y-3,6,6);if(Focused)ControlPaint.DrawFocusRectangle(g,Rectangle.Inflate(ClientRectangle,-2,-2));}
    }
    public partial class MainForm
    {
        ComboBox _themePicker; bool _modernReady, _syncThemeNames;
        ChoiceButton _themeChoice, _languageChoice;
        Label _themeCaption, _languageCaption;
        void RefreshThemeNames()
        {
            if(_themePicker==null)return;
            string[] names;string theme,language;
            switch(Loc.CurrentLang){
                case "tr":names=new[]{"Sarı","Beyaz","Kırmızı","Mavi","Siyah"};theme="Tema";language="Dil";break;
                case "es":names=new[]{"Amarillo","Blanco","Rojo","Azul","Negro"};theme="Tema";language="Idioma";break;
                case "fr":names=new[]{"Jaune","Blanc","Rouge","Bleu","Noir"};theme="Thème";language="Langue";break;
                case "zh":names=new[]{"黄色","白色","红色","蓝色","黑色"};theme="主题";language="语言";break;
                case "ja":names=new[]{"イエロー","ホワイト","レッド","ブルー","ブラック"};theme="テーマ";language="言語";break;
                case "ko":names=new[]{"노란색","흰색","빨간색","파란색","검은색"};theme="테마";language="언어";break;
                default:names=new[]{"Yellow","White","Red","Blue","Black"};theme="Theme";language="Language";break;
            }
            _syncThemeNames=true;
            try{_themePicker.Items.Clear();_themePicker.Items.AddRange(names);_themePicker.SelectedIndex=Math.Max(0,Array.IndexOf(new[]{"yellow","white","red","blue","black"},_settings.Theme));}finally{_syncThemeNames=false;}
            if(_themeCaption!=null){_themeCaption.Text=theme;_languageCaption.Text=language;_themeChoice.AccessibleName=theme;_languageChoice.AccessibleName=language;_themeChoice.Invalidate();_languageChoice.Invalidate();}
        }
        Color _ink=Color.FromArgb(225,230,240), _muted=Color.FromArgb(146,160,181), _control=Color.FromArgb(29,35,46);
        Color AccentInk {get{return _settings.Theme=="yellow"||_settings.Theme=="white"||_settings.Theme=="black" ? Color.FromArgb(18,22,30):Color.White;}}
        void InitializeModernUI()
        {
            SuspendLayout();
            FormBorderStyle=FormBorderStyle.None;
            var header=_lblTitle.Parent;header.SetBounds(20,18,792,60);
            _picLogo.SetBounds(0,0,48,48);_lblTitle.Location=new Point(60,0);_lblTitle.Font=new Font("Segoe UI",23,FontStyle.Regular);
            _lblStatusPill.Visible=false;_lblHotkeyHint.SetBounds(60,40,174,20);
            header.Controls.Add(_cbLanguage);_cbLanguage.SetBounds(598,12,194,30);
            _themePicker=new ComboBox {DropDownStyle=ComboBoxStyle.DropDownList,FlatStyle=FlatStyle.Flat,Bounds=new Rectangle(408,12,174,30),AccessibleName="Theme"};
            _themePicker.Items.AddRange(new object[]{"Yellow","White","Red","Blue","Black"});
            string[] themes={"yellow","white","red","blue","black"};_themePicker.SelectedIndex=Math.Max(0,Array.IndexOf(themes,_settings.Theme));
            _themePicker.SelectedIndexChanged+=delegate{if(_syncThemeNames||_themePicker.SelectedIndex<0)return;_settings.Theme=themes[_themePicker.SelectedIndex];ApplyThemeColors();_settings.Save();};header.Controls.Add(_themePicker);
            foreach(var combo in new[]{_themePicker,_cbLanguage,_cbCity}) {combo.DrawMode=DrawMode.OwnerDrawFixed;combo.ItemHeight=24;combo.DrawItem+=delegate(object sender,DrawItemEventArgs e){if(e.Index<0)return;var box=(ComboBox)sender;using(var brush=new SolidBrush(box.BackColor))e.Graphics.FillRectangle(brush,e.Bounds);TextRenderer.DrawText(e.Graphics,box.Items[e.Index].ToString(),box.Font,Rectangle.Inflate(e.Bounds,-6,0),box.ForeColor,TextFormatFlags.VerticalCenter|TextFormatFlags.Left);e.DrawFocusRectangle();};}
            _themePicker.Visible=false;_cbLanguage.Visible=false;
            _themeCaption=new Label{Bounds=new Rectangle(350,0,145,18)};_languageCaption=new Label{Bounds=new Rectangle(510,0,174,18)};
            _themeChoice=new ChoiceButton(_themePicker){Bounds=new Rectangle(350,20,145,34),FlatStyle=FlatStyle.Flat};
            _languageChoice=new ChoiceButton(_cbLanguage){Bounds=new Rectangle(510,20,174,34),FlatStyle=FlatStyle.Flat};
            header.Controls.AddRange(new Control[]{_themeCaption,_languageCaption,_themeChoice,_languageChoice});
            var minimize=new ModernButton{Text="−",AccessibleName="Minimize",Bounds=new Rectangle(700,20,40,34),FlatStyle=FlatStyle.Flat};
            var close=new ModernButton{Text="×",AccessibleName="Close",Bounds=new Rectangle(748,20,40,34),FlatStyle=FlatStyle.Flat};
            minimize.Click+=delegate{WindowState=FormWindowState.Minimized;};close.Click+=delegate{Close();};header.Controls.Add(minimize);header.Controls.Add(close);
            foreach(Control drag in new Control[]{header,_lblTitle,_picLogo}){Point mouse=Point.Empty,origin=Point.Empty;bool moving=false;drag.MouseDown+=delegate(object sender,MouseEventArgs e){if(e.Button!=MouseButtons.Left)return;mouse=Cursor.Position;origin=Location;moving=true;drag.Capture=true;};drag.MouseMove+=delegate{if(moving&&drag.Capture){Point now=Cursor.Position;Location=new Point(origin.X+now.X-mouse.X,origin.Y+now.Y-mouse.Y);}};drag.MouseUp+=delegate{moving=false;drag.Capture=false;};drag.MouseCaptureChanged+=delegate{if(!drag.Capture)moving=false;};}
            RefreshThemeNames();
            _cardToggle.SetBounds(20,94,792,110);_btnToggle.SetBounds(18,14,756,44);
            _cardsBelowToggle[0].Size=new Size(386,260);_cardsBelowToggle[1].Size=new Size(386,260);_cardsBelowToggle[2].Size=new Size(792,100);
            _chkAutoStart.SetBounds(18,15,300,25);_chkMinimizeTray.SetBounds(18,49,320,25);_chkMinimizeTray.MaximumSize=new Size(350,0);
            _btnExitApp.SetBounds(590,20,184,38);_lblFooter.SetBounds(370,65,404,22);_lblFooter.Text="ARCHLIGHT / DISPLAY COMFORT / 0.5.4";
            _modernReady=true;ApplyThemeColors();ResumeLayout();LayoutToggleStatus();
        }
        void PaintTheme(Control parent)
        {
            foreach(Control c in parent.Controls){c.ForeColor=_ink;if(c is Panel)c.BackColor=c.Parent==this?_cardDark:c.Parent.BackColor;else if(c is Button||c is ComboBox)c.BackColor=_control;else c.BackColor=c.Parent.BackColor;
                var b=c as Button;if(b!=null){b.FlatAppearance.BorderSize=0;b.FlatAppearance.MouseOverBackColor=ControlPaint.Light(_control);}
                var choice=c as ChoiceButton;if(choice!=null){choice.Accent=_accentOrange;choice.Invalidate();}
                var slider=c as ModernSlider;if(slider!=null){slider.Accent=_accentOrange;slider.Invalidate();}PaintTheme(c);}
        }
        void ApplyThemeColors()
        {
            if(!_modernReady)return;
            bool light=_settings.Theme=="white",black=_settings.Theme=="black";
            _bgDark=light?Color.FromArgb(239,242,247):black?Color.FromArgb(7,9,12):Color.FromArgb(15,18,24);
            _cardDark=light?Color.White:black?Color.FromArgb(17,20,26):Color.FromArgb(23,28,36);
            _ink=light?Color.FromArgb(27,35,47):Color.FromArgb(225,230,240);
            _control=light?Color.FromArgb(224,230,239):Color.FromArgb(32,39,50);
            _accentOrange=_settings.Theme=="yellow"?Color.FromArgb(246,198,66):_settings.Theme=="blue"?Color.FromArgb(52,139,246):light?Color.FromArgb(210,220,234):black?Color.FromArgb(185,195,210):Color.FromArgb(247,69,105);
            _accentGold=_accentOrange;_offGray=_control;BackColor=_bgDark;PaintTheme(this);_lblTitle.Parent.BackColor=_bgDark;_lblTitle.BackColor=_bgDark;_picLogo.BackColor=_bgDark;_themeCaption.BackColor=_bgDark;_languageCaption.BackColor=_bgDark;
            _btnToggle.BackColor=_settings.IsEnabled?_accentOrange:_control;_btnToggle.ForeColor=_settings.IsEnabled?AccentInk:_ink;
            _lblStatusPill.BackColor=_settings.IsEnabled?_accentOrange:_control;_lblStatusPill.ForeColor=_settings.IsEnabled?AccentInk:_ink;
            SetSegmentButtonState(_btnModeSun,_settings.ScheduleMode==0);SetSegmentButtonState(_btnModeCustom,_settings.ScheduleMode==1);SetSegmentButtonState(_btnModeManual,_settings.ScheduleMode==2);
            UpdatePresetButtonHighlight(_btnPresetCandle,_settings.Kelvin==2200);UpdatePresetButtonHighlight(_btnPresetNight,_settings.Kelvin==3400);UpdatePresetButtonHighlight(_btnPresetSunset,_settings.Kelvin==4500);UpdatePresetButtonHighlight(_btnPresetNormal,_settings.Kelvin==6500);Invalidate(true);
        }
    }
}
