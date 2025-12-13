namespace Modulverwaltungssoftware
{
    public class Rolle
    {
        public string RollenName { get; set; }
        public enum RollenID
        {
            Gast = 0,
            Dozent = 1,
            Koordination = 2,
            Gremium = 3,
            Admin = 99
        }
        public bool DarfBearbeiten { get; set; }
        public bool DarfFreigeben { get; set; }
        public bool DarfKommentieren { get; set; }
        public bool DarfStatusAendern { get; set; }
    }
}
