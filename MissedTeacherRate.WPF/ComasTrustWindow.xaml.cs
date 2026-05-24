using MissedTeacherRate.Algorithms.COMAS.Models;
using MissedTeacherRate.Models;
using MissedTeacherRate.WPF.ViewModel;
using System.Windows;

namespace MissedTeacherRate.WPF
{
    public partial class ComasTrustWindow : Window
    {
        public ComasTrustWindow(List<Trust<Person, Person>> trusts)
        {
            InitializeComponent();
            DataContext = new ComasTrustWindowViewModel(trusts);
            TrustGraph.SetTrusts(trusts);
        }
    }
}
