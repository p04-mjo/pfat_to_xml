using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Xml; //tillagt, bortvalt
using System.IO; //tillagt



namespace Softhouse_pfat_to_xml_mini
{

    /// <summary>
    /// base class for person and family objects
    /// </summary>
    public class Avatar
    {
        protected string cell = "";
        protected string landline = "";
        protected string street = "";
        protected string city = "";
        protected string zip = "";
        protected string[] unexpected;

        // T|mobilnummer|fastnätsnummer 
        // kan vara inkompletta poster, vi förutsätter att det är den/de sista som lämnats om någon/några saknas
        public void Phone(string pipeline)
        {
            string[] pipelist = pipeline.Split('|');
            try { this.cell = pipelist[1].Trim(); } catch { };
            try { this.landline = pipelist[2].Trim(); } catch { };
        }

        // build phone xml
        public string get_XmlPhone(byte tabs)
        {
            string data = "";
            string ts = "";
            for (byte i = 0; i < tabs; i++)
            {
                ts += '\t';
            }
            if (this.cell.Length > 0 || this.landline.Length > 0)
            {
                data += ts + "<phone>" + Environment.NewLine;
                if (this.cell.Length > 0) { data += ts + "\t<cell>" + this.cell + "</cell>" + Environment.NewLine; }
                if (this.landline.Length > 0) { data += ts + "\t<landline>" + this.landline + "</landline>" + Environment.NewLine; }
                data += ts + "</phone>" + Environment.NewLine;
            }
            return data;
        }

        // build address xml
        public string get_XmlAddress(byte tabs)
        {
            string data = "";
            string ts = "";
            for (byte i = 0; i < tabs; i++)
            {
                ts += '\t';
            }
            if (this.street.Length > 0 || this.city.Length > 0 || this.zip.Length > 0)
            {
                data += ts + "<address>" + Environment.NewLine;
                if (this.street.Length > 0) { data += ts + "\t<street>" + this.street + "</street>" + Environment.NewLine; }
                if (this.city.Length > 0) { data += ts + "\t<city>" + this.city + "</city>" + Environment.NewLine; }
                if (this.zip.Length > 0) { data += ts + "\t<zip>" + this.zip + "</zip>" + Environment.NewLine; }
                data += ts + "</address>" + Environment.NewLine;
            }
            return data;
        }

        // A|gata|stad|postnummer  
        // kan vara inkompletta poster, vi förutsätter att det är den/de sista som lämnats om någon/några saknas
        public void Address(string pipeline)
        {
            string[] pipelist = pipeline.Split('|');
            try { this.street = pipelist[1].Trim(); } catch { };
            try { this.city = pipelist[2].Trim();} catch { };
            try { this.zip = pipelist[3].Trim(); } catch { };
        }
    }

    /// <summary>
    /// Class for person-objects
    /// </summary>
    public class Person : Avatar
    {
        private string firstname = "";
        private string lastname = "";
        public List<FamilyMember> family = new List<FamilyMember>();

        // P constructor
        public Person()
        {
        }

        // P|förnamn|efternamn  
        // kan vara inkompletta poster, vi förutsätter att det är den/de sista som lämnats om någon/några saknas
        public void Name(string pipeline)
        {
            string[] pipelist = pipeline.Split('|');
            try { this.firstname = pipelist[1].Trim(); } catch { };
            try { this.lastname = pipelist[2].Trim(); } catch { };
        }

        // build object xml
        // we expect firstname and lastname to always be populated
        public string get_xml()
        {
            string family_xml = "";
            foreach (FamilyMember fm in family)
            {
                family_xml += fm.get_xml() + Environment.NewLine;
            }
            string data = "<person>" + Environment.NewLine
                + "\t<firstname>" + this.firstname + "</firstname>" + Environment.NewLine
                + "\t<lastname>" + this.lastname + "</lastname>" + Environment.NewLine
                + base.get_XmlAddress(1) + Environment.NewLine
                + base.get_XmlPhone(1) + Environment.NewLine
                + family_xml + Environment.NewLine
                + "</person>" + Environment.NewLine;
            return data;
        }
    }

