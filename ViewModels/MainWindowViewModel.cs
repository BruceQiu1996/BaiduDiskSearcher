using BaiduSearch.helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaiduSearch.ViewModels
{
    public class MainWindowViewModel
    {
        public DelegateCommand SearchCommand { get; set; }

        public MainWindowViewModel() 
        {
            SearchCommand = new DelegateCommand(Search);
        }

        private void Search(object obj) 
        {
            
        }
    }
}
