using MissedTeacherRate.Algorithms.COMAS.Models;
using MissedTeacherRate.Models;
using System.Collections.ObjectModel;

namespace MissedTeacherRate.WPF.ViewModel
{
    public class TrustDisplayItem
    {
        public string FirstPerson { get; set; } = string.Empty;
        public string FirstPersonType { get; set; } = string.Empty;
        public string SecondPerson { get; set; } = string.Empty;
        public string SecondPersonType { get; set; } = string.Empty;
        public string TrustValue { get; set; } = string.Empty;
        public string TrustType { get; set; } = string.Empty;
    }

    public class ComasTrustWindowViewModel
    {
        public ObservableCollection<TrustDisplayItem> TrustItems { get; } = new();

        public ComasTrustWindowViewModel(List<Trust<Person, Person>> trusts)
        {
            foreach (var trust in trusts)
            {
                TrustItems.Add(new TrustDisplayItem
                {
                    FirstPerson = trust.First.Id,
                    FirstPersonType = GetPersonType(trust.First),
                    SecondPerson = trust.Second.Id,
                    SecondPersonType = GetPersonType(trust.Second),
                    TrustValue = trust.Value?.ToString("0.##") ?? "N/A",
                    TrustType = trust.Type.ToString()
                });
            }
        }

        private static string GetPersonType(Person person)
        {
            return person switch
            {
                Teacher => "Teacher",
                Student => "Student",
                _ => "Unknown"
            };
        }
    }
}
