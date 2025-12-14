namespace Modulverwaltungssoftware.Services
{
    internal class LoginService
    {
        public string Login(string benutzername, string passwort) // DB einbinden
        {
            if (benutzername == "admin" && passwort == "admin123")
            {
                return "99";
            }
            if (benutzername == "Gast" && passwort == "")
            {
                return "0";
            }
            if (benutzername == "Dozent" && passwort == "dozent123")
            {
                return "1";
            }
            if (benutzername == "Koordination" && passwort == "koordination123")
            {
                return "2";
            }
            if (benutzername == "Gremium" && passwort == "gremium123")
            {
                return "3";
            }
            else return "false";
        }
    }
}
