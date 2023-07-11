using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace TMP.NET.Modules
{
    /// <summary>
    /// Controlling ListView sort filter
    /// </summary>
    internal class FilterController
    {
        private Config.FilterConfig filterSetting;
        private ObservableCollection<GameList> i_List = new ObservableCollection<GameList>();
        private ListView LV_List;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filterConfig"></param>
        /// <param name="i_List"></param>
        /// <param name="LV_List"></param>
        public FilterController(Config.FilterConfig filterConfig, ObservableCollection<GameList> i_List, ListView LV_List)
        {
            this.filterSetting = filterConfig;
            this.i_List = i_List;
            this.LV_List = LV_List;
        }

        public void FilterControl()
        {
            if (filterSetting.SortIndex == 0)
            {
                switch (filterSetting.FilterIndex)
                {
                    case 0:
                        i_List = new ObservableCollection<GameList>(i_List.OrderBy(item => item.GameName));
                        break;
                    case 1:
                        i_List = new ObservableCollection<GameList>(i_List.OrderBy(item => item.Playtime));
                        break;
                    default:
                        i_List = new ObservableCollection<GameList>(i_List.OrderByDescending(item => item.Tracker));
                        break;
                }
            }
            else
            {
                switch (filterSetting.FilterIndex)
                {
                    case 0:
                        i_List = new ObservableCollection<GameList>(i_List.OrderByDescending(item => item.GameName));
                        break;
                    case 1:
                        i_List = new ObservableCollection<GameList>(i_List.OrderByDescending(item => item.Playtime));
                        break;
                    default:
                        i_List = new ObservableCollection<GameList>(i_List.OrderBy(item => item.Tracker));
                        break;
                }
            }


            // Refresh ListView
            LV_List.ItemsSource = i_List;
            CollectionViewSource.GetDefaultView(i_List).Refresh();
        }
    }
}
