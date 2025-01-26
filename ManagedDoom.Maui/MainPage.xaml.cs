using DrawnUi.Maui.Draw;

namespace ManagedDoom.Maui
{
    public partial class MainPage
    {

        public MainPage()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception e)
            {
                Super.DisplayException(this, e);
            }
        }


    }

}