    /// <summary>
    /// Class for family member-objects
    /// </summary>
    public class FamilyMember : Avatar
    {
        private string name = "";
        private string dob = "";

        // F constructor
        public FamilyMember()
        {
        }

        // F|namn|födelseår  
        public void Name(string pipeline)
        {
            string[] pipelist = pipeline.Split('|');
            try { this.name = pipelist[1].Trim(); } catch { };
            try { this.dob = pipelist[2].Trim(); } catch { }; 
        }

        // build object xml
        // we expect name and dob to always be populated
        public string get_xml()
        {
            string data = "\t<family>" + Environment.NewLine
                + "\t\t<name>" + this.name + "</name>" + Environment.NewLine
                + "\t\t<dob>" + this.dob + "</dob>" + Environment.NewLine
                + base.get_XmlAddress(2) + Environment.NewLine
                + base.get_XmlPhone(2) + Environment.NewLine
                + "\t</family>" + Environment.NewLine;
            return data;
        }
    }


    
    /* Flöde
     * 
     * vi förutsätter tills vidare att indata är korrekt, ingen kod för felhantering (optimistiskt, jag vet)
     * vi förutsätter att listorna är av rimlig storlek och gott och väl får plats i internminnet
     * vi jobbar initialt med att testdata ligger i Program.cs som en array av strängar
     * vi tänker oss att utdata, xml-varianten, skrivs till Console tills vidare
     * 
     * vi väljer att skriva vår egen xml-parser trots att vi vet att det finns stöd i klassen Xml för detta, 
     * detta då den främsta fördelen med att använda klassen är att man jobbar mot disk och inte mot minne 
     * förlorar över den komplexitet som måste införas (många rader kod i onödan för mig med liten skärm)
     * för mig med liten skärm... kanske växlar vi sedan när denna modell fungerar?
     * 
     * läs indata och lägg i lista av objekt
     * 
     * bool pExist = false
     * bool fExist = false
     * skapa p-objekt (för att få igenom koden)
     * skapa f-objekt (för att få igenom koden)
     * 
     * för varje rad:
         * Om P
         *  om fExist, skriv f till f-lista, skriv p till p-lista
         *  om pExist, skriv p till p-lista
         *  skapa nytt p
         *  sätt pExist, !fExist
         *  populera p med förnamn, efternamn
         * 
         * Om F
         *  om fExist, skriv f till f-lista
         *  skapa nytt f
         *  sätt fExist
         *  populera f med name, dob
         *  
         * Om A
         *  om fExist populera aktuellt f med A
         *  annars (!fExist), populera aktuellt p med A
         *  
         * Om T
         *  om fExist, populera aktullt f med T
         *  annars (!fExist), populera aktuellt p med T
     *  om fExist, skriv f till f-lista och p till p-lista
     *  om !fExist men pExist (minst en post), skriv p till p-lista
     *
     * 
     * för varje p i p-lista:
        *  skriv xml(namn)
        *  om T angivet, skriv xml(T)
     *  *  om A angivet, skriv xml(A)
     *  *  för varje f i f-lista: (kanske finns inga men då kör vi noll varv)
     *        * skriv xml(namn, dob)
     *        * om T angivet, skriv xml(T)
     *        * om A angivet, skriv xml(A)
     * 
     * 
     * 
     */
    /// <summary>
    /// Main Program, reads data from file, add data to array of objects and finally prints it and pushes it to an output file. 
    /// uses the schema above to push data from input file to an array of objects
    /// </summary>
    class Program
    {
        public const string xmlPath = ""; //where xml-file is created
        public const string xmlFile = "people.xml"; //name for created xml-file

        public const string ptafPath = "";
        public const string ptafFile = "indata.txt";

