using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Net.Http.Headers;

namespace statisztikazh1
{
    internal class Program
    {
        static void print(string str)
        {
            Console.WriteLine(str);
        }

        class Adat
        {
            public static double gyakorissagSzum { get; set; }
            public static double relgyakSzum { get; set; }



            public double bevKezd { get; private set; }
            public double bevVeg { get; private set; }
            public double gyakorisag { get; private set; }



            public double Osztalykozep { get; private set; }
            public double Hossz { get; private set; }
            public double relgyakorisag { get; private set; }
            public double kumgyak { get; private set; }
            public double relkumgyak{ get; private set; }
            public double letkumgyak{ get; private set; }
            public double letkumrelgyak{ get; private set; }
            public double egyseghosszgyak{ get; private set; }



            public double atlvaloelteres { get; private set; }
            public double absatlvaloelteres { get; private set; }
            public double powatlvaloelteres { get; private set; }

            public Adat(string row)
            {
                if (row == "")
                    return;

                var splitted = row.Split(';');

                this.bevKezd = double.Parse(splitted[0]);
                this.bevVeg = double.Parse(splitted[1]);
                this.gyakorisag = double.Parse(splitted[2]);

                Adat.gyakorissagSzum += this.gyakorisag;

                this.Osztalykozep = (this.bevKezd + this.bevVeg) / 2;
                this.Hossz = this.bevVeg - this.bevKezd;
                this.egyseghosszgyak = this.gyakorisag / this.Hossz;
            }

            public void setRelGyakorisag()
            {
                this.relgyakorisag = this.gyakorisag / Adat.gyakorissagSzum * 100;
                Adat.relgyakSzum += this.relgyakorisag;
            }

            public void setKumGyak(double kumgyak)
            {
                this.kumgyak = kumgyak;
            }

            public void setRelkumGyak()
            {
                this.relkumgyak = this.kumgyak / Adat.gyakorissagSzum;
            }

            public void setLetkumgyak(double letkumgyak)
            {
                this.letkumgyak = letkumgyak;
            }

            public void setLetkumrelgyak()
            {
                this.letkumrelgyak = this.letkumgyak / Adat.gyakorissagSzum;
            }

            public void setAtlvaloelteres(double atlagbevetel, double pow)
            {
                this.atlvaloelteres = this.Osztalykozep - atlagbevetel;
                this.absatlvaloelteres = Math.Abs(this.atlvaloelteres);
                this.powatlvaloelteres = Math.Pow(this.atlvaloelteres, pow);
            }
        }

        static double szorzatosszeg(List<double> tomb1, List<double> tomb2) 
        {
            double osszeg = 0;

            if (tomb1.Count != tomb2.Count)
            {
                throw new ArgumentException("A két tömbnek ugyanakkora hosszúnak kell lennie.");
            }

            for (int i = 0; i < tomb1.Count; i++)
            {
                osszeg += tomb1[i] * tomb2[i];
            }

            return osszeg;
        }

