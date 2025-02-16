﻿using DrawnUi.Maui.Draw;
using Orbit.Input;

namespace ManagedDoom.Maui
{
    public partial class MainPage
    {

        public MainPage()
        {
            try
            {
                InitializeComponent();
#if IOS
                Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetPrefersHomeIndicatorAutoHidden(this, true);
#endif

            }
            catch (Exception e)
            {
                Super.DisplayException(this, e);
            }
        }
    }

}
