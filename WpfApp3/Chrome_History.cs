using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;


namespace WpfApp3
{
   static class Chrome_History
    {
       public static async Task<DataTable> GetChromeHistoryDataAsync()
        {
            return await Task.Run(() => {
                try
                {
                    DataTable tmpdt = new DataTable();
                    string google = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Google\Chrome\User Data\Default\History";
                    string fileName = DateTime.Now.Ticks.ToString();
                    File.Copy(google, System.AppDomain.CurrentDomain.BaseDirectory + "\\" + fileName);
                    using (SQLiteConnection con = new SQLiteConnection("DataSource = " + System.AppDomain.CurrentDomain.BaseDirectory + "\\" + fileName + ";New=True;UseUTF16Encoding=True;"))//Versio=3;New=False;Compress=True;"))
                    {
                        con.Open();
                        //SQLiteDataAdapter da = new SQLiteDataAdapter("select url,title,visit_count,last_visit_time from urls order by last_visit_time desc", con);
                        SQLiteDataAdapter da = new SQLiteDataAdapter("select * from urls order by last_visit_time desc", con);
                        DataSet ds = new DataSet();
                        da.Fill(ds);
                        DataTable dt = ds.Tables[0];
                        tmpdt.Columns.Add("id", typeof(string));
                        tmpdt.Columns.Add("url", typeof(string));
                        tmpdt.Columns.Add("title", typeof(string));
                        tmpdt.Columns.Add("visit_count", typeof(string));
                        tmpdt.Columns.Add("last_visit_time", typeof(DateTime));
                        DataRow myRow = tmpdt.NewRow();
                        try
                        {
                            //dt.Columns["last_visit_time"].DataType = typeof(DateTime);

                            foreach (DataRow row in dt.Rows)
                            {
                                byte[] myString = Encoding.UTF8.GetBytes(row[5].ToString().ToArray());
                                DataRow tmpRow = tmpdt.NewRow();
                                tmpRow[0] = row[0].ToString();
                                tmpRow[1] = row[1].ToString();
                                //tmpRow[2] = Encoding.UTF8.GetString(myString);
                                tmpRow[2] = row[2].ToString();
                                tmpRow[3] = row[3].ToString();
                                long time = Convert.ToInt64(row["last_visit_time"].ToString());
                                DateTime gmt = DateTime.FromFileTimeUtc(10 * time);

                                DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(gmt, TimeZoneInfo.Local);

                                tmpRow[4] = localTime;

                                tmpdt.Rows.Add(tmpRow);
                            }
                            return tmpdt;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                        finally
                        {
                            con.Close();
                        }
                    }
                    try // File already open error is skipped
                    {
                        if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\" + fileName))
                            File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\" + fileName);
                        return tmpdt;
                    }

                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        return null;
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return null;
                }
                
            });
        } 
    }
}
