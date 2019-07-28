/*
 * Program:         Password Manager
 * Date:            June 06, 2019
 * Description:     A database for Accounts
 * 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;        // File class
using Newtonsoft.Json;  // JsonConvert class
using Newtonsoft.Json.Schema;    // JSchema class
using Newtonsoft.Json.Linq; // JObject class

namespace PasswordManager
{

    class Program
    {
        // Declare constants for the file path names 
        private const string DATA_FILE = "myAccInfo.json";
        private const string SCHEMA_FILE = "accInfo-Schema.json";
        static int temp = 0;

        const string decor = "+----------------------------------------------------------------------+";
        static void Main(string[] args)
        {
            Console.Write("PASSWORD MANAGER PROGRAM!");

            //  a string variable
            string json_data;
            string json_schema = "";
            if (ReadFile(DATA_FILE, out json_data))
            {
                // Create a collection to hold all items
                List<Account> acc_list = new List<Account>();
                acc_list = ReadJsonFileToLib();

                //Number of acc
                int numOfAcc = 0;

                //print Info
                numOfAcc = PrintInfo(acc_list);

                //Check command
                bool done = false;
                char cm = ' ';
                do
                {
                    Console.Write("\n\tEnter a command: ");
                    cm = Console.ReadKey().KeyChar;
                    if (cm == 'm') numOfAcc = PrintInfo(acc_list);
                    else
                    {
                        if (cm == 'a')
                        {
                            bool valid;
                            Account acc = new Account();
                            do
                            {
                                Console.WriteLine("\n\nPlease enter value for the following field:\n");

                                do
                                {
                                    Console.Write("Description: ");
                                    acc.Description = Console.ReadLine();
                                } while (string.IsNullOrEmpty(acc.Description));

                                do
                                {
                                    Console.Write("User ID: ");
                                    acc.UserId = Console.ReadLine();
                                } while (string.IsNullOrEmpty(acc.UserId));

                                do
                                {
                                    Console.Write("Password: ");
                                    acc.Password.Value = Console.ReadLine();
                                } while (string.IsNullOrEmpty(acc.Password.Value));

                                // Use Passord tester class to add more property for Password Object
                                PasswordTester pw = new PasswordTester(acc.Password.Value);
                                DateTime dateNow = DateTime.Now;                                
                                acc.Password.LastReset = dateNow.ToShortDateString(); 
                                acc.Password.StrengthNum = pw.StrengthPercent;
                                acc.Password.StrengthText = pw.StrengthLabel;

                                Console.Write("Login URL: ");
                                acc.LoginUrl = Console.ReadLine();
                                Console.Write("Account #: ");
                                acc.AccountNum = Console.ReadLine();

                                try
                                {
                                    ReadFile(SCHEMA_FILE, out json_schema);
                                }
                                catch (Exception)
                                {
                                    Console.WriteLine("\nERROR:\tUnable to read the schema file.");
                                }

                                // Validate the item object against the schema using the ValidateItem() method
                                // and IF the object is invalid, repeat the user-input code above to repopulate the 
                                // object until it's valid
                                valid = ValidateItem(acc, json_schema);
                                if (!valid)
                                    Console.WriteLine("\nERROR:\tItem data does not match the required format. Please try again.\n");

                            } while (!valid);

                            acc_list.Add(acc);
                            numOfAcc = PrintInfo(acc_list);
                        }
                        else
                        {
                            if (Char.IsDigit(cm))
                            {
                                temp = int.Parse(cm.ToString());
                                if (temp < 0 || temp > numOfAcc)
                                {
                                    Console.WriteLine("\n\tInvalid input, try again!");
                                }
                                else
                                {
                                    printAccInfo(temp, acc_list);
                                }
                            }
                            else
                            {
                                //Edit account
                                if (cm == 'p')
                                {
                                    if (temp != 0)
                                    {
                                        Console.Write("\n\tEnter new password: ");
                                        acc_list[temp-1].Password.Value= Console.ReadLine();
                                        Console.Write($"\tPassword of account {temp} was changed!\n");
                                    }
                                    else
                                    {
                                        Console.WriteLine("\n\tInvalid input, try again!");
                                    }                                 
                                }
                                else
                                {
                                    if (cm == 'd')
                                    {
                                        if (temp != 0)
                                        {
                                            acc_list.RemoveAt(temp - 1);
                                            Console.WriteLine($"\n\tDeleted account number: {temp}. \n\tPress 'm' to see new account list.");
                                        }
                                        else
                                        {
                                            Console.WriteLine("\n\tInvalid input, try again!");
                                        }
                                    }
                                    else
                                    {
                                        if (cm == 'x')
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            Console.WriteLine("\n\tInvalid input, try again!");
                                        }
                                    }
                                }
                            }
                        }
                    }
                } while (!done);

                //Write the account list to a file
                string json_all = JsonConvert.SerializeObject(acc_list);
                try
                {
                    File.WriteAllText(DATA_FILE, json_all);
                    Console.WriteLine($"\nYour account list has been written to {DATA_FILE}.\n");
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"\n\nERROR: {ex.Message}.\n");
                }
            }
            else
            {
                Console.WriteLine("\nERROR:\tUnable to read the data file.");
            }

            int PrintInfo(List<Account> acc_list)
            {
                int j = 0;
                //Give info to the console
                Console.WriteLine($"\n\n{decor}");
                Console.WriteLine("\t\t\t\tACCOUNT ENTRIES");
                Console.WriteLine(decor);

                foreach (Account acc in acc_list)
                {
                    j++;
                    Console.WriteLine($"\t\t {j}.  {acc.Description}");
                }
                Console.WriteLine(decor);
                Console.WriteLine("\t*Select a number from the above list to select an entry.");
                Console.WriteLine("\t*Press 'a' to add new entry.");
                Console.WriteLine("\t*Press 'x' to quit.");
                Console.WriteLine(decor);
                return j;
            }

            void printAccInfo(int t, List<Account> acc_list)
            {
                Console.WriteLine($"\n{decor}");
                Console.WriteLine($"   {t}. {acc_list[t - 1].Description}");
                Console.WriteLine(decor);

                Console.WriteLine($"User ID            :\t{acc_list[t - 1].UserId} ");
                Console.WriteLine($"Password           :\t{acc_list[t - 1].Password.Value} ");
                Console.WriteLine($"Password strength  :\t{acc_list[t - 1].Password.StrengthText} ({acc_list[t - 1].Password.StrengthNum} %) ");
                Console.WriteLine($"Password Reset     :\t{acc_list[t - 1].Password.LastReset} ");
                Console.WriteLine($"Login URL          :\t{acc_list[t - 1].LoginUrl} ");
                Console.WriteLine(decor);
                Console.WriteLine("\t*Press 'p' to change this password.");
                Console.WriteLine("\t*Press 'd' to delete this entry.");
                Console.WriteLine("\t*Press 'm' to return to main menu.");
                Console.WriteLine(decor);
            }
        } // end Main()

        // Validates an item object against a schema (incomplete)
        private static bool ValidateItem(Account acc, string json_schema)
        {
            // Convert item object to a JSON string 
            string json_data = JsonConvert.SerializeObject(acc);

            JSchema schema = JSchema.Parse(json_schema);
            JObject itemObj = JObject.Parse(json_data);
            return itemObj.IsValid(schema);

        } // end ValidateItem()

        // Attempts to read the json file specified by 'path' into the string 'json'
        // Returns 'true' if successful or 'false' if it fails
        private static bool ReadFile(string path, out string json)
        {
            try
            {
                // Read JSON file data 
                json = File.ReadAllText(path);
                return true;
            }
            catch
            {
                json = null;
                return false;
            }
        } // end ReadFile()

        private static List<Account> ReadJsonFileToLib()
        {
            string json = File.ReadAllText("myAccInfo.json");
            return JsonConvert.DeserializeObject<List<Account>>(json);
        }
    } // end class Program

    // Definition of the classes
    public class Password
    {
        public string Value { get; set; }
        public double StrengthNum { get; set; }
        public string StrengthText { get; set; }
        public string LastReset { get; set; }

    }// end class Password
    public class Account
    {
        public string Description { get; set; }
        public string UserId { get; set; }
        public string LoginUrl { get; set; }
        public string AccountNum { get; set; }

        public Password Password = new Password();

    } // end class Account

}
