using MiscUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExternalSel
{
    public class Randomizer
    {
        public Randomizer(ThreadLocal<Random> rng)
        {
            rnd = rng;
        }

        private static readonly string[] firstMaleNames = { "Jack", "Jaydan", "Hugh", "Jaidyn", "Mario", "Cullen", "Octavio", "Raymond", "Harrison", "Kadin", "Maurice", "Layton", "Jamir", "Alden", "Bennett", "Bruno", "Izaiah", "Ignacio", "Aidyn", "Cristofer", "Sidney", "Marquise", "Deshawn", "Felix", "Conner", "Jose", "Gilbert", "Clark", "Ishaan", "Tyree", "Baron", "Byron", "Camron", "Donavan", "Lincoln", "Terry", "Eugene", "Mohamed", "Tyrell", "Wilson", "Erick", "Francis", "Quentin", "Enrique", "Giovani", "Antoine", "Everett", "Ramiro", "Andreas", "Orion", "Ayaan", "Chace", "Moises", "Keaton", "Tanner", "Gunner", "Jacob", "Albert", "Anthony", "Ryland", "Julian", "Richard", "Connor", "Kevin", "Rishi", "Zayne", "Lamont", "John", "Jaxon", "Braedon", "Dashawn", "Marcos", "Brennan", "Landin", "Nikolas", "Lawson", "Julius", "Fernando", "Reagan", "Colton", "Cooper", "Fabian", "Cody", "Davin", "Landyn", "Leonidas", "Cedric", "Denzel", "Patrick", "Malcolm", "Brenton", "Micah", "Jayvon", "Gunnar", "Jordon", "Paxton", "Jude", "Kymani", "Mitchell", "Marley", "Coby", "Dax", "Ellis", "Gilberto", "Houston", "Messiah", "Mathias", "Elvis", "Liam", "Callum", "Martin", "Makai", "Aydin", "William", "Keyon", "Kamren", "Adam", "Chance", "Jonah", "Charles", "Chase", "Augustus", "Matias", "Victor", "Zavier", "Ibrahim", "Simon", "Brenden", "Jayce", "Reilly", "Xavier", "Jonas", "Jackson", "Marco", "Cruz", "Griffin", "Pedro", "Ethan", "Jeremiah", "Allan", "Adan", "Tristin", "Rayan", "Jabari", "Keenan", "Bobby", "Andrew", "Dylan", "Xzavier", "Cale", "Aron", "Barrett", "Carsen", "Bradley", "Dereon", "Yair", "Hugo", "Kaeden", "Niko", "Antony", "Jaron", "Sawyer", "Killian", "Marques", "Devan", "Grady", "Walter", "Jesse", "Lucian", "Larry", "Joey", "Leonardo", "Angelo", "Reece", "Matteo", "Reese", "Randy", "Alfred", "Brock", "Demarcus", "Ronin", "Mekhi", "Jessie", "Evan", "Urijah", "Marshall", "Hezekiah", "Kasey", "Terrance", "Ty", "Marquis", "Robert", "Manuel", "Colin" };
        private static readonly string[] firstFemaleNames = { "Aryanna", "Gabrielle", "Christine", "India", "Kiley", "Lucia", "Bella", "Aracely", "Carlee", "Natalie", "Adrienne", "Giselle", "Elle", "Barbara", "Mira", "Aylin", "Aisha", "Kendra", "Caylee", "Makena", "Lainey", "Keyla", "Kaila", "Amelie", "Kayla", "Allie", "Anika", "Rayna", "Nola", "Micaela", "Kassandra", "Liana", "Ashlynn", "Azaria", "Kianna", "Janiyah", "Gabriela", "Shirley", "Kallie", "Skylar", "Sarai", "Lizeth", "Lucille", "Sarah", "Melina", "Mary", "Paisley", "Lucy", "Georgia", "Madisyn", "Charity", "Alani", "Lindsay", "Aimee", "Amaya", "Elisa", "Lilah", "Lyric", "Cherish", "Lila", "Kaylie", "Lara", "Carmen", "Savannah", "Daphne", "Ryleigh", "Aubrie", "Myah", "Mallory", "Karissa", "Carissa", "Delaney", "Tianna", "Kara", "Briana", "Alma", "Brielle", "Jaylen", "Abbie", "Ali", "Peyton", "Brooke", "Stacy", "Jazlynn", "Eileen", "Willow", "Olivia", "Jamya", "Lailah", "Alicia", "Asia", "Janessa", "Saniya", "Scarlett", "Aspen", "Adison", "Violet", "Jocelynn", "Martha", "Amira", "Amelia", "Kaleigh", "Emery", "Madalynn", "Belen", "Karly", "Cloe", "Julia", "Presley", "Monica", "Liliana", "Celeste", "Hanna", "Arabella", "Armani", "Veronica", "Elizabeth", "Krystal", "Audrey", "Elaina", "Maliyah", "Vanessa", "Molly", "Isabella", "Nadia", "Destiney", "Sierra", "Kristin", "Raquel", "Faith", "Aaliyah", "Mckayla", "Ava", "Kate", "Yaritza", "Cailyn", "Karlee", "Karma", "Margaret", "Reese", "Carina", "Alexus", "Jaliyah", "Aliya", "Tara", "Alina", "Lilly", "Gianna", "Arielle", "Jaylynn", "Annika", "Jaelyn", "Nicole", "Francesca", "Camryn", "Anastasia", "Kali", "Kendall", "Dayana", "Natalya", "Neveah", "Emily", "Jocelyn", "Yareli", "Addyson", "Samantha", "Sonia", "Chloe", "Megan", "Amya", "Jakayla", "Alana", "Lana", "Meghan", "Morgan", "Natasha", "Tatum", "Lorena", "Ayla", "Quinn", "Lena", "Angelique", "Jolie", "Nathalia", "Jordan", "Rosemary", "Carly", "Ruby", "Abby", "Anya", "Haven", "Eve", "Fatima", "Taryn", "Kylie", "Macey", "Karina", "Tessa", "Gillian", "Desiree" };
        private static readonly string[] firstNames = { "Aryanna", "Gabrielle", "Christine", "India", "Kiley", "Lucia", "Bella", "Aracely", "Carlee", "Natalie", "Adrienne", "Giselle", "Elle", "Barbara", "Mira", "Aylin", "Aisha", "Kendra", "Caylee", "Makena", "Lainey", "Keyla", "Kaila", "Amelie", "Kayla", "Allie", "Anika", "Rayna", "Nola", "Micaela", "Kassandra", "Liana", "Ashlynn", "Azaria", "Kianna", "Janiyah", "Gabriela", "Shirley", "Kallie", "Skylar", "Sarai", "Lizeth", "Lucille", "Sarah", "Melina", "Mary", "Paisley", "Lucy", "Georgia", "Madisyn", "Charity", "Alani", "Lindsay", "Aimee", "Amaya", "Elisa", "Lilah", "Lyric", "Cherish", "Lila", "Kaylie", "Lara", "Carmen", "Savannah", "Daphne", "Ryleigh", "Aubrie", "Myah", "Mallory", "Karissa", "Carissa", "Delaney", "Tianna", "Kara", "Briana", "Alma", "Brielle", "Jaylen", "Abbie", "Ali", "Peyton", "Brooke", "Stacy", "Jazlynn", "Eileen", "Willow", "Olivia", "Jamya", "Lailah", "Alicia", "Asia", "Janessa", "Saniya", "Scarlett", "Aspen", "Adison", "Violet", "Jocelynn", "Martha", "Amira", "Amelia", "Kaleigh", "Emery", "Madalynn", "Belen", "Karly", "Cloe", "Julia", "Presley", "Monica", "Liliana", "Celeste", "Hanna", "Arabella", "Armani", "Veronica", "Elizabeth", "Krystal", "Audrey", "Elaina", "Maliyah", "Vanessa", "Molly", "Isabella", "Nadia", "Destiney", "Sierra", "Kristin", "Raquel", "Faith", "Aaliyah", "Mckayla", "Ava", "Kate", "Yaritza", "Cailyn", "Karlee", "Karma", "Margaret", "Reese", "Carina", "Alexus", "Jaliyah", "Aliya", "Tara", "Alina", "Lilly", "Gianna", "Arielle", "Jaylynn", "Annika", "Jaelyn", "Nicole", "Francesca", "Camryn", "Anastasia", "Kali", "Kendall", "Dayana", "Natalya", "Neveah", "Emily", "Jocelyn", "Yareli", "Addyson", "Samantha", "Sonia", "Chloe", "Megan", "Amya", "Jakayla", "Alana", "Lana", "Meghan", "Morgan", "Natasha", "Tatum", "Lorena", "Ayla", "Quinn", "Lena", "Angelique", "Jolie", "Nathalia", "Jordan", "Rosemary", "Carly", "Ruby", "Abby", "Anya", "Haven", "Eve", "Fatima", "Taryn", "Kylie", "Macey", "Karina", "Tessa", "Gillian", "Desiree", "Jadyn", "Vance", "Leon", "Jaime", "Triston", "Jayson", "Jack", "Jaydan", "Hugh", "Jaidyn", "Mario", "Cullen", "Octavio", "Raymond", "Harrison", "Kadin", "Maurice", "Layton", "Jamir", "Alden", "Bennett", "Bruno", "Izaiah", "Ignacio", "Aidyn", "Cristofer", "Sidney", "Marquise", "Deshawn", "Felix", "Conner", "Jose", "Gilbert", "Clark", "Ishaan", "Tyree", "Baron", "Byron", "Camron", "Donavan", "Lincoln", "Terry", "Eugene", "Mohamed", "Tyrell", "Wilson", "Erick", "Francis", "Quentin", "Enrique", "Giovani", "Antoine", "Everett", "Ramiro", "Andreas", "Orion", "Ayaan", "Chace", "Moises", "Keaton", "Tanner", "Gunner", "Jacob", "Albert", "Anthony", "Ryland", "Julian", "Richard", "Connor", "Kevin", "Rishi", "Zayne", "Lamont", "John", "Jaxon", "Braedon", "Dashawn", "Marcos", "Brennan", "Landin", "Nikolas", "Lawson", "Julius", "Fernando", "Reagan", "Colton", "Cooper", "Fabian", "Cody", "Davin", "Landyn", "Leonidas", "Cedric", "Denzel", "Patrick", "Malcolm", "Brenton", "Micah", "Jayvon", "Gunnar", "Jordon", "Paxton", "Jude", "Kymani", "Mitchell", "Marley", "Coby", "Dax", "Ellis", "Gilberto", "Houston", "Messiah", "Mathias", "Elvis", "Liam", "Callum", "Martin", "Makai", "Aydin", "William", "Keyon", "Kamren", "Adam", "Chance", "Jonah", "Charles", "Chase", "Augustus", "Matias", "Victor", "Zavier", "Ibrahim", "Simon", "Brenden", "Jayce", "Reilly", "Xavier", "Jonas", "Jackson", "Marco", "Cruz", "Griffin", "Pedro", "Ethan", "Jeremiah", "Allan", "Adan", "Tristin", "Rayan", "Jabari", "Keenan", "Bobby", "Andrew", "Dylan", "Xzavier", "Cale", "Aron", "Barrett", "Carsen", "Bradley", "Dereon", "Yair", "Hugo", "Kaeden", "Niko", "Antony", "Jaron", "Sawyer", "Killian", "Marques", "Devan", "Grady", "Walter", "Jesse", "Lucian", "Larry", "Joey", "Leonardo", "Angelo", "Reece", "Matteo", "Reese", "Randy", "Alfred", "Brock", "Demarcus", "Ronin", "Mekhi", "Jessie", "Evan", "Urijah", "Marshall", "Hezekiah", "Kasey", "Terrance", "Ty", "Marquis", "Robert", "Manuel", "Colin" };
        private static readonly string[] firstSurnames = { "Brock", "Thomas", "Wallace", "Nguyen", "Hoffman", "Allison", "Pearson", "Kramer", "Shannon", "Gates", "Wheeler", "Willis", "Morris", "Ford", "Rosario", "Cherry", "Mendoza", "Serrano", "Wong", "Mckinney", "Vang", "Holmes", "Payne", "Barton", "Travis", "Dudley", "Robinson", "Pittman", "Russo", "Winters", "Hurst", "Bradley", "Ho", "Atkinson", "Huang", "Herman", "Liu", "Goodwin", "Benitez", "Mcknight", "Duarte", "Branch", "Harvey", "Rivera", "Middleton", "Soto", "Conway", "Newman", "Ross", "Reed", "Baxter", "Ferguson", "Macias", "Frederick", "Turner", "Myers", "Huynh", "Anderson", "Cook", "Perry", "Grant", "Gordon", "Caldwell", "Cordova", "Glover", "Madden", "Burke", "Compton", "Spears", "Kent", "Coleman", "Hampton", "Barron", "Tate", "Gill", "Pope", "Wolfe", "Ponce", "Jennings", "Farrell", "Chang", "Young", "Steele", "Montoya", "Horn", "Barnett", "Warner", "Good", "Kaufman", "Hayes", "Cantu", "Montgomery", "Reid", "Berger", "Erickson", "Gonzalez", "Chapman", "Hudson", "Wilkinson", "Rasmussen", "Sanford", "Marsh", "Hinton", "Fernandez", "Meyer", "Griffin", "Lozano", "Schmitt", "Pugh", "Todd", "Morgan", "Mcgee", "Olson", "Barrett", "Braun", "Shepard", "Riggs", "Farmer", "Dickson", "Holder", "Ruiz", "Patterson", "Higgins", "Kim", "Chung", "Glass", "Ritter", "Meza", "Neal", "Fox", "Stein", "Spence", "Maynard", "Beck", "Hill", "Hall", "Freeman", "Pratt", "Cooper", "Logan", "Fuller", "Davila", "Vaughn", "Rios", "Ray", "Frey", "Little", "Johns", "Lopez", "Ayers", "Becker", "Valdez", "Ali", "Stephenson", "Esparza", "Austin", "Mathis", "Moran", "Dunn", "Guerra", "Roberts", "Underwood", "Padilla", "Meyers", "Hester", "Trevino", "Manning", "Carson", "Lowe", "Le", "Morrison", "Pacheco", "Cameron", "Booker", "Norris", "Beltran", "Waller", "Wade", "Mendez", "Carr", "Hardin", "Campos", "Oconnell", "Benjamin", "Gonzales", "Clayton", "Bernard", "Baird", "Cline", "Jimenez", "Burns", "Terrell", "Robertson", "Flowers", "Conner", "Rangel", "Cooley", "Day", "Collins", "Vargas" };
        private static readonly string[] alphabet = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
        private static readonly string[] qualityEmails = { "@gmail.com", "@yahoo.com", "hotmail.com", "outlook.com", "aol.com", "hotmail.co.uk", "gmx.com" };
        private static ThreadLocal<Random> rnd;

        public string randMaleName()
        {
            return firstMaleNames[rnd.Value.Next(0, firstMaleNames.Length - 1)];
        }
        public string randFemaleName()
        {
            return firstFemaleNames[rnd.Value.Next(0, 200)];
        }
        public string randFirstName()
        {
            return firstNames[rnd.Value.Next(0, firstNames.Length - 1)];
        }
        public string randSurname()
        {
            return firstSurnames[rnd.Value.Next(0, 200)];
        }
        public string randPrefix()
        {
            const string pool = "abcdefghijklmnopqrstuvwxyz0123456789";
            var builder = new StringBuilder();

            for (var i = 0; i < 5; i++)
            {
                var c = pool[rnd.Value.Next(0, pool.Length)];
                builder.Append(c);
            }

            return builder.ToString().ToUpper();
        }
        public string randLetter()
        {
            const string pool = "abcdefghijklmnopqrstuvwxyz";
            var builder = new StringBuilder();

            var c = pool[rnd.Value.Next(0, pool.Length)];
            builder.Append(c);

            return builder.ToString().ToUpper();
        }
        public string randRange(int min, int max)
        {
            return rnd.Value.Next(min, max).ToString();
        }
        public string randPassword(bool RandomChars = false)
        {
            int length = rnd.Value.Next(8,12);

            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string extra = "@!.,$*";

            StringBuilder res = new StringBuilder();

            Random rnd2 = new Random();

            res.Append(rnd2.Next(0, 9).ToString());

            while (0 < length--)
            {
                res.Append(valid[rnd2.Next(valid.Length)]);
            }

            res.Append(extra[rnd2.Next(0, extra.Length)]);
            res.Append(rnd2.Next(0, 9).ToString());
            res.Append(extra[rnd2.Next(0, extra.Length)]);

            return res.ToString().ToUpper();
        }
        public string randFakeEmail(string CurrentMailService)
        {
            return randMaleName() + randPassword() + CurrentMailService;
        }
        public string randFakeEmail(string randMaleName, string CurrentMailService)
        {
            return randMaleName + randPassword() + CurrentMailService;
        }
        public string randNumber()
        {
            return rnd.Value.Next(0, 9).ToString();
        }
        public string randHQEmailDomain()
        {
            return qualityEmails[rnd.Value.Next(0, qualityEmails.Length - 1)];
        }
    }
}
