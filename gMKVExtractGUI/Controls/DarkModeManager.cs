using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace gMKVToolNix.Controls
{
    public class DarkModeManager
    {
        /// <summary>The Parent form</summary>
        public Form OwnerForm { get; }

        /// <summary>Gets the Display color mode applied to the Form and Controls.</summary>
        public DisplayMode ColorMode => _colorMode;

        private DisplayMode _colorMode = DisplayMode.OriginalMode;


        /// <summary>
        /// The event handler reference in order to prevent its uncontrolled multiple addition
        /// </summary>
        private readonly ControlEventHandler _ownerFormControlAdded;

        /// <summary>
        /// stores the event handler reference in order to prevent its uncontrolled multiple addition
        /// </summary>
        private readonly EventHandler _controlHandleCreated;

        /// <summary>
        /// stores the event handler reference in order to prevent its uncontrolled multiple addition
        /// </summary>
        private readonly ControlEventHandler _controlControlAdded;

        /// <summary>
        /// Contains the original control color and theme values
        /// </summary>
        private readonly ConcurrentDictionary<Control, ConcurrentDictionary<string, object>> _originalControlValues = 
            new ConcurrentDictionary<Control, ConcurrentDictionary<string, object>>();


        private static readonly OSThemeColors _OSDarkThemeColors = GetSystemColors(true);
        private static readonly OSThemeColors _OSClearThemeColors = GetSystemColors(false);

        static DarkModeManager()
        {
            
        }

        public DarkModeManager(
            Form form,
            DisplayMode colorMode = DisplayMode.OriginalMode)
        {
            OwnerForm = form ?? throw new ArgumentNullException(nameof(form));
            _colorMode = colorMode;

            // Set the event handler to apply the theme to new controls
            _controlHandleCreated = (object sender, EventArgs e) =>
            {
                ApplySystemDarkTheme((Control)sender);
            };

            _controlControlAdded = (object sender, ControlEventArgs e) =>
            {
                ApplyControlTheme(e.Control);
            };

            // Set the event handler to apply the theme to new controls
            _ownerFormControlAdded = (object sender, ControlEventArgs e) =>
            {
                ApplyControlTheme(e.Control);
            };

            OwnerForm.ControlAdded -= _ownerFormControlAdded; // prevent uncontrolled multiple addition
            OwnerForm.ControlAdded += _ownerFormControlAdded;

            // This Fires after the normal 'Form_Load' event
            form.Load += (object sender, EventArgs e) =>
            {
                ApplyTheme();
            };
        }

        /// <summary>
        /// Store the original values of the Form's BackColor and ForeColor
        /// </summary>
        private void StoreOriginalFormValues()
        {
            if (!_originalControlValues.ContainsKey(OwnerForm))
            {
                _originalControlValues.TryAdd(OwnerForm, new ConcurrentDictionary<string, object>());
            }
            var formValues = _originalControlValues[OwnerForm];
            formValues.TryAdd(nameof(OwnerForm.BackColor), OwnerForm.BackColor);
            formValues.TryAdd(nameof(OwnerForm.ForeColor), OwnerForm.ForeColor);
        }

        /// <summary>
        /// Store the original values of the Control's BackColor and ForeColor
        /// </summary>
        /// <param name="control"></param>
        private void StoreOriginalControlValues(Control control)
        {
            if (!_originalControlValues.ContainsKey(control))
            {
                _originalControlValues.TryAdd(control, new ConcurrentDictionary<string, object>());
            }
            var controlValues = _originalControlValues[control];
            controlValues.TryAdd(nameof(control.BackColor), control.BackColor);
            controlValues.TryAdd(nameof(control.ForeColor), control.ForeColor);

            if (control is LinkLabel linkLabel)
            {                
                controlValues.TryAdd(nameof(linkLabel.LinkColor), linkLabel.LinkColor);
                controlValues.TryAdd(nameof(linkLabel.VisitedLinkColor), linkLabel.VisitedLinkColor);
            }
            else if (control is TextBox txt)
            {
                controlValues.TryAdd(nameof(txt.BorderStyle), txt.BorderStyle);
            }
            else if (control is Button btn)
            {
                controlValues.TryAdd(nameof(btn.FlatStyle), btn.FlatStyle);
                controlValues.TryAdd(nameof(btn.FlatAppearance.CheckedBackColor), btn.FlatAppearance.CheckedBackColor);
                controlValues.TryAdd(nameof(btn.FlatAppearance.BorderColor), btn.FlatAppearance.BorderColor);
            }
            else if (control is Panel panel)
            {
                controlValues.TryAdd(nameof(panel.BorderStyle), panel.BorderStyle);
            }
            else if (control is TableLayoutPanel tableLayoutPanel)
            {
                controlValues.TryAdd(nameof(tableLayoutPanel.BorderStyle), tableLayoutPanel.BorderStyle);
            }
            else if (control is PictureBox pictureBox)
            {
                controlValues.TryAdd(nameof(pictureBox.BorderStyle), pictureBox.BorderStyle);
            }
            else if (control is RichTextBox richText)
            {
                controlValues.TryAdd(nameof(pictureBox.BorderStyle), richText.BorderStyle);
            }
            else if (control is FlowLayoutPanel flowLayout)
            {
                controlValues.TryAdd(nameof(flowLayout.BorderStyle), flowLayout.BorderStyle);
            }
            else if (control is TreeView trv)
            {
                controlValues.TryAdd(nameof(trv.BorderStyle), trv.BorderStyle);
            }
        }

        /// <summary>Returns Windows's System Colors for UI components following Google Material Design concepts.</summary>
        /// <param name="darkMode">True: Dark Mode, False: Clear Mode</param>
        /// <returns>List of Colors:  Background, OnBackground, Surface, OnSurface, Primary, OnPrimary, Secondary, OnSecondary</returns>
        private static OSThemeColors GetSystemColors(bool darkMode)
        {
            OSThemeColors ret = new OSThemeColors();

            if (darkMode)
            {
                // Dark Mode
                ret.Background = Color.FromArgb(32, 32, 32);   //<- Negro Claro
                ret.BackgroundDark = Color.FromArgb(18, 18, 18);
                ret.BackgroundLight = ControlPaint.Light(ret.Background);

                ret.Surface = Color.FromArgb(43, 43, 43);      //<- Gris Oscuro
                ret.SurfaceLight = Color.FromArgb(50, 50, 50);
                ret.SurfaceDark = Color.FromArgb(29, 29, 29);

                ret.TextActive = Color.White;
                ret.TextInactive = Color.FromArgb(176, 176, 176);  //<- Blanco Palido
                ret.TextInAccent = GetReadableColor(ret.Accent);

                ret.Control = Color.FromArgb(55, 55, 55);       //<- Gris Oscuro
                ret.ControlDark = ControlPaint.Dark(ret.Control);
                ret.ControlLight = Color.FromArgb(67, 67, 67);

                ret.Primary = Color.FromArgb(3, 218, 198);   //<- Verde Pastel
                ret.Secondary = Color.MediumSlateBlue;         //<- Magenta Claro
            }

            return ret;
        }

        private static Color GetReadableColor(Color backgroundColor)
        {
            // Calculate the relative luminance of the background color.
            // Normalize values to 0-1 range first.
            double normalizedR = backgroundColor.R / 255.0;
            double normalizedG = backgroundColor.G / 255.0;
            double normalizedB = backgroundColor.B / 255.0;
            double luminance = 0.299 * normalizedR + 0.587 * normalizedG + 0.114 * normalizedB;

            // Choose a contrasting foreground color based on the luminance,
            // with a slight bias towards lighter colors for better readability.
            return luminance < 0.5 ? Color.FromArgb(182, 180, 215) : Color.FromArgb(34, 34, 34); // Dark gray for light backgrounds
        }

        public void ApplyTheme(DisplayMode colorMode)
        {
            _colorMode = colorMode;
            ApplyTheme();
        }

        /// <summary>Apply the Theme into the Window and all its controls.</summary>
        /// <param name="pIsDarkMode">'true': apply Dark Mode, 'false': apply Clear Mode</param>
        private void ApplyTheme()
        {
            try
            {
                // First try to store the original values of the form
                // If the form was already added in the original values dictionary
                // the values won't be updated
                StoreOriginalFormValues();

                // Apply Window's Dark Mode to the Form's Title bar:
                ApplySystemDarkTheme(OwnerForm);

                switch (ColorMode)
                {
                    case DisplayMode.ClearMode:
                        OwnerForm.BackColor = _OSClearThemeColors.Background;
                        OwnerForm.ForeColor = _OSClearThemeColors.TextInactive;
                        break;
                    case DisplayMode.DarkMode:
                        OwnerForm.BackColor = _OSDarkThemeColors.Background;
                        OwnerForm.ForeColor = _OSDarkThemeColors.TextInactive;
                        break;
                    case DisplayMode.OriginalMode:
                        OwnerForm.BackColor = (Color)_originalControlValues[OwnerForm][nameof(OwnerForm.BackColor)];
                        OwnerForm.ForeColor = (Color)_originalControlValues[OwnerForm][nameof(OwnerForm.ForeColor)];
                        break;
                    default:
                        break;
                }

                if (OwnerForm != null && OwnerForm.Controls != null)
                {
                    foreach (Control control in OwnerForm.Controls)
                    {
                        ApplyControlTheme(control);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(OwnerForm, ex.Message + ex.StackTrace, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
            }
        }

        /// <summary>Recursively apply the Colors from 'OScolors' to the Control and all its childs.</summary>
        /// <param name="control">Can be a Form or any Winforms Control.</param>
        public void ApplyControlTheme(Control control)
        {
            // First try to store the original values of the control
            // If the control was already added in the original values dictionary
            // the values won't be updated
            StoreOriginalControlValues(control);

            bool isDarkMode = ColorMode == DisplayMode.DarkMode;

            BorderStyle BStyle = isDarkMode ? BorderStyle.FixedSingle : BorderStyle.Fixed3D;
            FlatStyle FStyle = isDarkMode ? FlatStyle.Flat : FlatStyle.Standard;

            control.HandleCreated -= _controlHandleCreated; // prevent uncontrolled multiple addition
            control.HandleCreated += _controlHandleCreated;

            control.ControlAdded -= _controlControlAdded; //prevent uncontrolled multiple addition
            control.ControlAdded += _controlControlAdded;

            string controlDisplayMode = isDarkMode ? "DarkMode_Explorer" : "ClearMode_Explorer";
            PlatformInvokeManager.SetWindowThemeManaged(control.Handle, controlDisplayMode, null); //<- Attempts to apply Dark Mode using Win32 API if available.

            Type controlType = control.GetType();
            OSThemeColors OScolors = isDarkMode ? _OSDarkThemeColors : _OSClearThemeColors;

            switch (ColorMode)
            {
                case DisplayMode.ClearMode:
                    controlType.GetProperty(nameof(control.BackColor))?.SetValue(control, _OSClearThemeColors.Control, null);
                    controlType.GetProperty(nameof(control.ForeColor))?.SetValue(control, _OSClearThemeColors.TextActive, null);
                    OScolors = _OSClearThemeColors;
                    break;
                case DisplayMode.DarkMode:
                    controlType.GetProperty(nameof(control.BackColor))?.SetValue(control, _OSDarkThemeColors.Control, null);
                    controlType.GetProperty(nameof(control.ForeColor))?.SetValue(control, _OSDarkThemeColors.TextActive, null);
                    OScolors = _OSDarkThemeColors;
                    break;
                case DisplayMode.OriginalMode:
                    controlType.GetProperty(nameof(control.BackColor))?.SetValue(control, (Color)_originalControlValues[control][nameof(control.BackColor)], null);
                    controlType.GetProperty(nameof(control.ForeColor))?.SetValue(control, (Color)_originalControlValues[control][nameof(control.ForeColor)], null);
                    OScolors = _OSClearThemeColors;
                    break;
                default:
                    break;
            }

            /* Here we Finetune individual Controls */
            if (control is Label lbl)
            {
                controlType.GetProperty("BackColor")?.SetValue(control, control.Parent.BackColor, null);
                controlType.GetProperty("BorderStyle")?.SetValue(control, BorderStyle.None, null);

                // Paint Event Handler for Labels
                void labelPaintEventHandler(object sender, PaintEventArgs e)
                {
                    if (control.Enabled == false && isDarkMode)
                    {
                        e.Graphics.Clear(control.Parent.BackColor);
                        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

                        using (Brush B = new SolidBrush(control.ForeColor))
                        {
                            // StringFormat sf = lbl.CreateStringFormat();
                            MethodInfo mi = lbl.GetType().GetMethod("CreateStringFormat", BindingFlags.NonPublic | BindingFlags.Instance);
                            StringFormat sf = mi.Invoke(lbl, new object[] { }) as StringFormat;

                            e.Graphics.DrawString(lbl.Text, lbl.Font, B, new System.Drawing.PointF(1, 0), sf);
                        }
                    }
                };

                control.Paint -= labelPaintEventHandler;
                control.Paint += labelPaintEventHandler;
            }
            else if (control is LinkLabel)
            {
                switch (ColorMode)
                {
                    case DisplayMode.ClearMode:
                        controlType.GetProperty("LinkColor")?.SetValue(control, _OSClearThemeColors.AccentLight, null);
                        controlType.GetProperty("VisitedLinkColor")?.SetValue(control, _OSClearThemeColors.Primary, null);
                        break;
                    case DisplayMode.DarkMode:
                        controlType.GetProperty("LinkColor")?.SetValue(control, _OSDarkThemeColors.AccentLight, null);
                        controlType.GetProperty("VisitedLinkColor")?.SetValue(control, _OSDarkThemeColors.Primary, null);
                        break;
                    case DisplayMode.OriginalMode:
                        controlType.GetProperty("LinkColor")?.SetValue(control, (Color)_originalControlValues[control]["LinkColor"], null);
                        controlType.GetProperty("VisitedLinkColor")?.SetValue(control, (Color)_originalControlValues[control]["VisitedLinkColor"], null);
                        break;
                    default:
                        break;
                }
            }
            else if (control is TextBox txt)
            {
                // SetRoundBorders(tb, 4, OScolors.SurfaceDark, 1);
                switch (ColorMode)
                {
                    case DisplayMode.ClearMode:
                    case DisplayMode.DarkMode:
                        controlType.GetProperty(nameof(txt.BorderStyle))?.SetValue(control, BStyle, null);
                        break;
                    case DisplayMode.OriginalMode:
                        controlType.GetProperty(nameof(txt.BorderStyle))?.SetValue(control, (BorderStyle)_originalControlValues[control][nameof(txt.BorderStyle)], null);
                        break;
                    default:
                        break;
                }
            }
            else if (control is NumericUpDown)
            {
                // Mode = IsDarkMode ? "DarkMode_CFD" : "ClearMode_CFD";
                controlDisplayMode = isDarkMode ? "DarkMode_ItemsView" : "ClearMode_ItemsView";
                PlatformInvokeManager.SetWindowThemeManaged(control.Handle, controlDisplayMode, null);
            }
            else if (control is Button button)
            {
                switch (ColorMode)
                {
                    case DisplayMode.ClearMode:
                        controlType.GetProperty(nameof(button.FlatStyle))?.SetValue(control, FlatStyle.Standard, null);
                        controlType.GetProperty(nameof(button.FlatAppearance.CheckedBackColor))?.SetValue(control, _OSClearThemeColors.Accent, null);
                        controlType.GetProperty(nameof(button.BackColor))?.SetValue(control, _OSClearThemeColors.Control, null);
                        controlType.GetProperty(nameof(button.FlatAppearance.BorderColor))?.SetValue(control, (OwnerForm.AcceptButton == button) ? _OSClearThemeColors.Accent : _OSClearThemeColors.Control, null);
                       break;
                    case DisplayMode.DarkMode:
                        controlType.GetProperty(nameof(button.FlatStyle))?.SetValue(control, FlatStyle.Flat, null);
                        controlType.GetProperty(nameof(button.FlatAppearance.CheckedBackColor))?.SetValue(control, _OSDarkThemeColors.Accent, null);
                        controlType.GetProperty(nameof(button.BackColor))?.SetValue(control, _OSDarkThemeColors.Control, null);
                        controlType.GetProperty(nameof(button.FlatAppearance.BorderColor))?.SetValue(control, (OwnerForm.AcceptButton == button) ? _OSDarkThemeColors.Accent : _OSDarkThemeColors.Control, null);
                        break;
                    case DisplayMode.OriginalMode:
                        controlType.GetProperty(nameof(button.FlatStyle))?.SetValue(control, (FlatStyle)_originalControlValues[control][nameof(button.FlatStyle)], null);
                        controlType.GetProperty(nameof(button.FlatAppearance.CheckedBackColor))?.SetValue(control, (Color)_originalControlValues[control][nameof(button.FlatAppearance.CheckedBackColor)], null);
                        controlType.GetProperty(nameof(button.BackColor))?.SetValue(control, (Color)_originalControlValues[control][nameof(button.BackColor)], null);
                        controlType.GetProperty(nameof(button.FlatAppearance.BorderColor))?.SetValue(control, (Color)_originalControlValues[control][nameof(button.FlatAppearance.BorderColor)], null);
                        break;
                    default:
                        break;
                }

                button.UseVisualStyleBackColor = true;
            }
            else if (control is ComboBox comboBox)
            {
                // Fixing a glitch that makes all instances of the ComboBox showing as having a Selected value, even when they dont
                control.BeginInvoke(new Action(() => (control as ComboBox).SelectionLength = 0));

                // Fixes a glitch showing the Combo Backgroud white when the control is Disabled:
                if (!control.Enabled && isDarkMode)
                {
                    comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                }

                // Apply Windows Color Mode:
                controlDisplayMode = isDarkMode ? "DarkMode_CFD" : "ClearMode_CFD";
                PlatformInvokeManager.SetWindowThemeManaged(control.Handle, controlDisplayMode, null);
            }
            else if (control is Panel panel)
            {
                // Process the panel within the container
                switch (ColorMode)
                {
                    case DisplayMode.ClearMode:
                        controlType.GetProperty(nameof(panel.BackColor))?.SetValue(control, _OSClearThemeColors.Surface, null);
                        controlType.GetProperty(nameof(panel.BorderStyle))?.SetValue(control, BorderStyle.None, null);
                        break;
                    case DisplayMode.DarkMode:
                        controlType.GetProperty(nameof(panel.BackColor))?.SetValue(control, _OSDarkThemeColors.Surface, null);
                        controlType.GetProperty(nameof(panel.BorderStyle))?.SetValue(control, BorderStyle.None, null);
                        break;
                    case DisplayMode.OriginalMode:
                        controlType.GetProperty(nameof(panel.BackColor))?.SetValue(control, (Color)_originalControlValues[control][nameof(panel.BackColor)], null);
                        controlType.GetProperty(nameof(panel.BorderStyle))?.SetValue(control, (BorderStyle)_originalControlValues[control][nameof(panel.BorderStyle)], null);
                        break;
                    default:
                        break;
                }
            }
            else if (control is GroupBox groupBox)
            {
                switch (ColorMode)
                {
                    case DisplayMode.ClearMode:
                        controlType.GetProperty(nameof(groupBox.BackColor))?.SetValue(control, control.Parent.BackColor, null);
                        controlType.GetProperty(nameof(groupBox.ForeColor))?.SetValue(control, _OSClearThemeColors.TextActive, null);
                        break;
                    case DisplayMode.DarkMode:
                        controlType.GetProperty(nameof(groupBox.BackColor))?.SetValue(control, control.Parent.BackColor, null);
                        controlType.GetProperty(nameof(groupBox.ForeColor))?.SetValue(control, _OSDarkThemeColors.TextActive, null);
                        break;
                    case DisplayMode.OriginalMode:
                        controlType.GetProperty(nameof(groupBox.BackColor))?.SetValue(control, (Color)_originalControlValues[control][nameof(groupBox.BackColor)], null);
                        controlType.GetProperty(nameof(groupBox.ForeColor))?.SetValue(control, (Color)_originalControlValues[control][nameof(groupBox.ForeColor)], null);
                        break;
                    default:
                        break;
                }

                void groupBoxPaintEventHandler(object sender, PaintEventArgs e)
                {
                    if (control.Enabled == false && isDarkMode)
                    {
                        var radio = (sender as GroupBox);
                        using (Brush B = new SolidBrush(control.ForeColor))
                        {
                            e.Graphics.DrawString(radio.Text, radio.Font,
                              B, new System.Drawing.PointF(6, 0));
                        }
                    }
                }

                control.Paint -= groupBoxPaintEventHandler;
                control.Paint += groupBoxPaintEventHandler;
            }
            else if (control is TableLayoutPanel tableLayoutPanel)
            {
                switch (ColorMode)
                {
                    case DisplayMode.ClearMode:
                        controlType.GetProperty(nameof(tableLayoutPanel.BackColor))?.SetValue(control, control.Parent.BackColor, null);
                        controlType.GetProperty(nameof(tableLayoutPanel.ForeColor))?.SetValue(control, _OSClearThemeColors.TextActive, null);
                        controlType.GetProperty(nameof(tableLayoutPanel.BorderStyle))?.SetValue(control, BorderStyle.None, null);
                        break;
                    case DisplayMode.DarkMode:
                        controlType.GetProperty(nameof(tableLayoutPanel.BackColor))?.SetValue(control, control.Parent.BackColor, null);
                        controlType.GetProperty(nameof(tableLayoutPanel.ForeColor))?.SetValue(control, _OSDarkThemeColors.TextActive, null);
                        controlType.GetProperty(nameof(tableLayoutPanel.BorderStyle))?.SetValue(control, BorderStyle.None, null);
                        break;
                    case DisplayMode.OriginalMode:
                        controlType.GetProperty(nameof(tableLayoutPanel.BackColor))?.SetValue(control, (Color)_originalControlValues[control][nameof(tableLayoutPanel.BackColor)], null);
                        controlType.GetProperty(nameof(tableLayoutPanel.ForeColor))?.SetValue(control, (Color)_originalControlValues[control][nameof(tableLayoutPanel.ForeColor)], null);
                        controlType.GetProperty(nameof(tableLayoutPanel.BorderStyle))?.SetValue(control, (BorderStyle)_originalControlValues[control][nameof(tableLayoutPanel.BorderStyle)], null);
                        break;
                    default:
                        break;
                }
            }
            else if (control is TabControl tab)
            {
                tab.Appearance = TabAppearance.Normal;
                tab.DrawMode = TabDrawMode.OwnerDrawFixed;

                Color surfaceColor = _OSClearThemeColors.Surface;
                Color textInactiveColor = _OSClearThemeColors.TextInactive;
                switch (ColorMode)
                {
                    case DisplayMode.ClearMode:
                        surfaceColor = _OSClearThemeColors.Surface;
                        textInactiveColor = _OSClearThemeColors.TextInactive;
                        break;
                    case DisplayMode.DarkMode:
                        surfaceColor = _OSDarkThemeColors.Surface;
                        textInactiveColor = _OSDarkThemeColors.TextInactive;
                        break;
                    case DisplayMode.OriginalMode:
                        surfaceColor = (Color)_originalControlValues[control][nameof(control.BackColor)];
                        textInactiveColor = (Color)_originalControlValues[control][nameof(control.ForeColor)];
                        break;
                    default:
                        break;
                }

                void tabControlDrawItemEventHandler(object sender, DrawItemEventArgs e)
                {
                    // Draw the background of the main control
                    using (SolidBrush backColor = new SolidBrush(tab.Parent.BackColor))
                    {
                        e.Graphics.FillRectangle(backColor, tab.ClientRectangle);
                    }

                    using (Brush tabBack = new SolidBrush(surfaceColor))
                    {
                        for (int i = 0; i < tab.TabPages.Count; i++)
                        {
                            TabPage tabPage = tab.TabPages[i];
                            tabPage.BackColor = surfaceColor;
                            tabPage.BorderStyle = BorderStyle.FixedSingle;

                            tabPage.ControlAdded -= _controlControlAdded;
                            tabPage.ControlAdded += _controlControlAdded;

                            var tBounds = e.Bounds;
                            //tBounds.Inflate(100, 100);

                            bool IsSelected = (tab.SelectedIndex == i);
                            if (IsSelected)
                            {
                                e.Graphics.FillRectangle(tabBack, tBounds);
                                TextRenderer.DrawText(e.Graphics, tabPage.Text, tabPage.Font, e.Bounds, textInactiveColor);
                            }
                            else
                            {
                                TextRenderer.DrawText(e.Graphics, tabPage.Text, tabPage.Font, tab.GetTabRect(i), textInactiveColor);
                            }
                        }
                    }
                }

                tab.DrawItem -= tabControlDrawItemEventHandler;
                tab.DrawItem += tabControlDrawItemEventHandler;
            }
            else if (control is PictureBox pictureBox)
            {
                switch (ColorMode)
                {
                    case DisplayMode.ClearMode:
                        pictureBox.BackColor = control.Parent.BackColor;
                        pictureBox.ForeColor = _OSClearThemeColors.TextActive;
                        pictureBox.BorderStyle = BorderStyle.None;
                        break;
                    case DisplayMode.DarkMode:
                        pictureBox.BackColor = control.Parent.BackColor;
                        pictureBox.ForeColor = _OSDarkThemeColors.TextActive;
                        pictureBox.BorderStyle = BorderStyle.None;
                        break;
                    case DisplayMode.OriginalMode:
                        pictureBox.BackColor = (Color)_originalControlValues[control][nameof(pictureBox.BackColor)];
                        pictureBox.ForeColor = (Color)_originalControlValues[control][nameof(pictureBox.ForeColor)];
                        pictureBox.BorderStyle = (BorderStyle)_originalControlValues[control][nameof(pictureBox.BorderStyle)];
                        break;
                    default:
                        break;
                }
            }
            else if (control is CheckBox checkBox)
            {
                switch (ColorMode)
                {
                    case DisplayMode.ClearMode:
                        controlType.GetProperty(nameof(checkBox.BackColor))?.SetValue(control, control.Parent.BackColor, null);
                        controlType.GetProperty(nameof(checkBox.ForeColor))?.SetValue(control, control.Enabled ? _OSClearThemeColors.TextActive : _OSClearThemeColors.TextInactive, null);
                        break;
                    case DisplayMode.DarkMode:
                        controlType.GetProperty(nameof(checkBox.BackColor))?.SetValue(control, control.Parent.BackColor, null);
                        controlType.GetProperty(nameof(checkBox.ForeColor))?.SetValue(control, control.Enabled ? _OSDarkThemeColors.TextActive : _OSDarkThemeColors.TextInactive, null);
                        break;
                    case DisplayMode.OriginalMode:
                        controlType.GetProperty(nameof(checkBox.BackColor))?.SetValue(control, (Color)_originalControlValues[control][nameof(checkBox.BackColor)], null);
                        controlType.GetProperty(nameof(checkBox.ForeColor))?.SetValue(control, (Color)_originalControlValues[control][nameof(checkBox.ForeColor)], null);
                        break;
                    default:
                        break;
                }
            }
            else if (control is RadioButton radioButton)
            {
                switch (ColorMode)
                {
                    case DisplayMode.ClearMode:
                        controlType.GetProperty(nameof(radioButton.BackColor))?.SetValue(control, control.Parent.BackColor, null);
                        controlType.GetProperty(nameof(radioButton.ForeColor))?.SetValue(control, control.Enabled ? _OSClearThemeColors.TextActive : _OSClearThemeColors.TextInactive, null);
                        break;
                    case DisplayMode.DarkMode:
                        controlType.GetProperty(nameof(radioButton.BackColor))?.SetValue(control, control.Parent.BackColor, null);
                        controlType.GetProperty(nameof(radioButton.ForeColor))?.SetValue(control, control.Enabled ? _OSDarkThemeColors.TextActive : _OSDarkThemeColors.TextInactive, null);
                        break;
                    case DisplayMode.OriginalMode:
                        controlType.GetProperty(nameof(radioButton.BackColor))?.SetValue(control, (Color)_originalControlValues[control][nameof(radioButton.BackColor)], null);
                        controlType.GetProperty(nameof(radioButton.ForeColor))?.SetValue(control, (Color)_originalControlValues[control][nameof(radioButton.ForeColor)], null);
                        break;
                    default:
                        break;
                }
            }
            else if (control is MenuStrip menuStrip)
            {
            }
            else if (control is ToolStrip toolStrip)
            {
                toolStrip.ItemAdded += (object sender, ToolStripItemEventArgs e) =>
                {
                    if (e.Item is ToolStripDropDownItem itemControl)
                    {
                        ApplyControlTheme(itemControl.DropDown);
                    }
                };

                foreach (ToolStripItem item in toolStrip.Items)
                {
                    if (item is ToolStripDropDownItem itemControl)
                    {                        
                        ApplyControlTheme(itemControl.DropDown);
                    }
                }
            }
            else if (control is ToolStripPanel toolStripPanel) //<- empty area around ToolStrip
            {
                controlType.GetProperty("BackColor")?.SetValue(control, control.Parent.BackColor, null);
            }
            else if (control is ToolStripDropDown toolStripDropDown)
            {
                toolStripDropDown.Opening -= Tsdd_Opening; //just to make sure
                toolStripDropDown.Opening += Tsdd_Opening;
            }
            else if (control is ToolStripDropDownMenu toolStripDropDownMenu)
            {
                toolStripDropDownMenu.Opening -= Tsdd_Opening; //just to make sure
                toolStripDropDownMenu.Opening += Tsdd_Opening;
            }
            else if (control is ContextMenuStrip contextMenuStrip)
            {
                contextMenuStrip.Opening -= Tsdd_Opening; //just to make sure
                contextMenuStrip.Opening += Tsdd_Opening;
            }
            else if (control is MdiClient) //<- empty area of MDI container window
            {
                controlType.GetProperty("BackColor")?.SetValue(control, OScolors.Surface, null);
            }
            else if (control is PropertyGrid pGrid)
            {
                pGrid.BackColor = OScolors.Control;
                pGrid.ViewBackColor = OScolors.Control;
                pGrid.LineColor = OScolors.Surface;
                pGrid.ViewForeColor = OScolors.TextActive;
                //pGrid.ViewBorderColor = OScolors.ControlDark;
                pGrid.CategoryForeColor = OScolors.TextActive;
                //pGrid.CategorySplitterColor = OScolors.ControlLight;
            }
            else if (control is ListView lView)
            {
                //Mode = IsDarkMode ? "DarkMode_ItemsView" : "ClearMode_ItemsView";
                controlDisplayMode = isDarkMode ? "DarkMode_Explorer" : "ClearMode_Explorer";
                PlatformInvokeManager.SetWindowThemeManaged(control.Handle, controlDisplayMode, null);

                if (lView.View == View.Details)
                {
                    lView.OwnerDraw = true;
                    lView.DrawColumnHeader += (object sender, DrawListViewColumnHeaderEventArgs e) =>
                    {
                        //e.DrawDefault = true;
                        //e.DrawBackground();
                        //e.DrawText();

                        using (SolidBrush backBrush = new SolidBrush(OScolors.ControlLight))
                        {
                            using (SolidBrush foreBrush = new SolidBrush(OScolors.TextActive))
                            {
                                using (var sf = new StringFormat())
                                {
                                    sf.Alignment = StringAlignment.Center;
                                    e.Graphics.FillRectangle(backBrush, e.Bounds);
                                    e.Graphics.DrawString(e.Header.Text, lView.Font, foreBrush, e.Bounds, sf);
                                }
                            }
                        }
                    };
                    lView.DrawItem += (sender, e) => { e.DrawDefault = true; };
                    lView.DrawSubItem += (sender, e) =>
                    {
                        e.DrawDefault = true;

                        //IntPtr headerControl = GetHeaderControl(lView);
                        //IntPtr hdc = GetDC(headerControl);
                        //Rectangle rc = new Rectangle(
                        //  e.Bounds.Right, //<- Right instead of Left - offsets the rectangle
                        //  e.Bounds.Top,
                        //  e.Bounds.Width,
                        //  e.Bounds.Height
                        //);
                        //rc.Width += 200;

                        //using (SolidBrush backBrush = new SolidBrush(OScolors.ControlLight))
                        //{
                        //  e.Graphics.FillRectangle(backBrush, rc);
                        //}

                        //ReleaseDC(headerControl, hdc);

                    };

                    controlDisplayMode = isDarkMode ? "DarkMode_Explorer" : "ClearMode_Explorer";
                    PlatformInvokeManager.SetWindowThemeManaged(control.Handle, controlDisplayMode, null);
                }
            }
            else if (control is TreeView trv)
            {
                switch (ColorMode)
                {
                    case DisplayMode.ClearMode:
                        controlType.GetProperty(nameof(trv.BorderStyle))?.SetValue(control, BorderStyle.None, null);
                        break;
                    case DisplayMode.DarkMode:
                        controlType.GetProperty(nameof(trv.BorderStyle))?.SetValue(control, BorderStyle.None, null);
                        break;
                    case DisplayMode.OriginalMode:
                        controlType.GetProperty(nameof(trv.BorderStyle))?.SetValue(control, (BorderStyle)_originalControlValues[control][nameof(trv.BorderStyle)], null);
                        break;
                    default:
                        break;
                }
            }
            else if (control is DataGridView)
            {
                var grid = control as DataGridView;
                grid.EnableHeadersVisualStyles = false;
                grid.BorderStyle = BorderStyle.FixedSingle;
                grid.BackgroundColor = OScolors.Control;
                grid.GridColor = OScolors.Control;

                //paint the bottom right corner where the scrollbars meet
                grid.Paint += (object sender, PaintEventArgs e) =>
                {
                    DataGridView dgv = sender as DataGridView;

                    //get the value of dgv.HorizontalScrollBar protected property
                    HScrollBar hs = (HScrollBar)typeof(DataGridView).GetProperty("HorizontalScrollBar", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(dgv, null);
                    if (hs.Visible)
                    {
                        //get the value of dgv.VerticalScrollBar protected property
                        VScrollBar vs = (VScrollBar)typeof(DataGridView).GetProperty("VerticalScrollBar", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(dgv, null);

                        if (vs.Visible)
                        {
                            //only when both the scrollbars are visible, do the actual painting
                            Brush brush = new SolidBrush(OScolors.SurfaceDark);
                            var w = vs.Size.Width;
                            var h = hs.Size.Height;
                            e.Graphics.FillRectangle(brush, dgv.ClientRectangle.X + dgv.ClientRectangle.Width - w - 1,
                              dgv.ClientRectangle.Y + dgv.ClientRectangle.Height - h - 1, w, h);
                        }
                    }
                };

                grid.DefaultCellStyle.BackColor = OScolors.Surface;
                grid.DefaultCellStyle.ForeColor = OScolors.TextActive;

                grid.ColumnHeadersDefaultCellStyle.BackColor = OScolors.Surface;
                grid.ColumnHeadersDefaultCellStyle.ForeColor = OScolors.TextActive;
                grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = OScolors.Surface;
                grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

                grid.RowHeadersDefaultCellStyle.BackColor = OScolors.Surface;
                grid.RowHeadersDefaultCellStyle.ForeColor = OScolors.TextActive;
                grid.RowHeadersDefaultCellStyle.SelectionBackColor = OScolors.Surface;
                grid.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            }
            else if (control is RichTextBox richText)
            {
                switch (ColorMode)
                {
                    case DisplayMode.ClearMode:
                        controlType.GetProperty(nameof(richText.BackColor))?.SetValue(control, control.Parent.BackColor, null);
                        controlType.GetProperty(nameof(richText.ForeColor))?.SetValue(control, control.Enabled ? _OSClearThemeColors.TextActive : _OSClearThemeColors.TextInactive, null);
                        controlType.GetProperty(nameof(richText.BorderStyle))?.SetValue(control, BorderStyle.None, null);
                        break;
                    case DisplayMode.DarkMode:
                        controlType.GetProperty(nameof(richText.BackColor))?.SetValue(control, control.Parent.BackColor, null);
                        controlType.GetProperty(nameof(richText.ForeColor))?.SetValue(control, control.Enabled ? _OSDarkThemeColors.TextActive : _OSDarkThemeColors.TextInactive, null);
                        controlType.GetProperty(nameof(richText.BorderStyle))?.SetValue(control, BorderStyle.None, null);
                        break;
                    case DisplayMode.OriginalMode:
                        controlType.GetProperty(nameof(richText.BackColor))?.SetValue(control, (Color)_originalControlValues[control][nameof(richText.BackColor)], null);
                        controlType.GetProperty(nameof(richText.ForeColor))?.SetValue(control, (Color)_originalControlValues[control][nameof(richText.ForeColor)], null);
                        controlType.GetProperty(nameof(richText.BorderStyle))?.SetValue(control, (BorderStyle)_originalControlValues[control][nameof(richText.BorderStyle)], null);
                        break;
                    default:
                        break;
                }
            }
            else if (control is FlowLayoutPanel flowLayout)
            {
                switch (ColorMode)
                {
                    case DisplayMode.ClearMode:
                        controlType.GetProperty(nameof(flowLayout.BackColor))?.SetValue(control, control.Parent.BackColor, null);
                        controlType.GetProperty(nameof(flowLayout.ForeColor))?.SetValue(control, control.Enabled ? _OSClearThemeColors.TextActive : _OSClearThemeColors.TextInactive, null);
                        controlType.GetProperty(nameof(flowLayout.BorderStyle))?.SetValue(control, BorderStyle.None, null);
                        break;
                    case DisplayMode.DarkMode:
                        controlType.GetProperty(nameof(flowLayout.BackColor))?.SetValue(control, control.Parent.BackColor, null);
                        controlType.GetProperty(nameof(flowLayout.ForeColor))?.SetValue(control, control.Enabled ? _OSDarkThemeColors.TextActive : _OSDarkThemeColors.TextInactive, null);
                        controlType.GetProperty(nameof(flowLayout.BorderStyle))?.SetValue(control, BorderStyle.None, null);
                        break;
                    case DisplayMode.OriginalMode:
                        controlType.GetProperty(nameof(flowLayout.BackColor))?.SetValue(control, (Color)_originalControlValues[control][nameof(flowLayout.BackColor)], null);
                        controlType.GetProperty(nameof(flowLayout.ForeColor))?.SetValue(control, (Color)_originalControlValues[control][nameof(flowLayout.ForeColor)], null);
                        controlType.GetProperty(nameof(flowLayout.BorderStyle))?.SetValue(control, (BorderStyle)_originalControlValues[control][nameof(flowLayout.BorderStyle)], null);
                        break;
                    default:
                        break;
                }
            }

            Debug.Print(string.Format("{0}: {1}", control.Name, controlType.Name));

            if (control.ContextMenuStrip != null)
            {
                ApplyControlTheme(control.ContextMenuStrip);
            }

            foreach (Control childControl in control.Controls)
            {
                // Recursively process its children
                ApplyControlTheme(childControl);
            }
        }

        /// <summary>
        /// handle hierarchical context menus (otherwise, only the root level gets themed)
        /// </summary>
        private void Tsdd_Opening(object sender, CancelEventArgs e)
        {
            ToolStripDropDown tsdd = sender as ToolStripDropDown;
            if (tsdd == null) return; // should not occur

            foreach (ToolStripMenuItem toolStripMenuItem in tsdd.Items.OfType<ToolStripMenuItem>())
            {
                toolStripMenuItem.DropDownOpening -= Tsmi_DropDownOpening; //just to make sure
                toolStripMenuItem.DropDownOpening += Tsmi_DropDownOpening;
            }
        }

        /// <summary>
        /// handle hierarchical context menus (otherwise, only the root level gets themed)
        /// </summary>
        private void Tsmi_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            if (tsmi == null) return; // should not occur

            if (tsmi.DropDown.Items.Count > 0)
            {
                ApplyControlTheme(tsmi.DropDown);
            }

            // once processed, remove itself to prevent multiple executions (when user leaves and reenters the sub-menu)
            tsmi.DropDownOpening -= Tsmi_DropDownOpening;
        }


        private static readonly int[] _darkModeValues = new[] { 0x01 }; //<- 1=True, 0=False
        private static readonly int[] _clearModeValues = new[] { 0x00 }; //<- 1=True, 0=False

        /// <summary>Attemps to apply Window's Dark Style to the Control and all its childs.</summary>
        /// <param name="control"></param>
        private void ApplySystemDarkTheme(Control control = null)
        {
            if (gMKVHelper.IsOnLinux)
            {
                return;
            }

            bool isDarkMode = ColorMode == DisplayMode.DarkMode;

            /*
			DWMWA_USE_IMMERSIVE_DARK_MODE:   https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/ne-dwmapi-dwmwindowattribute

			Use with DwmSetWindowAttribute. Allows the window frame for this window to be drawn in dark mode colors when the dark mode system setting is enabled.
			For compatibility reasons, all windows default to light mode regardless of the system setting.
			The pvAttribute parameter points to a value of type BOOL. TRUE to honor dark mode for the window, FALSE to always use light mode.

			This value is supported starting with Windows 11 Build 22000.

			SetWindowTheme:     https://learn.microsoft.com/en-us/windows/win32/api/uxtheme/nf-uxtheme-setwindowtheme
			Causes a window to use a different set of visual style information than its class normally uses. Fix for Scrollbars!
			*/
            int[] darkModeOn = isDarkMode ? _darkModeValues : _clearModeValues; //<- 1=True, 0=False
            string mode = isDarkMode ? "DarkMode_Explorer" : "ClearMode_Explorer";

            PlatformInvokeManager.SetWindowThemeManaged(control.Handle, mode, null); // DarkMode_Explorer, ClearMode_Explorer, DarkMode_CFD, DarkMode_ItemsView,

            if (PlatformInvokeManager.DwmSetWindowAttributeManaged(
                control.Handle, (int)DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, darkModeOn, 4) != 0)
            {
                PlatformInvokeManager.DwmSetWindowAttributeManaged(
                    control.Handle, (int)DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, darkModeOn, 4);
            }

            foreach (Control child in control.Controls)
            {
                if (child.Controls.Count != 0)
                {
                    ApplySystemDarkTheme(child);
                }
            }
        }

        /// <summary>Returns the Accent Color used by Windows.</summary>
        /// <returns>a Color</returns>
        public static Color GetWindowsAccentColor()
        {
            try
            {
                if (gMKVHelper.IsOnLinux)
                {
                    return Color.CadetBlue;
                }

                DWMCOLORIZATIONcolors colors = new DWMCOLORIZATIONcolors();
                PlatformInvokeManager.DwmGetColorizationParametersManaged(ref colors);

                //get the theme --> only if Windows 10 or newer
                if (IsWindows10orGreater())
                {
                    var color = colors.ColorizationColor;

                    var colorValue = long.Parse(color.ToString(), System.Globalization.NumberStyles.HexNumber);

                    var transparency = (colorValue >> 24) & 0xFF;
                    var red = (colorValue >> 16) & 0xFF;
                    var green = (colorValue >> 8) & 0xFF;
                    var blue = (colorValue >> 0) & 0xFF;

                    return Color.FromArgb((int)transparency, (int)red, (int)green, (int)blue);
                }
                else
                {
                    return Color.CadetBlue;
                }
            }
            catch (Exception)
            {
                return Color.CadetBlue;
            }
        }

        /// <summary>Returns the Accent Color used by Windows.</summary>
        /// <returns>an opaque Color</returns>
        public static Color GetWindowsAccentOpaqueColor()
        {
            if (gMKVHelper.IsOnLinux)
            {
                return Color.CadetBlue;
            }

            DWMCOLORIZATIONcolors colors = new DWMCOLORIZATIONcolors();
            PlatformInvokeManager.DwmGetColorizationParametersManaged(ref colors);

            //get the theme --> only if Windows 10 or newer
            if (IsWindows10orGreater())
            {
                var color = colors.ColorizationColor;

                var colorValue = long.Parse(color.ToString(), System.Globalization.NumberStyles.HexNumber);

                var red = (colorValue >> 16) & 0xFF;
                var green = (colorValue >> 8) & 0xFF;
                var blue = (colorValue >> 0) & 0xFF;

                return Color.FromArgb(255, (int)red, (int)green, (int)blue);
            }
            else
            {
                return Color.CadetBlue;
            }
        }

        private static bool IsWindows10orGreater()
        {
            return WindowsVersion() >= 10;
        }

        private static int? _windowsVersion = null;

        private static int WindowsVersion()
        {
            if (_windowsVersion != null)
            {
                return (int)_windowsVersion;
            }

            // for .Net4.8 and Minor
            int result;
            try
            {
                var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                string[] productName = reg.GetValue("ProductName").ToString().Split((char)32);
                int.TryParse(productName[1], out result);
            }
            catch (Exception)
            {
                OperatingSystem os = Environment.OSVersion;
                result = os.Version.Major;
            }

            _windowsVersion = result;
            return result;
        }

        private class PlatformInvokeManager
        {
            [DllImport("DwmApi")]
            private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);

            public static int DwmSetWindowAttributeManaged(IntPtr hwnd, int attr, int[] attrValue, int attrSize)
            {
                if (gMKVHelper.IsOnLinux)
                {
                    return 1;
                }

                return DwmSetWindowAttribute(hwnd, attr, attrValue, attrSize);
            }

            [DllImport("dwmapi.dll", EntryPoint = "#127")]
            private static extern void DwmGetColorizationParameters(ref DWMCOLORIZATIONcolors colors);

            public static void DwmGetColorizationParametersManaged(ref DWMCOLORIZATIONcolors colors)
            {
                if (gMKVHelper.IsOnLinux)
                {
                    return;
                }

                DwmGetColorizationParameters(ref colors);
            }

            [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
            private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

            public static int SetWindowThemeManaged(IntPtr hWnd, string pszSubAppName, string pszSubIdList)
            {
                if (gMKVHelper.IsOnLinux)
                {
                    return 1;
                }

                return SetWindowTheme(hWnd, pszSubAppName, pszSubIdList);
            }
        }
    }

    /// <summary>Windows 10+ System Colors for Clear Color Mode.</summary>
    public class OSThemeColors
    {
        public OSThemeColors()
        {
        }

        /// <summary>For the very back of the Window</summary>
        public Color Background { get; set; } = SystemColors.Control;

        /// <summary>For Borders around the Background</summary>
        public Color BackgroundDark { get; set; } = SystemColors.ControlDark;

        /// <summary>For hightlights over the Background</summary>
        public Color BackgroundLight { get; set; } = SystemColors.ControlLight;

        /// <summary>For Container above the Background</summary>
        public Color Surface { get; set; } = SystemColors.ControlLightLight;

        /// <summary>For Borders around the Surface</summary>
        public Color SurfaceDark { get; set; } = SystemColors.ControlLight;

        /// <summary>For Highligh over the Surface</summary>
        public Color SurfaceLight { get; set; } = Color.White;

        /// <summary>For Main Texts</summary>
        public Color TextActive { get; set; } = SystemColors.ControlText;

        /// <summary>For Inactive Texts</summary>
        public Color TextInactive { get; set; } = SystemColors.GrayText;

        /// <summary>For Hightligh Texts</summary>
        public Color TextInAccent { get; set; } = SystemColors.HighlightText;

        /// <summary>For the background of any Control</summary>
        public Color Control { get; set; } = SystemColors.ButtonFace;

        /// <summary>For Bordes of any Control</summary>
        public Color ControlDark { get; set; } = SystemColors.ButtonShadow;

        /// <summary>For Highlight elements in a Control</summary>
        public Color ControlLight { get; set; } = SystemColors.ButtonHighlight;

        /// <summary>Windows 10+ Chosen Accent Color</summary>
        public Color Accent { get; set; } = DarkModeManager.GetWindowsAccentColor();

        public Color AccentOpaque { get; set; } = DarkModeManager.GetWindowsAccentOpaqueColor();

        public Color AccentDark { get { return ControlPaint.Dark(Accent); } }

        public Color AccentLight { get { return ControlPaint.Light(Accent); } }

        /// <summary>the color displayed most frequently across your app's screens and components.</summary>
        public Color Primary { get; set; } = SystemColors.Highlight;

        public Color PrimaryDark { get { return ControlPaint.Dark(Primary); } }

        public Color PrimaryLight { get { return ControlPaint.Light(Primary); } }

        /// <summary>to accent select parts of your UI.</summary>
        public Color Secondary { get; set; } = SystemColors.HotTrack;

        public Color SecondaryDark { get { return ControlPaint.Dark(Secondary); } }

        public Color SecondaryLight { get { return ControlPaint.Light(Secondary); } }
    }

    public enum DisplayMode
    {
        /// <summary>Forces to use Clear Mode.</summary>
        ClearMode,
        /// <summary>Forces to use Dark Mode.</summary>
        DarkMode,
        /// <summary>
        /// Uses the Original Mode of the Form and Controls, as they were designed.
        /// </summary>
        OriginalMode,
    }

    public struct DWMCOLORIZATIONcolors
    {
        public uint ColorizationColor,
          ColorizationAfterglow,
          ColorizationColorBalance,
          ColorizationAfterglowBalance,
          ColorizationBlurBalance,
          ColorizationGlassReflectionIntensity,
          ColorizationOpaqueBlend;
    }

    [Flags]
    public enum DWMWINDOWATTRIBUTE : uint
    {
        /// <summary>
        /// Use with DwmGetWindowAttribute. Discovers whether non-client rendering is enabled. The retrieved value is of type BOOL. TRUE if non-client rendering is enabled; otherwise, FALSE.
        /// </summary>
        DWMWA_NCRENDERING_ENABLED = 1,

        /// <summary>
        /// Use with DwmSetWindowAttribute. Sets the non-client rendering policy. The pvAttribute parameter points to a value from the DWMNCRENDERINGPOLICY enumeration.
        /// </summary>
        DWMWA_NCRENDERING_POLICY,

        /// <summary>
        /// Use with DwmSetWindowAttribute. Enables or forcibly disables DWM transitions. The pvAttribute parameter points to a value of type BOOL. TRUE to disable transitions, or FALSE to enable transitions.
        /// </summary>
        DWMWA_TRANSITIONS_FORCEDISABLED,

        /// <summary>
        /// Use with DwmSetWindowAttribute. Enables content rendered in the non-client area to be visible on the frame drawn by DWM. The pvAttribute parameter points to a value of type BOOL. TRUE to enable content rendered in the non-client area to be visible on the frame; otherwise, FALSE.
        /// </summary>
        DWMWA_ALLOW_NCPAINT,

        /// <summary>
        /// Use with DwmGetWindowAttribute. Retrieves the bounds of the caption button area in the window-relative space. The retrieved value is of type RECT. If the window is minimized or otherwise not visible to the user, then the value of the RECT retrieved is undefined. You should check whether the retrieved RECT contains a boundary that you can work with, and if it doesn't then you can conclude that the window is minimized or otherwise not visible.
        /// </summary>
        DWMWA_CAPTION_BUTTON_BOUNDS,

        /// <summary>
        /// Use with DwmSetWindowAttribute. Specifies whether non-client content is right-to-left (RTL) mirrored. The pvAttribute parameter points to a value of type BOOL. TRUE if the non-client content is right-to-left (RTL) mirrored; otherwise, FALSE.
        /// </summary>
        DWMWA_NONCLIENT_RTL_LAYOUT,

        /// <summary>
        /// Use with DwmSetWindowAttribute. Forces the window to display an iconic thumbnail or peek representation (a static bitmap), even if a live or snapshot representation of the window is available. This value is normally set during a window's creation, and not changed throughout the window's lifetime. Some scenarios, however, might require the value to change over time. The pvAttribute parameter points to a value of type BOOL. TRUE to require a iconic thumbnail or peek representation; otherwise, FALSE.
        /// </summary>
        DWMWA_FORCE_ICONIC_REPRESENTATION,

        /// <summary>
        /// Use with DwmSetWindowAttribute. Sets how Flip3D treats the window. The pvAttribute parameter points to a value from the DWMFLIP3DWINDOWPOLICY enumeration.
        /// </summary>
        DWMWA_FLIP3D_POLICY,

        /// <summary>
        /// Use with DwmGetWindowAttribute. Retrieves the extended frame bounds rectangle in screen space. The retrieved value is of type RECT.
        /// </summary>
        DWMWA_EXTENDED_FRAME_BOUNDS,

        /// <summary>
        /// Use with DwmSetWindowAttribute. The window will provide a bitmap for use by DWM as an iconic thumbnail or peek representation (a static bitmap) for the window. DWMWA_HAS_ICONIC_BITMAP can be specified with DWMWA_FORCE_ICONIC_REPRESENTATION. DWMWA_HAS_ICONIC_BITMAP normally is set during a window's creation and not changed throughout the window's lifetime. Some scenarios, however, might require the value to change over time. The pvAttribute parameter points to a value of type BOOL. TRUE to inform DWM that the window will provide an iconic thumbnail or peek representation; otherwise, FALSE. Windows Vista and earlier: This value is not supported.
        /// </summary>
        DWMWA_HAS_ICONIC_BITMAP,

        /// <summary>
        /// Use with DwmSetWindowAttribute. Do not show peek preview for the window. The peek view shows a full-sized preview of the window when the mouse hovers over the window's thumbnail in the taskbar. If this attribute is set, hovering the mouse pointer over the window's thumbnail dismisses peek (in case another window in the group has a peek preview showing). The pvAttribute parameter points to a value of type BOOL. TRUE to prevent peek functionality, or FALSE to allow it. Windows Vista and earlier: This value is not supported.
        /// </summary>
        DWMWA_DISALLOW_PEEK,

        /// <summary>
        /// Use with DwmSetWindowAttribute. Prevents a window from fading to a glass sheet when peek is invoked. The pvAttribute parameter points to a value of type BOOL. TRUE to prevent the window from fading during another window's peek, or FALSE for normal behavior. Windows Vista and earlier: This value is not supported.
        /// </summary>
        DWMWA_EXCLUDED_FROM_PEEK,

        /// <summary>
        /// Use with DwmSetWindowAttribute. Cloaks the window such that it is not visible to the user. The window is still composed by DWM. Using with DirectComposition: Use the DWMWA_CLOAK flag to cloak the layered child window when animating a representation of the window's content via a DirectComposition visual that has been associated with the layered child window. For more details on this usage case, see How to animate the bitmap of a layered child window. Windows 7 and earlier: This value is not supported.
        /// </summary>
        DWMWA_CLOAK,

        /// <summary>
        /// Use with DwmGetWindowAttribute. If the window is cloaked, provides one of the following values explaining why. DWM_CLOAKED_APP (value 0x0000001). The window was cloaked by its owner application. DWM_CLOAKED_SHELL(value 0x0000002). The window was cloaked by the Shell. DWM_CLOAKED_INHERITED(value 0x0000004). The cloak value was inherited from its owner window. Windows 7 and earlier: This value is not supported.
        /// </summary>
        DWMWA_CLOAKED,

        /// <summary>
        /// Use with DwmSetWindowAttribute. Freeze the window's thumbnail image with its current visuals. Do no further live updates on the thumbnail image to match the window's contents. Windows 7 and earlier: This value is not supported.
        /// </summary>
        DWMWA_FREEZE_REPRESENTATION,

        /// <summary>
        /// Use with DwmSetWindowAttribute. Enables a non-UWP window to use host backdrop brushes. If this flag is set, then a Win32 app that calls Windows::UI::Composition APIs can build transparency effects using the host backdrop brush (see Compositor.CreateHostBackdropBrush). The pvAttribute parameter points to a value of type BOOL. TRUE to enable host backdrop brushes for the window, or FALSE to disable it. This value is supported starting with Windows 11 Build 22000.
        /// </summary>
        DWMWA_USE_HOSTBACKDROPBRUSH,

        /// <summary>
        /// Use with DwmSetWindowAttribute. Allows the window frame for this window to be drawn in dark mode colors when the dark mode system setting is enabled. For compatibility reasons, all windows default to light mode regardless of the system setting. The pvAttribute parameter points to a value of type BOOL. TRUE to honor dark mode for the window, FALSE to always use light mode. This value is supported starting with Windows 10 Build 17763.
        /// </summary>
        DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19,

        /// <summary>
        /// Use with DwmSetWindowAttribute. Allows the window frame for this window to be drawn in dark mode colors when the dark mode system setting is enabled. For compatibility reasons, all windows default to light mode regardless of the system setting. The pvAttribute parameter points to a value of type BOOL. TRUE to honor dark mode for the window, FALSE to always use light mode. This value is supported starting with Windows 11 Build 22000.
        /// </summary>
        DWMWA_USE_IMMERSIVE_DARK_MODE = 20,

        /// <summary>
        /// Use with DwmSetWindowAttribute. Specifies the rounded corner preference for a window. The pvAttribute parameter points to a value of type DWM_WINDOW_CORNER_PREFERENCE. This value is supported starting with Windows 11 Build 22000.
        /// </summary>
        DWMWA_WINDOW_CORNER_PREFERENCE = 33,

        /// <summary>
        /// Use with DwmSetWindowAttribute. Specifies the color of the window border. The pvAttribute parameter points to a value of type COLORREF. The app is responsible for changing the border color according to state changes, such as a change in window activation. This value is supported starting with Windows 11 Build 22000.
        /// </summary>
        DWMWA_BORDER_COLOR,

        /// <summary>
        /// Use with DwmSetWindowAttribute. Specifies the color of the caption. The pvAttribute parameter points to a value of type COLORREF. This value is supported starting with Windows 11 Build 22000.
        /// </summary>
        DWMWA_CAPTION_COLOR,

        /// <summary>
        /// Use with DwmSetWindowAttribute. Specifies the color of the caption text. The pvAttribute parameter points to a value of type COLORREF. This value is supported starting with Windows 11 Build 22000.
        /// </summary>
        DWMWA_TEXT_COLOR,

        /// <summary>
        /// Use with DwmGetWindowAttribute. Retrieves the width of the outer border that the DWM would draw around this window. The value can vary depending on the DPI of the window. The pvAttribute parameter points to a value of type UINT. This value is supported starting with Windows 11 Build 22000.
        /// </summary>
        DWMWA_VISIBLE_FRAME_BORDER_THICKNESS,

        /// <summary>
        /// The maximum recognized DWMWINDOWATTRIBUTE value, used for validation purposes.
        /// </summary>
        DWMWA_LAST,
    }
}
