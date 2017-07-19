using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WpfApp3
{
    static class Chrome_Passwords
    {
        public static async Task<DataTable> GetPasswordsAsync()
        {
            try
            {
                return await Task.Run(() =>
                {
                    DataTable dt = new DataTable();
                //try
                //{
                //string filename = "my_chrome_passwords.html";
                //StreamWriter Writer = new StreamWriter(filename, false, Encoding.UTF8);
                string db_way = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                        + "/Google/Chrome/User Data/Default/Login Data"; // a path to a database file
                Console.WriteLine("DB file = " + db_way);
                    string db_field = "logins";   // DB table field name
                byte[] entropy = null; // DPAPI class does not use entropy but requires this parameter
                string description;    // I could not understand the purpose of a this mandatory parameter
                                       // Output always is Null
                                       // Connect to DB
                string google = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Google\Chrome\User Data\Default\Login Data";
                    string fileName = DateTime.Now.Ticks.ToString();
                    File.Copy(google, System.AppDomain.CurrentDomain.BaseDirectory + "\\" + fileName);
                //string ConnectionString = "data source=" + db_way + ";New=True;UseUTF16Encoding=True";
                string ConnectionString = "DataSource = " + System.AppDomain.CurrentDomain.BaseDirectory + "\\" + fileName + ";Versio=3;New=False;Compress=True;";
                    DataTable DB = new DataTable();
                    string sql = string.Format("SELECT * FROM {0} {1} {2}", db_field, "", "");
                //DataTable dt = new DataTable();
                dt.Columns.Add("Site URL");
                    dt.Columns.Add("Login Info");
                    dt.Columns.Add("Password");

                    using (SQLiteConnection connect = new SQLiteConnection(ConnectionString))
                    {
                        SQLiteCommand command = new SQLiteCommand(sql, connect);
                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                        adapter.Fill(DB);
                        int rows = DB.Rows.Count;
                        for (int i = 0; i < rows; i++)
                        {
                            if (DB.Rows[i][1].ToString() != string.Empty)
                            {
                            //Writer.Write(i + 1 + ") "); // Here we print order number of our trinity "site-login-password"
                            //Writer.WriteLine(DB.Rows[i][1] + "<br>"); // site URL
                            //Writer.WriteLine(DB.Rows[i][3] + "<br>"); // login
                            //                                          // Here the password description
                            byte[] byteArray = (byte[])DB.Rows[i][5];
                                byte[] decrypted = DPAPI.Decrypt(byteArray, entropy, out description);
                                //string password = new UTF8Encoding(true).GetString(decrypted);
                                string password = Encoding.UTF8.GetString(decrypted);
                                DataRow dr = dt.NewRow();
                                dr["Site URL"] = DB.Rows[i][1];
                                dr["Login Info"] = DB.Rows[i][3];
                                dr["Password"] = password;
                                dt.Rows.Add(dr);

                            //Writer.WriteLine(password + "<br><br>");
                        }

                        }

                    }
                //System.Threading.Thread.Sleep(3000);
                //foreach (DataRow item in dt.Rows)
                //{
                //    if (!item[0].ToString().ToLower().Contains("https"))
                //    {
                //        item
                //    }
                //}

                return ReverseRowsInDataTable(dt);
                //Writer.Close();
                //dataGridView1.ItemsSource = ReverseRowsInDataTable(dt).AsDataView();
                //return dt;

                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine(ex.Message);
                //    ex = ex.InnerException;
                //}
                //finally
                //{
                //    return dt;
                //}
            });
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }
            //}


            private static DataTable ReverseRowsInDataTable(DataTable inputTable)
            {
                DataTable outputTable = inputTable.Clone();

                for (int i = inputTable.Rows.Count - 1; i >= 0; i--)
                {
                    outputTable.ImportRow(inputTable.Rows[i]);
                }

                return outputTable;
            }
        }
    }