        static void Main(string[] args)
        {
            string[] testdata_original = {"P|Elof|Sundin", 
                "T|073-101801|018-101801", 
                "A|S:t Johannesgatan 16|Uppsala|75330",
                "F|Hans|1967",
                "A|Frodegatan 13B|Uppsala|75325",
                "F|Anna|1969", 
                "T|073-101802|08-101802",
                "P|Boris|Johnson",
                "A|10 Downing Street|London"};

            string[] testdata_extra = {"P|Elof|Sundin", 
                "T|073-101801|018-101801", 
                "A|S:t Johannesgatan 16|Uppsala|75330",
                "F|Hans|1967",
                "A|Frodegatan 13B|Uppsala|75325",
                "F|Anna|1969", 
                "T|073-101802|08-101802",
                "F|Petra|2001", 
                "F|Sandra|2000", 
                "A|Frodegatan 12A|Jönköping",
                "P|Boris|Johnson",
                "S|Jönköpingsposten",
                "A|10 Downing Street|London"};


            //hämta indata från fil
            string[] testdata = { };
            try { testdata = File.ReadAllLines(Path.Combine(ptafPath, ptafFile)); } catch { };

            bool pExist = false;
            bool fExist = false;
            List<Person> people = new List<Person>();
            Person person = new Person();
            FamilyMember familyMember = new FamilyMember();

            foreach (string pipeline in testdata)
            {
                switch (pipeline[0])
                {
                    case 'P': // P|förnamn|efternamn 
                        if (fExist && pExist)
                        {
                            person.family.Add(familyMember);
                            people.Add(person);
                        }
                        if (!fExist && pExist)
                        {
                            people.Add(person);
                        }
                        person = new Person();
                        pExist = true;
                        fExist = false;
                        person.Name(pipeline);
                        break;
                    case 'T': // T|mobilnummer|fastnätsnummer 
                        if (fExist)
                        {
                            familyMember.Phone(pipeline);
                        }
                        if (!fExist && pExist)
                        {
                            person.Phone(pipeline);
                        }
                        break;
                    case 'F': // F|namn|födelseår 
                        if (fExist)
                        {
                            person.family.Add(familyMember);
                        }
                        familyMember = new FamilyMember();
                        fExist = true;
                        familyMember.Name(pipeline);
                        break;
                    case 'A': // A|gata|stad|postnummer  
                        if (fExist)
                        {
                            familyMember.Address(pipeline);
                        }
                        if (!fExist && pExist)
                        {
                            person.Address(pipeline);
                        }
                        break;
                    default: // undefind tag found, add to current object?
                        break;
                }
            }
            if (fExist)
            {
                person.family.Add(familyMember);
                people.Add(person);
            }
            if (!fExist && pExist)
            {
                people.Add(person);
            }

            // testutskrift till console nedan
            string xml_converted = "";
            //Console.WriteLine("<people>");
            xml_converted += "<people>" + Environment.NewLine;
            foreach (Person p in people)
            {
                //Console.WriteLine(p.get_xml());
                xml_converted += p.get_xml();
            }
            xml_converted += "</people>";
            //Console.WriteLine("</people>");
            //Console.WriteLine(xml_converted);

            // städa bort tomma rader (borde skapat xml-koden snyggare egentlingen)
            string[] lines = xml_converted.Split(new string[] { Environment.NewLine },StringSplitOptions.None);
            foreach (string line in lines)
            {
                if (!String.IsNullOrEmpty(line))
                {
                    Console.WriteLine(line);
                }
            }

            

            //string docPath = "";
            using (StreamWriter outFile = new StreamWriter(Path.Combine(xmlPath, xmlFile)))
            {
                foreach (string line in lines)
                {
                    if (!String.IsNullOrEmpty(line)) //only write non-empty lines to file!
                    {
                        outFile.WriteLine(line);
                    }
                }
            }

            // invänta tangentbordet innan standard output släcks!
            Console.ReadKey();
        }
    }
}
