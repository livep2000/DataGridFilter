using System;
using System.Diagnostics;
using System.Windows;

namespace DemoAppNet5
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow
   {
      #region Public Constructors

      public MainWindow()
      {
         InitializeComponent();

#if DEBUG
         AppDomain.CurrentDomain.FirstChanceException += (source, e) =>
         {
                
             Debug.WriteLine("FirstChanceException event raised in " +
                             $"{AppDomain.CurrentDomain.FriendlyName}: {e.Exception.Message} {source}");
         };
#endif
         DataContext = new ModelView.ModelView();
      }

        #endregion Public Constructors
        private void SaveFilterButton_Click(object sender, RoutedEventArgs e)
        {
            this.FilterDataGrid.SaveFilters("filters.json");
        }

        private void LoadFilterButton_Click(object sender, RoutedEventArgs e)
        {
            this.FilterDataGrid.LoadFilters( "filters.json");
        }

    }
}