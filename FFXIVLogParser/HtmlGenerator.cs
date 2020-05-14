using FFXIVLogParser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Highsoft.Web.Mvc.Charts;
using System.Diagnostics;

namespace FFXIVLogParser
{
    class HtmlGenerator
    {
        private readonly List<Encounter> encounters;
        private readonly Encounter currentEncounter;

        StreamWriter htmlWriter;
        StringBuilder htmlBuilder = new StringBuilder();

        readonly DirectoryInfo encounterFolder;

        public HtmlGenerator(DirectoryInfo directoryInfo, List<Encounter> encounters)
        {
            encounterFolder = directoryInfo;

            DirectoryCopy(@"C:\Users\bryce\Desktop\FFXIVLogParser\FFXIVLogParser\Assets", directoryInfo.Parent.FullName, true);

            this.encounters = encounters;

            int index = 1;
            foreach (Encounter encounter in encounters)
            {
                currentEncounter = encounter;
                GenerateHtml(index);
                index++;
            }
        }

        public void GenerateHtml(int encounterNumber)
        {
            htmlWriter = File.CreateText(Path.Combine(encounterFolder.FullName, $"Encounter {encounterNumber}.html"));

            htmlWriter.WriteLine("<html>");
            htmlWriter.WriteLine("<link rel=\"stylesheet\" href=\"https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/css/bootstrap.min.css\" integrity=\"sha384-Vkoo8x4CGsO3+Hhxv8T/Q5PaXtkKtu6ug5TOeNV6gBiFeWPGFN9MuhOf23Q9Ifjh\" crossorigin=\"anonymous\">");
            htmlWriter.WriteLine("<script src = \"https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/js/bootstrap.min.js\" integrity = \"sha384-wfSDF2E50Y2D1uUdj0O3uMBJnjuUD4Ih7YwaYd1iqfktj0Uod8GCExl3Og8ifwB6\" crossorigin = \"anonymous\"> </script>");

            htmlWriter.WriteLine("<link rel = \"stylesheet\" href = \"..\\CSS\\classes.css\">");

            htmlWriter.WriteLine("<body>");

            GeneratePartyComposition();
            
            htmlWriter.WriteLine("</body>");
            htmlWriter.Write("</html>");

            htmlWriter.Close();
        }

        private void GeneratePartyComposition()
        {
            List<Combatant> combatants = currentEncounter.GetPartyCombatants();

            List<Combatant> tanks = combatants.Where(combatant => combatant.JobInformation.JobCategory.Equals(JobType.Tank)).ToList();
            List<Combatant> healers = combatants.Where(combatant => combatant.JobInformation.JobCategory.Equals(JobType.Healer)).ToList();
            List<Combatant> DPS = combatants.Where(combatant => combatant.JobInformation.JobCategory.Equals(JobType.DPS)).ToList();

            htmlBuilder = new StringBuilder("<table class=\"table table-dark table-condensed\">");
            htmlBuilder.AppendLine("<thead>");
            htmlBuilder.AppendLine("<tr>");
            htmlBuilder.AppendLine("<th scope=\"col\">Party Composition</th>");
            htmlBuilder.AppendLine("</tr>");
            htmlBuilder.AppendLine("</thead>");
            htmlBuilder.AppendLine("<tbody>");
            htmlBuilder.AppendLine("<tr>");
            htmlBuilder.AppendLine("<th scope=\"row\">Tanks:</th>");
            htmlBuilder.AppendLine("<td>");

            foreach(Combatant tank in tanks)
            {
                htmlBuilder.AppendLine($"<span class=\"{tank.JobInformation.JobName.Replace(' ', '_')}\"><img src=\"..\\Icons\\{tank.JobInformation.JobName.Replace(' ', '_')}.png\" alt=\"{tank.JobInformation.JobName.Replace(' ', '_')}\">{tank.Name}</span>");
            }
            
            htmlBuilder.AppendLine("</td>");
            htmlBuilder.AppendLine("</tr>");
            htmlBuilder.AppendLine("<tr>");
            htmlBuilder.AppendLine("<th scope=\"row\">DPS:</th>");
            htmlBuilder.AppendLine("<td>");

            foreach (Combatant dps in DPS)
            {
                htmlBuilder.AppendLine($"<span class=\"{dps.JobInformation.JobName.Replace(' ', '_')}\"><img src=\"..\\Icons\\{dps.JobInformation.JobName.Replace(' ', '_')}.png\" alt=\"{dps.JobInformation.JobName.Replace(' ', '_')}\">{dps.Name}</span>");
            }

            htmlBuilder.AppendLine("</td>");
            htmlBuilder.AppendLine("</tr>");
            htmlBuilder.AppendLine("<tr>");
            htmlBuilder.AppendLine("<th scope=\"row\">Healers:</th>");
            htmlBuilder.AppendLine("<td>");

            foreach (Combatant healer in healers)
            {
                htmlBuilder.AppendLine($"<span class=\"{healer.JobInformation.JobName.Replace(' ', '_')}\"><img src=\"..\\Icons\\{healer.JobInformation.JobName.Replace(' ', '_')}.png\" alt=\"{healer.JobInformation.JobName.Replace(' ', '_')}\">{healer.Name}</span>");
            }

            htmlBuilder.AppendLine("</td>");
            htmlBuilder.AppendLine("</tr>");
            htmlBuilder.AppendLine("</tbody>");
            htmlBuilder.AppendLine("</table>");


            htmlWriter.WriteLine(htmlBuilder.ToString());
        }

        private string GenerateCharts()
        {
            return "";
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }
            else
            {
                return;
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}
