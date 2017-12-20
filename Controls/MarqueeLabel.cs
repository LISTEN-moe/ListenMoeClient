﻿using System;
using System.Drawing;
using System.Drawing.Text;

namespace ListenMoeClient
{
	class MarqueeLabel
	{
		public float ScrollSpeed { get; set; } = 50; //In pixels per second
		private string text = "";
		public string Text
		{
			get { return text; }
			set
			{
				text = value;
				RecalculateBounds();
			}
		}

		private string subtext = "";
		public string Subtext
		{
			get { return subtext; }
			set
			{
				subtext = value;
				RecalculateBounds();
			}
		}

		private float currentPosition = 0;
		private float spacing = 0.7f; //As a multiple of the width of the label
		private float subtextDistance = 3; //In pixels

		private DateTime last;

		private SizeF mainTextSize;
		private SizeF subTextSize;
		private float totalStringWidth;

		bool scrolling = false;

		public Rectangle Bounds = Rectangle.Empty;
		public Font Font = new Font("Segoe UI", 9);
		public Font Subfont = new Font("Segoe UI", 8);
		public Brush FontBrush = Brushes.White;

		private bool textChanged = true;

		private void UpdateTextPosition(float scale)
		{
			DateTime current = DateTime.Now;
			double ms = (current - last).TotalMilliseconds;

			if (scrolling)
			{
				float distance = (float)(ScrollSpeed * (ms / 1000));
				currentPosition -= distance;
				if (currentPosition < -totalStringWidth)
					currentPosition = Bounds.Width * spacing * scale;
			}

			last = current;
		}

		public void RecalculateBounds()
		{
			textChanged = true;
		}

		public void Render(Graphics g)
		{
			var scale = Settings.Get<float>("Scale");
			if (textChanged)
			{
				mainTextSize = g.MeasureString(text, Font);
				subTextSize = g.MeasureString(subtext, Subfont);

				totalStringWidth = mainTextSize.Width;
				if (subtext.Trim() != "")
				{
					totalStringWidth += subtextDistance; //Spacing between main and subtext
					totalStringWidth += subTextSize.Width;
				}
				totalStringWidth += 2; //Padding

				if (totalStringWidth > Bounds.Width * scale)
					scrolling = true;
				else
				{
					scrolling = false;
					currentPosition = 0;
				}
				textChanged = false;
			}

			if (scale < 2)
				g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
			else
				g.TextRenderingHint = TextRenderingHint.AntiAlias;

			UpdateTextPosition(scale);

			g.TranslateTransform((int)currentPosition, 0);
			RectangleF rect = new RectangleF(new PointF(Bounds.Location.X * scale, Bounds.Location.Y * scale), new SizeF(totalStringWidth, Bounds.Height * scale));
			g.DrawString(text, Font, FontBrush, rect.Location);
			if (subtext.Trim() != "")
			{
				g.DrawString(subtext, Subfont, FontBrush, new PointF(rect.Location.X + mainTextSize.Width + subtextDistance * scale, rect.Location.Y + ((mainTextSize.Height - subTextSize.Height) / 2)));
			}

			if (scrolling)
			{
				//Draw it on the other side for seamless looping
				float secondPosition = totalStringWidth + Bounds.Width * spacing * scale;
				g.TranslateTransform(secondPosition, 0);
				g.DrawString(text, Font, FontBrush, rect.Location);
				if (subtext.Trim() != "")
				{
					g.DrawString(subtext, Subfont, FontBrush, new PointF(rect.Location.X + mainTextSize.Width + subtextDistance * scale, rect.Location.Y + ((mainTextSize.Height - subTextSize.Height) / 2)));
				}

				g.TranslateTransform(-secondPosition, 0);
			}

			g.TranslateTransform(-(int)currentPosition, 0);
		}
	}
}
