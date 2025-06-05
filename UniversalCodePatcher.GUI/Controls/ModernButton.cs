using System;
using System.Drawing;
using System.Windows.Forms;

namespace UniversalCodePatcher.Controls
{
    /// <summary>
    /// Flat style button with customizable accent color and optional icon.
    /// </summary>
    public class ModernButton : Button
    {
        public Color AccentColor { get; set; } = Color.FromArgb(66, 153, 225);

        public bool ShowIcon { get; set; } = true;

        public Image? IconImage { get; set; }

        public ModernButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = AccentColor;
            ForeColor = Color.White;
            MouseEnter += (_, _) => BackColor = ControlPaint.Light(AccentColor);
            MouseLeave += (_, _) => BackColor = AccentColor;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            if (ShowIcon && IconImage != null)
            {
                var imgRect = new Rectangle(4, (Height - IconImage.Height) / 2, IconImage.Width, IconImage.Height);
                pevent.Graphics.DrawImage(IconImage, imgRect);
                var textRect = new Rectangle(imgRect.Right + 4, 0, Width - imgRect.Right - 4, Height);
                TextRenderer.DrawText(pevent.Graphics, Text, Font, textRect, ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            }
        }
    }
}