        static void Main(string[] args)
        {
            var filePath = "adatok.txt";

            var adatok = new List<Adat>();

            foreach (var line in File.ReadAllLines(filePath))
            {
                adatok.Add(new Adat(line));
            }

            foreach (var adat in adatok)
                adat.setRelGyakorisag();


            double szum = 0;
            foreach (var adat in adatok)
            {
                szum += adat.gyakorisag;
                adat.setKumGyak(szum);
                adat.setRelkumGyak();
            }

            szum = 0;
            foreach(var adat in adatok.Select(e => e).Reverse().ToList())
            {
                szum += adat.gyakorisag;
                adat.setLetkumgyak(szum);
                adat.setLetkumrelgyak();
            }

            var osszdbszam = Adat.gyakorissagSzum;

            var gyakorissagList = adatok.Select(e => e.gyakorisag).ToList();
            var osztalykozepList = adatok.Select(e => e.Osztalykozep).ToList();
            var osszbevetel = szorzatosszeg(gyakorissagList, osztalykozepList);

            var atlagbevetel = osszbevetel / osszdbszam;



            // Write \\

            var file = File.CreateText("result.csv");

            var headers = new List<string>()
            {
                "Bevetel",
                " ",
                " ",
                "Gyakorisag",
                "Osztalykozep",
                "Hossz",
                "Relativ gyak.",
                "Kum. gyak.",
                "Kum. rel. gyak.",
                "Let. kum. gyak.",
                "Let. kum. rel. gyak.",
                "Egysegnyi osztalyhosszra juto gyak.",
            };

            file.WriteLine(string.Join(";", headers));

            foreach(var adat in adatok)
            {
                var text = $"{adat.bevKezd};-;{adat.bevVeg};{adat.gyakorisag};{adat.Osztalykozep};{adat.Hossz};{Math.Round(adat.relgyakorisag, 4)}%;{adat.kumgyak};{Math.Round(adat.relkumgyak * 100, 4)}%;{adat.letkumgyak};{Math.Round(adat.letkumrelgyak*100, 4)}%;{adat.egyseghosszgyak}";

                file.WriteLine(text);
            }

            file.WriteLine($"Összesen:; ; ;{Adat.gyakorissagSzum}; ; ;{Math.Round(Adat.relgyakSzum, 4)}%");
            file.WriteLine(";");
            file.WriteLine($"Cegek osszdarabszama:;{osszdbszam}");
            file.WriteLine($"Cegek osszbevetel:;{osszbevetel}");
            file.WriteLine($"Atlagos bevetel:;{Math.Round(atlagbevetel, 4)}");


            // Módusz \\
            var row = adatok.Select(e => e).OrderByDescending(e => e.egyseghosszgyak).First();
            var rowIndex = adatok.FindIndex(e => e == row);

            var mo = row.bevKezd;
            var h = row.Hossz;
            var k1 = row.egyseghosszgyak - adatok[rowIndex - 1].egyseghosszgyak;
            var k2 = row.egyseghosszgyak - adatok[rowIndex + 1].egyseghosszgyak;

            var Mo = mo + k1 / (k1 + k2) * h; // Módusz eredmény \\

            file.WriteLine(";");
            file.WriteLine(";");
            file.WriteLine("Modusz:; ;Mo=mo+k_1/(k_1+k_2)*h");
            file.WriteLine(";");
            file.WriteLine($"mo=;{mo}");
            file.WriteLine($"h=;{h}");
            file.WriteLine($"k_1=;{k1}");
            file.WriteLine($"k_2=;{k2}");
            file.WriteLine(";");
            file.WriteLine($"Mo=;{Math.Round(Mo, 4)}");


            // Medián \\
            var N = Adat.gyakorissagSzum;
            var Np2 = N / 2;
            row = adatok.Select(e => e).OrderBy(e => Math.Abs(e.bevKezd - Np2)).First();
            rowIndex = adatok.FindIndex(e => e == row);

            var me = row.bevKezd;
            var fmeMinus1 = adatok[rowIndex - 1].kumgyak;
            var fme = row.gyakorisag;
            h = row.Hossz;

            var Me = me + (Np2 - fmeMinus1) / fme * h; // Medián eredmény 

            file.WriteLine(";");
            file.WriteLine(";");
            file.WriteLine("Median:; ;Me=me+(N/2-f'_(me-1))/f_me*h");
            file.WriteLine(";");
            file.WriteLine($"N=;{N}");
            file.WriteLine($"N/2=;{Np2}");
            file.WriteLine($"me=;{me}");
            file.WriteLine($"f'_(me-1)=;{fmeMinus1}");
            file.WriteLine($"f'_me=;{fme}");
            file.WriteLine($"h=;{h}");
            file.WriteLine(";");
            file.WriteLine($"Me=;{Math.Round(Me, 4)}");


            // Q1 \\
            N = Adat.gyakorissagSzum;
            var Np4 = N / 4;

            row = adatok.Select(e => e).OrderBy(e => Math.Abs(e.bevKezd - Np4)).First();
            rowIndex = adatok.FindIndex(e => e == row);

            var q1 = row.bevKezd;
            var fq1minus1 = adatok[rowIndex - 1].kumgyak;
            var fq1 = row.gyakorisag;
            h = row.Hossz;

            var Q1 = q1 + (Np4 - fq1minus1) / fq1 * h; // Q1 eredmény \\

            file.WriteLine(";");
            file.WriteLine(";");
            file.WriteLine("Q1:; ;Q1=q1+(N/4-f'_(q1-1))/f_q1*h");
            file.WriteLine(";");
            file.WriteLine($"N=;{N}");
            file.WriteLine($"N/4=;{Np4}");
            file.WriteLine($"q1=;{q1}");
            file.WriteLine($"f'_(q1-1)=;{fq1minus1}");
            file.WriteLine($"f'_q1=;{fq1}");
            file.WriteLine($"h=;{h}");
            file.WriteLine(";");
            file.WriteLine($"Q1=;{Math.Round(Q1, 4)}");


            // Q3 \\
            N = Adat.gyakorissagSzum;
            Np4 = N / 4;
            var Np4mul3 = 3 * N / 4;

            row = adatok.Select(e => e).OrderBy(e => Math.Abs(e.bevKezd - Np4mul3)).First();
            rowIndex = adatok.FindIndex(e => e == row);

            var q3 = row.bevKezd;
            var fq3Minus1 = adatok[rowIndex - 1].kumgyak;
            var fq3 = row.gyakorisag;
            h = row.Hossz;

            var Q3 = q3 + (Np4mul3 - fq3Minus1) / fq3 * h; // Q3 eredmény \\

            file.WriteLine(";");
            file.WriteLine(";");
            file.WriteLine("Q3:; ;Q3=q3+(3N/4-f'_(q3-1))/f_q3*h");
            file.WriteLine(";");
            file.WriteLine($"N=;{N}");
            file.WriteLine($"3*N/4=;{Np4mul3}");
            file.WriteLine($"q3=;{q3}");
            file.WriteLine($"f'_(q3-1)=;{fq3Minus1}");
            file.WriteLine($"f'_q3=;{fq3}");
            file.WriteLine($"h=;{h}");
            file.WriteLine(";");
            file.WriteLine($"Q3=;{Math.Round(Q3, 4)}");




            // Eltérés \\
            file.WriteLine(";");
            file.WriteLine(";");
            file.WriteLine("Elteres:");
            file.WriteLine(";");

            foreach (var adat in adatok)
            {
                adat.setAtlvaloelteres(atlagbevetel, 2);
            }



            gyakorissagList = adatok.Select(e => e.gyakorisag).ToList();
            var AbsatlagvaloelteresList = adatok.Select(e => e.absatlvaloelteres).ToList();
            var PowatlagvaloelteresList = adatok.Select(e => e.powatlvaloelteres).ToList();

            var sulyozottatlagAbs = szorzatosszeg(gyakorissagList, AbsatlagvaloelteresList) / Adat.gyakorissagSzum;
            var sulyozottatlagPow = szorzatosszeg(gyakorissagList, PowatlagvaloelteresList) / Adat.gyakorissagSzum;

            var atlelteres = sulyozottatlagAbs;
            var szorasnegyzet = sulyozottatlagPow;
            var szoras = Math.Sqrt(szorasnegyzet);
            var relSzoras = szoras / atlagbevetel;


            file.WriteLine($"Gyakorissg;Osztalykozep;Atl. valo elteres;abs(elteres);elteres^2");
            
            foreach(var adat in adatok)
            {
                var text = $"{adat.gyakorisag};{adat.Osztalykozep};{adat.atlvaloelteres};{adat.absatlvaloelteres};{adat.powatlvaloelteres}";
                file.WriteLine(text);
            }
            file.WriteLine($"Sulyozott atlagok:; ; ;{sulyozottatlagAbs};{sulyozottatlagPow}");
            file.WriteLine(";");
            file.WriteLine($"Atlagos elteres:;{Math.Round(sulyozottatlagAbs, 4)}");
            file.WriteLine($"Szorasnegyzet:;{Math.Round(sulyozottatlagPow, 4)}");
            file.WriteLine($"szoras:;{Math.Round(szoras, 4)}");
            file.WriteLine($"relativ szoras:;{Math.Round(relSzoras * 100, 4)}%");

            file.Close();


            Console.WriteLine("result.csv fájl elkészült!\n");

            file = File.CreateText("result.txt");

            foreach(var line in File.ReadAllLines("result.csv"))
            {
                file.WriteLine(line.Replace(";", "\t"));
                Console.WriteLine(line.Replace(";", "\t"));
            }
            

            file.Close();

            Console.WriteLine("\n\nresult.txt fájl elkészült!\n");

            Console.ReadKey();
        }
    }
}
