using System;

namespace Modulverwaltungssoftware
{
    internal class PlausibilitaetsService
    {
        public string pruefeWorkloadStandard(int stunden, int ects)
        {
            if (stunden / ects != 30)
            {
                return "Der Workload entspricht nicht dem Standard von 30 Stunden pro ECTS.";
            }
            else if (stunden / 30 >= 2.5 && stunden / 30 <= 15)
            {
                return "Der Workload entspricht dem Standard.";
            }
            else if (stunden / 30 >= 15 && stunden / 30 <= 30)
            {
                return "Ungewöhnlich hoher Workload. Bitte stellen Sie sicher, dass dies beabsichtigt ist.";
            }
            else
            {
                return "Der Workload liegt außerhalb des üblichen Bereichs. Bitte prüfen Sie, ob ein Eingabefehler vorliegt.";
            }
        }
        public void pruefeForm()
        {
            throw new NotImplementedException();
        }
    }
}
