namespace VacationExplorer
{
    public class ComboBoxItem
    {
        public string data { get; set; }
        public string display { get; set; }

        public ComboBoxItem (string data, string display)
        {
            this.data = data;
            this.display = display;
        }
    }
}
