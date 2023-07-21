using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TMP.NET.Modules;

namespace TMP.NET.WindowUI
{
    /// <summary>
    /// Interaction logic for TagSelectorWindow.xaml
    /// </summary>
    public partial class TagSelectorWindow : Window
    {
        //private Config.TagList _tagList;
        private string[] tag = { "Tag1", "Tag2", "Tag3" };
        private string[] selectedTag = { "Tag1", "Tag3" };
        public TagSelectorWindow()
        {
            InitializeComponent();

            FirstLoad();
        }

        private void FirstLoad()
        {
            //tagListView.ItemsSource = _tagList.Tag;
            tagListView.ItemsSource = tag;
        }
    }
}
